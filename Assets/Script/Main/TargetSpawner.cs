using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class TargetSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _targetPrefab;

    [Range(0.1f, 1f)] public float widthRate = 0.5f;
    [Range(0.1f, 1f)] public float heightRate = 0.5f;

    // 表示する秒数の最短、最長時間
    [SerializeField] private float _minDelay = 2f;
    [SerializeField] private float _maxDelay = 5f;

    public async UniTask SpawnAsync()
    {
        float delay = Random.Range(_minDelay, _maxDelay);
        Debug.Log("ターゲット出現待機中: " + delay);
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        SpawnTargetWithRatio();
    }

    private void SpawnTargetWithRatio()
    {
        // 乱数で比率を決定
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        // ホスト自身にターゲットを生成
        Vector3 spawnPos = GetSpawnPositionFromRate(xRate, yRate);
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);

        // クライアントに比率を送信
        SendSpawnDataClientRpc(xRate, yRate);
    }

    [ClientRpc]
    private void SendSpawnDataClientRpc(float xRate, float yRate)
    {
        if (IsHost) return;

        Vector3 spawnPos = GetSpawnPositionFromRate(xRate, yRate);
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
    }

    // 各クライアントが自分のカメラサイズに基づいて出現位置を計算する
    private Vector3 GetSpawnPositionFromRate(float xRate, float yRate)
    {
        //カメラの中心とサイズ取得
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        //出現エリアの幅を制限
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        //出現エリアの左隅
        float originX = camCenter.x - areaWidth / 2f;
        float originY = camCenter.y - areaHeight / 2f;

        //範囲から座標を算出
        float x = originX + areaWidth * xRate;
        float y = originY + areaHeight * yRate;

        return new Vector3(x, y, 0f);
    }
}