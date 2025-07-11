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

    //�e�X�g�p�@�L�����N�^�[�I����ʂւ̑J�ڃ��\�b�h
    public void LoadCharactarSelectScene()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsClient)
        {
            NetworkManager.Singleton.SceneManager.LoadScene("CharactarChoiceScene", LoadSceneMode.Single);
        }
        else
        {
            Debug.LogWarning("�l�b�g���[�N�ɐڑ����Ă��܂���");
        }
    }
}
