using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Threading;
using Random = UnityEngine.Random;



public class GameManager : NetworkBehaviour
{
    // ゲームの状態
    public enum GameState
    {
        Connecting,
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

    [SerializeField] private int maxBullets = 5;

    private NetworkVariable<float> targetDisplayTime = new(writePerm: NetworkVariableWritePermission.Server);
    public float TargetDisplayTime => targetDisplayTime.Value;

    private float minDelay = 2f;
    private float maxDelay = 8f;

    private bool roundEndCheckScheduled = false;

    private CancellationTokenSource targetSpawnCts;

    private HashSet<ulong> readyPlayers = new();


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

    [ServerRpc(RequireOwnership = false)]
    public void NotifyReadyForRoundServerRpc(ulong clientId)
    {
        if (!readyPlayers.Contains(clientId))
            readyPlayers.Add(clientId);

        Debug.Log($"Client {clientId} is ready. Total ready: {readyPlayers.Count}");

        if (readyPlayers.Count >= 2)
        {
            readyPlayers.Clear();
            StartRoundCountdown().Forget();
        }
    }

    private async UniTaskVoid StartRoundCountdown()
    {
        foreach (var kv in playerDataDict)
        {
            var clientId = kv.Key;
            var data = kv.Value;

            if (data.SelectedSkill == SkillType.Artillery && data.WillUseSkill)
            {
                data.WillUseSkill = false;
                ApplyArtillerySkillClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }
            else
            {
                ResetCrosshairClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }
        }

        ShowCountdownTextClientRpc("3");
        await UniTask.Delay(1000);
        ShowCountdownTextClientRpc("2");
        await UniTask.Delay(1000);
        ShowCountdownTextClientRpc("1");
        await UniTask.Delay(1000);
        ShowCountdownTextClientRpc("");

        SetGameState(GameState.Playing);
        StartNextRound();
    }

    [ClientRpc]
    private void ApplyArtillerySkillClientRpc(ClientRpcParams clientRpcParams = default)
    {
        PlayerController.LocalInstance.ApplyArtillerySkill();
        GameUIManager.Instance.toggle.Value = false;
        GameUIManager.Instance.toggle.gameObject.SetActive(false);
    }

    [ClientRpc]
    private void ResetCrosshairClientRpc(ClientRpcParams clientRpcParams = default)
    {
        if (PlayerController.LocalInstance != null)
        {
            PlayerController.LocalInstance.ResetCrosshairSize();
        }
    }

    [ClientRpc]
    private void ShowCountdownTextClientRpc(string text)
    {
        GameUIManager.Instance.ShowCountdownText(text);
        GameUIManager.Instance.HideBeforeRoundPanel();
    }

    public void SetGameState(GameState newState)
    {
        currentState.Value = newState;
        Debug.Log($"GameState changed to: {newState}");
    }


    private void StartNextRound()
    {
        if (!IsServer) return;

        // キャンセル前のタスクの中断
        targetSpawnCts?.Cancel();
        targetSpawnCts = new CancellationTokenSource();

        currentRound++;
        Debug.Log($"ラウンド{currentRound}開始");

        foreach (var p in playerDataDict.Values)
        {
            p.ResetRound(maxBullets);
        }

        ResetClientStateClientRpc();

        UpdateRoundClientRpc(currentRound);

        SetGameState(GameState.Playing);

        SpawnTargetAsync(targetSpawnCts.Token).Forget();
    }

    [ClientRpc]
    private void ResetClientStateClientRpc()
    {

        // 例: クロスヘアの弾数・命中フラグをリセット（オーナーだけ）
        if (PlayerController.LocalInstance != null)
        {
            PlayerController.LocalInstance.ResetClientRound();

        }

    }

