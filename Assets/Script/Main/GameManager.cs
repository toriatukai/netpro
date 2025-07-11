using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;

/*
// 状態の定義
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
    [SerializeField] private int _totalRounds = 3; // 合計ラウンド
    private int _currentRound = 0;

    private NetworkVariable<GameState> _state = new NetworkVariable<GameState>(GameState.Waiting);

    private int _finishedPlayerCount = 0; // 打ち終わったプレイヤー数

    private void Awake()
    {
        // Singletonとして存在させる
        if (Instance != null && Instance != this) Destroy(gameObject);
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    // ステートを返すプロパティ
    public GameState CurrentState => _state.Value;
    // 現在のステートがプレイ中かを返すプロパティ
    public bool IsPlaying() => _state.Value == GameState.Playing;

    void Update()
    {
        // TODO: ボタンを押したらラウンド開始にするようにする
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

        // ラウンド開始時にPlayerControllerへ通知して初期化
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
        Debug.Log("終わったプレイヤー" + _finishedPlayerCount);
        if (_finishedPlayerCount >= playerCount) // 1 vs 1 想定
        {
            EndRound().Forget();
        }
    }

    private async UniTask EndRound()
    {
        _state.Value = GameState.RoundEnd;

        int result = ScoreManager.Instance.DidPlayerWin(_currentRound - 1);

        // 勝敗の確認
        if (result == -1)
            Debug.Log($"Round {_currentRound} is a draw.");
        else if (result == 0)
            Debug.Log("Host wins the round!");
        else
            Debug.Log("Client wins the round!");


        // ラウンドがまだあるなら続ける
        // TODO: 勝手にスタートさせずUI上でスタートできるようにする
        if (_currentRound >= _totalRounds)
        {
            FinishGame();
        }
        else
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(3f)); // 少し待って次ラウンド開始
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
        Debug.Log("ゲーム開始");
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
        Debug.Log($"ラウンド{currentRound}開始");

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
        Debug.Log($"ラウンド{currentRound}: ターゲット出現まで {delay:F2}秒待機");
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        if (!IsServer) return;

        // 的の出現位置比率を決定
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Debug.Log($"ターゲット出現座標比率 x:{xRate:F2} y:{yRate:F2}");

        targetDisplayTime = Time.time;

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
        roundInProgress = false;

        var players = playerDataDict.Values.ToList();

        float timeA = players[0].ReactionTimes[^1];
        float timeB = players[1].ReactionTimes[^1];

        Debug.Log($"ラウンド{currentRound}結果 判定中...");

        int result = CompareTimes(timeA, timeB);
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

        StartNextRound();
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