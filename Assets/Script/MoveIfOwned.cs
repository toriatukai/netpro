using Unity.Netcode;
using UnityEngine;

public class MoveIfOwned : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // 自分が所有していないオブジェクトなら何もしない
        if (!IsOwner) return;

        // キーボード入力で移動
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime);
    }
}
