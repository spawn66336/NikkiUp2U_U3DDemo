using UnityEngine;
using System.Collections;

public class IEventInterface
{
    public virtual GameEventType eventType { 
         get{
             return GameEventType.Event_null;
         }
    }
}

public enum GameEventType{
    Event_null=0,
    Event_CollectColth,
    Event_CollectTypeCloth,
    Event_LevelInGrade
}