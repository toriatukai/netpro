using Unity.Services.Authentication;
using Unity.Services.Core;
using UnityEngine;
using Unity.Netcode; // ← Netcode用のusingを忘れずに！

public class UnityServicesInitializer : MonoBehaviour
{
    async void Awake()
    {
        // Unity Services を初期化し、匿名ログインする
        await UnityServices.InitializeAsync();

        if (!AuthenticationService.Instance.IsSignedIn)
        {
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in anonymously.");
        }
    }

    void OnEnable()
    {
        // NetworkManagerが正しく初期化された後に使う
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
            NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;
        }
        else
        {
            Debug.LogWarning("NetworkManager is not initialized yet.");
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
    }

    void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client disconnected: {clientId}");
    }
}
