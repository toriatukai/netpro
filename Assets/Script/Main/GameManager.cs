using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class GameManager : NetworkBehaviour
{
    public enum GameState
    {
        Connecting,
        WaitingForPlayers,
        Countdown,
        Playing,
        RoundEnd,
        GameOver
    }
    //public static GameManager Instance;

    public static GameManager Instance { get; private set; }

    private NetworkVariable<GameState> currentState = new NetworkVariable<GameState>(GameState.Connecting, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Server);

    public GameState CurrentState => currentState.Value;

    private Dictionary<ulong, PlayerRoundData> playerDataDict = new();
    private int currentRound = 0;
    //private bool roundInProgress = false;

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private int maxBullets = 5;

    private NetworkVariable<float> targetDisplayTime = new(writePerm: NetworkVariableWritePermission.Server);
    public float TargetDisplayTime => targetDisplayTime.Value;

    private float minDelay = 2f;
    private float maxDelay = 8f;

    private bool roundEndCheckScheduled = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);
        else
            Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            InitPlayers();
        }
    }

    private void InitPlayers()
    {
        playerDataDict.Clear();
        foreach (var clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            playerDataDict[clientId] = new PlayerRoundData(clientId);
            Debug.Log($"Player added: {clientId}");
        }
        Debug.Log("All players initialized.");
    }

    public void StartGame()
    {
        if (!IsServer) return;

        currentRound = 0;
        Debug.Log("�Q�[���J�n");
        SetGameState(GameState.Countdown);
        CountdownAndStartNextRound().Forget();
    }


    private async UniTaskVoid CountdownAndStartNextRound()
    {
        await UniTask.Delay(2000); // �J�E���g�_�E�����o�i�C�Ӂj

        SetGameState(GameState.Playing); // �����Ō��Ă�悤�ɂȂ�

        StartNextRound();
    }
    public void SetGameState(GameState newState)
    {
        currentState.Value = newState;
        Debug.Log($"GameState changed to: {newState}");
    }


    private void StartNextRound()
    {
        //GameUIManager.Instance.ResetReactionTime(); // �e�L�X�g�̃��Z�b�g
        if (!IsServer) return;

        if (currentRound >= 3)
        {
            EvaluateFinalResult();
            return;
        }

        currentRound++;
        Debug.Log($"���E���h{currentRound}�J�n");

        foreach (var p in playerDataDict.Values)
        {
            p.ResetRound(maxBullets);
        }

        ResetClientStateClientRpc();

        UpdateRoundClientRpc(currentRound);

        SpawnTargetAsync().Forget();
    }

    [ClientRpc]
    private void ResetClientStateClientRpc()
    {
        Debug.Log("�N���C�A���g���Ń��Z�b�g�������s");

        // ��: �N���X�w�A�̒e���E�����t���O�����Z�b�g�i�I�[�i�[�����j
        if (PlayerController.LocalInstance != null)
        {
            PlayerController.LocalInstance.ResetClientRound();
        }
    }

    [ClientRpc]
    private void UpdateRoundClientRpc(int round)
    {
        GameUIManager.Instance.UpdateRoundText(round);
        GameUIManager.Instance.ResetReactionTime();
        GameUIManager.Instance.UpdateBulletsText(maxBullets);
    }

    private async UniTask SpawnTargetAsync()
    {
        float delay = Random.Range(minDelay, maxDelay);
        Debug.Log($"���E���h{currentRound}: �^�[�Q�b�g�o���܂� {delay:F2}�b�ҋ@");
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        if (!IsServer) return;

        // �I�̏o���ʒu�䗦������
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Debug.Log($"�^�[�Q�b�g�o�����W�䗦 x:{xRate:F2} y:{yRate:F2}");

        targetDisplayTime.Value = Time.time;

        // �S�N���C�A���g�ɓI�o���ʒm
        SendSpawnTargetClientRpc(xRate, yRate);
    }

    [ClientRpc]
    private void SendSpawnTargetClientRpc(float xRate, float yRate)
    {
        TargetSpawner.Instance.SpawnTargetByRatio(xRate, yRate);
    }

    // �v���C���[���甽�����Ԃ���M����RPC
    [ServerRpc(RequireOwnership = false)]
    public void SubmitReactionTimeServerRpc(ulong clientId, float reactionTime)
    {
        if (!playerDataDict.ContainsKey(clientId)) return;

        var player = playerDataDict[clientId];

        if (player.HasFinishedThisRound)
        {
            Debug.LogWarning($"Client {clientId} �͊��ɔ������Ԃ𑗐M�ς�");
            return;
        }

        player.ReactionTimes.Add(reactionTime);
        player.HasFinishedThisRound = true;

        Debug.Log($"Client {clientId} �������Ԏ�M: {reactionTime}");

        CheckRoundEnd();
    }

    private void CheckRoundEnd()
    {
        if (roundEndCheckScheduled) return;

        if (playerDataDict.Values.All(p => p.HasFinishedThisRound))
        {
            roundEndCheckScheduled = true;
            EvaluateRoundResult();
        }
    }

    private void EvaluateRoundResult()
    {
        //roundInProgress = false;

        SetGameState(GameState.RoundEnd); // ���E���h�I��

        ClearTargetClientRpc(); // �I�̍폜

        var players = playerDataDict.Values.ToList();

        float timeA = players[0].ReactionTimes[^1];
        float timeB = players[1].ReactionTimes[^1];

        Debug.Log($"���E���h{currentRound}���� ���蒆...");

        int result = CompareTimes(timeA, timeB);
        Debug.Log("Player0: " + timeA + ", Player1: " + timeB + ", result: " + result);
        if (result == 0)
            Debug.Log("���E���h��������");
        else if (result == 1)
        {
            players[0].WinCount++;
            Debug.Log($"Player {players[0].ClientId} �̏���");
        }
        else
        {
            players[1].WinCount++;
            Debug.Log($"Player {players[1].ClientId} �̏���");
        }

        roundEndCheckScheduled = false;

        UniTask.Void(async () =>
        {
            await UniTask.Delay(2000);
            SetGameState(GameState.Playing);
            
            StartNextRound();
        });
    }

    [ClientRpc]
    private void ClearTargetClientRpc()
    {
        TargetSpawner.Instance.ClearAllTargets();
    }


    private int CompareTimes(float a, float b)
    {
        // -1�͌����؂�⎸�s�Ȃǂŕ�������
        if (a < 0 && b < 0) return 0;
        if (a < 0) return -1;
        if (b < 0) return 1;
        if (a < b) return 1;
        if (a > b) return -1;
        return 0;
    }

    private void EvaluateFinalResult()
    {
        Debug.Log("�Q�[���I���B�ŏI���Ҕ���");

        var players = playerDataDict.Values.ToList();
        int winA = players[0].WinCount;
        int winB = players[1].WinCount;

        if (winA > winB)
            Debug.Log($"Player {players[0].ClientId} �̏����I");
        else if (winB > winA)
            Debug.Log($"Player {players[1].ClientId} �̏����I");
        else
        {
            float sumA = players[0].ReactionTimes.Sum();
            float sumB = players[1].ReactionTimes.Sum();

            if (sumA < sumB)
                Debug.Log($"Player {players[0].ClientId} ���^�C�����v�ŏ����I");
            else if (sumB < sumA)
                Debug.Log($"Player {players[1].ClientId} ���^�C�����v�ŏ����I");
            else
                Debug.Log("���v�^�C�����������߃z�X�g�̏����I");
        }

        SetGameState(GameState.GameOver); // �Q�[���I��
    }

}

public class PlayerRoundData
{
    
    public ulong ClientId;
    public List<float> ReactionTimes = new();
    public int WinCount = 0;
    public int RemainingBullets = 5;
    public bool HasFinishedThisRound = false;

    public PlayerRoundData(ulong clientId)
    {
        ClientId = clientId;
    }

    public void ResetRound(int maxBullets)
    {
        RemainingBullets = maxBullets;
        HasFinishedThisRound = false;
    }
}