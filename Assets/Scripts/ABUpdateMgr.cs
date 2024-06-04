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
    //�洢Զ��AB���Ա���Ϣ������
    private Dictionary<string,ABInfo> remoteABInfo = new Dictionary<string,ABInfo>();
    //����AB���Ա���Ϣ������
    private Dictionary<string,ABInfo> localABInfo = new Dictionary<string,ABInfo>();
    //���ԱȺ�����ص�AB����������
    private List<string> downloadList = new List<string>();
    public void CheckUpdate(UnityAction<bool> overCallback,UnityAction<string> updateInfoCallback) {
        remoteABInfo.Clear();
        localABInfo.Clear();
        downloadList.Clear();
        DownloadABCompareFile((isOver) => {
            updateInfoCallback?.Invoke("��ʼ������Դ");
            if(isOver) {
                updateInfoCallback?.Invoke("Զ�˶Ա��ļ����ؽ���");
                string remoteInfo = File.ReadAllText(Application.persistentDataPath + "/ABComparable_TEMP.txt");
                updateInfoCallback?.Invoke("��ʼ����Զ�˶Ա��ļ�");
                GetRemoteABCompareFileInfo(remoteInfo,remoteABInfo);
                updateInfoCallback?.Invoke("����Զ�˶Ա��ļ����");

                GetLocalABCompareFileInfo((isOver) => {
                    if(isOver) {
                        updateInfoCallback?.Invoke("�������ضԱ��ļ����");

                        //���жԱ�
                        updateInfoCallback?.Invoke("��ʼ�Ա�");
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
                        updateInfoCallback?.Invoke("�Ա����");
                        updateInfoCallback?.Invoke("ɾ������AB��");
                        //ɾ�����ض�����ļ���������AB��
                        foreach(string abName in localABInfo.Keys) {
                            if(File.Exists(Application.persistentDataPath + "/" + abName))
                                File.Delete(Application.persistentDataPath + "/" + abName);
                        }
                        updateInfoCallback?.Invoke("��ʼ���ظ���AB��");
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
        //1.�����Ա������ļ�
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
                    tempList.Add(downloadList[i]);//��¼���سɹ���
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
            Debug.Log("����ʧ�ܣ�" + e.Message);
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
