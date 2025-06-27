using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    // �V�[���J�ڃ��\�b�h�����Ǘ�������(�o���Ă�����)
    public void LoadMainScene()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("�l�b�g���[�N�ɐڑ����Ă��܂���");
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
            Debug.LogWarning("�l�b�g���[�N�ɐڑ����Ă��܂���");
        }
    }
}
