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

public class PlayerLevelRewardUIInfo
{
    public int score;
    public LevelRank levelRank;
    public string comment;
    public List<BagItemUIInfo> rewards = new List<BagItemUIInfo>();
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

    //玩家最大能量值
    public int MaxEnergy
    {
        get { return 30; }
    }
     

    //玩家钻石
    public int Diamond
    {
        get { return playerInfo.diamond; }
        set { playerInfo.diamond = value; }
    }

    //当前区域地图S及以上评级的关卡数量
    public int CurrAreaMapRankSLevelCount
    {
        get { return currAreaMapRankSLevelCount; }
    }

    //当前区域地图通过关卡数量
    public int CurrAreaMapFinishedLevelCount
    {
        get { return currAreaMapFinishedLevelCount; }
    }

    //当前区域地图关卡记录表（key:关卡索引，value:玩家记录）
    public Dictionary<int,PlayerLevelRecordInfo> 
        CurrAreaMapLevelRecordTable
    {
        get { return currAreaMapLevelRecordTable; }
    }

    //当前区域地图索引(建议使用)
    public int CurrAreaMapIndex
    {
        get 
        { 
            return currAreaMapIndex; 
        }

        set
        {
            if( value >= 0 && value < areaMapIdList.Count )
            {
                currAreaMapIndex = value;
                CurrLevelIndex = 0;
                _UpdateCurrAreaMapInfos();
            }
        }
    }

    //相对于当前地图的关卡索引（建议使用）
    public int CurrLevelIndex
    {
        get
        { 
            int id = CurrLevelId;
            int indx = 0;

            if( CurrAreaMapUIInfo.levels == null )
            {
                return 0;
            }

            foreach( var levelId in CurrAreaMapUIInfo.levels )
            {
                if( levelId == id )
                {
                    return indx;
                }
                indx++;
            } 
            return 0;
        }

        set
        {
            if (CurrAreaMapUIInfo.levels == null)
            {
                return;
            }

            if( value < 0 || 
                value >= CurrAreaMapUIInfo.levels.Length )
            {
                return;
            } 
            CurrLevelId = CurrAreaMapUIInfo.levels[value];
        }
    }


    //当前关卡id 
    private int CurrLevelId
    {
        get { return playerInfo.currLevelId; }
        set
        {
            playerInfo.currLevelId = value;
            PlayerModule playerModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<PlayerModule>();
            playerModule.ChangeCurrLevel(playerInfo.currAreaMapId, playerInfo.currLevelId, null);
        }
    }


    public PlayerLevelRewardUIInfo CurrRaingUIInfo
    {
        get { return currLevelRatingUIInfo; }
    }

    //当前关卡评分信息
    public RatingInfo CurrRatingInfo
    { 
        set { ratingInfo = value; }
    }

    //玩家当前关卡信息
    public PlayerLevelUIInfo CurrLevelUIInfo
    {
        get 
        {
            var currAreaMapLevels = CurrAreaMapLevelUIInfos;
            return currAreaMapLevels[CurrLevelIndex];
        }
    }
     
    //获取当前地图信息
    public AreaMapUIInfo CurrAreaMapUIInfo
    {
        get 
        {
            return areaMapUIInfos[CurrAreaMapIndex]; 
        }
    }
 
    //获取当前地图所有关卡信息  
    public List<PlayerLevelUIInfo> CurrAreaMapLevelUIInfos
    {
        get 
        {
            if( !levelInfos.ContainsKey(CurrAreaMapIndex) )
            {
                return null;
            }

            return levelInfos[CurrAreaMapIndex];  
        }
    }

   
    //获取背包信息
    public List<BagItemUIInfo> BagItemUIInfos
    {
        get { return bagItemUIInfos; }
    }
     
    public void DressFinished( DressSetInfo dressSet , RatingResultCallback callback )
    {
        RatingSystemModule ratingSysModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<RatingSystemModule>();
        ratingSysModule.Rate(CurrLevelId, dressSet, callback);
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


        //更新区域地图列表
        syncEnumator = AreaMapUIResourceManager.GetInstance().Sync();
        while( syncEnumator.MoveNext() )
        {
            yield return 0;
        }

        //获得区域地图id列表
        areaMapIdList = AreaMapUIResourceManager.GetInstance().GetAreaMapIdList(); 
        foreach ( var areaMapId in areaMapIdList )
        {
            AreaMapUIResourceManager.GetInstance().TryGetAreaMapUIInfo(areaMapId);
        }

        syncEnumator = AreaMapUIResourceManager.GetInstance().Sync();
        while (syncEnumator.MoveNext())
        {
            yield return 0;
        }

        //获取所有区域地图UI信息
        areaMapUIInfos.Clear();
        foreach( var areaMapId in areaMapIdList )
        {
            var areaMapUIInfo = AreaMapUIResourceManager.GetInstance().TryGetAreaMapUIInfo(areaMapId); 
            if( areaMapUIInfo != null )
            {
                areaMapUIInfos.Add(areaMapUIInfo);
            }
        }

        //更新所有关卡信息
        levelInfos.Clear(); 
        foreach( var areaMapUIInfo in areaMapUIInfos )
        {
            foreach( var levelId in areaMapUIInfo.levels )
            {
                LevelUIResourceManager.GetInstance().TryGetLevelInfo(levelId);
            }
        }

        syncEnumator = LevelUIResourceManager.GetInstance().Sync();
        while (syncEnumator.MoveNext())
        {
            yield return 0;
        }

        int mapIndex = 0;
        foreach (var areaMapUIInfo in areaMapUIInfos)
        {
            if( !levelInfos.ContainsKey(mapIndex) )
            {
                levelInfos.Add(mapIndex, new List<PlayerLevelUIInfo>());
            }

            List<PlayerLevelUIInfo> areaMapLevelUIInfoList;
            levelInfos.TryGetValue(mapIndex, out areaMapLevelUIInfoList);

            foreach (var levelId in areaMapUIInfo.levels)
            {
                LevelUIInfo levelUIInfo = LevelUIResourceManager.GetInstance().TryGetLevelInfo(levelId);
                if (levelUIInfo != null)
                {
                    PlayerLevelUIInfo playerLevelUIInfo = new PlayerLevelUIInfo();
                    playerLevelUIInfo.levelInfo = levelUIInfo;
                    playerLevelUIInfo.state = LevelState.Invisible;

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
                    areaMapLevelUIInfoList.Add(playerLevelUIInfo);
                }
                else
                {
                    Debug.LogError("关卡" + levelId + "不存在！");
                }
            }
            mapIndex++;
        }

        _UpdateAreaMapIndex();

        _UpdateCurrAreaMapInfos();

    }

