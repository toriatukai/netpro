using UnityEngine;

public class SpawnAreaVisualizer : MonoBehaviour
{
    [Range(0.1f, 1f)] public float widthRate = 0.5f;
    [Range(0.1f, 1f)] public float heightRate = 0.5f;

    [SerializeField] private SpriteRenderer areaSprite;

    void Update()
    {
        if (Camera.main == null || areaSprite == null) return;

        float camHeight = Camera.main.orthographicSize * 2f;
        float camWidth = camHeight * Camera.main.aspect;

        float areaWidth = camWidth * widthRate;
        float areaHeight = camHeight * heightRate;

        // スプライトの位置とスケールをカメラに合わせて調整
        areaSprite.transform.localScale = new Vector3(areaWidth, areaHeight, 1f);
    }
}
