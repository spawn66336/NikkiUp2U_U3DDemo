using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum LevelRank
{
    [XmlEnum("0")]
    F = 0,
    [XmlEnum("1")]
    C,
    [XmlEnum("2")]
    B,
    [XmlEnum("3")]
    A,
    [XmlEnum("4")]
    S,
    [XmlEnum("5")]
    SS,
    [XmlEnum("6")]
    SSS
}

public class LevelRewardInfo
{
    //奖品id
    public int itemId;
    //奖品数量
    public int itemCount;
}

public class RatingInfo
{
   //评分
   public int score;
   //关卡评级
   public LevelRank levelRank;
   //通关评语
   public string comment;
   //此关的奖品信息
   public List<LevelRewardInfo> rewards = 
       new List<LevelRewardInfo>();
}

public delegate void RatingResultCallback( RatingInfo info );

public interface IRatingSystem
{
    void Rate(int id, DressSetInfo dressSet, RatingResultCallback callback);
}
