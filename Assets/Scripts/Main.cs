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
                print("检测更新结束");
            }
            else
                print("请检查网络");
        },(str) => {
            print(str);
        });

        //ABUpdateMgr.Instance.Test();

        //ABUpdateMgr.Instance.DownloadABCompareFile((isOver) => {
        //    if(isOver) {
        //        //解析AB包对比文件
        //        ABUpdateMgr.Instance.GetRemoteABCompareFileInfo();
        //        //下载AB包
        //        ABUpdateMgr.Instance.DownloadABFile((isOver) => {
        //            if(isOver) {
        //                print("ALL AB COMPLETED!");
        //            }
        //            else
        //                print("Downlaod Error");
        //        },(nowNum,maxNum) => {
        //            print("下载进度：（" + nowNum + "/" + maxNum + "）");
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
