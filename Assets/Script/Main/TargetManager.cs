using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{
    //ターゲットをリスト管理する
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

    // 外部からターゲットリストを読み取り専用で取得可能にする
    public IReadOnlyList<TargetUI> Targets => _targets.AsReadOnly();
}
