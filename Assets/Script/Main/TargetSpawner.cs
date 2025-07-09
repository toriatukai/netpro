using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class TargetSpawner : NetworkBehaviour
{
    [SerializeField] private NetworkObject _targetPrefab;

    [Range(0.1f, 1f)] public float widthRate = 0.5f;
    [Range(0.1f, 1f)] public float heightRate = 0.5f;

    // �\������b���̍ŒZ�A�Œ�����
    [SerializeField] private float _minDelay = 2f;
    [SerializeField] private float _maxDelay = 5f;

    public async UniTask SpawnAsync()
    {
        float delay = Random.Range(_minDelay, _maxDelay);
        Debug.Log("�^�[�Q�b�g�o���ҋ@��: " + delay);
        await UniTask.Delay(System.TimeSpan.FromSeconds(delay));

        SpawnTargetWithRatio();
    }

    private void SpawnTargetWithRatio()
    {
        // �����Ŕ䗦������
        float xRate = Random.Range(0f, 1f);
        float yRate = Random.Range(0f, 1f);

        // �z�X�g���g�Ƀ^�[�Q�b�g�𐶐�
        Vector3 spawnPos = GetSpawnPositionFromRate(xRate, yRate);
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);

        // �N���C�A���g�ɔ䗦�𑗐M
        SendSpawnDataClientRpc(xRate, yRate);
    }

    [ClientRpc]
    private void SendSpawnDataClientRpc(float xRate, float yRate)
    {
        if (IsHost) return;

        Vector3 spawnPos = GetSpawnPositionFromRate(xRate, yRate);
        Instantiate(_targetPrefab, spawnPos, Quaternion.identity);
    }

    // �e�N���C�A���g�������̃J�����T�C�Y�Ɋ�Â��ďo���ʒu���v�Z����
    private Vector3 GetSpawnPositionFromRate(float xRate, float yRate)
    {
        //�J�����̒��S�ƃT�C�Y�擾
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        //�o���G���A�̕��𐧌�
        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        //�o���G���A�̍���
        float originX = camCenter.x - areaWidth / 2f;
        float originY = camCenter.y - areaHeight / 2f;

        //�͈͂�����W���Z�o
        float x = originX + areaWidth * xRate;
        float y = originY + areaHeight * yRate;

        return new Vector3(x, y, 0f);
    }
}