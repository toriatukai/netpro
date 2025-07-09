using Unity.Netcode;
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

        // 命中時間をスコアマネージャーに保存
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        ScoreManager.Instance.SetHitTimeList(clientId, elapsedTime);

        GameManager.Instance.NotifyPlayerFinished();

        Destroy(gameObject);
    }
}