    private async UniTask SpawnTargetAsync(CancellationToken token)
    {
        float delay = Random.Range(minDelay, maxDelay);
        Debug.Log($"ラウンド{currentRound}: ターゲット出現まで {delay:F2}秒待機");

        SendSkillCountdownToClientsClientRpc(delay); // ガンマンのスキル確認


        try
        {
            await UniTask.Delay(System.TimeSpan.FromSeconds(delay), cancellationToken: token);
        }
        catch (OperationCanceledException)
        {
            Debug.Log("的の出現はキャンセルされました");
            return;
        }


        if (!IsServer) return;

        // 的の出現位置比率を決定
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Debug.Log($"ターゲット出現座標比率 x:{xRate:F2} y:{yRate:F2}");

        targetDisplayTime.Value = Time.time;

        // 全クライアントに的出現通知
        SendSpawnTargetClientRpc(xRate, yRate);
        TryShowDecoyTarget(xRate, yRate);
    }

    private void TryShowDecoyTarget(float trueX, float trueY)
    {
        // デコイ表示処理
        foreach (var kv in playerDataDict)
        {
            ulong clientId = kv.Key;
            var data = kv.Value;

            if (data.SelectedSkill == SkillType.Engineer && data.WillUseSkill)
            {
                data.WillUseSkill = false;

                // 本物とかぶらない位置
                Vector2 decoyPos;
                do
                {
                    decoyPos = new Vector2(Random.Range(0f, 1f), Random.Range(0f, 1f));
                } while (Vector2.Distance(decoyPos, new Vector2(trueX, trueY)) < 0.2f); // 距離が近すぎたら再生成

                // 相手にだけ送信
                foreach (var otherClient in playerDataDict.Keys)
                {
                    Debug.Log($"デコイ送信対象: {otherClient} に decoyX:{decoyPos.x} / decoyY:{decoyPos.y}");
                    if (otherClient != clientId)
                    {
                        SendDecoyTargetClientRpc(decoyPos.x, decoyPos.y, new ClientRpcParams
                        {
                            Send = new ClientRpcSendParams
                            {
                                TargetClientIds = new[] { otherClient }
                            }
                        });
                    }
                }

                DisableSkillToggleClientRpc(new ClientRpcParams
                {
                    Send = new ClientRpcSendParams
                    {
                        TargetClientIds = new[] { clientId }
                    }
                });
            }
            else
            {
                Debug.Log(GameUIManager.Instance.toggle.Value);
            }
        }
    }
    [ClientRpc]
    private void DisableSkillToggleClientRpc(ClientRpcParams clientRpcParams = default)
    {
        GameUIManager.Instance.toggle.Value = false;
        GameUIManager.Instance.toggle.gameObject.SetActive(false);
    }


    [ClientRpc]
    private void SendDecoyTargetClientRpc(float xRate, float yRate, ClientRpcParams clientRpcParams = default)
    {
        TargetSpawner.Instance.SpawnDecoyByRatio(xRate, yRate);
    }

