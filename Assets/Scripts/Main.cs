using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Main : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        ABUpdateMgr.Instance.CheckUpdate((isOver) => {
            if(isOver) {
                print("�����½���");
            }
            else
                print("��������");
        },(str) => {
            print(str);
        });

        //ABUpdateMgr.Instance.Test();

        //ABUpdateMgr.Instance.DownloadABCompareFile((isOver) => {
        //    if(isOver) {
        //        //����AB���Ա��ļ�
        //        ABUpdateMgr.Instance.GetRemoteABCompareFileInfo();
        //        //����AB��
        //        ABUpdateMgr.Instance.DownloadABFile((isOver) => {
        //            if(isOver) {
        //                print("ALL AB COMPLETED!");
        //            }
        //            else
        //                print("Downlaod Error");
        //        },(nowNum,maxNum) => {
        //            print("���ؽ��ȣ���" + nowNum + "/" + maxNum + "��");
        //        });
        //    }
        //    else {
                
        //    }
        //});
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
