using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private RectTransform _crosshair; // �}�E�X�J�[�\���ɒǏ]����I�u�W�F�N�g���W
    [SerializeField] private float _hitRadius = 30f;   // �����蔻��
    [SerializeField] private int _maxBullets = 5;      // 1���E���h������̍ő唭�ː�

    private int _bulletsLeft;

    void Start()
    {
        _bulletsLeft = _maxBullets;
    }

    // Update is called once per frame
    void Update()
    {
        // TODO: ���E���h���̂ݑłĂ�悤�ɕύX����
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
        {

        }
    }

    public void Shoot()
    {

    }
}