    [ClientRpc]
    private void SendSkillCountdownToClientsClientRpc(float delay)
    {
        if (GameUIManager.Instance.currentSkill == SkillType.Gunman &&
            GameUIManager.Instance.toggle.Value) // トグルがオンなら発動
        {
            // スキルを使い終わったら無効化
            GameUIManager.Instance.toggle.Value = false;
            GameUIManager.Instance.toggle.gameObject.SetActive(false);

            GameUIManager.Instance.StartSkillCountdown(delay);
        }
        else
        {
            GameUIManager.Instance.HideSkillCountdown();
        }
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

    [ServerRpc(RequireOwnership = false)]
    public void SetSelectedSkillServerRpc(ulong clientId, SkillType skill, bool willUse)
    {
        if (playerDataDict.TryGetValue(clientId, out var data))
        {
            data.SelectedSkill = skill;
            data.WillUseSkill = willUse;
            Debug.Log($"Client {clientId} selected skill: {skill}, willUse: {willUse}");
        }
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

        SetGameState(GameState.RoundEnd); // ラウンド終了

        targetSpawnCts?.Cancel(); // 的の出現をキャンセル

        ClearTargetClientRpc(); // 的の削除

        var players = playerDataDict.Values.ToList();

        float timeA = players[0].ReactionTimes[^1];
        float timeB = players[1].ReactionTimes[^1];

        Debug.Log($"ラウンド{currentRound}結果 判定中...");

        GameUIManager.Instance.UpdateRoundResult(currentRound - 1, timeA, timeB);

        int result = CompareTimes(timeA, timeB);

        string winnerName = result switch
        {
            1 => "ホストの勝利！",
            -1 => "クライアントの勝利！",
            _ => "引き分け"
        };

        UpdateRoundResultClientRpc(currentRound - 1, timeA, timeB);
        ShowResultPanelClientRpc(winnerName);

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

        bool hasNextRound = currentRound < 3;
        GameUIManager.Instance.ShowNextOrEndButton(hasNextRound);
        ShowNextOrEndButtonClientRpc(hasNextRound);

        foreach (var data in playerDataDict.Values)
        {
            //data.WillUseSkill = false;
        }

        roundEndCheckScheduled = false;

        UniTask.Void(async () =>
        {
            await UniTask.Delay(2000);
            if (currentRound >= 3)
            {
                EvaluateFinalResult(); // 最終判定に進む
            }
            else
            {
                UpdateRoundClientRpc(currentRound + 1);
                //ShowBeforeRoundPanelClientRpc(); // パネル再表示
            }

        });
    }

    [ClientRpc]
    private void ShowResultPanelClientRpc(string winnerName)
    {
        GameUIManager.Instance.ShowResultPanel();
        GameUIManager.Instance.SetWinnerText(winnerName);
    }

    [ClientRpc]
    private void UpdateRoundResultClientRpc(int roundIndex, float hostTime, float clientTime)
    {
        GameUIManager.Instance.UpdateRoundResult(roundIndex, hostTime, clientTime);
    }

    [ClientRpc]
    private void ShowNextOrEndButtonClientRpc(bool hasNextRound)
    {
        GameUIManager.Instance.ShowNextOrEndButton(hasNextRound);
        GameUIManager.Instance.EnableStartButton();
    }

    [ClientRpc]
    private void UpdateRoundClientRpc(int round)
    {
        GameUIManager.Instance.UpdateRoundText(round);
        GameUIManager.Instance.ResetReactionTime();
        GameUIManager.Instance.UpdateBulletsText(maxBullets);
    }

    [ClientRpc]
    private void ClearTargetClientRpc()
    {
        TargetSpawner.Instance.ClearAllTargets();
        TargetSpawner.Instance.ClearDecoy(); // デコイも削除
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

        string winnerName = "";
        var players = playerDataDict.Values.ToList();
        int winA = players[0].WinCount;
        int winB = players[1].WinCount;

        if (winA > winB)
        {
            Debug.Log($"Player {players[0].ClientId} の勝利！");
            winnerName = "ラウンド取得数でホストの勝利！";
        }
        else if (winB > winA)
        {
            Debug.Log($"Player {players[1].ClientId} の勝利！");
            winnerName = "ラウンド取得数でクライアントの勝利！";
        }
        else
        {
            float sumA = players[0].ReactionTimes.Sum();
            float sumB = players[1].ReactionTimes.Sum();

            if (sumA < sumB)
            {
                Debug.Log($"Player {players[0].ClientId} がタイム合計で勝利！");
                winnerName = "タイム合計でホストの勝利！";
            }
            else if (sumB < sumA)
            {
                Debug.Log($"Player {players[1].ClientId} がタイム合計で勝利！");
                winnerName = "タイム合計でクライアントの勝利！";
            }
            else
            {
                Debug.Log("合計タイムも同じためホストの勝ち！");
                winnerName = "タイム合計で引き分け！";
            }
        }

        
        ShowResultPanelClientRpc(winnerName);

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
    public bool WillUseSkill = false;

    public SkillType SelectedSkill = SkillType.None;

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