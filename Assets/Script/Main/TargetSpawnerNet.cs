using UnityEngine;
using Unity.Netcode;

public class TargetSpawnerNet : NetworkBehaviour
{
    [SerializeField] private RectTransform _spawnArea;       // �I���o���͈́iUI��̘g�j
    [SerializeField] private RectTransform _targetPrefab;    // �I��UI�v���n�u�iImage�t���j

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
            // �z�X�g���䗦�����肵�đS�N���C�A���g�֑��M
            float xRate = Random.Range(0f, 1f);
            float yRate = Random.Range(0f, 1f);
            SpawnTargetForAllClientsClientRpc(xRate, yRate);
        }
    }

    [ClientRpc]
    private void SpawnTargetForAllClientsClientRpc(float xRate, float yRate)
    {
        // �󂯎�����䗦�Ń��[�J��UI��Ƀ^�[�Q�b�g���o��
        Vector2 localPos = new Vector2(
            _spawnArea.rect.width * xRate,
            _spawnArea.rect.height * yRate
        );

        // �s�{�b�g�␳�iPivot�������Ȃ�0.5�j
        localPos.x -= _spawnArea.rect.width * _spawnArea.pivot.x;
        localPos.y -= _spawnArea.rect.height * _spawnArea.pivot.y;

        RectTransform target = Instantiate(_targetPrefab, _spawnArea);
        target.localPosition = localPos;
    }
}
