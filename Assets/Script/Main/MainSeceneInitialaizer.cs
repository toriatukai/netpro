using UnityEngine;
using Unity.Netcode; // Netcode for GameObjects を使用するために必要
using System.Collections; // Coroutine を使用する場合に必要

public class MainSceneInitializer : MonoBehaviour
{
    [SerializeField]
    private GameObject gameManagerPrefab; // インスペクターからGameManagerプレハブを割り当てる

    void Start()
    {
        // NetworkManagerが初期化されるのを待つ
        // NetworkManager.Singleton.IsServer または NetworkManager.Singleton.IsHost が true になるまで待つ
        StartCoroutine(WaitForNetworkManagerReady());
    }

    private IEnumerator WaitForNetworkManagerReady()
    {
        // NetworkManagerが利用可能になるまで待機
        yield return new WaitUntil(() => NetworkManager.Singleton != null && NetworkManager.Singleton.IsListening);

        // ホスト（または専用サーバー）の場合のみGameManagerをスポーンする
        if (NetworkManager.Singleton.IsServer || NetworkManager.Singleton.IsHost)
        {
            // GameManagerプレハブが割り当てられているか確認
            if (gameManagerPrefab != null)
            {
                // GameManagerのインスタンスを生成
                GameObject gameManagerInstance = Instantiate(gameManagerPrefab);
                // ネットワーク上でスポーン
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