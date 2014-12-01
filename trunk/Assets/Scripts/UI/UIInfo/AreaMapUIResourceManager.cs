using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaMapUIInfo
{
    //区域地图Id
    public int id;
    //区域地图名
    public string name; 
    //当前区域地图预览图标
    public Texture2D mapIconImg;
    //当前区域地图纹理
    public List<Texture2D> mapImgs = new List<Texture2D>();
    //当前区域地图所对应关卡id列表
    public int[] levels; 
}

public class AreaMapUIResourceManager : UIResourceManager 
{

    public AreaMapUIInfo TryGetAreaMapUIInfo( int id )
    {
        if (!_IsCached(id))
        {
            PostRequest(new UIResourceSyncRequest(id, ""));
            return null;
        }

        AreaMapUIInfo areaMapUIInfo = null;
        areaMapUIInfoCache.TryGetValue(id , out areaMapUIInfo);
        return areaMapUIInfo;
    }

    //获取区域地图id列表
    public List<int> GetAreaMapIdList()
    {
        return areaMapIdList;
    }

    public override IEnumerator Sync()
    {
        AreaMapModule areaMapModule = 
            GlobalObjects.GetInstance().GetLogicMain().GetModule<AreaMapModule>();

        syncCount = 1;
        areaMapModule.GetAreaMapList(_OnSyncAreaMapIdListFinished);
        while( syncCount != 0 )
        {
            yield return 0;
        }

        syncCount = requestList.Count;
        foreach( var req in requestList )
        {
            if (_IsCached(req.ResID))
            {
                syncCount--;
            }
            else
            {
                areaMapModule.GetAreaMapInfo(req.ResID, _OnSyncAreaMapInfoFinished);
            }
        }

        while( syncCount != 0)
        {
            yield return 0;
        }


    }

    bool _IsCached(int id)
    {
        return areaMapUIInfoCache.ContainsKey(id);
    }
    
    void _OnSyncAreaMapIdListFinished(List<int> mapIdList)
    {
        areaMapIdList = mapIdList;
        syncCount--;
    }

    void _OnSyncAreaMapInfoFinished( AreaMapInfo info )
    {
        AreaMapUIInfo areaMapUIInfo = new AreaMapUIInfo();
        areaMapUIInfo.id = info.id;
        areaMapUIInfo.name = info.name; 
        areaMapUIInfo.levels = info.levels;
        areaMapUIInfo.mapIconImg = ResourceManager.GetInstance().Load(ResourceType.AreaMap, info.mapIconImgPath) as Texture2D;  
        //foreach( var imgName in info.mapImgPaths )
        //{
        //    Texture2D mapImg = ResourceManager.GetInstance().Load(ResourceType.AreaMap, imgName) as Texture2D;
        //    areaMapUIInfo.mapImgs.Add(mapImg);
        //}
        areaMapUIInfoCache.Add(info.id, areaMapUIInfo);
        syncCount--;
    }

    int syncCount = 0;
    List<int> areaMapIdList = new List<int>();
    Dictionary<int, AreaMapUIInfo> areaMapUIInfoCache = new Dictionary<int, AreaMapUIInfo>();

	public static AreaMapUIResourceManager GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new AreaMapUIResourceManager();
        }
        return s_instance;
    }

    static AreaMapUIResourceManager s_instance;
}
