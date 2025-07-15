using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Cysharp.Threading.Tasks;

public class GameUIManager : MonoBehaviour
{
    // ゲーム内で表示するよう
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI reactionTimeText;
    [SerializeField] private TextMeshProUGUI bulletsText;
    [SerializeField] private TextMeshProUGUI countdownText;

    [SerializeField] private TextMeshProUGUI showTargetText;

    // ラウンド開始前に表示する用
    [SerializeField] private GameObject beforeRoundPanel;
    [SerializeField] private TextMeshProUGUI skillNameText;
    [SerializeField] private TextMeshProUGUI skillExplanationText;
    [SerializeField] private TextMeshProUGUI attentionText;
    [SerializeField] public  Toggle toggle;
    [SerializeField] private Button startButton;

    // 結果を表示する用
    [SerializeField] private GameObject resultPanel;
    [SerializeField] private TextMeshProUGUI[] roundResultText;
    [SerializeField] private TextMeshProUGUI winnerText;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button endButton;

    public SkillType currentSkill { get; private set; } = SkillType.None;

    public static GameUIManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }

    }

    private void Start()
    {
        startButton.onClick.AddListener(OnStartButtonClicked);
        countdownText.text = "";
        winnerText.text = "";
        endButton.gameObject.SetActive(false);

        nextButton.onClick.AddListener(() =>
        {
            HideResultPanel();
            ShowBeforeRoundPanel();
        });

        endButton.onClick.AddListener(() =>
        {
            // ネットワーク切断とシーン遷移処理
            Unity.Netcode.NetworkManager.Singleton.Shutdown();
            UnityEngine.SceneManagement.SceneManager.LoadScene("TitleScene");
        });
        HideResultPanel();
        ShowBeforeRoundPanel();

        SetSkill(currentSkill);
    }

    private void Update()
    {
        // デバッグ用キー操作でスキル変更
        if (Input.GetKeyDown(KeyCode.Alpha1)) SetSkill(SkillType.Gunman);
        if (Input.GetKeyDown(KeyCode.Alpha2)) SetSkill(SkillType.Artillery);
        if (Input.GetKeyDown(KeyCode.Alpha3)) SetSkill(SkillType.Engineer);
    }
    private void OnStartButtonClicked()
    {
        // プレイヤーのローカルコントローラーから通知を呼ぶ（以下で説明）
        if (PlayerController.LocalInstance != null)
        {
            PlayerController.LocalInstance.NotifyReadyForRound();
        }

        startButton.interactable = false;
    }

    public void EnableStartButton()
    {
        startButton.interactable = true;
    }
    public void ShowResultPanel()
    {
        resultPanel.SetActive(true);
    }

    public void HideResultPanel()
    {
        resultPanel.SetActive(false);
    }

    public void ShowBeforeRoundPanel()
    {
        beforeRoundPanel.SetActive(true);
        countdownText.text = "";
        startButton.interactable = true;
    }
    public void ShowNextOrEndButton(bool hasNextRound)
    {
        nextButton.gameObject.SetActive(hasNextRound);
        endButton.gameObject.SetActive(!hasNextRound);
    }

    public void ShowCountdownText(string text)
    {
        countdownText.text = text;
    }

    public void HideBeforeRoundPanel()
    {
        beforeRoundPanel.SetActive(false);
    }

    public void UpdateRoundText(int round)
    {
        roundText.text = $"Round {round}";
    }

    public void UpdateBulletsText(int num)
    {
        bulletsText.text = $"Bullets: {num}";
    }


    public void SetReactionTime(float time)
    {
        if (time < 0f)
        {
            reactionTimeText.text = "Time: emptyAmmo";
        }
        else
        {
            reactionTimeText.text = $"Time: {time:00.00}sec";
        }
    }

    public void ResetReactionTime()
    {
        reactionTimeText.text = "Time: 00.00sec";
    }

    public void UpdateRoundResult(int roundIndex, float hostTime, float clientTime)
    {
        if (roundIndex < 0 || roundIndex >= roundResultText.Length) return;

        string hostStr = hostTime >= 0 ? hostTime.ToString("00.00") : "emptyAmmo";
        string clientStr = clientTime >= 0 ? clientTime.ToString("00.00") : "emptyAmmo";

        roundResultText[roundIndex].text = $"{hostStr}sec vs {clientStr}sec";
    }
    public void SetWinnerText(string winnerName)
    {
        winnerText.text = winnerName;
    }
    public void SetNextButtonActive(bool active)
    {
        nextButton.gameObject.SetActive(active);
    }

    // スキル関連メソッド
    public void SetSkill(SkillType skill)
    {
        currentSkill = skill;

        switch (skill)
        {
            case SkillType.Gunman:
                skillNameText.text = "スキル: ガンマン";
                skillExplanationText.text = "ラウンド開始時ターゲットの出現時間が表示されます。";
                break;

            case SkillType.Artillery:
                skillNameText.text = "スキル: 砲兵";
                skillExplanationText.text = "クロスヘアの命中判定が広がり、命中しやすくなります。";
                break;

            case SkillType.Engineer:
                skillNameText.text = "スキル: エンジニア";
                skillExplanationText.text = "相手の画面にデコイの的が表示されます。";
                break;

            default:
                skillNameText.text = "スキル: 未選択";
                skillExplanationText.text = "スキルを選択してください。";
                break;
        }
    }

    public void ShowSkillCountdown(string text)
    {
        showTargetText.gameObject.SetActive(true);
        showTargetText.text = text;
    }

    public void HideSkillCountdown()
    {
        showTargetText.gameObject.SetActive(false);
    }

    public void StartSkillCountdown(float totalTime)
    {
        showTargetText.gameObject.SetActive(true);
        _ = UpdateCountdownAsync(totalTime);
    }

    private async UniTaskVoid UpdateCountdownAsync(float totalTime)
    {
        float elapsed = 0f;

        while (elapsed < totalTime)
        {
            float remaining = totalTime - elapsed;
            showTargetText.text = $"Target in {remaining:F1} sec";

            await UniTask.Delay(100); // 0.1秒ごとに更新
            elapsed += 0.1f;
        }

        HideSkillCountdown(); // 最後に非表示
    }

}
