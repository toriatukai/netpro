using UnityEngine;

public class SkillSelector : MonoBehaviour
{
    public GameObject[] skillSetArray; // �� Image + Text ���܂񂾐e�I�u�W�F�N�g
    private int count;

    void Start()
    {
        count = 0;

        // �S�Ĕ�\��
        foreach (var obj in skillSetArray)
        {
            obj.SetActive(false);
        }

        // �ŏ������\��
        skillSetArray[count].SetActive(true);
    }

    public void SetNext()
    {
        skillSetArray[count].SetActive(false);

        count = (count + 1) % skillSetArray.Length;

        skillSetArray[count].SetActive(true);
        Debug.Log("���̃X�L����\��");
    }

    public void SetBack()
    {
        skillSetArray[count].SetActive(false);

        count = (count - 1 + skillSetArray.Length) % skillSetArray.Length;

        skillSetArray[count].SetActive(true);
        Debug.Log("�O�̃X�L����\��");
    }
}