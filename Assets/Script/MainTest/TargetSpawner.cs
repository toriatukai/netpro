using UnityEngine;
using Unity.Netcode;

public class TargetSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _targetPrefab;

    [Range(0.1f, 1f)] public float widthRate = 0.5f;
    [Range(0.1f, 1f)] public float heightRate = 0.5f;

    private void Update()
    {
        if (IsHost && Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("osita");
            SpawnTargetWithRatio();
        }
    }

    //的を表示表示させるメソッド
    private void SpawnTargetWithRatio()
    {
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Vector3 spawnPos = GetWorldPositionFromRate(xRate, yRate);

        // ネットワークオブジェクトとして生成＆全クライアントに同期
        NetworkObject target = Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
        target.Spawn(true); // 全クライアントに送信
    }

    // 比率とカメラの範囲からターゲットを出現する座標を決める
    private Vector3 GetWorldPositionFromRate(float xRate, float yRate)
    {
        // カメラの中心とサイズ取得
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // 出現エリアの幅を制限
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        // 指定された比率からワールド座標へ変換
        float x = camCenter.x - areaWidth / 2f + areaWidth * xRate;
        float y = camCenter.y - areaHeight / 2f + areaHeight * yRate;

        x = camCenter.x - areaWidth / 2f;
        y = camCenter.y - areaHeight / 2f;

        Debug.Log(x + ", " + y);

        return new Vector3(x, y, 0f);
    }
}
