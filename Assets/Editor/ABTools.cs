using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class ABTools : EditorWindow
{
    private int nowSelIndex = 0;
    string[] targetStrs = new string[] { "PC","IOS","Android"};
    private string serverIP = "ftp://127.0.0.1";
    [MenuItem("ABTool/OpenToolWindow")]
    private static void OpenWindow() {
        ABTools window = EditorWindow.GetWindowWithRect(typeof(ABTools),new Rect(0,0,380,220)) as ABTools;
        window.Show();
    }
    private void OnGUI() {
        GUI.Label(new Rect(10,10,150,15),"平台选择");
        nowSelIndex = GUI.Toolbar(new Rect(10,30,250,20),nowSelIndex,targetStrs);
        GUI.Label(new Rect(10,60,150,15),"资源服务器地址");
        serverIP = GUI.TextField(new Rect(10,80,150,20),serverIP);
        //创建对比文件按钮
        if(GUI.Button(new Rect(10,110,100,40),"创建对比文件")) {
            CreateABCompareFile();
        }
        //保存默认资源到StreamingAssets
        if(GUI.Button(new Rect(140,110,200,40),"保存默认资源到StreamingAssets")) {
            MoveABToStreamingAssets();
        }
        //上传AB包和对比文件
        if(GUI.Button(new Rect(10,160,330,40),"上传AB包和对比文件")) {
            UploadAllABFile();
        }
    }
    private void CreateABCompareFile() {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetStrs[nowSelIndex]);
        FileInfo[] fileInfos = directory.GetFiles();
        string abCompareInfo = "";
        foreach(FileInfo fileInfo in fileInfos) {
            if(fileInfo.Extension == "") {
                Debug.Log(fileInfo.Name);
                abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + GetMD5(fileInfo.FullName) + '|';
            }
            //Debug.Log(fileInfo.Name);
            //Debug.Log(fileInfo.FullName);
            //Debug.Log(fileInfo.Extension);
            //Debug.Log(fileInfo.Length);
        }
        abCompareInfo = abCompareInfo.Substring(0,abCompareInfo.Length - 1);
        //Debug.Log(abCompareInfo);
        File.WriteAllText(Application.dataPath + "/ArtRes/AB/" + targetStrs[nowSelIndex] + "/ABCompareInfo.txt",abCompareInfo);
        AssetDatabase.Refresh();
        Debug.Log("AB Compared File Generates Successfully");
    }
    private string GetMD5(string filePath) {
        using(FileStream file = new FileStream(filePath,FileMode.Open,FileAccess.Read)) {
            MD5 md5 = new MD5CryptoServiceProvider();
            byte[] md5Info = md5.ComputeHash(file);
            file.Close();
            StringBuilder sb = new StringBuilder();
            foreach(byte b in md5Info) {
                sb.Append(b.ToString("x2"));
            }
            return sb.ToString();
        }

    }

    private void MoveABToStreamingAssets() {
        UnityEngine.Object[] selectAssets = Selection.GetFiltered(typeof(UnityEngine.Object),SelectionMode.DeepAssets);
        if(selectAssets.Length == 0)
            return;
        string abCompareInfo = "";
        foreach(UnityEngine.Object asset in selectAssets) {
            string assetPath = AssetDatabase.GetAssetPath(asset);
            string fileName = assetPath.Substring(assetPath.LastIndexOf('/'));
            if(fileName.IndexOf(".") != -1)
                continue;
            AssetDatabase.CopyAsset(assetPath,"Assets/StreamingAssets" + fileName);

            FileInfo fileInfo = new FileInfo(Application.streamingAssetsPath + fileName);

            abCompareInfo += fileInfo.Name + " " + fileInfo.Length + " " + CreateABCompare.GetMD5(fileInfo.FullName);
            abCompareInfo += '|';

        }
        abCompareInfo = abCompareInfo.Substring(0,abCompareInfo.Length - 1);
        File.WriteAllText(Application.streamingAssetsPath + "/ABCompareInfo.txt",abCompareInfo);
        AssetDatabase.Refresh();
    }

    private void UploadAllABFile() {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/" + targetStrs[nowSelIndex] + "/");
        FileInfo[] fileInfos = directory.GetFiles();
        foreach(FileInfo fileInfo in fileInfos) {
            if(fileInfo.Extension == "" || fileInfo.Extension == ".txt") {
                //上传到FTP服务器中
                FTPUploadFile(fileInfo.FullName,fileInfo.Name);
            }
        }

    }
    private async void FTPUploadFile(string filePath,string fileName) {
        await Task.Run(() => {
            try {
                FtpWebRequest req = FtpWebRequest.Create(new Uri(serverIP + "/AB/" + targetStrs[nowSelIndex] + "/" + fileName)) as FtpWebRequest;
                NetworkCredential n = new NetworkCredential("kangkang","08875799");
                req.Proxy = null;
                req.Credentials = n;
                req.KeepAlive = false;
                req.Method = WebRequestMethods.Ftp.UploadFile;
                req.UseBinary = true;
                Stream uploadStream = req.GetRequestStream();
                using(FileStream file = File.OpenRead(filePath)) {
                    byte[] bytes = new byte[2048];
                    int contentLength = file.Read(bytes,0,bytes.Length);
                    while(contentLength != 0) {
                        uploadStream.Write(bytes,0,contentLength);
                        contentLength = file.Read(bytes,0,bytes.Length);
                    }
                    file.Close();
                    uploadStream.Close();
                }
            }
            catch(Exception ex) {
                Debug.Log("上传出错：" + ex.Message);
            }
        });

    }
}
