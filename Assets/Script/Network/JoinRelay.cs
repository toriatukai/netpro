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

            // Relay から Allocation 情報を取得（Join）
            var joinAllocation = await RelayService.Instance.JoinAllocationAsync(joinCode);
            Debug.Log("Joined Relay with code: " + joinCode);

            // UnityTransport コンポーネント取得
            var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

            // Relay 接続情報を UnityTransport に設定
            transport.SetRelayServerData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData,
                false
            );

            // クライアントとして接続開始
            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.LogError("Relay join failed: " + e.Message);
        }
    }
}
