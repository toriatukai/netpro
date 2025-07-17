using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayHost : MonoBehaviour
{
    public TMP_InputField joinCodeOutputField;

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
}
