using UnityEngine;
using Unity.Netcode;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Collider2D _crosshair;     // マウスカーソルに追従するオブジェクト座標
    [SerializeField] private int _maxBullets = 5;       // 1ラウンドあたりの最大発射数
    private int _bulletsLeft;   // 残弾数

    void Start()
    {
        // 最大数にする(ラウンド開始時にも行う)
        _bulletsLeft = _maxBullets;
    }

    // Update is called once per frame
    void Update()
    {
        MouseFollow();

        // TODO: ラウンド中のみ打てるように変更する
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
        {
            Shoot();
            Debug.Log("残り弾数" + _bulletsLeft);
        }
    }

    public void MouseFollow()
    {
        //マウス位置に追従
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    public void Shoot()
    {
        // 自分のCollider2Dと接触しているCollider2Dを取得
        Collider2D[] hits = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };

        int hitCount = _crosshair.Overlap(filter, hits);
        bool hitObject = false;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = hits[i];
            if (hitCollider == null)
            {
                Debug.Log("null");
                continue;
            }

            if (hitCollider.CompareTag("Target"))
            {
                Debug.Log("Hit target: " + hitCollider.name);
                hitObject = true;

                // 命中時の処理
                hitCollider.GetComponent<Target>().OnHit();
                break;
            }
            else if (hitCollider.CompareTag("Decoy"))
            {
                Debug.Log("Hit Decoy: " + hitCollider.name);
                hitObject = true;

                // 命中時の処理
                Destroy(hitCollider.gameObject);
            }
            else if (hitCollider.CompareTag("Area"))
            {
                Debug.Log("Hit Area:" + hitCollider.name);
                hitObject = true;
            }
        }

        if (hitObject)
        {
            _bulletsLeft--;
            if(_bulletsLeft <= 0)
            {
                // 残弾数が0になったらタイム送信
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                ScoreManager.Instance.SetHitTimeList(clientId, 0.0f);

            }
        }
    }
}
