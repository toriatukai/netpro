using UnityEngine;
using Unity.Netcode;
using Cysharp.Threading.Tasks;

public class TargetSpawner : MonoBehaviour
{
    public static TargetSpawner Instance;

    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private GameObject decoyPrefab;
    private GameObject currentDecoy;

    [Range(0.1f, 1f)] public float widthRate = 0.5f;
    [Range(0.1f, 1f)] public float heightRate = 0.5f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
            Destroy(gameObject);

            Instance = this;
    }

    public void SpawnTargetByRatio(float xRate, float yRate)
    {
        Vector3 spawnPos = CalculateSpawnPosition(xRate, yRate);
        Instantiate(targetPrefab, spawnPos, Quaternion.identity);
    }

    private Vector3 CalculateSpawnPosition(float xRate, float yRate)
    {
        Vector3 camCenter = Camera.main.transform.position;
        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        float originX = camCenter.x - areaWidth / 2f;
        float originY = camCenter.y - areaHeight / 2f;

        float x = originX + areaWidth * xRate;
        float y = originY + areaHeight * yRate;

        return new Vector3(x, y, 0f);
    }


    public void ClearAllTargets()
    {
        foreach (var target in GameObject.FindGameObjectsWithTag("Target"))
        {
            Destroy(target);
        }

        foreach (var decoy in GameObject.FindGameObjectsWithTag("Decoy"))
        {
            Destroy(decoy);
        }
    }
    
    // デコイ出現の処理
    public void SpawnDecoyByRatio(float xRate, float yRate)
    {
        ClearDecoy();

        Vector3 screenPos = CalculateSpawnPosition(xRate, yRate);

        currentDecoy = Instantiate(decoyPrefab, screenPos, Quaternion.identity);
    }

    public void ClearDecoy()
    {
        if (currentDecoy != null)
        {
            Destroy(currentDecoy);
            currentDecoy = null;
        }
    }
}