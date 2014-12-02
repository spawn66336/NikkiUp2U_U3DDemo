using UnityEngine;
using System.Collections;

public class LevelCollectColthEvent : IEventInterface
{
    public override GameEventType eventType
    {
        get
        {
            return GameEventType.Event_CollectColth;
        }
    }
    public int clothNum;
}
