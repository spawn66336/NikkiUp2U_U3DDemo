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

            string imgPrefix = "0_" + dress.Id + "_"; 
            int indx = 0;
            Texture2D dressImg = null;
            do
            {
                string imgName = imgPrefix + indx.ToString();
                dressImg = ResourceManager.GetInstance().Load(ResourceType.DressImage, imgName) as Texture2D; 
                if( dressImg != null )
                {
                    dress.DressImgs.Add(dressImg);
                }
                indx++; 
            }while (dressImg != null);

            if( dress.DressImgs.Count > 0 )
            {
                dress.Icon = dress.DressImgs[0];
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
