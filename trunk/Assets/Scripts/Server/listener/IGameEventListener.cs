using UnityEngine;
using System.Collections;
public class IGameEventListener
{

    public virtual void handlerEvent(IEventInterface eventBean){}
    public virtual void registeredEvent() { }

    public virtual string getName()
    {
        return "";
    }
}
