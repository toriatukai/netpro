using UnityEngine;

public class WaitingTextController : MonoBehaviour
{
    public GameObject waitingText;
    private bool firstClicked = true;

    public void ShowWaitingText()
    {
        if (firstClicked)
        {
            if (waitingText != null)
            {
                waitingText.SetActive(true);
            }
            else
            {
                Debug.LogWarning("waitingText ���ݒ肳��Ă��܂���");
            }
            firstClicked = false;
        }
    }
}
