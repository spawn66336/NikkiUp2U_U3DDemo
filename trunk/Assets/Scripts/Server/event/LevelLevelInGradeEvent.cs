using UnityEngine;
using System.Collections;

public class LevelLevelInGradeEvent : IEventInterface
{

    public override GameEventType eventType
    {
        get
        {
            return GameEventType.Event_LevelInGrade;
        }
    }

    public int levelId;
    public int levelRank;
}
