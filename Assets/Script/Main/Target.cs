using UnityEngine;

public class Target : MonoBehaviour
{
    private float _spawnTime;

    void Start()
    {
        _spawnTime = Time.time;
    }

    public void OnHit()
    {
        float elapsedTime = Time.time - _spawnTime;
        Debug.Log($"Hit! Time to hit: {elapsedTime:F2} seconds");

        // �K�v�ɉ����Ă����ŋL�^�ۑ��E�X�R�A���M�Ȃ�
        Destroy(gameObject);
    }
}
