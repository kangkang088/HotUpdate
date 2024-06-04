using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEditor;
using UnityEngine;

public class CreateABCompare
{
    //[MenuItem("ABTool/GenerateCompareFile")]
    public static void CreateABCompareFile() {
        DirectoryInfo directory = Directory.CreateDirectory(Application.dataPath + "/ArtRes/AB/PC/");
        FileInfo[] fileInfos = directory.GetFiles();
        string abCompareInfo = "";
        foreach (FileInfo fileInfo in fileInfos) {
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
        File.WriteAllText(Application.dataPath + "/ArtRes/AB/PC/ABCompareInfo.txt",abCompareInfo);
        AssetDatabase.Refresh();
        Debug.Log("AB Compared File Generates Successfully");
    }
    public static string GetMD5(string filePath) {
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
}
