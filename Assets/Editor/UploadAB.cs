using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

public class UploadAB {
    //[MenuItem("ABTool/UploadAllABFile")]
    private static void UploadAllABFile() {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC/");
        FileInfo[] fileInfos = directory.GetFiles();
        foreach(FileInfo fileInfo in fileInfos) {
            if(fileInfo.Extension == "" || fileInfo.Extension == ".txt") {
                //上传到FTP服务器中
                FTPUploadFile(fileInfo.FullName,fileInfo.Name);
            }
        }

    }
    private async static void FTPUploadFile(string filePath,string fileName) {
        await Task.Run(() => {
            try {
                FtpWebRequest req = FtpWebRequest.Create(new Uri("ftp://127.0.0.1/AB/PC/" + fileName)) as FtpWebRequest;
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
