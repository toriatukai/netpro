using UnityEngine;
using Unity.Netcode; // Netcode for GameObjects ���g�p���邽�߂ɕK�v
using System.Collections; // Coroutine ���g�p����ꍇ�ɕK�v

public class MainSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private GameObject gameManagerPrefab; // �C���X�y�N�^�[����GameManager�v���n�u�����蓖�Ă�

    void Start()
    {
        // NetworkManager�������������̂�҂�
        // NetworkManager.Singleton.IsServer �܂��� NetworkManager.Singleton.IsHost �� true �ɂȂ�܂ő҂�
        StartCoroutine(WaitForNetworkManagerReady());
    }

    private IEnumerator WaitForNetworkManagerReady()
    {
        // NetworkManager�����p�\�ɂȂ�܂őҋ@
        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);

        // �z�X�g�i�܂��͐�p�T�[�o�[�j�̏ꍇ�̂�GameManager���X�|�[������
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            // GameManager�v���n�u�����蓖�Ă��Ă��邩�m�F
            if (gameManagerPrefab != null)
            {
                // GameManager�̃C���X�^���X�𐶐�
                GameObject gameManagerInstance = Instantiate(gameManagerPrefab);
                // �l�b�g���[�N��ŃX�|�[��
                gameManagerInstance.GetComponent<NetworkObject>().Spawn();
                Debug.Log("GameManager spawned on server/host.");
            }
            else
            {
                Debug.LogError("GameManager Prefab is not assigned in MainSceneInitializer!");
            }
        }
    }
}