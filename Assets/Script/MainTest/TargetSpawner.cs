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
            SpawnTargetWithRatio();
        }
    }

    //的を表示表示させるメソッド
    private void SpawnTargetWithRatio()
    {
        // 乱数
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        // カメラの中心とサイズ取得
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // 出現エリアの幅を制限
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        // 出現エリアの左隅
        float originalX = camCenter.x - areaWidth / 2f;
        float originalY = camCenter.y - areaHeight / 2f;

        // 出現エリアの範囲から座標を算出
        float x = originalX + areaWidth * xRate;
        float y = originalY + areaHeight * yRate;

        Vector3 spawnPos = new Vector3(x, y, 0f);

        // ターゲットを表示(ホスト側)
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);

        // クライアント側に座標を送信
        SendSpawnDataClientRpc(originalX, originalY, x, y);
    }

    [ClientRpc]
    private void SendSpawnDataClientRpc(float hostX, float hostY, float x, float y)
    {
        if (IsHost) return;

        // カメラの中心とサイズ取得
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // 出現エリアの幅を制限
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        float originalX = camCenter.x - areaWidth / 2f;
        float originalY = camCenter.y - areaHeight / 2f;

        float ratioX = x / hostX;
        float ratioY = y / hostY;

        float targetX = originalX * ratioX;
        float targetY = originalY * ratioY;

        Vector3 spawnPos = new Vector3(targetX, targetY, 0f);

        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
    }

}
