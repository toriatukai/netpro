using UnityEngine;
using Unity.Netcode;

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private CrosshairController crosshairController;

    private bool canShoot = false;


    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            canShoot = true;
            LocalInstance = this;
        }
    }

    private void Update()
    {
        crosshairController.MouseFollow();

        if (!IsOwner) return;

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (crosshairController.HasAlreadyHit)
        {
            Debug.Log("もう命中済み");
            return;
        }

        bool hitTarget = crosshairController.CheckHit();

        crosshairController.RemainingBullets--;
        GameUIManager.Instance.UpdateBulletsText(crosshairController.RemainingBullets);

        if (crosshairController.RemainingBullets <= 0 && !crosshairController.HasAlreadyHit)
        {
            Debug.Log("撃ち切りで外した");
            GameUIManager.Instance.SetReactionTime(-1f); // テキストの配置
            SendReactionTime(-1f);
            canShoot = false;
        }
        else
        {
            Debug.Log("外した、残り弾数: " + crosshairController.RemainingBullets);
            // 弾は減るが反応時間は送らない（撃ち切りになるまで）
        }
    }

    public void OnTargetHit(float reactionTime)
    {
        if (crosshairController.HasAlreadyHit) return;

        crosshairController.HasAlreadyHit = true;
        canShoot = false;

        GameUIManager.Instance.SetReactionTime(reactionTime);

        SendReactionTime(reactionTime);

    }

    private void SendReactionTime(float time)
    {
        SubmitReactionTimeServerRpc(OwnerClientId, time);
    }

    [ServerRpc(RequireOwnership = false)]
    private void SubmitReactionTimeServerRpc(ulong clientId, float reactionTime)
    {
        GameManager.Instance.SubmitReactionTimeServerRpc(clientId, reactionTime);
    }

    public static PlayerController LocalInstance { get; private set; }
    public CrosshairController Crosshair => crosshairController;

    public void ResetClientRound()
    {
        //GameUIManager.Instance.ResetReactionTime();
        
        if (crosshairController != null)
        {
            canShoot = true;
            crosshairController.HasAlreadyHit = false;
            crosshairController.RemainingBullets = 5;
        }
    }

    public void NotifyReadyForRound()
    {
        if (!IsOwner) return;
        NotifyReadyServerRpc();
    }

    [ServerRpc]
    private void NotifyReadyServerRpc(ServerRpcParams rpcParams = default)
    {
        GameManager.Instance.NotifyReadyForRoundServerRpc(OwnerClientId);
    }
}