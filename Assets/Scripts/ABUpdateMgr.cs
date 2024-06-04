using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Networking;

public class ABUpdateMgr : MonoBehaviour {
    private static ABUpdateMgr instance;
    public static ABUpdateMgr Instance {
        get {
            if(instance == null) {
                GameObject obj = new GameObject("ABUpdateMgr");
                instance = obj.AddComponent<ABUpdateMgr>();
            }
            return instance;
        }
    }
    private string serverIP = "ftp://127.0.0.1";
    //存储远端AB包对比信息的容器
    private Dictionary<string,ABInfo> remoteABInfo = new Dictionary<string,ABInfo>();
    //本地AB包对比信息的容器
    private Dictionary<string,ABInfo> localABInfo = new Dictionary<string,ABInfo>();
    //经对比后待下载的AB包名字容器
    private List<string> downloadList = new List<string>();
    public void CheckUpdate(UnityAction<bool> overCallback,UnityAction<string> updateInfoCallback) {
        remoteABInfo.Clear();
        localABInfo.Clear();
        downloadList.Clear();
        DownloadABCompareFile((isOver) => {
            updateInfoCallback?.Invoke("开始更新资源");
            if(isOver) {
                updateInfoCallback?.Invoke("远端对比文件下载结束");
                string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABComparable_TEMP.txt");
                updateInfoCallback?.Invoke("开始解析远端对比文件");
                GetRemoteABCompareFileInfo(remoteInfo,remoteABInfo);
                updateInfoCallback?.Invoke("解析远端对比文件完成");

                GetLocalABCompareFileInfo((isOver) => {
                    if(isOver) {
                        updateInfoCallback?.Invoke("解析本地对比文件完成");

                        //进行对比
                        updateInfoCallback?.Invoke("开始对比");
                        foreach(string abName in remoteABInfo.Keys) {
                            if(!localABInfo.ContainsKey(abName)) {
                                downloadList.Add(abName);
                            }
                            else {
                                if(localABInfo[abName].md5 != remoteABInfo[abName].md5) {
                                    downloadList.Add(abName);
                                    
                                }
                                localABInfo.Remove(abName);
                            }
                        }
                        updateInfoCallback?.Invoke("对比完成");
                        updateInfoCallback?.Invoke("删除无用AB包");
                        //删除本地多余的文件，再下载AB包
                        foreach(string abName in localABInfo.Keys) {
                            if(File.Exists(Application.persistentDataPath + "/" + abName))
                                File.Delete(Application.persistentDataPath + "/" + abName);
                        }
                        updateInfoCallback?.Invoke("开始下载更新AB包");
                        DownloadABFile((isOver) => {
                            if(isOver) {
                                File.WriteAllText(Application.persistentDataPath + "/ABComparable.txt",remoteInfo);
                            }
                            overCallback(isOver);
                        },updateInfoCallback);
                    }

                    else
                        overCallback(false);
                });
            }
            else {
                overCallback?.Invoke(false);
            }
        });
    }
    public async void DownloadABCompareFile(UnityAction<bool> overCallback) {
        bool isOver = false;
        int reDownloadMaxNum = 5;
        string localPath = Application.persistentDataPath + "/";
        while(!isOver && reDownloadMaxNum > 0) {
            await Task.Run(() => {
                isOver = DownlaodFile("ABComparable.txt",localPath + "ABComparable_TEMP.txt");
            });
            reDownloadMaxNum--;
        }
        overCallback?.Invoke(isOver);
    }
    public void GetRemoteABCompareFileInfo(string info,Dictionary<string,ABInfo> abInfo) {
        //string info = File.ReadAllText(Application.persistentDataPath + "/ABCompareInfo_TMP.txt");
        string[] strs = info.Split(new char[] { '|' });
        string[] infos = null;
        foreach(string str in strs) {
            infos = str.Split(' ');
            //ABInfo abInfo = new ABInfo(infos[0],infos[1],infos[2]);
            abInfo.Add(infos[0],new ABInfo(infos[0],infos[1],infos[2]));
        }
    }
    public void GetLocalABCompareFileInfo(UnityAction<bool> overCallback) {
        if(File.Exists(Application.persistentDataPath + "/ABComparable.txt")) {
            StartCoroutine(GetLocalABCompareFileInfo("file:///" + Application.persistentDataPath + "/ABComparable.txt",overCallback));
        }
        else if(File.Exists(Application.streamingAssetsPath + "/ABComparable.txt")) {
            string path =
                #if UNITY_ANDROID
                Application.streamingAssetsPath;
                #else
                "file:///" + Application.streamingAssetsPath;
                #endif
            StartCoroutine(GetLocalABCompareFileInfo(path + "/ABComparable.txt",overCallback));
        }
        else {
            overCallback?.Invoke(true);
        }
    }
    private IEnumerator GetLocalABCompareFileInfo(string filePath,UnityAction<bool> overCallback) {
        UnityWebRequest req = UnityWebRequest.Get(filePath);
        yield return req.SendWebRequest();
        print(req.downloadHandler.text);
        if(req.result == UnityWebRequest.Result.Success) {
            GetRemoteABCompareFileInfo(req.downloadHandler.text,localABInfo);
            overCallback?.Invoke(true);
        }
        else
            overCallback?.Invoke(false);
    }
    public async void DownloadABFile(UnityAction<bool> overCallBack,UnityAction<string> updatePro) {
        //1.遍历对比容器的键
        //foreach(string name in remoteABInfo.Keys) {
        //    downloadList.Add(name);
        //}
        string localPath = Application.persistentDataPath + "/";
        bool isOver = false;
        int reDownloadMaxNum = 5;
        int downloadOverNum = 0;
        int downloadMaxNum = downloadList.Count;
        List<string> tempList = new List<string>();
        while(downloadList.Count > 0 && reDownloadMaxNum > 0) {
            for(int i = 0;i < downloadList.Count;i++) {
                isOver = false;
                await Task.Run(() => {
                    isOver = DownlaodFile(downloadList[i],localPath + downloadList[i]);
                });
                if(isOver) {
                    tempList.Add(downloadList[i]);//记录下载成功的
                    updatePro?.Invoke(++downloadOverNum + "/" + downloadMaxNum);
                }
            }
            for(int i = 0;i < tempList.Count;i++) {
                downloadList.Remove(tempList[i]);
            }
            reDownloadMaxNum--;
        }
        overCallBack?.Invoke(downloadList.Count == 0);
    }
    private bool DownlaodFile(string fileName,string localPath) {

        try {
            string pInfo =
#if UNITY_IOS
            "IOS";
#elif UNITY_ANDROID
            "Android";
#else
            "PC";
#endif
            FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AB/" + pInfo +"/" + fileName)) as FtpWebRequest;
            NetworkCredential n = new NetworkCredential("kangkang","08875799");
            req.Proxy = null;
            req.Credentials = n;
            req.KeepAlive = false;
            req.Method = WebRequestMethods.Ftp.DownloadFile;
            req.UseBinary = true;
            FtpWebResponse res = req.GetResponse() as FtpWebResponse;
            Stream downloadStream = res.GetResponseStream();
            using(FileStream file = File.Create(localPath)) {
                byte[] bytes = new byte[2048];
                int contentLength = downloadStream.Read(bytes,0,bytes.Length);
                while(contentLength != 0) {
                    file.Write(bytes,0,contentLength);
                    contentLength = downloadStream.Read(bytes,0,bytes.Length);
                }
                file.Close();
                downloadStream.Close();
            }
            return true;
        }
        catch(Exception e) {
            Debug.Log("下载失败：" + e.Message);
            return false;
        }
    }
    private void OnDestroy() {
        instance = null;
    }
    public class ABInfo {
        public string name;
        public long size;
        public string md5;
        public ABInfo(string name,string size,string md5) {
            this.name = name;
            this.size = long.Parse(size);
            this.md5 = md5;
        }
    }
}
