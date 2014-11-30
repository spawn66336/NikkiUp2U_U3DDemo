using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class NikkiDemoBundlePacker : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    [MenuItem("H3D/NikkiUp2UDemo/打包所有资源")]
    static void PackAll()
    {
        string packTargetPath = "Assets/../StreamingAssets/";

        _PackAllDress(packTargetPath + "Dress/");

        Debug.Log("打包完毕！");
    }


    [MenuItem("H3D/NikkiUp2UDemo/打包选定资源")]
    static void PackSelectionObjects()
    {

    }

    static void _PackAllDress( string targetPath )
    {

        Dictionary<string, List<string>> dressTable = new Dictionary<string, List<string>>();

        string[] dressAssetPaths = _GetAssetPathsInSpecDir("Assets/PackAssets/Dress/");
        foreach (var path in dressAssetPaths)
        {
            string fileName = Path.GetFileNameWithoutExtension(path);

            if (fileName[0] != '0')
            {
                continue;
            }

            if (fileName.IndexOf('_') < 0)
            {//非暖暖衣服命名规则
                continue;
            }

            int idIndx = fileName.IndexOf('_') + 1;
            int idCount = fileName.LastIndexOf('_') - fileName.IndexOf('_') - 1;

            string dressId = fileName.Substring(idIndx, idCount);

            if (!dressTable.ContainsKey(dressId))
            {
                dressTable.Add(dressId, new List<string>());
            }


            dressTable[dressId].Add(path);
        }

        foreach (var dressPaths in dressTable)
        {
            List<UnityEngine.Object> dressAssets = new List<Object>();

            string dressId = dressPaths.Key;

            string finalBundlePath = targetPath + dressId + ".assetbundle";

            foreach (var path in dressPaths.Value)
            {
                dressAssets.Add(AssetDatabase.LoadMainAssetAtPath(path));
            }


            BuildPipeline.BuildAssetBundle(null, dressAssets.ToArray(), finalBundlePath,
                BuildAssetBundleOptions.UncompressedAssetBundle | BuildAssetBundleOptions.CollectDependencies | BuildAssetBundleOptions.CompleteAssets,
                BuildTarget.Android);
            Resources.UnloadUnusedAssets();
            AssetDatabase.Refresh();
        }

    }


    static string[] _GetAssetPathsInSpecDir( string path )
    {
       string[] assetPaths = AssetDatabase.GetAllAssetPaths();
       List<string> pathsSpecDir = new List<string>();
        
       foreach( var p in assetPaths )
       { 
           if( p.IndexOf(path) >= 0 )
           { 
               if ( (File.GetAttributes(p) & FileAttributes.Directory) != 0 )
               {//如果文件是文件夹
                   continue;
               } 
               pathsSpecDir.Add(p);
           }
       }

       return pathsSpecDir.ToArray();
    }

}
