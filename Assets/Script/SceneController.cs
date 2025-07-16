using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    // シーン遷移メソッド統括管理したい(覚えていたら)
    public void LoadMainScene()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("ネットワークに接続していません");
        }
    }

    //テスト用　キャラクター選択画面への遷移メソッド
    public void LoadCharactarSelectScene()
    {
        Debug.Log("飛びます1");
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            Debug.Log("飛びます");
            NetworkManager.Singleton.SceneManager.LoadScene("CharactarChoiceScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("ネットワークに接続していません");
        }
    }
}
