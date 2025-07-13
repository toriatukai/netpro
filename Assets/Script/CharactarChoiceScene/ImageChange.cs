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

    // 表示するスキルアイコンを管理するメソッド
    public void ImageSet()
    {
        Destroy(imageObj);
        count++;
        if(count >= imageArray.Length)
        {
            count = 0;
        }
        Debug.Log("ボタンが押されました！"); //確認用
        imageObj = GameObject.Instantiate(imageArray[count]) as GameObject;
    }
}
