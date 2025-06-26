using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // クロスヘアを動かすメソッド
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        _rectTransform.position = mousePos;
    }
}
