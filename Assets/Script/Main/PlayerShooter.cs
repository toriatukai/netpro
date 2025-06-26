using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private RectTransform _crosshair; // マウスカーソルに追従するオブジェクト座標
    [SerializeField] private float _hitRadius = 30f; // 当たり判定
    [SerializeField] private int _maxBullets = 5;     // 1ラウンドあたりの最大発射数

    private int _bulletsLeft;
    private TargetManager _targetManager;

    private void Start()
    {
        _bulletsLeft = _maxBullets;

        // シーン内の TargetManager コンポーネントを取得
        _targetManager = FindObjectOfType<TargetManager>();

        if (_targetManager == null)
        {
            Debug.LogError("TargetManager が見つかりません！");
        }
    }

    private void Update()
    {
        // TODO: ラウンド中のみ打てるように変更する
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
        {
            Shoot();
        }
    }

    //発射処理
    public void Shoot()
    {
        Vector2 screenPoint = _crosshair.position;

        foreach (var target in _targetManager.Targets)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
            float distance = Vector2.Distance(screenPoint, targetScreenPos);

            if (distance <= _hitRadius)
            {
                Debug.Log("Hit!");
                target.OnTargetClicked();
            }
            else
            {
                Debug.Log("Miss.");
            }
        }

        _bulletsLeft--;
        Debug.Log($"Bullets left: {_bulletsLeft}");
    }

    private void OnDrawGizmos()
    {
        if (_crosshair == null) return;

        // スクリーン座標をワールド座標に変換
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(_crosshair.position);
        worldPos.z = 0f; // 2D平面のZ固定

        // Gizmosの色を設定
        Gizmos.color = Color.green;

        // 当たり判定をワールド単位に換算（スクリーンピクセル → ワールド単位に変換）
        Vector3 rightWorld = Camera.main.ScreenToWorldPoint(_crosshair.position + new Vector3(_hitRadius, 0, 0));
        float worldRadius = Vector3.Distance(worldPos, rightWorld);

        // 円を描く（ワールド座標の半径で）
        Gizmos.DrawWireSphere(worldPos, worldRadius);
    }
}
