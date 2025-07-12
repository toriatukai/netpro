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
        Debug.Log("ゲーム開始");
        SetGameState(GameState.Countdown);
        CountdownAndStartNextRound().Forget();
    }


    private async UniTaskVoid CountdownAndStartNextRound()
    {
        await UniTask.Delay(2000); // カウントダウン演出（任意）

        SetGameState(GameState.Playing); // ここで撃てるようになる

        StartNextRound();
    }
    public void SetGameState(GameState newState)
    {
        currentState.Value = newState;
        Debug.Log($"GameState changed to: {newState}");
    }


    private void StartNextRound()
    {
        //GameUIManager.Instance.ResetReactionTime(); // テキストのリセット
        if (!IsServer) return;

        if (currentRound >= 3)
        {
            EvaluateFinalResult();
            return;
        }

        currentRound++;
        Debug.Log($"ラウンド{currentRound}開始");

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
        Debug.Log("クライアント側でリセット処理実行");

        // 例: クロスヘアの弾数・命中フラグをリセット（オーナーだけ）
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
        Debug.Log($"ラウンド{currentRound}: ターゲット出現まで {delay:F2}秒待機");
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        if (!IsServer) return;

        // 的の出現位置比率を決定
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Debug.Log($"ターゲット出現座標比率 x:{xRate:F2} y:{yRate:F2}");

        targetDisplayTime.Value = Time.time;

        // 全クライアントに的出現通知
        SendSpawnTargetClientRpc(xRate, yRate);
    }

    [ClientRpc]
    private void SendSpawnTargetClientRpc(float xRate, float yRate)
    {
        TargetSpawner.Instance.SpawnTargetByRatio(xRate, yRate);
    }

    // プレイヤーから反応時間を受信するRPC
    [ServerRpc(RequireOwnership = false)]
    public void SubmitReactionTimeServerRpc(ulong clientId, float reactionTime)
    {
        if (!playerDataDict.ContainsKey(clientId)) return;

        var player = playerDataDict[clientId];

        if (player.HasFinishedThisRound)
        {
            Debug.LogWarning($"Client {clientId} は既に反応時間を送信済み");
            return;
        }

        player.ReactionTimes.Add(reactionTime);
        player.HasFinishedThisRound = true;

        Debug.Log($"Client {clientId} 反応時間受信: {reactionTime}");

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

        SetGameState(GameState.RoundEnd); // ラウンド終了

        ClearTargetClientRpc(); // 的の削除

        var players = playerDataDict.Values.ToList();

        float timeA = players[0].ReactionTimes[^1];
        float timeB = players[1].ReactionTimes[^1];

        Debug.Log($"ラウンド{currentRound}結果 判定中...");

        int result = CompareTimes(timeA, timeB);
        Debug.Log("Player0: " + timeA + ", Player1: " + timeB + ", result: " + result);
        if (result == 0)
            Debug.Log("ラウンド引き分け");
        else if (result == 1)
        {
            players[0].WinCount++;
            Debug.Log($"Player {players[0].ClientId} の勝ち");
        }
        else
        {
            players[1].WinCount++;
            Debug.Log($"Player {players[1].ClientId} の勝ち");
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
        // -1は撃ち切りや失敗などで負け扱い
        if (a < 0 && b < 0) return 0;
        if (a < 0) return -1;
        if (b < 0) return 1;
        if (a < b) return 1;
        if (a > b) return -1;
        return 0;
    }

    private void EvaluateFinalResult()
    {
        Debug.Log("ゲーム終了。最終勝者判定");

        var players = playerDataDict.Values.ToList();
        int winA = players[0].WinCount;
        int winB = players[1].WinCount;

        if (winA > winB)
            Debug.Log($"Player {players[0].ClientId} の勝利！");
        else if (winB > winA)
            Debug.Log($"Player {players[1].ClientId} の勝利！");
        else
        {
            float sumA = players[0].ReactionTimes.Sum();
            float sumB = players[1].ReactionTimes.Sum();

            if (sumA < sumB)
                Debug.Log($"Player {players[0].ClientId} がタイム合計で勝利！");
            else if (sumB < sumA)
                Debug.Log($"Player {players[1].ClientId} がタイム合計で勝利！");
            else
                Debug.Log("合計タイムも同じためホストの勝ち！");
        }

        SetGameState(GameState.GameOver); // ゲーム終了
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