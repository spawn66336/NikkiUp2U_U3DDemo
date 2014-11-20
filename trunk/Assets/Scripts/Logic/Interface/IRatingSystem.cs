using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum LevelRank
{
    F = 0,
    D,
    C,
    B,
    A,
    S,
    SS,
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
   public LevelRank rank;
   //此关的奖品信息
   public List<LevelRewardInfo> rewards = 
       new List<LevelRewardInfo>();
}

public delegate void RatingResultCallback( RatingInfo info );

public interface IRatingSystem
{
    void Rate(int id, DressSetInfo dressSet, RatingResultCallback callback);
}
