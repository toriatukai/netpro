using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;


public class SceneController : MonoBehaviour
{
    // �V�[���J�ڃ��\�b�h�����Ǘ�������(�o���Ă�����)
    public void LoadMainScene()
    {
        if (NetworkManager.Singleton.IsHost)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("MainScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("�l�b�g���[�N�ɐڑ����Ă��܂���");
        }
    }
}
