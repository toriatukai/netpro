using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    // シーン遷移メソッド統括管理したい(覚えていたら)
    public void LoadMainScene()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("ネットワークに接続していません");
        }
    }

    public void LoadMainTestScene()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainTestScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("ネットワークに接続していません");
        }
    }
}