    //同步奖励信息
    public IEnumerator SyncReward()
    {
        if( ratingInfo != null )
        {
            currLevelRatingUIInfo = new PlayerLevelRewardUIInfo();
            currLevelRatingUIInfo.score = ratingInfo.score;
            currLevelRatingUIInfo.levelRank = ratingInfo.levelRank;
            currLevelRatingUIInfo.comment = ratingInfo.comment;

            foreach (var reward in ratingInfo.rewards )
            { 
                GameItemUIResourceManager.GetInstance().TryGetGameItem(reward.itemId);
            }

            //等待物品更新完成
            IEnumerator syncEnumator = GameItemUIResourceManager.GetInstance().Sync();
            while (syncEnumator.MoveNext())
            {
                yield return 0;
            }
             
            foreach (var reward in ratingInfo.rewards)
            {
                GameItem item = GameItemUIResourceManager.GetInstance().TryGetGameItem(reward.itemId);
                BagItemUIInfo itemUIInfo = new BagItemUIInfo();
                itemUIInfo.item = item;
                itemUIInfo.count = reward.itemCount;
                currLevelRatingUIInfo.rewards.Add(itemUIInfo); 
            } 
        }
        yield return 0;
    }

    void _UpdateAreaMapIndex()
    {
        //根据当前关卡id推断区域地图index
        int mapIndex = 0;
        foreach (var areaMapUIInfo in areaMapUIInfos)
        {
            foreach (var levelId in areaMapUIInfo.levels)
            {
                if (levelId == CurrLevelId)
                {
                    currAreaMapIndex = mapIndex;
                    return;
                }
            }
            mapIndex++;
        }
        currAreaMapIndex = 0;
    }

    void _UpdateCurrAreaMapInfos()
    {
        currAreaMapRankSLevelCount = 0;
        currAreaMapFinishedLevelCount = 0;
        currAreaMapLevelRecordTable.Clear();

        AreaMapUIInfo currAreaMap = CurrAreaMapUIInfo;

        if (currAreaMap != null)
        {
            int levelIndx = 0;
            foreach (var levelId in currAreaMap.levels)
            {
                foreach( var levelInfo in playerInfo.levelRecordList )
                {
                    //若在玩家信息中找到当前关卡信息
                    if( levelInfo.levelId == levelId )
                    {
                        if( levelInfo.state == LevelState.Finished )
                        {
                            currAreaMapFinishedLevelCount++;
                            if( levelInfo.highestRank >= LevelRank.S )
                            {
                                currAreaMapRankSLevelCount++;
                            }
                        } 
                        currAreaMapLevelRecordTable.Add(levelIndx, levelInfo);
                    }
                } 
                levelIndx++;
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

    int syncCount = 0;

    //当前玩家信息
    PlayerInfo playerInfo = new PlayerInfo(); 
    //背包物品信息列表
    List<BagItemInfo> bagItems = new List<BagItemInfo>(); 
    //当前背包物品UI信息列表
    List<BagItemUIInfo> bagItemUIInfos = new List<BagItemUIInfo>();
    //当前关卡通关奖励信息
    PlayerLevelRewardUIInfo currLevelRatingUIInfo;

    //当前关卡通关奖励
    RatingInfo ratingInfo;

    //当前地图S及以上评级的关卡数
    int currAreaMapRankSLevelCount;
    //当前地图通过关卡数
    int currAreaMapFinishedLevelCount;
    //当前区域地图关卡记录表
    Dictionary<int, PlayerLevelRecordInfo> currAreaMapLevelRecordTable = new Dictionary<int,PlayerLevelRecordInfo>();

    //当前区域地图索引
    int currAreaMapIndex = 0; 
    //区域地图id列表
    List<int> areaMapIdList = new List<int>();
    //区域地图UI信息列表
    List<AreaMapUIInfo> areaMapUIInfos = new List<AreaMapUIInfo>();
    

    //所有关卡信息
    Dictionary<int, List<PlayerLevelUIInfo>> levelInfos = new Dictionary<int, List<PlayerLevelUIInfo>>();
     

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
