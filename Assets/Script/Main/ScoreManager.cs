using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    private List<float> hostHitTimes;
    private List<float> clientHitTimes;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        hostHitTimes = new List<float>();
        clientHitTimes = new List<float>();
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void SetHitTimeList(ulong clientId, float hitTime)
    {
        // clientId == 0�̓z�X�g�A����ȍ~�͘A�ԂŃN���C�A���g
        // �����1 vs 1�Ȃ̂�list�ŕۑ�
        if (clientId == 0)
            hostHitTimes.Add(hitTime);
        else
            clientHitTimes.Add(hitTime);
    }

    public bool DidPlayerWin()
    {

        return true;
    }
}
