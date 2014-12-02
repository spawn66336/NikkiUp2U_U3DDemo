using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;

//过滤资源用接口类
public interface IAssetFilter
{
    bool Check(U3DAssetInfo assetInfo);
}
 
public class AssetNameFilter : IAssetFilter
{
    public bool Check(U3DAssetInfo assetInfo)
    {
        if( Path.GetFileName(assetInfo.path).ToLower().
            Contains(SearchText))
        {
            return true;
        }
        return false;
    }

    public string SearchText
    {
        get { return searchText; }
        set { searchText = value.ToLower(); }
    }

    private string searchText = "";
}

//资源类型过滤器
public class AssetTypeFilter : IAssetFilter
{
    protected AssetTypeFilter() { }
    
    public AssetTypeFilter( string[] ts , string display ,int tIndx = 0)
    {
        typeNames = ts;
        displayTypeName = display;
        typeIndx = tIndx;
    }

    public virtual bool Check(U3DAssetInfo assetInfo )
    {
        foreach( var t in TypeNames )
        {
            if( assetInfo.TypeName.Equals(t))
            {
                return true;
            }
        } 
        return false;
    }

    //可以有多种类型映射为一种显示类型
    public  string[] TypeNames
    {
        get { return typeNames; }
    }

    //显示用类型名
    public  string DisplayTypeName
    {
        get { return displayTypeName; }
    }

    //类型索引
    public  int TypeIndex
    {
        get { return typeIndx; }
        set { typeIndx = value; }
    }

    protected string[] typeNames;
    protected string displayTypeName;
    protected int typeIndx;
}

public class NullTypeFilter : AssetTypeFilter
{
    public NullTypeFilter() 
    {
        typeNames = new string[]{};
        displayTypeName = "全部资源";
        typeIndx = 0;
    }

    public override bool Check(U3DAssetInfo assetInfo) { return true; } 
}

public class AssetInfoComparer : IComparer<U3DAssetInfo>
{
    private AssetInfoComparer() { }

    public AssetInfoComparer(List<IAssetFilter> filters) { assetFilters = filters; }

    public int Compare(U3DAssetInfo x, U3DAssetInfo y)
    {
        int xType = GetAssetTypeIndex(x);
        int yType = GetAssetTypeIndex(y);

        //若类型相等则比较两个资源的名字
        if( xType == yType )
        {
            string xAssetName = Path.GetFileName(x.path);
            string yAssetName = Path.GetFileName(y.path);
            StringComparer strCmp = StringComparer.CurrentCultureIgnoreCase;
            return strCmp.Compare(xAssetName, yAssetName);
        }
        return xType - yType;       
    
    }

    int GetAssetTypeIndex( U3DAssetInfo info )
    {
        if (info.TypeName.Equals("Folder"))
            return 0;

        foreach( var filter in assetFilters )
        {
            if( (filter as NullTypeFilter) != null )
            {
                continue;
            }

            AssetTypeFilter typeFilter = filter as AssetTypeFilter;
            if( typeFilter.Check(info))
            {
                return typeFilter.TypeIndex;
            }
        }
        return 0;
    }

    List<IAssetFilter> assetFilters;
}

public class ResourceManageToolModel  
{
    public enum State
    {
        STATE_INIT,
        STATE_BUILD
    }

    private ResourceManageToolModel() 
    {
        //初始化所有过滤器
        _InitAssetFilters(); 
    }

    public delegate void ModelChangeDelegate(ResourceManageToolModel model);
    
    public ModelChangeDelegate onResourceDBUpdate;
    public ModelChangeDelegate onResrouceDBStateChange;


    public void Init()
    {  
        bool tryLoadCache = false;
        if( U3DAssetDB.GetInstance().IsCacheFileExist() )
        {
            tryLoadCache = EditorUtility.DisplayDialog("缓存", "侦测到有Asset数据库缓存，是否现在载入？", "是", "否");
        }

        if (tryLoadCache)
        {
            if (U3DAssetDB.GetInstance().TryLoad())
            {
                _UpdateAllAssetList();
                _NotifyAssetDatabaseUpdate();

                state = State.STATE_BUILD;
                _NotifyAssetDatabaseStateChange();
                return;
            }
        }
        
        U3DAssetDB.GetInstance().Init();
        _UpdateAllAssetList();

        state = State.STATE_INIT;
        _NotifyAssetDatabaseStateChange();
    }

