using UnityEngine;

public class PlayerShooter : MonoBehaviour
{
    [SerializeField] private RectTransform _crosshair; // �}�E�X�J�[�\���ɒǏ]����I�u�W�F�N�g���W
    [SerializeField] private float _hitRadius = 30f; // �����蔻��
    [SerializeField] private int _maxBullets = 5;     // 1���E���h������̍ő唭�ː�

    private int _bulletsLeft;
    private TargetManager _targetManager;

    private void Start()
    {
        _bulletsLeft = _maxBullets;

        // �V�[������ TargetManager �R���|�[�l���g���擾
        _targetManager = FindObjectOfType<TargetManager>();

        if (_targetManager == null)
        {
            Debug.LogError("TargetManager ��������܂���I");
        }
    }

    private void Update()
    {
        // TODO: ���E���h���̂ݑłĂ�悤�ɕύX����
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
        {
            Shoot();
        }
    }

    //���ˏ���
    public void Shoot()
    {
        Vector2 screenPoint = _crosshair.position;

        foreach (var target in _targetManager.Targets)
        {
            RectTransform targetRect = target.GetComponent<RectTransform>();
            Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(null, targetRect.position);
            float distance = Vector2.Distance(screenPoint, targetScreenPos);

            if (distance <= _hitRadius)
            {
                Debug.Log("Hit!");
                target.OnTargetClicked();
            }
            else
            {
                Debug.Log("Miss.");
            }
        }

        _bulletsLeft--;
        Debug.Log($"Bullets left: {_bulletsLeft}");
    }

    private void OnDrawGizmos()
    {
        if (_crosshair == null) return;

        // �X�N���[�����W�����[���h���W�ɕϊ�
        Vector3 worldPos = Camera.main.ScreenToWorldPoint(_crosshair.position);
        worldPos.z = 0f; // 2D���ʂ�Z�Œ�

        // Gizmos�̐F��ݒ�
        Gizmos.color = Color.green;

        // �����蔻������[���h�P�ʂɊ��Z�i�X�N���[���s�N�Z�� �� ���[���h�P�ʂɕϊ��j
        Vector3 rightWorld = Camera.main.ScreenToWorldPoint(_crosshair.position + new Vector3(_hitRadius, 0, 0));
        float worldRadius = Vector3.Distance(worldPos, rightWorld);

        // �~��`���i���[���h���W�̔��a�Łj
        Gizmos.DrawWireSphere(worldPos, worldRadius);
    }
}
