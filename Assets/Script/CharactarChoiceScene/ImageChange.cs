using UnityEngine;

public class SkillSelector : MonoBehaviour
{
    public GameObject[] skillSetArray; // ← Image + Text を含んだ親オブジェクト
    private int count;

    void Start()
    {
        count = 0;

        // 全て非表示
        foreach (var obj in skillSetArray)
        {
            obj.SetActive(false);
        }

        // 最初だけ表示
        skillSetArray[count].SetActive(true);
    }

    public void SetNext()
    {
        skillSetArray[count].SetActive(false);

        count = (count + 1) % skillSetArray.Length;

        skillSetArray[count].SetActive(true);
        Debug.Log("次のスキルを表示");
    }

    public void SetBack()
    {
        skillSetArray[count].SetActive(false);

        count = (count - 1 + skillSetArray.Length) % skillSetArray.Length;

        skillSetArray[count].SetActive(true);
        Debug.Log("前のスキルを表示");
    }
}