using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnityServicesInitializer : MonoBehaviour
{
    // �C������(RelayHost���玝���Ă���)
    private int requiredPlayers = 2; // �z�X�g + 1�l
    private bool sceneLoaded = false; // ��d�J�ږh�~

    async void Awake()
    {
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously.");
        }
    }

    void Start()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
            Debug.Log("NetworkManager callbacks registered in Start().");
        }
        else
        {
            // �����ŃG���[���o��ꍇ�́A���������{�I�ɂ�������
            Debug.LogError("NetworkManager.Singleton is NULL in Start()! This indicates a serious issue with NetworkManager setup.");
        }
    }

    void OnDisable()
    {
        // �R�[���o�b�N�����i�Đ���~���̃G���[�h�~�j
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        Debug.Log("OnClientConnected (in UnityServicesInitializer) triggered."); // �����ɐV�������O��ǉ�

        //�z�X�g�܂߂� requiredPlayers�@�ɓ��B������V�[���J��
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= requiredPlayers &&
            NetworkManager.Singleton.IsHost && !sceneLoaded)
        {
            Debug.Log("��т܂�3: All required players connected as host!");
            Debug.Log($"ConnectedClientsList.Count: {NetworkManager.Singleton.ConnectedClientsList.Count}");
            Debug.Log($"requiredPlayers: {requiredPlayers}");
            Debug.Log($"IsHost: {NetworkManager.Singleton.IsHost}");
            Debug.Log($"sceneLoaded: {sceneLoaded}");
            sceneLoaded = true; // ��d�J�ږh�~�t���O�𗧂Ă�

            //SceneController���Ăяo���đJ��
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.LoadCharactarSelectScene();
            }
            else
            {
                Debug.LogError("SceneController ���V�[�����ɑ��݂��܂���B�r���h�ݒ���m�F���Ă��������B");
            }
        }
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        // �K�v�ɉ����āA�v���C���[�����������ꍇ�̏�����AsceneLoaded�t���O�̃��Z�b�g�Ȃǂ��s��
        if (NetworkManager.Singleton.IsHost && sceneLoaded && NetworkManager.Singleton.ConnectedClientsList.Count < requiredPlayers)
        {
            Debug.Log("Required players count dropped below threshold. Resetting sceneLoaded flag.");
            sceneLoaded = false; // �ēx�ڑ��ł���悤�Ƀt���O�����Z�b�g����Ȃ�
        }
    }
}
