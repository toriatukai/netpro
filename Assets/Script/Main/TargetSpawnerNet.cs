using UnityEngine;
using Unity.Netcode;

public class TargetSpawnerNet : NetworkBehaviour
{
    [SerializeField] private RectTransform _spawnArea;       // 的を出す範囲（UI上の枠）
    [SerializeField] private RectTransform _targetPrefab;    // 的のUIプレハブ（Image付き）

    private void Update()
    {
        if (IsHost && Input.GetKeyDown(KeyCode.Space))
        {
            RequestSpawnFromHost();
        }
    }

    public void RequestSpawnFromHost()
    {
        if (IsHost)
        {
            // ホストが比率を決定して全クライアントへ送信
            float xRate = Random.Range(0f, 1f);
            float yRate = Random.Range(0f, 1f);
            SpawnTargetForAllClientsClientRpc(xRate, yRate);
        }
    }

    [ClientRpc]
    private void SpawnTargetForAllClientsClientRpc(float xRate, float yRate)
    {
        // 受け取った比率でローカルUI上にターゲットを出現
        Vector2 localPos = new Vector2(
            _spawnArea.rect.width * xRate,
            _spawnArea.rect.height * yRate
        );

        // ピボット補正（Pivotが中央なら0.5）
        localPos.x -= _spawnArea.rect.width * _spawnArea.pivot.x;
        localPos.y -= _spawnArea.rect.height * _spawnArea.pivot.y;

        RectTransform target = Instantiate(_targetPrefab, _spawnArea);
        target.localPosition = localPos;
    }
}
