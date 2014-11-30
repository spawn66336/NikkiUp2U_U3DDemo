using UnityEngine;
using System.Collections;

public class GameUtil 
{



    public static string GetStorageBasePath()
    {
        if( Application.isEditor )
        {//若在编辑器中执行
           return Application.dataPath + "/../StreamingAssets/";
        }
        else if (Application.platform == RuntimePlatform.Android)
        {
           return "/mnt/sdcard/Android/data/NikkiUp2UDemoAssets/";
        }  
        return string.Empty; 
    }

    public static string GetStorageBaseUrl()
    {
        return "file://" + GetStorageBasePath();
    }

    //获取配置文件所在路径
    public static string GetServerConfigBasePath()
    {
        return GetStorageBasePath() + "Config/Server/";
    }

    public static string GetClientConfigBasePath()
    {
        return GetStorageBasePath() + "Config/Client/";
    }

    //获取衣服所在路径
    public static string GetDressBasePath()
    {
        return GetStorageBasePath() + "Dress/";
    }

    public static string GetDressBaseUrl()
    {
        return GetStorageBaseUrl() + "Dress/";
    }

    //生成文件的Assetbundle名
    public static string GetFileAssetbundleName( string fileName )
    {
        return fileName + ".assetbundle";
    }

    //获取配置我呢件路径
    public static string GetServerConfigFilePath( string fileName )
    {
        return GetServerConfigBasePath() + fileName;
    }

    //获取衣服Assetbundle路径
    public static string GetDressAssetbundlePath( int id )
    {
        return GetDressBasePath() + GetFileAssetbundleName(id.ToString());
    }

}
