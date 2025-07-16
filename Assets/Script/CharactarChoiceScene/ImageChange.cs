using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SkillSelector : MonoBehaviour
{
    public GameObject[] skillSetArray; // ← Image + Text を含んだ親オブジェクト
    private int count;
    public SkillType currentSkill { get; private set; } = SkillType.None;
    [SerializeField] private Button startButton;

    void Start()
    {
        count = 0;

        // 全て非表示
        foreach (var obj in skillSetArray)
        {
            obj.SetActive(false);
        }

        currentSkill = SetSkill();

        // 最初だけ表示
        skillSetArray[count].SetActive(true);
    }

    public void SetNext()
    {
        skillSetArray[count].SetActive(false);

        count = (count + 1) % skillSetArray.Length;

        currentSkill =  SetSkill();

        skillSetArray[count].SetActive(true);
        Debug.Log("次のスキルを表示");
    }

    public void SetBack()
    {
        skillSetArray[count].SetActive(false);

        count = (count - 1 + skillSetArray.Length) % skillSetArray.Length;

        currentSkill = SetSkill();

        skillSetArray[count].SetActive(true);
        Debug.Log("前のスキルを表示");
    }

    private SkillType SetSkill()
    {
        switch (count)
        {
            case 0:
                return SkillType.Gunman;
            case 1:
                return SkillType.Artillery;
            case 2:
                return SkillType.Engineer;
        }
        return SkillType.None;
    }

    public void ConfirmSelection()
    {
        ulong clientId = NetworkManager.Singleton.LocalClientId;
        SkillRetention.Instance.SetSkillForClient(clientId, currentSkill);
        startButton.interactable = false;
        //SkillRetention.Instance.SubmitReadyServerRpc(clientId);
    }

}