using UnityEngine;
using Unity.Netcode;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using Unity.Netcode.Transports.UTP;
using System.Threading.Tasks;

public class JoinRelayClient : MonoBehaviour
{
    public async void JoinRelay(string joinCode)
    {
        try
        {

            if (string.IsNullOrEmpty(joinCode))
            {
                Debug.LogError("Join code is empty.");
                return;
            }

            // Relay ���� Allocation �����擾�iJoin�j
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined Relay with code: " + joinCode);

            // UnityTransport �R���|�[�l���g�擾
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // Relay �ڑ����� UnityTransport �ɐݒ�
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                false
            );

            // �N���C�A���g�Ƃ��Đڑ��J�n
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay join failed: " + e.Message);
        }
    }
}
