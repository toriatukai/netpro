using UnityEngine;

public class TargetController : MonoBehaviour
{
    public void OnHit()
    {
        Debug.Log("Target hit: " + gameObject.name);
        Destroy(gameObject);
    }
}
