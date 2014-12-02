using UnityEngine;
using System.Collections;

public class LevelCollectTypeClothEvent : IEventInterface
{

    public override GameEventType eventType
    {
        get
        {
            return GameEventType.Event_CollectTypeCloth;
        }
    }
    public int type;
    public int num;
}
