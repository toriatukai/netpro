using UnityEngine;

public class TargetController : MonoBehaviour
{
    private float spawnTime;
    private float reactionTime;
    private void Start()
    {
        spawnTime = Time.time;
        Debug.Log($"�I���o����������: {spawnTime}");
    }

    public void OnHit()
    {
        reactionTime = Time.time - spawnTime;

        Debug.Log("Time.time: " + Time.time + ", spawnTime: " + spawnTime);

        PlayerController.LocalInstance?.OnTargetHit(reactionTime);

        Destroy(gameObject);
    }
}
