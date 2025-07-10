using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

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
}
