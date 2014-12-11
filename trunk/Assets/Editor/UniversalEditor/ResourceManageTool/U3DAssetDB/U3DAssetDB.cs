using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Threading;
using YamlDotNet.Serialization;


public class U3DAssetDBRebuildTask : IEditorCoroutineTask
{
    public enum Step
    {
        STEP_INIT_ASSETINFO = 1,
        STEP_BUILD_ASSETREFERENCED ,
        STEP_BUILD_UNUSEDASSETS
    }

    public class Result
    {
        public string[] assetPaths;
        public Dictionary<Guid, U3DAssetInfo> assetTable = new Dictionary<Guid, U3DAssetInfo>(); 
        public Dictionary<Guid, List<Guid>> assetReferencedTable = new Dictionary<Guid, List<Guid>>(); 
        public List<Guid> unUsedAssets = new List<Guid>();
    }

    public EditorCoroutine Co { get { return coroutine; } set { coroutine = value; } }

    public Guid TaskID { get { return taskID; } }

    public U3DAssetDBRebuildTask( string[] paths )
    {
        taskID = Guid.NewGuid();
        resultObj.assetPaths = paths;
        iter = resultObj.assetPaths.GetEnumerator();
        curr = 0;
        total = paths.Length;
    }

    public bool DoOneStep()
    {
        if (cancelFlag)
            return false;

        if (step == Step.STEP_INIT_ASSETINFO )
        {
            if( !iter.MoveNext() )
            {//下一个路径
                step = Step.STEP_BUILD_ASSETREFERENCED;
                iter = resultObj.assetTable.GetEnumerator();
                curr = 0;
                total = resultObj.assetTable.Count;
            }
            else
            {
                msg = "更新Asset数据...";
                curr++;
                _PostUIProgressMessage();

                _UpdateAssetTableStep();
            }

        }else if( step == Step.STEP_BUILD_ASSETREFERENCED )
        {
            if( !iter.MoveNext() )
            {
                step = Step.STEP_BUILD_UNUSEDASSETS;
                iter = resultObj.assetReferencedTable.GetEnumerator();
                curr = 0;
                total = resultObj.assetReferencedTable.Count;
            }
            else
            {
                msg = "构建Asset反向引用...";
                curr++;
                _PostUIProgressMessage();

                _UpdateAssetReferencedTableStep();
            }

        }else if( step == Step.STEP_BUILD_UNUSEDASSETS )
        {
            if( !iter.MoveNext() )
            {
                EditorUtility.UnloadUnusedAssets();
                GC.Collect();
                finished = true;
            }
            else
            {
                msg = "构建未使用Asset列表...";
                curr++;
                _PostUIProgressMessage();

                _UpdateUnUsedAssetListStep();
            }
        }
        return true;
    }

    public void Cancel()
    {
        cancelFlag = true;
    }

    public bool IsFinished()
    {
        return finished;
    }

    public bool IsCanceled() 
    { 
        return cancelFlag; 
    }

    public object GetFinishedObject() { return resultObj; }

    private void _PostUIProgressMessage()
    {
        EditorUIProgressInfo progressInfo = new EditorUIProgressInfo();
        progressInfo.msg = msg;
        progressInfo.curr = curr;
        progressInfo.total = total;
        EditorCoroutineMessage newMsg =
            new EditorCoroutineMessage(taskID, EditorCoroutineMessage.Message.PROGRESS_UI, progressInfo,null);
        Co.PostUIMessage(newMsg);
    }
    
    private void _UpdateAssetTableStep()
    {
        string path = iter.Current as string;
        if (_IsIgnoreAsset(path))
        {
            return;
        } 
         
        U3DAssetInfo assetInfo = new U3DAssetInfo();
        ResourceManageToolUtility.InitAssetInfo(path, ref assetInfo);
        Guid guid = assetInfo.guid;


        string[] deps = ResourceManageToolUtility.GetDependencies(path);
        foreach (var depPath in deps)
        { 
            Guid depGuid = ResourceManageToolUtility.PathToGuid(depPath);
            if (depGuid.Equals(guid))
            {
                continue;
            }
            assetInfo.deps.Add(depGuid);
        }
        resultObj.assetTable.Add(guid, assetInfo);
    }

