using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Collider2D crosshairCollider;

    public bool HasAlreadyHit { get; private set; } = false;
    public int RemainingBullets { get; set; } = 5;

    private void Start()
    {
        HasAlreadyHit = false;
        RemainingBullets = 5;
    }

    public void MouseFollow()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        transform.position = mousePos;
    }

    public bool CheckHit()
    {
        Collider2D[] results = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D();
        int count = crosshairCollider.Overlap(filter, results);

        for (int i = 0; i < count; i++)
        {
            var hitCollider = results[i];

            if (hitCollider.CompareTag("Target"))
            {
                hitCollider.GetComponent<TargetController>().OnHit();
                HasAlreadyHit = true;
                return true;
            }
            else if (hitCollider.CompareTag("Decoy"))
            {
                Destroy(hitCollider.gameObject);
                return false; // �f�R�C�͖����Ƃ��Ȃ����e�͌���
            }
            else if (hitCollider.CompareTag("Area"))
            {
                return false; // �͈͓����������ł͂Ȃ�
            }
        }

        return false; // �͈͊O�i�^�O�Ȃ��j�ł̃N���b�N�͒e�����炳�Ȃ�
    }
}
