using UnityEngine;
using System.Collections;

public class EventCenter
{

    public static void onCollectColthForLevel(int num)
    {
        LevelCollectColthEvent levelEvent = new LevelCollectColthEvent();
        levelEvent.clothNum = num;
        EventEngine.GetInstance().broadcastEvent(levelEvent);
    }

    public static void onCollecTypeClothForLevel(int bodyIndex, int num)
    {
        LevelCollectTypeClothEvent levelEvent = new LevelCollectTypeClothEvent();
        levelEvent.type = bodyIndex;
        levelEvent.num = num;
        EventEngine.GetInstance().broadcastEvent(levelEvent);
    }

    public static void onLevelInGrade(int levelId, int rank)
    {
        LevelLevelInGradeEvent levelEvent = new LevelLevelInGradeEvent();
        levelEvent.levelId = levelId;
        levelEvent.levelRank = rank;
        EventEngine.GetInstance().broadcastEvent(levelEvent);
    }
}
