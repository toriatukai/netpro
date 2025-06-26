using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    //�^�[�Q�b�g�����X�g�Ǘ�����
    private readonly List<TargetUI> _targets = new();

    public void Register(TargetUI target)
    {
        if (!_targets.Contains(target))
            _targets.Add(target);
    }

    public void Unregister(TargetUI target)
    {
        if (_targets.Contains(target))
            _targets.Remove(target);
    }

    // �O������^�[�Q�b�g���X�g��ǂݎ���p�Ŏ擾�\�ɂ���
    public IReadOnlyList<TargetUI> Targets => _targets.AsReadOnly();
}
