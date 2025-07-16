using Unity.Services.Relay.Models;
using Unity.Netcode;
using Unity.Networking.Transport.Relay;
using UnityEngine;
using Unity.Netcode.Transports.UTP;
using TMPro;

public class RelayHost : MonoBehaviour
{
    public TMP_InputField joinCodeOutputField;

    //タイトル画面で使う用の追加部分 以下2行
    private int requiredPlayers = 2;     //ホスト + 1人
    private bool sceneLoaded = false;   //二重遷移防止

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

    //タイトル画面で使う用の追加部分

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

        //ホスト含めて requiredPlayers　に到達したらシーン遷移
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= requiredPlayers &&
            NetworkManager.Singleton.IsHost && !sceneLoaded)
        {
            Debug.Log("飛びます3");
            sceneLoaded = true;

            //SceneControllerを呼び出して遷移
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.LoadCharactarSelectScene();
            }
            else
            {
                Debug.LogError("SceneController がシーン内に存在しません");
            }
        }
    }
}