    private void _UpdateAssetReferencedTableStep()
    {
        KeyValuePair<Guid, U3DAssetInfo> pair = (KeyValuePair<Guid, U3DAssetInfo>)iter.Current;

        Guid assetGuid = pair.Key;
        List<Guid> refList = new List<Guid>();

        //查找引用当前资源的资源文件
        foreach (var o in resultObj.assetTable)
        {
            //自身不可能对自身引用
            if (assetGuid.Equals(o.Key))
            {
                continue;
            }

            foreach (var dep in o.Value.deps)
            {
                if (dep.Equals(assetGuid))
                {
                    refList.Add(o.Key);
                    break;
                }
            }
        }

        resultObj.assetReferencedTable.Add(assetGuid, refList);
    }

    private void _UpdateUnUsedAssetListStep()
    {
        KeyValuePair<Guid, List<Guid>> pair = (KeyValuePair<Guid, List<Guid>>)iter.Current;
        if (pair.Value.Count == 0)
        {
            resultObj.unUsedAssets.Add(pair.Key);
        }
    }

    private bool _IsIgnoreAsset(string path)
    {
        //只扫描Assets文件夹中的资源
        if (path.IndexOf("Assets") != 0)
            return true;

        //string ext = Path.GetExtension(path);
        //if (ext.Equals(".dll"))
        //{
        //    return true;
        //}else if(ext.Equals(".cs"))
        //{
        //    return true;
        //}
               

        return false;
    }

  

    Guid taskID;
    EditorCoroutine coroutine;
    bool cancelFlag = false;
    bool finished = false;
    Step step = Step.STEP_INIT_ASSETINFO;
    IEnumerator iter = null;

    string msg = "";
    int curr = 0;
    int total = 0;

    Result resultObj = new Result();
}

[Serializable]
public class U3DAssetInfo
{
    public string ID { get { return guid.ToString(); } set { guid = new Guid(value); } }

    public string Path { get { return path; } set { path = value; } }

    public string TypeName { get { return typeName; } set { typeName = value; } }

    public bool Corrupted { get { return corrupted; } set { corrupted = value; } }

    public List<string> Deps 
    { 
        get 
        {
            List<string> depAssetIDs = new List<string>();
            foreach( var id in deps )
            {
                depAssetIDs.Add(id.ToString());
            }
            return depAssetIDs; 
        } 
        set 
        {
            deps.Clear();
            foreach( var strID in value )
            {
                deps.Add(new Guid(strID));
            } 
        } 
    }

    //ID
    public Guid guid;
    //资源文件路径
    public string path;
    //资源类型
    public string typeName;
    //资源是否已经破损
    public bool corrupted;
    //资源图标
    public Texture icon;
    //当前资源依赖的资源
    public List<Guid> deps = new List<Guid>();
}

public class U3DAssetDBSerializeObject
{
    public string StrUpdateTime
    {
        get { return strUpdateTime; }
        set { strUpdateTime = value; }
    }

    public List<KeyValuePair<string,U3DAssetInfo>> AssetTable
    {
        get { return assetTable; }
        set { assetTable = value; }
    }

    public List<KeyValuePair<string, List<string>>> AssetReferencedTable
    {
        get { return assetReferencedTable; }
        set { assetReferencedTable = value; }
    }

    public List<string> UnUsedAssets
    { 
        get { return unUsedAssetList; }
        set { unUsedAssetList = value; }
    }

    //更新时间
    private string strUpdateTime;

    //资源总查找表
    private List<KeyValuePair<string,U3DAssetInfo>> assetTable;

    //反向引用查找表
    private List<KeyValuePair<string, List<string>>> assetReferencedTable;

    //无用资源查找表
    private List<string> unUsedAssetList;

}

public class U3DAssetDB : IDisposable
{
    private U3DAssetDB() 
    {
        _RegisterCorruptedAssetsCatchCallback();
    }

   
    public void Dispose()
    {

    }

