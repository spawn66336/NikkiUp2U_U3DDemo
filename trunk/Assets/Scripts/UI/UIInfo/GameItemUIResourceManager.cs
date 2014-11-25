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
    }

    bool _IsCached(int id)
    {
        return gameItemsCache.ContainsKey(id);
    }

    void _OnGetGameItemInfoFinished(GameItemInfo info)
    {
        GameItem item = null;
        if( info.type == GameItemType.Dress )
        {
            Dress dress = new Dress();
            dress.Id = info.id;
            dress.Name = info.name;
            dress.Pos = info.showPos;
            dress.Scale = info.showScale;
            dress.ClothType = info.dressType; 
            dress.Icon = ResourceManager.GetInstance().Load(ResourceType.DressImage, info.iconPath) as Texture2D; 
            foreach( var imgName in info.imgs )
            {
                Texture2D dressImg = ResourceManager.GetInstance().Load(ResourceType.DressImage, imgName) as Texture2D;
                dress.DressImgs.Add(dressImg);
            }
            item = dress;
        }

        if( item != null )
        {
            gameItemsCache.Add(item.Id, item);
        } 
        syncCount--;
    }

    int syncCount = 0;
    Dictionary<int, GameItem> gameItemsCache = new Dictionary<int, GameItem>();

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
