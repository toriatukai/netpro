using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    private RectTransform _rectTransform;

    private void Awake()
    {
        _rectTransform = GetComponent<RectTransform>();
    }

    // �N���X�w�A�𓮂������\�b�h
    void Update()
    {
        Vector2 mousePos = Input.mousePosition;
        _rectTransform.position = mousePos;
    }
}
