using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum ResourceType
{
    UI = 1,
    DressImage,
    UISound,
    Npc
}

public class ResourceManager : MonoBehaviour 
{

    public delegate void OnResourceLoadFinished( ResourceLoadResult result ,UnityEngine.Object resource);

    public enum ResourceLoadResult
    { 
        Failed = 0,
        Loading,
        Ok
    }

    class ResourceLoadTask
    {  
        public IEnumerator Load()
        {
            yield return 0;

            try
            {
                obj = Resources.Load(path);
                if (obj != null)
                {
                    state = ResourceLoadResult.Ok;
                }
                else
                {
                    state = ResourceLoadResult.Failed;
                }
            }catch(Exception err)
            {
                state = ResourceLoadResult.Failed;
                Debug.Log(err.Message);
            }
            yield return 0;
        }
     
        public string path;
        public bool isUrl = false;
        public UnityEngine.Object obj;
        public ResourceLoadResult state = ResourceLoadResult.Loading;
        public OnResourceLoadFinished onLoadFinishedCallback;
    }

    void Awake()
    {
        if( s_instance == null )
        {
            s_instance = this;
        }
    }

    public UnityEngine.Object Load( ResourceType resType , string name )
    {
        string resPath = _GetResourcePath(resType) + name;
        UnityEngine.Object resObj;

        //先检查缓存中是否已经有资源
        if (resourceCache.ContainsKey(resPath))
        { 
            if( resourceCache.TryGetValue(resPath, out resObj) )
            { 
                return resObj;
            } 
        }

        resObj = Resources.Load(resPath);
        if (resObj != null)
        {
            resourceCache.Add(resPath,resObj);
        }
        return resObj;
    }

    public bool Load(string path , OnResourceLoadFinished callback , bool isUrl = false)
    {
        if( callback == null || path == string.Empty )
        {
            return false;
        }

       //先检查缓存中是否已经有资源
       if( resourceCache.ContainsKey(path) )
       {
           UnityEngine.Object obj = null;
           if( resourceCache.TryGetValue(path,out obj))
           {
               Debug.Log("击中资源管理器缓存！");
               callback( ResourceLoadResult.Ok , obj);
               return true;
           }
           else
           {
               Debug.Log("资源缓存管理器，缓存错误！");
               return false;
           }
       }

       ResourceLoadTask task = new ResourceLoadTask();
       task.path = path;
       task.isUrl = isUrl;
       task.onLoadFinishedCallback = callback; 
       taskList.Add(task);

       CoroutineManager.GetInstance().StartCoroutine(task.Load());

       return true;
    }
 
	void Update() 
    { 
	    foreach( var t in taskList )
        {
            if( t.state != ResourceLoadResult.Loading )
            {
                if( t.state == ResourceLoadResult.Ok )
                {
                    resourceCache.Add(t.path, t.obj);
                } 
                t.onLoadFinishedCallback(t.state, t.obj);

                removeTasks.Add(t);
            }
        }

        foreach( var t in removeTasks )
        {
            taskList.Remove(t);
        }
        removeTasks.Clear();
	}

    string _GetResourcePath( ResourceType resType )
    {
        switch(resType)
        {
            case ResourceType.UI:
                return "Art/UI/Prefab/"; 
            case ResourceType.DressImage:
                return "Art/Dress/";
            case ResourceType.UISound:
                return "Art/Sound/UI/";
            case ResourceType.Npc:
                return "Art/Sound/NPC/";
            default:
                break;
        }
        return "";
    }

    
    private List<ResourceLoadTask> taskList = new List<ResourceLoadTask>();
    private Dictionary<string, UnityEngine.Object> resourceCache = new Dictionary<string, UnityEngine.Object>();

    //临时列表
    List<ResourceLoadTask> removeTasks = new List<ResourceLoadTask>();


    public static ResourceManager GetInstance()
    {
        return s_instance;
    }

    private static ResourceManager s_instance;
}