    public IEditorCoroutineTask NewUpdateTask()
    { 
        return U3DAssetDB.GetInstance().NewRebuildTask();
    }
    
    public bool UpdateAssetDatabase(Guid taskID, object resultObj)
    {
        bool ret = false;
        U3DAssetDBRebuildTask.Result rs = resultObj as U3DAssetDBRebuildTask.Result;
        ret = U3DAssetDB.GetInstance().Rebuild(taskID, rs);
        _UpdateAllAssetList();
        _NotifyAssetDatabaseUpdate();

        if( ret )
        {
            state = State.STATE_BUILD;
            _NotifyAssetDatabaseStateChange();
        }

        return ret;
    }

    public bool IsAssetDatabaseContain( string path )
    {
        return U3DAssetDB.GetInstance().Contain(path);
    }

    public string GetAssetDatabaseUpdateTime() { return U3DAssetDB.GetInstance().DBUpdateTime; }

    public void GetAssetDependencies( Guid resID , out List<U3DAssetInfo> assets )
    {
        assets = new List<U3DAssetInfo>();
        U3DAssetInfo assetInfo = null;

        if (U3DAssetDB.GetInstance().Find(resID, out assetInfo))
        {//若找到了查依赖的资源

            //若资源已经损坏则不再构建正向依赖
            if( assetInfo.Corrupted )
            {
                return;
            }

            if (assetInfo.deps.Count > 0)
            {//已初始化正向依赖信息
                foreach( var depId in assetInfo.deps )
                {
                    U3DAssetInfo depAsset = null;
                    if (U3DAssetDB.GetInstance().Find(depId, out depAsset))
                    {//正向依赖资源已经收集到资源数据库中
                        assets.Add(depAsset);
                    }
                    else
                    {//未收集到资源数据库中，建临时信息
                        depAsset = new U3DAssetInfo();
                        depAsset.guid = depId;
                        ResourceManageToolUtility.InitAssetInfo(ResourceManageToolUtility.GuidToPath(depId), ref depAsset);
                        assets.Add(depAsset);
                    }
                }
                assets.Sort(new AssetInfoComparer(AssetFilterList));
            }
            else
            {//未初始化正向依赖信息
                _GetAssetDependencies(resID, out assets);
            }
        }
    }
     
    //查找某个资源被谁引用
    public void GetAssetReferenced( Guid resID , out List<U3DAssetInfo> assets )
    {
        List<Guid> referencedList = null;
        U3DAssetDB.GetInstance().FindReferencedList(resID , out referencedList);
        assets = new List<U3DAssetInfo>();
        if( referencedList != null )
        {
            foreach( var r in referencedList )
            {
                U3DAssetInfo info = null;
                if( U3DAssetDB.GetInstance().Find(r,out info) )
                {
                    assets.Add(info);
                }
            }
        }

        assets.Sort(new AssetInfoComparer(AssetFilterList));
    }

    public bool Find(string path, out U3DAssetInfo info)
    {
        return U3DAssetDB.GetInstance().Find(path, out info);
    }

    //设置当前所用过滤器
    public void SetFilter( int typeIndx )
    {
        foreach( var filter in AssetFilterList)
        {
            AssetTypeFilter f = filter as AssetTypeFilter;
            if( f.TypeIndex == typeIndx )
            {
                currAssetFilter = f;
                _UpdateAllAssetList();
                _NotifyAssetDatabaseUpdate();
                return;
            }
        }
    }

    public void SetFilter(IAssetFilter filter)
    {
        if (filter == null)
        {
            return;
        }
        currAssetFilter = filter;
        _UpdateAllAssetList();
        _NotifyAssetDatabaseUpdate();
    }

    private void _GetAssetDependencies(Guid id, out List<U3DAssetInfo> assets)
    {
        U3DAssetInfo assetInfo = null;
        assets = new List<U3DAssetInfo>();

        if(!U3DAssetDB.GetInstance().Find(id, out assetInfo) )
        {
            return;
        } 
                
        string[] depPaths = ResourceManageToolUtility.GetDependencies(assetInfo.path); 
        foreach (var p in depPaths)
        {
            if (p.Equals(assetInfo.Path))
            {
                continue;
            }

            U3DAssetInfo depAsset = null; 
            Guid depId = ResourceManageToolUtility.PathToGuid(p);
            //在数据库中没有找到此资源则初始化
            if( !U3DAssetDB.GetInstance().Find(depId,out depAsset) )
            {
                depAsset = new U3DAssetInfo();
                depAsset.guid = depId;
                ResourceManageToolUtility.InitAssetInfo(p, ref depAsset);
                U3DAssetDB.GetInstance().AssetTable.Add(depAsset.guid, depAsset);
            } 
            assets.Add(depAsset); 
        } 
        assets.Sort(new AssetInfoComparer(AssetFilterList));
    }

