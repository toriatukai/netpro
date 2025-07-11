using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private List<float> _hostTimeList;
    [SerializeField] private List<float> _clientTimeList;

    // �I�u�W�F�N�g�������ɌĂяo�����(Start���O)
    private void Awake()
    {
        // Singleton�Ƃ��đ��݂�����
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        _hostTimeList = new List<float>();
        _clientTimeList = new List<float>();
    }

    public void SetHitTimeList(ulong clientId, float hitTime)
    {
        // clientId == 0�̓z�X�g�A����ȍ~�͘A�ԂŃN���C�A���g
        // �����1 vs 1�Ȃ̂�list�ŕۑ�
        if (clientId == 0)
            _hostTimeList.Add(hitTime);
        else
            _clientTimeList.Add(hitTime);
    }

    public int DidPlayerWin(int round)
    {
        if (_hostTimeList[round] == _clientTimeList[round])
            // ���������Ȃ�-1
            return -1;
        else if (_hostTimeList[round] < _clientTimeList[round])
            // �z�X�g�������Ȃ�0
            return 0;
        else
            //�N���C�A���g�������Ȃ�1
            return 1;
    }


    public int GamePlayerWin()
    {
        // �������E���h�̏����������ł���ꍇ�Ɋe���E���h�̍��v�^�C���Ō��߂�
        float hostTotalTime = 0;
        float clientTotalTime = 0;

        for(int i = 0; i < _hostTimeList.Count; i++)
        {
            hostTotalTime += _hostTimeList[i];
        }

        for (int i = 0; i < _clientTimeList.Count; i++)
        {
            clientTotalTime += _clientTimeList[i];
        }

        if (hostTotalTime == clientTotalTime)
            return -1;
        else if (hostTotalTime < clientTotalTime)
            return 0;
        else
            return 1;
    }
}
