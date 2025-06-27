using UnityEngine;

public class TargetUI : MonoBehaviour
{
    public bool IsHit { get; private set; } = false; // 命中フラグ（1度しか当たらないように）
    public bool IsFake = false; //デコイかどうか

    private void OnEnable()
    {
        TargetManager manager = FindObjectOfType<TargetManager>();
        manager?.Register(this);
    }

    private void OnDisable()
    {
        TargetManager manager = FindObjectOfType<TargetManager>();
        manager?.Unregister(this);
    }

    public void OnTargetClicked()
    {
        if (IsHit) return; // すでに当たっていれば無視

        IsHit = true;

        if (!IsFake)
        {
            // 命中時の処理：削除、アニメ、スコア通知など
        }
        Destroy(gameObject);
    }
}
