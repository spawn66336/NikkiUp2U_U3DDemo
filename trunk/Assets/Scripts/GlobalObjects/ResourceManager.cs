using UnityEngine;
using System.Collections;
using System;
using System.Collections.Generic;

public enum ResourceType
{
    UI = 1,
    AreaMap,
    DressImage,
    UISound,
    Npc,
    LevelDialogBackground,
	CommentNpc
}

public enum ResourceLoadResult
{
    Failed = 0,
    Loading,
    Ok
}

public delegate void OnResourceLoadFinished(ResourceLoadResult result, UnityEngine.Object[] objs , object param );

public class ResourceManager : MonoBehaviour 
{ 
      
    class CacheObject
    {
        public List<UnityEngine.Object> objs = new List<UnityEngine.Object>();
    }


    class ResourceLoadTask
    { 
         
        public IEnumerator Load()
        {
            WWW www = WWW.LoadFromCacheOrDownload(path, version);  
            yield return www;

            UnityEngine.Object[] objs = null;

            if( www.isDone )
            {
                if ( www.error == null )
                {
                    state = ResourceLoadResult.Ok;
                    AssetBundle bundle = www.assetBundle;
                    cacheObj = new CacheObject();
                    objs = UnpackBundle<UnityEngine.Object>(bundle);
                    foreach (var o in objs)
                    {
                        cacheObj.objs.Add(o);
                    }
                    bundle.Unload(false);
                    Debug.Log("资源" + path + "读取成功！");
                }
                else
                {
                    state = ResourceLoadResult.Failed;
                    Debug.LogWarning("资源" + path + "读取失败！error = " + www.error );
                }
            }
            else
            {
                state = ResourceLoadResult.Failed;  
            }
            onLoadFinishedCallback(state, objs, param);
        }
     
        public string path;
        public int version = 1;
        public bool isUrl = false;
        public CacheObject cacheObj;
        public object param;
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
        CacheObject cacheObj;

        //先检查缓存中是否已经有资源
        if (resourceCache.ContainsKey(resPath))
        {
            if (resourceCache.TryGetValue(resPath, out cacheObj))
            {
                if (cacheObj.objs.Count > 0)
                {
                    return cacheObj.objs[0];
                }
                else
                {
                    return null;
                }
            } 
        }

        UnityEngine.Object resObj = Resources.Load(resPath);
        if (resObj != null)
        {
            cacheObj = new CacheObject();
            cacheObj.objs.Add(resObj);
            resourceCache.Add(resPath, cacheObj);
        }
        return resObj;
    }

    public bool LoadAsyn(ResourceType type , string fileName , int version , object param ,OnResourceLoadFinished callback)
    {
        if (callback == null || fileName == string.Empty)
        {
            if( callback != null )
            {
                callback(ResourceLoadResult.Failed, null, param);
            }
            return false;
        }

       string path = _GetResourcePath(type) + fileName;

       //先检查缓存中是否已经有资源
       if( resourceCache.ContainsKey(path) )
       {
           CacheObject cacheObj = null;
           if (resourceCache.TryGetValue(path, out cacheObj))
           { 
               callback(ResourceLoadResult.Ok, cacheObj.objs.ToArray(), param);
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
       task.version = version;
       task.param = param;
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
                    resourceCache.Add(t.path, t.cacheObj);
                }   
                removeTasks.Add(t);
            }
        }

        foreach( var t in removeTasks )
        {
            taskList.Remove(t);
        }
        removeTasks.Clear();
	}

    public static T[] UnpackBundle<T>(AssetBundle bundle) where T : UnityEngine.Object
    {
        if (bundle == null)
        {
            return null;
        }

        List<T> resList = new List<T>();

        UnityEngine.Object[] objs = bundle.LoadAll();
        foreach (var obj in objs)
        {
            T res = obj as T;
            if (res != null)
            {
                resList.Add(res);
            }
        }
        return resList.ToArray();
    }

    string _GetResourcePath( ResourceType resType )
    {
        switch(resType)
        {
            case ResourceType.UI:
                return "Art/UI/Prefab/"; 
            case ResourceType.AreaMap:
                return "Art/UI/Texture/AreaMap/";
            case ResourceType.DressImage:
                return GameUtil.GetDressBaseUrl(); 
            case ResourceType.UISound:
                return "Art/Sound/UI/";
            case ResourceType.Npc:
                return "Art/UI/Texture/NPC/";
            case ResourceType.LevelDialogBackground:
                return "Art/UI/Texture/LevelDialogBackground/";
			case ResourceType.CommentNpc:
				return"Art/UI/Texture/common-1/";
            default:
                break;
        }
        return "";
    }

    
    private List<ResourceLoadTask> taskList = new List<ResourceLoadTask>();
    private Dictionary<string, CacheObject> resourceCache = new Dictionary<string,CacheObject>();

    //临时列表
    List<ResourceLoadTask> removeTasks = new List<ResourceLoadTask>();


    public static ResourceManager GetInstance()
    {
        return s_instance;
    }

    private static ResourceManager s_instance;
}
