using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private RectTransform _crosshair; // マウスカーソルに追従するオブジェクト座標
    [SerializeField] private float _hitRadius = 30f;   // 当たり判定
    [SerializeField] private int _maxBullets = 5;      // 1ラウンドあたりの最大発射数

    private int _bulletsLeft;

    void Start()
    {
        _bulletsLeft = _maxBullets;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: ラウンド中のみ打てるように変更する
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
        {

        }
    }

    public void Shoot()
    {

    }
}
