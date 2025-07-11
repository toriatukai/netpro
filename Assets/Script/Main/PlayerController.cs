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

        if (IsServer && Input.GetKeyDown(KeyCode.Space))
        {
            if (GameManager.Instance.CurrentState == GameManager.GameState.Connecting)
            {
                GameManager.Instance.StartGame();
            }
        }
        /*if(GameManager.Instance != null)
        {
            Debug.Log("GameManagerInstance: " + GameManager.Instance + ",  GameManager.Instance.CurrentState: " + GameManager.Instance.CurrentState);
        }*/

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
        {
            return;
        }

        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            TryShoot();
        }
        else
        {
            Debug.Log("canShoot: " + canShoot);
        }
    }

    private void TryShoot()
    {
        if (crosshairController.HasAlreadyHit)
        {
            Debug.Log("もう命中済み");
            return;
        }

        if (crosshairController.RemainingBullets <= 0)
        {
            Debug.Log("弾切れ");
            SendReactionTime(-1f);
            canShoot = false;
            return;
        }

        bool hitTarget = crosshairController.CheckHit();

        crosshairController.RemainingBullets--;

        if (hitTarget)
        {
            Debug.Log("Time.time: " + Time.time + ", GameManager.Instance.TargetDisplayTime: " + GameManager.Instance.TargetDisplayTime);
            float reactionTime = Time.time - GameManager.Instance.TargetDisplayTime;

            SendReactionTime(reactionTime);
            canShoot = false;
        }
        else
        {
            Debug.Log("外した");
            // 弾は減るが反応時間は送らない（撃ち切りになるまで）
        }
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
        Debug.Log("クライアント側のプレイヤーがラウンドをリセット");
        if (crosshairController != null)
        {
            canShoot = true;
            crosshairController.HasAlreadyHit = false;
            crosshairController.RemainingBullets = 5;
        }
    }
}