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

    //�I��\���\�������郁�\�b�h
    private void SpawnTargetWithRatio()
    {
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        Vector3 spawnPos = GetWorldPositionFromRate(xRate, yRate);

        // �l�b�g���[�N�I�u�W�F�N�g�Ƃ��Đ������S�N���C�A���g�ɓ���
        NetworkObject target = Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
        target.Spawn(true); // �S�N���C�A���g�ɑ��M
    }

    // �䗦�ƃJ�����͈̔͂���^�[�Q�b�g���o��������W�����߂�
    private Vector3 GetWorldPositionFromRate(float xRate, float yRate)
    {
        // �J�����̒��S�ƃT�C�Y�擾
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        // �o���G���A�̕��𐧌�
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        // �w�肳�ꂽ�䗦���烏�[���h���W�֕ϊ�
        float x = camCenter.x - areaWidth / 2f + areaWidth * xRate;
        float y = camCenter.y - areaHeight / 2f + areaHeight * yRate;

        x = camCenter.x - areaWidth / 2f;
        y = camCenter.y - areaHeight / 2f;

        Debug.Log(x + ", " + y);

        return new Vector3(x, y, 0f);
    }
}
