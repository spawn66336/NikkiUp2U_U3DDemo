using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameItemUIResourceManager : UIResourceManager 
{ 
    public GameItem TryGetGameItem(int id)
    {
        if (!_IsCached(id))
        {
            PostRequest(new UIResourceSyncRequest(id, ""));
            return null;
        }

        GameItem item = null;
        gameItemsCache.TryGetValue(id, out item);
        return item;
    }

    public override IEnumerator Sync()
    {
        GameItemDatabaseModule gameItemDBModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<GameItemDatabaseModule>();

        gameItemInfos.Clear();

        syncCount = requestList.Count;
        foreach (var req in requestList)
        {
            if (_IsCached(req.ResID))
            {
                syncCount--;
            }
            else
            {
                gameItemDBModule.GetGameItemInfoById(req.ResID, _OnGetGameItemInfoFinished);
                yield return 0;
            }
        }
         
        
        while (syncCount != 0)
        {
            yield return 0;
        }

        syncCount = gameItemInfos.Count;

        foreach( var info in gameItemInfos )
        {
            if (_IsCached(info.id))
            {
                syncCount--;
            }
            else
            {
                ResourceManager.GetInstance().LoadAsyn(ResourceType.DressImage, info.id.ToString() + ".assetbundle", 1, info, _OnFinishLoadDressImageBundle);
            }
        }

        while (syncCount != 0)
        {
            yield return 0;
        }
    }

    bool _IsCached(int id)
    {
        return gameItemsCache.ContainsKey(id);
    }

    void _OnFinishLoadDressImageBundle(ResourceLoadResult result, UnityEngine.Object[] objs, object param)
    {
        syncCount--;

        if (result == ResourceLoadResult.Ok)
        {

            Dress dress = new Dress();
            GameItemInfo info = param as GameItemInfo;

            dress.Id = info.id;
            dress.Name = info.name;
            dress.Pos = info.showPos;
            dress.Scale = info.showScale;
            dress.ClothType = info.dressType;

            foreach( var o in objs )
            {
                Texture2D dressImg = o as Texture2D;

                if( dressImg != null )
                {
                    dress.DressImgs.Add(dressImg);
                }
            }

            if( dress.DressImgs.Count > 0 )
            {
                dress.Icon = dress.DressImgs[0];
                dress.DressImgs.RemoveAt(0);
            } 
            gameItemsCache.Add(dress.Id, dress);
        }
    }

    void _OnGetGameItemInfoFinished(GameItemInfo info)
    {
        syncCount--;
        if (info != null)
        {
            gameItemInfos.Add(info);
        }
     
        
    }

    int syncCount = 0;
    Dictionary<int, GameItem> gameItemsCache = new Dictionary<int, GameItem>();
    List<GameItemInfo> gameItemInfos = new List<GameItemInfo>();

	public static GameItemUIResourceManager GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new GameItemUIResourceManager();
        }
        return s_instance;
    }

    static GameItemUIResourceManager s_instance;
}
