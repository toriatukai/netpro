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
            Debug.Log("���������ς�");
            return;
        }

        bool hitTarget = crosshairController.CheckHit();

        crosshairController.RemainingBullets--;

        if (crosshairController.RemainingBullets <= 0)
        {
            Debug.Log("�����؂�ŊO����");
            GameUIManager.Instance.SetReactionTime(-1f); // �e�L�X�g�̔z�u
            SendReactionTime(-1f);
            canShoot = false;
        }
        else
        {
            Debug.Log("�O�����A�c��e��: " + crosshairController.RemainingBullets);
            // �e�͌��邪�������Ԃ͑���Ȃ��i�����؂�ɂȂ�܂Łj
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
        Debug.Log("�N���C�A���g���̃v���C���[�����E���h�����Z�b�g");
        if (crosshairController != null)
        {
            canShoot = true;
            crosshairController.HasAlreadyHit = false;
            crosshairController.RemainingBullets = 5;
        }
    }
}