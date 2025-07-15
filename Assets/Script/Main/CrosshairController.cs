using UnityEngine;

public class CrosshairController : MonoBehaviour
{
    [SerializeField] private Collider2D crosshairCollider;

    public bool HasAlreadyHit { get; set; } = false;
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
        Collider2D[] hits = new Collider2D[5];
        ContactFilter2D filter = new ContactFilter2D { useTriggers = true };
        int count = crosshairCollider.Overlap(filter, hits);


        for (int i = 0; i < count; i++)
        {
            var hitCollider = hits[i];
            Debug.Log("ヒットしたオブジェクトのタグ: " + hitCollider.tag);
        }

        for (int i = 0; i < count; i++)
        {

            var hitCollider = hits[i];

            if (hitCollider.CompareTag("Target"))
            {
                hitCollider.GetComponent<TargetController>().OnHit();
                //HasAlreadyHit = true;
                return true;
            }
            else if (hitCollider.CompareTag("Decoy"))
            {
                Destroy(hitCollider.gameObject);
                return false;
            }
            else if (hitCollider.CompareTag("Area"))
            {
                return false;
            }
        }

        return false;
    }
}
