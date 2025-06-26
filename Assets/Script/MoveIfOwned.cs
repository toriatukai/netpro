using Unity.Netcode;
using UnityEngine;

public class MoveIfOwned : NetworkBehaviour
{
    public float moveSpeed = 5f;

    void Update()
    {
        // ���������L���Ă��Ȃ��I�u�W�F�N�g�Ȃ牽�����Ȃ�
        if (!IsOwner) return;

        // �L�[�{�[�h���͂ňړ�
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");

        transform.Translate(new Vector3(h, 0, v) * moveSpeed * Time.deltaTime);
    }
}
