using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class EventEngine
{
    private Dictionary<GameEventType, Dictionary<String, IGameEventListener>> listenerMap = new Dictionary<GameEventType, Dictionary<string, IGameEventListener>>();


    public void addListener(GameEventType eventType,IGameEventListener listener)
    {
        Dictionary<String, IGameEventListener> map1=null;
        if (!listenerMap.ContainsKey(eventType))
        {
            map1 = new Dictionary<string, IGameEventListener>();
            listenerMap.Add(eventType, map1);
        }
        else
        {
            map1 = listenerMap[eventType];
        }
        map1.Add(listener.getName(), listener);
    }

    public void broadcastEvent(IEventInterface gameEvent){
        Dictionary<String, IGameEventListener> map1 = listenerMap[gameEvent.eventType];
        if(map1 == null){
            return ;
        }
        foreach(IGameEventListener listener in map1.Values){
            listener.handlerEvent(gameEvent);
        }
   }

    public static EventEngine GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new EventEngine();
        }
        return s_instance;
    }

    static EventEngine s_instance; 
}

