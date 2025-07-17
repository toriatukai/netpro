using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;

public class UnityServicesInitializer : MonoBehaviour
{
    // 修正部分(RelayHostから持ってきた)
    private int requiredPlayers = 2; // ホスト + 1人
    private bool sceneLoaded = false; // 二重遷移防止

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
            // ここでエラーが出る場合は、何かが根本的におかしい
            Debug.LogError("NetworkManager.Singleton is NULL in Start()! This indicates a serious issue with NetworkManager setup.");
        }
    }

    void OnDisable()
    {
        // コールバック解除（再生停止時のエラー防止）
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback -= OnClientDisconnected;
        }
    }

    void OnClientConnected(ulong clientId)
    {
        Debug.Log($"Client connected: {clientId}");

        Debug.Log("OnClientConnected (in UnityServicesInitializer) triggered."); // ここに新しいログを追加

        //ホスト含めて requiredPlayers　に到達したらシーン遷移
        if (NetworkManager.Singleton.ConnectedClientsList.Count >= requiredPlayers &&
            NetworkManager.Singleton.IsHost && !sceneLoaded)
        {
            Debug.Log("飛びます3: All required players connected as host!");
            Debug.Log($"ConnectedClientsList.Count: {NetworkManager.Singleton.ConnectedClientsList.Count}");
            Debug.Log($"requiredPlayers: {requiredPlayers}");
            Debug.Log($"IsHost: {NetworkManager.Singleton.IsHost}");
            Debug.Log($"sceneLoaded: {sceneLoaded}");
            sceneLoaded = true; // 二重遷移防止フラグを立てる

            //SceneControllerを呼び出して遷移
            SceneController controller = FindFirstObjectByType<SceneController>();
            if (controller != null)
            {
                controller.LoadCharactarSelectScene();
            }
            else
            {
                Debug.LogError("SceneController がシーン内に存在しません。ビルド設定を確認してください。");
            }
        }
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
        // 必要に応じて、プレイヤー数が減った場合の処理や、sceneLoadedフラグのリセットなどを行う
        if (NetworkManager.Singleton.IsHost && sceneLoaded && NetworkManager.Singleton.ConnectedClientsList.Count < requiredPlayers)
        {
            Debug.Log("Required players count dropped below threshold. Resetting sceneLoaded flag.");
            sceneLoaded = false; // 再度接続できるようにフラグをリセットするなど
        }
    }
}
