using UnityEngine;
using UnityEngine.UI; // InputField —p
using TMPro;
public class RelayUI : MonoBehaviour
{
    public TMP_InputField joinCodeInput;

    public JoinRelayClient joinRelayClient; // Joinˆ—‚ğ‚·‚éƒNƒ‰ƒX

    public void OnClickJoin()
    {
        string code = joinCodeInput.text; // “ü—Í‚³‚ê‚½•¶š—ñ‚ğæ“¾
        joinRelayClient.JoinRelay(code);  // æ“¾‚µ‚½•¶š—ñ‚ğJoinŠÖ”‚É“n‚·
    }
}
