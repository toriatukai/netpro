using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayHost : MonoBehaviour
{
    public TMP_InputField joinCodeOutputField;

    //�^�C�g����ʂŎg���p�̒ǉ����� �ȉ�2�s
    private int requiredPlayers = 2;     //�z�X�g + 1�l
    private bool sceneLoaded = false;   //��d�J�ږh�~

    //Relay�T�[�o�[���g���ăz�X�g�Ƃ��Đڑ����郁�\�b�h
    public async void StartRelayHost()
    {
        //Relay�T�[�o�[���1�l���̃N���C�A���g�X���b�g���m��
        var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(1);

        //�ڑ��p�R�[�h�̎擾
        string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log("Join Code: " + joinCode);

        if (joinCodeOutputField != null)
        {
            joinCodeOutputField.text = joinCode;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //�T�[�o�[�̐ڑ����̐ݒ�
        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            new byte[0],
            false
        );

        //�z�X�g�Ƃ��ċN��
        NetworkManager.Singleton.StartHost();
    }

    //�^�C�g����ʂŎg���p�̒ǉ�����

    void OnEnable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        }   
    }

    void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
        }
    }
    private void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        //�z�X�g�܂߂� requiredPlayers�@�ɓ��B������V�[���J��
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= requiredPlayers &&
            NetworkManager.Singleton.IsHost && !sceneLoaded)
        {
            Debug.Log("��т܂�3");
            sceneLoaded = true;

            //SceneController���Ăяo���đJ��
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.LoadCharactarSelectScene();
            }
            else
            {
                Debug.LogError("SceneController ���V�[�����ɑ��݂��܂���");
            }
        }
    }
}
