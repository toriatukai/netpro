using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;

/*
// ��Ԃ̒�`
public enum GameState
{
    Waiting,
    Playing,
    RoundEnd,
    GameOver
}

public class GameManager : NetworkBehaviour
{

    public static GameManager Instance { get; private set; }

    [SerializeField] private TargetSpawner _targetSpawner;
    [SerializeField] private int _totalRounds = 3; // ���v���E���h
    private int _currentRound = 0;

    private NetworkVariable<GameState> _state = new NetworkVariable<GameState>(GameState.Waiting);

    private int _finishedPlayerCount = 0; // �ł��I������v���C���[��

    private void Awake()
    {
        // Singleton�Ƃ��đ��݂�����
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // �X�e�[�g��Ԃ��v���p�e�B
    public GameState CurrentState => _state.Value;
    // ���݂̃X�e�[�g���v���C������Ԃ��v���p�e�B
    public bool IsPlaying() => _state.Value == GameState.Playing;

    void Update()
    {
        // TODO: �{�^�����������烉�E���h�J�n�ɂ���悤�ɂ���
        if (NetworkManager.Singleton.IsHost && Input.GetKeyDown(KeyCode.Space))
        {

            StartRound();
        }
    }

    public void StartRound()
    {
        _currentRound++;
        _finishedPlayerCount = 0;
        _state.Value = GameState.Playing;
        Debug.Log($"[Round {_currentRound}] Started!");

        // ���E���h�J�n����PlayerController�֒ʒm���ď�����
        foreach (var player in FindObjectsByType<PlayerController>(FindObjectsSortMode.None))
        {
            player.StartRound();
        }

        _targetSpawner.SpawnAsync().Forget();
    }

    public void NotifyPlayerFinished()
    {
        _finishedPlayerCount++;
        int playerCount = NetworkManager.Singleton.ConnectedClientsList.Count;
        Debug.Log("�I������v���C���[" + _finishedPlayerCount);
        if (_finishedPlayerCount >= playerCount) // 1 vs 1 �z��
        {
            EndRound().Forget();
        }
    }

    private async UniTask EndRound()
    {
        _state.Value = GameState.RoundEnd;

        int result = ScoreManager.Instance.DidPlayerWin(_currentRound - 1);

        // ���s�̊m�F
        if (result == -1)
            Debug.Log($"Round {_currentRound} is a draw.");
        else if (result == 0)
            Debug.Log("Host wins the round!");
        else
            Debug.Log("Client wins the round!");


        // ���E���h���܂�����Ȃ瑱����
        // TODO: ����ɃX�^�[�g������UI��ŃX�^�[�g�ł���悤�ɂ���
        if (_currentRound >= _totalRounds)
        {
            FinishGame();
        }
        else
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(3f)); // �����҂��Ď����E���h�J�n
            StartRound();
        }
    }

    private void FinishGame()
    {
        _state.Value = GameState.GameOver;

        int winner = ScoreManager.Instance.GamePlayerWin();
        if (winner == -1)
            Debug.Log("Game result: Draw.");
        else if (winner == 0)
            Debug.Log("Game result: Host wins!");
        else
            Debug.Log("Game result: Client wins!");
    }
}*/

public class GameManager : NetworkBehaviour
{
    public static GameManager Instance;

    private Dictionary<ulong, PlayerRoundData> playerDataDict = new();
    private int currentRound = 0;
    private bool roundInProgress = false;

    [SerializeField] private GameObject targetPrefab;

    private float targetDisplayTime;
    public float TargetDisplayTime => targetDisplayTime;

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
        StartNextRound();
    }

    private void StartNextRound()
    {
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
            p.ResetRound();
        }
        roundInProgress = true;

        SpawnTargetAsync().Forget();
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

        targetDisplayTime = Time.time;

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
        roundInProgress = false;

        var players = playerDataDict.Values.ToList();

        float timeA = players[0].ReactionTimes[^1];
        float timeB = players[1].ReactionTimes[^1];

        Debug.Log($"���E���h{currentRound}���� ���蒆...");

        int result = CompareTimes(timeA, timeB);
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

        StartNextRound();
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

    public void ResetRound()
    {
        RemainingBullets = 5;
        HasFinishedThisRound = false;
    }
}