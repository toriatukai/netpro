using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ImageChange : MonoBehaviour
{

    public GameObject[] imageArray;
    private int count;
    public GameObject imageObj;

    void Start()
    {
        count = 0;
        imageObj = GameObject.Instantiate(imageArray[count]) as GameObject;
    }

    // �\������X�L���A�C�R�����Ǘ����郁�\�b�h
    public void ImageSet()
    {
        Destroy(imageObj);
        count++;
        if(count >= imageArray.Length)
        {
            count = 0;
        }
        Debug.Log("�{�^����������܂����I"); //�m�F�p
        imageObj = GameObject.Instantiate(imageArray[count]) as GameObject;
    }
}
