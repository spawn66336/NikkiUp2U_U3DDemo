using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerUIResource : UIResourceManager 
{
    public override void Init()
    {
        
    }

    public int GetGold() 
    {
        return playerInfo.gold;
    }

    public int GetEnergy()
    {
        return playerInfo.energy;
    }

    public int GetDiamond()
    {
        return playerInfo.diamond;
    }

    public int GetCurrLevel()
    {
        return playerInfo.currLevelId;
    }
     
    public List<PlayerLevelRecordInfo> GetLevelRecordInfoList()
    {
        return playerInfo.levelRecordList;
    }

    public List<BagItemInfo> GetBagItemList()
    {
        return bagItems;
    }

	public override IEnumerator Sync()
    {
        PlayerModule playerModule =  GlobalObjects.GetInstance().GetLogicMain().GetModule<PlayerModule>();
        isSyncing = true;
        playerModule.GetPlayerInfo(_OnUpdatePlayerInfoFinished);
        while (isSyncing)
        {
            yield return 0;
        }

        BagModule bagModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<BagModule>();
        isSyncing = true;
        bagModule.GetBagItemInfos(_OnUpdateBagItemInfoListFinished);
        while(isSyncing)
        {
            yield return 0;
        }
    }

    void _OnUpdatePlayerInfoFinished(PlayerInfo info)
    {
        playerInfo = info;
        isSyncing = false;
    }

    void _OnUpdateBagItemInfoListFinished(List<BagItemInfo> infos)
    {
        bagItems = infos;
        isSyncing = false;
    }

    bool isSyncing = false;
    PlayerInfo playerInfo = new PlayerInfo();
    List<BagItemInfo> bagItems = new List<BagItemInfo>();

    public static PlayerUIResource GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new PlayerUIResource();
        }
        return s_instance;
    }

    static PlayerUIResource s_instance;
}
