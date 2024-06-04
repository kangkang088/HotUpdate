using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

public class Lesson1_MD5 : MonoBehaviour {
    // Start is called before the first frame update
    void Start() {
        string md5Str = GetMD5(Application.dataPath + "/ArtRes/AB/PC/lua");
        print(md5Str);
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
    // Update is called once per frame
    void Update() {

    }
}