    public void Init()
    {
        Clear(); 

        string[] assetPaths = ResourceManageToolUtility.GetAllAssetPaths();
        foreach (var p in assetPaths)
        {
            if (IsPathContain(p))
            {
                U3DAssetInfo newInfo = new U3DAssetInfo();
                ResourceManageToolUtility.InitAssetInfo(p, ref newInfo); 
                assetTable.Add(newInfo.guid, newInfo);
            }
        }
    }
     
    public bool IsPathContain( string path )
    {
        if (ResourceManageConfig.GetInstance().Paths.Count == 0)
        {
            //只有Assets下的资源可被包含
            if (path.IndexOf("Assets") == 0)
                return true;
            return false;
        }

        foreach (var p in ResourceManageConfig.GetInstance().Paths)
        {
            if (0 == path.IndexOf(p))
                return true;
        }

        //此时当前路径不完全包含待扫描路径
        //如果当前路径为文件夹，则看其是否
        //在待扫描路径中出现
        if( ResourceManageToolUtility.PathIsFolder(path) )
        {
            foreach (var p in ResourceManageConfig.GetInstance().Paths)
            {
                if (0 == p.IndexOf(path))
                    return true;
            }
        }

        return false;
    }
        
    public IEditorCoroutineTask NewRebuildTask()
    {
        U3DAssetDBRebuildTask task = null;
        if (rebuildTaskID.Equals(Guid.Empty))
        {
            string[] allPaths = ResourceManageToolUtility.GetAllAssetPaths();

            //确定要扫描的资源
            List<string> searchPaths = new List<string>();
            foreach( var p in allPaths )
            {
                if( IsPathContain(p) )
                {
                    searchPaths.Add(p);
                }
            }

            task = new U3DAssetDBRebuildTask(searchPaths.ToArray());
            rebuildTaskID = task.TaskID; 
        }
        return task;
    }

    public bool Rebuild( Guid taskID ,U3DAssetDBRebuildTask.Result res )
    {
        if( !taskID.Equals(rebuildTaskID) )
        {
            return false;
        }

        assetTable = res.assetTable;
        assetReferencedTable = res.assetReferencedTable;
        unUsedAssets = res.unUsedAssets; 
        rebuildTaskID = Guid.Empty;
        //更新数据库时间
        dbUpdateTime = DateTime.Now.ToLocalTime().ToString();

        //标记已经破损的资源
        foreach( var corruptedId in corruptedAssets )
        {
            U3DAssetInfo info = null;
            if( assetTable.TryGetValue(corruptedId,out info) )
            {
                ResourceManageToolUtility.MarkAssetCorrupted(ref info);
            }
        } 
        corruptedAssets.Clear(); 
        Save();

        return true;
    }

    public void Save()
    {
        _TouchCacheFolder();
        UniversalEditorUtility.MakeFileWriteable(_GetCacheFilePath());
        StreamWriter yamlWriter = File.CreateText(_GetCacheFilePath());
        Serializer yamlSerializer = new Serializer(); 
        object obj = _GetSerializeObject(); 
        yamlSerializer.Serialize(yamlWriter, obj);
        yamlWriter.Close();
       
    }

    public bool TryLoad()
    {
        _TouchCacheFolder();
         
        StreamReader yamlReader = null;
        try
        {
            yamlReader = File.OpenText(_GetCacheFilePath());
        }
        catch (Exception e)
        {
            return false;
        }

        Deserializer yamlDeserializer = new Deserializer();
        var obj = yamlDeserializer.Deserialize<U3DAssetDBSerializeObject>(yamlReader);
        if( !_ApplySerializeObject(obj) )
        {
            yamlReader.Close();
            return false;
        }
        yamlReader.Close();
        return true;
    }

    public bool IsCacheFileExist()
    {
        return File.Exists(_GetCacheFilePath());
    }

    public bool Contain( string path )
    {
        Guid guid = ResourceManageToolUtility.PathToGuid(path);
        return assetTable.ContainsKey(guid);
    }
             
    //根据路径查找
    public bool Find( string path , out U3DAssetInfo info )
    {
        Guid guid = ResourceManageToolUtility.PathToGuid(path);
        return Find(guid, out info);
    }

    public bool Find( Guid guid , out U3DAssetInfo info )
    {
        return assetTable.TryGetValue(guid, out info);
    }

