using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum LevelState
{
    [XmlEnum("0")]
    Invisible = 0,
    [XmlEnum("1")]
    Locked,
    [XmlEnum("2")]
    Unlocked,
    [XmlEnum("3")]
    Finished
}

//衣服搭配信息
[XmlRoot("DressSet")]
public class DressSetInfo
{
    [XmlArray("Dress"), XmlArrayItem("ItemId")]
    public List<int> dressList = new List<int>();
}


public class PlayerAreaMapRecordInfo
{
    //区域地图ID
    public int areaMapId;
    //区域地图是否锁定
    public bool isLocked;
    //区域地图如果锁定，锁定原因
    public string lockReason;
}

//玩家关卡记录信息
[XmlRoot("PlayerLevelRecortdInfo")]
public class PlayerLevelRecordInfo
{
    //记录所对应的关卡id
    [XmlAttribute("LevelId")]
    public int levelId;
    //关卡状态
    [XmlAttribute("State")]
    public LevelState state;

    //如果关卡锁定，则锁定原因
    [XmlIgnore]
    public string lockReason;

    //此关卡最高分
    [XmlAttribute("HighestScore")]
    public int highestScore;
    //此关卡最高评级
    [XmlAttribute("HighestRank")]
    public LevelRank highestRank;
    //最高分衣服搭配
    [XmlElement("HighestDressSet", typeof(DressSetInfo))]
    public DressSetInfo highestDressSet = new DressSetInfo();
    //最近一次分数
    [XmlAttribute("LastestScore")]
    public int latestScore;
    //最近一次评级
    [XmlAttribute("LastestRank")]
    public LevelRank latestRank;
    //最近一次衣服搭配
    [XmlElement("LastestDressSet", typeof(DressSetInfo))]
    public DressSetInfo latestDressSet = new DressSetInfo();
}

//当前玩家信息
[XmlRoot("PlayerInfo")]
public class PlayerInfo
{ 
    //当前金币数
    [XmlAttribute("Gold")]
    public int gold;

    //当前能量
    [XmlAttribute("Energy")]
    public int energy;

    //当前钻石
    [XmlAttribute("Diamond")]
    public int diamond;
     
    //当前关卡id
    [XmlAttribute("CurrLevelId")]
    public int currLevelId;

    //当前地图id
    [XmlIgnore]
    public int currAreaMapId;
    //玩家关卡记录列表
    [XmlArray("LevelRecordList"), XmlArrayItem("LevelRecordInfo", typeof(PlayerLevelRecordInfo))]
    public List<PlayerLevelRecordInfo> levelRecordList
        = new List<PlayerLevelRecordInfo>();

    //玩家的区域地图记录信息
    [XmlIgnore]
    public List<PlayerAreaMapRecordInfo> areaMapRecordList = new List<PlayerAreaMapRecordInfo>();
}

public delegate void GetPlayerInfoCallback( PlayerInfo info );

public interface IPlayer 
{

    

    void GetPlayerInfo( GetPlayerInfoCallback callback ); 
}