    private void _InitAssetFilters()
    {
        assetFilterList.Add(new NullTypeFilter());
        assetFilterList.Add(new AssetTypeFilter(new string[] { "AnimationClip" }, "动画片段"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "AnimatorController", "OverrideController" }, "动画控制器"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "AudioClip" }, "音频片段"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Prefab" }, "Prefab"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Model" }, "模型文件"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Shader", "ComputeShader" }, "Shader"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Material" }, "材质"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "PhyMaterial", "Phy2DMaterial" }, "物理材质"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Texture", "RenderTexture", "CubeMap" }, "纹理"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "GUISkin" }, "GUISkin"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Font", "TrueTypeFont" }, "字体"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "AvatarMask" }, "AvatarMask"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Flare" }, "Flare"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Scene" }, "场景"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Script", "MonoAssembly" }, "脚本"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Text" }, "文本文件"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "UnKnown" }, "未知"));
        assetFilterList.Add(new AssetTypeFilter(new string[] { "Corrupted" }, "已破损"));

        int i = 0;
        //分配类型索引
        foreach( var filter in assetFilterList )
        {
            AssetTypeFilter tFilter = filter as AssetTypeFilter;
            tFilter.TypeIndex = i++;
        }

        currAssetFilter = assetFilterList[0];

    }

    //更新总资源列表与未使用资源列表
    private void _UpdateAllAssetList()
    {
        _UpdateAssetList();
        _UpdateUnUsedAssetList();
    }


    private void _UpdateAssetList()
    {
        assetList.Clear();
        foreach (var a in U3DAssetDB.GetInstance().AssetTable)
        {
            if (
                currAssetFilter.Check(a.Value)&& 
                U3DAssetDB.GetInstance().IsPathContain(a.Value.path)
                )
            {
                assetList.Add(a.Value);
            }
        }

        assetList.Sort(new AssetInfoComparer(AssetFilterList));
    }

    private void _UpdateUnUsedAssetList()
    {
        unUsedAssetList = new List<U3DAssetInfo>(); 
        List<U3DAssetInfo> assets = U3DAssetDB.GetInstance().GetUnUsedAssets(); 
        foreach( var a in assets )
        {
            if( 
                currAssetFilter.Check(a) &&
                U3DAssetDB.GetInstance().IsPathContain(a.path)
               )
            {
                unUsedAssetList.Add(a);
            }
        }

        unUsedAssetList.Sort(new AssetInfoComparer(AssetFilterList));
    }

    private void _NotifyAssetDatabaseUpdate()
    {
        if( onResourceDBUpdate != null )
        {
            onResourceDBUpdate(this);
        }
    }

    private void _NotifyAssetDatabaseStateChange()
    {
        if( onResrouceDBStateChange != null )
        {
            onResrouceDBStateChange(this);
        }
    }


    public List<U3DAssetInfo> AssetList { get { return assetList; } }

    public List<U3DAssetInfo> UnUsedAssetList { get { return unUsedAssetList; } }

    public List<IAssetFilter> AssetFilterList { get { return assetFilterList; } }

    public IAssetFilter CurrFilter { get { return currAssetFilter; } }

    public State CurrDBState { get { return state; } }

    //数据库状态
    private State state = State.STATE_INIT;

    private List<U3DAssetInfo> assetList = new List<U3DAssetInfo>();

    private List<U3DAssetInfo> unUsedAssetList = new List<U3DAssetInfo>();

    private List<IAssetFilter> assetFilterList = new List<IAssetFilter>();

    private IAssetFilter currAssetFilter;
  
	public static ResourceManageToolModel GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new ResourceManageToolModel();
        }
        return s_instance;
    }

    public static void DestroyInstance()
    {
        U3DAssetDB.DestoryInstance();
        s_instance = null;
        GC.Collect();
    }

    private static ResourceManageToolModel s_instance;
}