    //查找某一资源被哪些其他资源引用
    public bool FindReferencedList(Guid guid, out List<Guid> referencedList)
    {
        return assetReferencedTable.TryGetValue(guid, out referencedList);
    }

    public bool FindReferencedList(string path , out List<Guid> referencedList)
    {
        Guid guid = ResourceManageToolUtility.PathToGuid(path);
        return FindReferencedList(guid, out referencedList);
    }

    public List<U3DAssetInfo> GetUnUsedAssets()
    {
        List<U3DAssetInfo> list = new List<U3DAssetInfo>();
        foreach( var id in unUsedAssets )
        {
            U3DAssetInfo info = null;
            Find(id,out info);

            if (
                info.typeName.Equals("Scene") ||
                info.typeName.Equals("Folder")
               )
            {
                continue;
            }

            list.Add(info);
        }
        return list;
    }
    
    public string[] GetUnUsedAssetsPath()
    {
        List<string> unUsedPathList = new List<string>();
        Dictionary<string,List<U3DAssetInfo>> unUsedGroupedAssets 
            = new Dictionary<string,List<U3DAssetInfo>>();

        foreach( var p in unUsedAssets )
        {
            U3DAssetInfo info = null;
            Find(p,out info); 
            
            if( !unUsedGroupedAssets.ContainsKey(info.typeName) )
            {
                List<U3DAssetInfo> assets = new List<U3DAssetInfo>();
                unUsedGroupedAssets.Add(info.typeName,assets);
            }

            List<U3DAssetInfo> assetList = null;
            unUsedGroupedAssets.TryGetValue(info.typeName,out assetList);
            assetList.Add(info);
        }

        foreach( var t in unUsedGroupedAssets )
        {
            if (t.Key.Equals("Scene"))
            {
                continue;
            }

           List<U3DAssetInfo> assets = t.Value; 
           
           foreach( var a in assets )
           {
               unUsedPathList.Add(a.path);
           }
        }

        return unUsedPathList.ToArray();
    }

    public void Clear()
    {
        rebuildTaskID = Guid.Empty;
        assetTable.Clear();
        assetReferencedTable.Clear();
        unUsedAssets.Clear();
        GC.Collect();
    }

    public void Destory()
    {
        Clear();
        _UnRegisterCorruptedAssetsCatchCallback();
    }


    /*
     *  AssetDB持久化相关函数 
     */

    private U3DAssetDBSerializeObject _GetSerializeObject()
    {
        U3DAssetDBSerializeObject obj = new U3DAssetDBSerializeObject();

        obj.StrUpdateTime = DBUpdateTime;

        List<KeyValuePair<string, U3DAssetInfo>> assetList =
         new List<KeyValuePair<string, U3DAssetInfo>>();
        foreach (var asset in assetTable)
        {
            KeyValuePair<string, U3DAssetInfo> assetKeyPair = new KeyValuePair<string, U3DAssetInfo>
            (asset.Key.ToString(), asset.Value);
            assetList.Add(assetKeyPair);
        }

        obj.AssetTable = assetList;


        List<KeyValuePair<string,List<string>>> assetReferenced = new List<KeyValuePair<string,List<string>>>();
        foreach( var asset in assetReferencedTable)
        {
            KeyValuePair<string,List<string>> assetKeyPair = new KeyValuePair<string,List<string>>
                ( asset.Key.ToString(),new List<string>());
            
            foreach( var id in asset.Value )
            {
                assetKeyPair.Value.Add(id.ToString());
            }
            assetReferenced.Add(assetKeyPair);
        }
        obj.AssetReferencedTable = assetReferenced;

        List<string> unUsedList = new List<string>();
        foreach( var id in unUsedAssets )
        {
            unUsedList.Add(id.ToString());
        }

        obj.UnUsedAssets = unUsedList;

        return obj;
    }

