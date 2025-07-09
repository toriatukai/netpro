using UnityEngine;
using Unity.Netcode;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Collider2D _crosshair;     // �}�E�X�J�[�\���ɒǏ]����I�u�W�F�N�g���W
    [SerializeField] private int _maxBullets = 5;       // 1���E���h������̍ő唭�ː�
    private int _bulletsLeft;   // �c�e��

    void Start()
    {
        // �ő吔�ɂ���(���E���h�J�n���ɂ��s��)
        _bulletsLeft = _maxBullets;
    }

    // Update is called once per frame
    void Update()
    {
        MouseFollow();

        // TODO: ���E���h���̂ݑłĂ�悤�ɕύX����
        if (Input.GetMouseButtonDown(0) && _bulletsLeft > 0)
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
            {
                Debug.Log("null");
                continue;
            }

            if (hitCollider.CompareTag("Target"))
            {
                Debug.Log("Hit target: " + hitCollider.name);
                hitObject = true;

                // �������̏���
                hitCollider.GetComponent<Target>().OnHit();
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
                ScoreManager.Instance.SetHitTimeList(clientId, 0.0f);

            }
        }
    }
}
