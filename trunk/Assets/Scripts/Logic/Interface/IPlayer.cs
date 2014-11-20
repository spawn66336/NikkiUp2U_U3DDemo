using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelState
{
    Invisible = 0,
    Locked,
    Unlocked,
    Finished
}

//衣服搭配信息
public class DressSetInfo
{
    public List<int> dressList = new List<int>();
}

//玩家关卡记录信息
public class PlayerLevelRecordInfo
{
    //记录所对应的关卡id
    public int levelId;
    //关卡状态
    public LevelState state;
    //此关卡最高分
    public int highestScore;
    //此关卡最高评级
    public int highestRank;
    //最高分衣服搭配
    public DressSetInfo highestDressSet = new DressSetInfo();
    //最近一次分数
    public int latestScore;
    //最近一次评级
    public int latestRank;
    //最近一次衣服搭配
    public DressSetInfo latestDressSet = new DressSetInfo();
}

//当前玩家信息
public class PlayerInfo
{
    //当前金币数
    public int gold;
    //当前能量
    public int energy;
    //当前钻石
    public int diamond;
    //当前关卡id
    public int currLevelId;
    //玩家关卡记录列表
    public List<PlayerLevelRecordInfo> levelRecordList 
        = new List<PlayerLevelRecordInfo>();
}

public delegate void GetPlayerInfoCallback( PlayerInfo info );

public interface IPlayer 
{
    void GetPlayerInfo( GetPlayerInfoCallback callback ); 
}
