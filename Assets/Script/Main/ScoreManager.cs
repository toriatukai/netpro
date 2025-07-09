using UnityEngine;
using System.Collections.Generic;

public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; }

    [SerializeField] private List<float> _hostTimeList;
    [SerializeField] private List<float> _clientTimeList;

    // オブジェクト生成時に呼び出される(Startより前)
    private void Awake()
    {
        // Singletonとして存在させる
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
        // clientId == 0はホスト、それ以降は連番でクライアント
        // 今回は1 vs 1なのでlistで保存
        if (clientId == 0)
            _hostTimeList.Add(hitTime);
        else
            _clientTimeList.Add(hitTime);
    }

    public int DidPlayerWin(int round)
    {
        if (_hostTimeList[round] == _clientTimeList[round])
            // 引き分けなら-1
            return -1;
        else if (_hostTimeList[round] < _clientTimeList[round])
            // ホストが勝ちなら0
            return 0;
        else
            //クライアントが勝ちなら1
            return 1;
    }


    public int GamePlayerWin()
    {
        // もしラウンドの勝数が同じである場合に各ラウンドの合計タイムで決める
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
