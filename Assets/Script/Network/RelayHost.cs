using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayHost : MonoBehaviour
{
    public TMP_InputField joinCodeOutputField;

    //Relayサーバーを使ってホストとして接続するメソッド
    public async void StartRelayHost()
    {
        //Relayサーバー上で1人分のクライアントスロットを確保
        var allocation = await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(1);

        //接続用コードの取得
        string joinCode = await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId);
        Debug.Log("Join Code: " + joinCode);

        if (joinCodeOutputField != null)
        {
            joinCodeOutputField.text = joinCode;
        }

        var transport = NetworkManager.Singleton.GetComponent<UnityTransport>();

        //サーバーの接続情報の設定
        transport.SetRelayServerData(
            allocation.RelayServer.IpV4,
            (ushort)allocation.RelayServer.Port,
            allocation.AllocationIdBytes,
            allocation.Key,
            allocation.ConnectionData,
            new byte[0],
            false
        );

        //ホストとして起動
        NetworkManager.Singleton.StartHost();
    }
}
