using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

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
}
