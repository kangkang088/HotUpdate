using System.Collections;
using System.Collections.Generic;
using System.Drawing.Printing;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;

public class MoveABToSA
{
    //[MenuItem("ABTool/MoveABToStreamingAssets")]
    private static void MoveABToStreamingAssets() {
        Object[] selectAssets = Selection.GetFiltered(typeof(Object),SelectionMode.DeepAssets);
        if(selectAssets.Length == 0)
            return;
        string abCompareInfo = "";
        foreach(Object asset in selectAssets) {
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
}
