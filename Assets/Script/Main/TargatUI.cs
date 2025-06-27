using UnityEngine;

public class TargetUI : MonoBehaviour
{
    public bool IsHit { get; private set; } = false; // �����t���O�i1�x����������Ȃ��悤�Ɂj
    public bool IsFake = false; //�f�R�C���ǂ���

    private void OnEnable()
    {
        TargetManager manager = FindObjectOfType<TargetManager>();
        manager?.Register(this);
    }

    private void OnDisable()
    {
        TargetManager manager = FindObjectOfType<TargetManager>();
        manager?.Unregister(this);
    }

    public void OnTargetClicked()
    {
        if (IsHit) return; // ���łɓ������Ă���Ζ���

        IsHit = true;

        if (!IsFake)
        {
            // �������̏����F�폜�A�A�j���A�X�R�A�ʒm�Ȃ�
        }
        Destroy(gameObject);
    }
}
