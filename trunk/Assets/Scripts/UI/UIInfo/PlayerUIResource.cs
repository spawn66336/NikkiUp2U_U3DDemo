using UnityEngine;
using System.Collections;
using System.Collections.Generic;

//背包物品
public class BagItemUIInfo
{
    public GameItem item;
    public int count;
}

public class PlayerLevelUIInfo
{
    public LevelUIInfo levelInfo;
    public LevelState state;
    public string lockReason;
}

public class PlayerUIResource : UIResourceManager 
{
    public override void Init()
    { 
    }

    //玩家金币数
    public int Gold
    {
        get { return playerInfo.gold; }
        set { playerInfo.gold = value; }
    }

    //玩家能量
    public int Energy
    {
        get { return playerInfo.energy; }
        set { playerInfo.energy = value; }
    }

    //玩家钻石
    public int Diamond
    {
        get { return playerInfo.diamond; }
        set { playerInfo.diamond = value; }
    }
    
    //当前关卡id 
    public int CurrLevelId
    {
        get { return playerInfo.currLevelId; }
        set 
        { 
            playerInfo.currLevelId = value;
            PlayerModule playerModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<PlayerModule>();
            playerModule.ChangeCurrLevel(playerInfo.currAreaMapId, playerInfo.currLevelId, null);
        }
    }

    //玩家当前关卡信息
    public PlayerLevelUIInfo CurrLevelUIInfo
    {
        get 
        {
            foreach( var level in playerCurrAreaMapLevelUIInfos )
            {
                if( level.levelInfo.id == CurrLevelId )
                {
                    return level;
                }
            }
            return null; 
        }
    }
     
    //获取当前地图信息
    public AreaMapUIInfo CurrAreaMapUIInfo
    {
        get { return currAreaMapUIInfo; }
    }
 
    //获取当前地图所有关卡信息  
    public List<PlayerLevelUIInfo> CurrAreaMapLevelUIInfos
    {
        get { return playerCurrAreaMapLevelUIInfos; }
    }
    public int CurrentMapLevelIndex=0;
   
    //获取背包信息
    public List<BagItemUIInfo> BagItemUIInfos
    {
        get { return bagItemUIInfos; }
    }
     
    
	public override IEnumerator Sync()
    {
        PlayerModule playerModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<PlayerModule>();
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

        foreach( var bagItem in bagItems )
        {
            GameItemUIResourceManager.GetInstance().TryGetGameItem(bagItem.itemId);
        }

        //等待背包物品数据配置完成
        IEnumerator syncEnumator = GameItemUIResourceManager.GetInstance().Sync();
        while( syncEnumator.MoveNext() )
        {
            yield return 0;
        }

        //更新背包UI信息
        bagItemUIInfos.Clear();
        foreach (var bagItem in bagItems)
        {
            GameItem item =  GameItemUIResourceManager.GetInstance().TryGetGameItem(bagItem.itemId);
            BagItemUIInfo itemUIInfo = new BagItemUIInfo();
            itemUIInfo.item = item;
            itemUIInfo.count = bagItem.itemCount;
            bagItemUIInfos.Add(itemUIInfo);
        }

        //获取当前关卡数据
        var levelUIInfo = LevelUIResourceManager.GetInstance().TryGetLevelInfo(CurrLevelId);
        if (levelUIInfo == null)
        {
            syncEnumator = LevelUIResourceManager.GetInstance().Sync();
            while (syncEnumator.MoveNext())
            {
                yield return 0;
            }

            levelUIInfo = LevelUIResourceManager.GetInstance().TryGetLevelInfo(CurrLevelId);
        }

        if( levelUIInfo == null )
        {//关卡信息获取失败！
            currAreaMapId = 1; 
        }
        else
        {
            currAreaMapId = levelUIInfo.areaMapId;
        }
         
        ////获得当前区域地图信息
        
        currAreaMapUIInfo = AreaMapUIResourceManager.GetInstance().TryGetAreaMapUIInfo(currAreaMapId);
        if (currAreaMapUIInfo == null)
        {
            syncEnumator = AreaMapUIResourceManager.GetInstance().Sync();
            while (syncEnumator.MoveNext())
            {
                yield return 0;
            }
            currAreaMapUIInfo = AreaMapUIResourceManager.GetInstance().TryGetAreaMapUIInfo(currAreaMapId);
        }
        
        if( currAreaMapUIInfo == null )
        {//区域地图获取失败
            yield break;
        }

        ////更新当前区域关卡信息
        playerCurrAreaMapLevelUIInfos.Clear();
        if (currAreaMapUIInfo != null)
        {//更新当前区域地图所有关卡信息 
            foreach (var levelId in currAreaMapUIInfo.levels)
            {
                LevelUIResourceManager.GetInstance().TryGetLevelInfo(levelId);
            }

            syncEnumator = LevelUIResourceManager.GetInstance().Sync();
            while (syncEnumator.MoveNext())
            {
                yield return 0;
            }

            foreach (var levelId in currAreaMapUIInfo.levels)
            {
                levelUIInfo = LevelUIResourceManager.GetInstance().TryGetLevelInfo(levelId);
                if (levelUIInfo != null)
                {
                    PlayerLevelUIInfo playerLevelUIInfo = new PlayerLevelUIInfo();
                    playerLevelUIInfo.levelInfo = levelUIInfo;

                    var recordList = playerInfo.levelRecordList;
                    foreach (var levelRecord in recordList)
                    {
                        if (levelRecord.levelId == levelId)
                        {
                            playerLevelUIInfo.state = levelRecord.state;
                            playerLevelUIInfo.lockReason = levelRecord.lockReason;
                            break;
                        }
                    }
                    playerCurrAreaMapLevelUIInfos.Add(playerLevelUIInfo);
                }
                else
                {
                    Debug.LogError("关卡" + levelId + "不存在！");
                }  
            }
        } 
    }
     
    void _KickForceSync()
    {
        GlobalObjects.GetInstance().ShowLoadingPanel(true);
        GlobalObjects.GetInstance().GetCoroutineManager().StartCoroutine(_ForceSync());
    }

    IEnumerator _ForceSync()
    {
        var syncEnumator = Sync();
        while( syncEnumator.MoveNext() )
        {
            yield return 0;
        }

        _OnForceSyncFinshed();
    }

    void _OnForceSyncFinshed()
    {
        GlobalObjects.GetInstance().ShowLoadingPanel(false);
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
    int currAreaMapId = 0;
    List<BagItemInfo> bagItems = new List<BagItemInfo>();

    //当前玩家信息
    PlayerInfo playerInfo = new PlayerInfo();

    //当前背包物品UI信息
    List<BagItemUIInfo> bagItemUIInfos = new List<BagItemUIInfo>(); 
    //当前区域地图UI信息
    AreaMapUIInfo currAreaMapUIInfo = new AreaMapUIInfo();
    //当前区域关卡UI信息
    List<PlayerLevelUIInfo> playerCurrAreaMapLevelUIInfos = new List<PlayerLevelUIInfo>();

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
