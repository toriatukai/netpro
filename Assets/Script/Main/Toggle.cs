using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

// トグル
public class Toggle : MonoBehaviour
{
    [SerializeField] private Image backgroundImage;
    [SerializeField] private RectTransform handle;
    [SerializeField] private bool onAwake;

    // トグルの値
    [NonSerialized] public bool Value;

    private float handlePosX;
    private Sequence sequence;

    private static readonly Color OFF_BG_COLOR = new Color(0.6f, 0.6f, 0.6f);
    private static readonly Color ON_BG_COLOR = new Color(0.2f, 0.84f, 0.3f);

    private const float SWITCH_DURATION = 0.36f;

    private void Start()
    {
        handlePosX = Mathf.Abs(handle.anchoredPosition.x);
        Value = onAwake;
        UpdateToggle(0);
    }

    // トグルのボタンアクションに設定しておく
    public void SwitchToggle()
    {
        Value = !Value;
        UpdateToggle(SWITCH_DURATION);
        if (GameUIManager.Instance != null && GameManager.Instance != null)
        {
            ulong clientId = Unity.Netcode.NetworkManager.Singleton.LocalClientId;
            SkillType selectedSkill = Value ? GameUIManager.Instance.currentSkill : SkillType.None;
            GameManager.Instance.SetSelectedSkillServerRpc(clientId, selectedSkill, Value);
        
        }
    }

    // 状態を反映させる
    private void UpdateToggle(float duration)
    {
        var bgColor = Value ? ON_BG_COLOR : OFF_BG_COLOR;
        var handleDestX = Value ? handlePosX : -handlePosX;

        sequence?.Complete();
        sequence = DOTween.Sequence();
        sequence.Append(backgroundImage.DOColor(bgColor, duration))
            .Join(handle.DOAnchorPosX(handleDestX, duration / 2));
    }
}