    private bool _ApplySerializeObject(U3DAssetDBSerializeObject obj)
    {
        string projPath = EditorHelper.GetProjectPath();

        dbUpdateTime = obj.StrUpdateTime;

        assetTable.Clear();
        foreach( var asset in obj.AssetTable )
        {
            Guid id = new Guid(asset.Key);
            U3DAssetInfo info = asset.Value;
            string absPath = projPath + info.path;
            //数据库中所述路径所对应文件已经不存在
            if( 
                !Directory.Exists(absPath) && 
                !File.Exists(absPath) 
              )
            {
                Debug.LogError("Asset数据库已过期！路径\"" + info.path + "\"不存在!");
                return false;
            } 
            ResourceManageToolUtility.InitAssetInfoIcon(ref info);
            assetTable.Add(id, info);
        }

        assetReferencedTable.Clear();
        foreach( var pair in obj.AssetReferencedTable )
        {
            Guid id = new Guid(pair.Key);
            List<Guid> referencedList = new List<Guid>();
            foreach( var referencedID in pair.Value )
            {
                Guid refId = new Guid(referencedID);
                referencedList.Add(refId);
            }
            assetReferencedTable.Add(id, referencedList);
        }

        unUsedAssets.Clear();
        foreach( var unUsedAsset in obj.UnUsedAssets )
        {
            Guid id = new Guid(unUsedAsset);
            unUsedAssets.Add(id);
        }
        return true;
    }

    private void _TouchCacheFolder()
    {
        if( !Directory.Exists(_GetCacheFolderPath()) )
        {
            Directory.CreateDirectory(_GetCacheFolderPath());
        }
        ResourceManageToolUtility.RefreshAssetDatabase(); 
    }

    private string _GetCacheFolderPath()
    {
        string projPath = EditorHelper.GetProjectPath();
        return projPath + s_AssetDBCacheRelPath;
    }

    private string _GetCacheFilePath()
    {
        string projPath = EditorHelper.GetProjectPath();
        return projPath + s_AssetDBCacheRelPath + s_AssetDBCacheFileName;
    }

    private void _RegisterCorruptedAssetsCatchCallback()
    { 
        EditorManager.GetInstance().logCallback += _CorruptedAssetsCatchCallback;
    }

    private void _UnRegisterCorruptedAssetsCatchCallback()
    { 
        EditorManager.GetInstance().logCallback -= _CorruptedAssetsCatchCallback;
    }

    private void _CorruptedAssetsCatchCallback(string condition, string stackTrace, LogType type)
    {
        if( condition.Contains("Failed to read file") && 
            condition.Contains("because it is corrupted.")
           )
        {//资源已经破损
            string subStr = condition.Substring(condition.IndexOf("Assets"));
            string corruptedAssetPath = subStr.Substring(0,subStr.LastIndexOf('\''));
            corruptedAssets.Add( ResourceManageToolUtility.PathToGuid(corruptedAssetPath) );
        }
    }
     

    public Dictionary<Guid,U3DAssetInfo> AssetTable
    {
        get { return assetTable; }
    }

    public string DBUpdateTime
    {
        get { return dbUpdateTime; }
    }

    //重建任务ID
    private Guid rebuildTaskID = Guid.Empty;

    //数据库更新时间
    private string dbUpdateTime;
    
    //主U3D资源映射表
    private Dictionary<Guid, U3DAssetInfo> assetTable = new Dictionary<Guid,U3DAssetInfo>();

    //某个资源被哪些其他资源引用
    private Dictionary<Guid, List<Guid>> assetReferencedTable = new Dictionary<Guid, List<Guid>>();

    //未被引用资源
    private List<Guid> unUsedAssets = new List<Guid>();

    //已损坏的资源列表
    private List<Guid> corruptedAssets = new List<Guid>(); 
     

    public static U3DAssetDB GetInstance()
    {
        if( s_Instance == null )
        {
            s_Instance = new U3DAssetDB();
        }
        return s_Instance;
    }

    public static void DestoryInstance()
    {
        if( s_Instance != null )
        {
            s_Instance.Destory();
            s_Instance = null;
        }
        GC.Collect();
    }

    private static U3DAssetDB s_Instance = null;

    private static string s_AssetDBCacheRelPath = "Assets/Editor/UniversalEditor/ResourceManageTool/Cache/";

    private static string s_AssetDBCacheFileName = "AssetDB.cache";
}
