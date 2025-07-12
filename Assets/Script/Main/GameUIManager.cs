using TMPro;
using UnityEngine;

public class GameUIManager : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI roundText;
    [SerializeField] private TextMeshProUGUI reactionTimeText;

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

    public void UpdateRoundText(int round)
    {
        roundText.text = $"Round {round}";
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
}
