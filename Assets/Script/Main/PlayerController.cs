using UnityEngine;
using Unity.Netcode;

/*
public class PlayerController : NetworkBehaviour
{
    [SerializeField] private Collider2D _crosshair;     // �}�E�X�J�[�\���ɒǏ]����I�u�W�F�N�g���W
    [SerializeField] private int _maxBullets = 5;       // 1���E���h������̍ő唭�ː�
    
    private int _bulletsLeft;   // �c�e��
    private bool _alreadyHit = false; // �^�[�Q�b�g�����ς݂�

    void Start()
    {
        // �ő吔�ɂ���(���E���h�J�n���ɂ��s��)
        _bulletsLeft = _maxBullets;
    }
    public void StartRound()
    {
        _bulletsLeft = _maxBullets;
        _alreadyHit = false;
    }

    // Update is called once per frame
    void Update()
    {

        MouseFollow();
        Debug.Log("�c��e��" + _bulletsLeft);

        // GameManager.Instance �� null �̊Ԃ́A���̌�̏������X�L�b�v����
        // ����ɂ��AGameManager �����S�ɏ���������A�X�|�[�������̂�҂�
        if (GameManager.Instance == null)
        {
            return;
        }

        if (!GameManager.Instance.IsPlaying()) return;

        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0 && !_alreadyHit)
        {
            Shoot();
            Debug.Log("�c��e��" + _bulletsLeft);
        }
    }

    public void MouseFollow()
    {
        //�}�E�X�ʒu�ɒǏ]
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    public void Shoot()
    {
        // ������Collider2D�ƐڐG���Ă���Collider2D���擾
        Collider2D[] hits = new Collider2D[10];
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };

        int hitCount = _crosshair.Overlap(filter, hits);
        bool hitObject = false;
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCollider = hits[i];
            if (hitCollider == null)
                continue;

            if (hitCollider.CompareTag("Target"))
            {
                Debug.Log("Hit target: " + hitCollider.name);
                hitObject = true;

                // �������̏���
                hitCollider.GetComponent<Target>().OnHit();
                _alreadyHit = true;
                break;
            }
            else if (hitCollider.CompareTag("Decoy"))
            {
                Debug.Log("Hit Decoy: " + hitCollider.name);
                hitObject = true;

                // �������̏���
                Destroy(hitCollider.gameObject);
            }
            else if (hitCollider.CompareTag("Area"))
            {
                Debug.Log("Hit Area:" + hitCollider.name);
                hitObject = true;
            }
        }

        if (hitObject)
        {
            _bulletsLeft--;
            if(_bulletsLeft <= 0)
            {
                // �c�e����0�ɂȂ�����^�C�����M
                ulong clientId = NetworkManager.Singleton.LocalClientId;
                ScoreManager.Instance.SetHitTimeList(clientId, 500.0f);

                GameManager.Instance.NotifyPlayerFinished();

            }
        }
    }
}*/

public class PlayerController : NetworkBehaviour
{
    [SerializeField] private CrosshairController crosshairController;

    private bool canShoot = false;

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            canShoot = true;
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

        if (GameManager.Instance == null || GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        if (Input.GetMouseButtonDown(0))
        {
            TryShoot();
        }
    }

    private void TryShoot()
    {
        if (crosshairController.HasAlreadyHit)
        {
            Debug.Log("���������ς�");
            return;
        }

        if (crosshairController.RemainingBullets <= 0)
        {
            Debug.Log("�e�؂�");
            SendReactionTime(-1f);
            canShoot = false;
            return;
        }

        bool hitTarget = crosshairController.CheckHit();

        crosshairController.RemainingBullets--;

        if (hitTarget)
        {
            float reactionTime = Time.time - GameManager.Instance.TargetDisplayTime;
            SendReactionTime(reactionTime);
            canShoot = false;
        }
        else
        {
            Debug.Log("�O����");
            // �e�͌��邪�������Ԃ͑���Ȃ��i�����؂�ɂȂ�܂Łj
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
}