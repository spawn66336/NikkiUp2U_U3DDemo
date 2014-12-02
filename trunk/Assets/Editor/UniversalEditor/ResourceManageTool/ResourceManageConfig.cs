using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using YamlDotNet.Serialization;

public class ResourceManageConfig  
{
    public ResourceManageConfig() 
    { 
    }
    
    public void Init()
    {
        if( Load() )
        {
            return;
        }

        Paths.Add("Assets");
    }

    public void Save()
    {
        _TouchConfigDir(); 

        var configWriter =  File.CreateText(_GetConfigFilePath());
        Serializer yamlSerialzer = new Serializer();
        yamlSerialzer.Serialize(configWriter, Paths);
        configWriter.Close();
    }
            
    public bool Load()
    {
        _TouchConfigDir();

        StreamReader configReader;
        try
        {
            configReader = File.OpenText(_GetConfigFilePath());
        }
        catch (Exception e)
        {
            return false;
        }

        Deserializer yamlDeserializer = new Deserializer();
        paths = yamlDeserializer.Deserialize<List<string>>(configReader);
        configReader.Close(); 
        return true;
    } 

    private void _TouchConfigDir()
    {
        if( Directory.Exists(s_configDirPath) )
        {
            return;
        }
        Directory.CreateDirectory(s_configDirPath);
    }

    private string _GetConfigFilePath()
    {
        return s_configDirPath + s_configFileName;
    }

    //资源管理器搜索路径
    public List<string> Paths
    {
        get { return paths; }
        set { paths = value; }
    }
        
    private List<string> paths = new List<string>();

    public static ResourceManageConfig GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new ResourceManageConfig();
        }
        return s_instance;
    }

    public static void DestoryInstance()
    {
        if( s_instance != null )
        {
            s_instance = null;
        }

        GC.Collect();
    }

    private static ResourceManageConfig s_instance = null;

    private static string s_configDirPath = "Assets/Editor/UniversalEditor/ResourceManageTool/Config/";

    private static string s_configFileName = "ResourceManageToolConfig.cfg";
}
