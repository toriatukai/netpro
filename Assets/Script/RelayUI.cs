using UnityEngine;
using UnityEngine.UI; // InputField �p
using TMPro;
public class RelayUI : MonoBehaviour
{
    public TMP_InputField joinCodeInput;

    public JoinRelayClient joinRelayClient; // Join����������N���X

    public void OnClickJoin()
    {
        string code = joinCodeInput.text; // ���͂��ꂽ��������擾
        joinRelayClient.JoinRelay(code);  // �擾�����������Join�֐��ɓn��
    }
}
