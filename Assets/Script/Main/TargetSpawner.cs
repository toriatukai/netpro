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

    //�I��\���\�������郁�\�b�h
    private void SpawnTargetWithRatio()
    {
        // ����
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        // �J�����̒��S�ƃT�C�Y�擾
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // �o���G���A�̕��𐧌�
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        // �o���G���A�̍���
        float originalX = camCenter.x - areaWidth / 2f;
        float originalY = camCenter.y - areaHeight / 2f;

        // �o���G���A�͈̔͂�����W���Z�o
        float x = originalX + areaWidth * xRate;
        float y = originalY + areaHeight * yRate;

        Vector3 spawnPos = new Vector3(x, y, 0f);

        // �^�[�Q�b�g��\��(�z�X�g��)
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);

        // �N���C�A���g���ɍ��W�𑗐M
        SendSpawnDataClientRpc(originalX, originalY, x, y);
    }

    [ClientRpc]
    private void SendSpawnDataClientRpc(float hostX, float hostY, float x, float y)
    {
        if (IsHost) return;

        // �J�����̒��S�ƃT�C�Y�擾
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // �o���G���A�̕��𐧌�
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
