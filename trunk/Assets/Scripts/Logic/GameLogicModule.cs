using UnityEngine;
using System.Collections;

public class GameLogicModule 
{
    public virtual void Init() { }

    
    public void SendRequest( RequestMessage msg , object param )
    {
        RequestCenter.GetInstance().SendRequest( msg , HandleReply , param );
    }

    //需要后续各模块重写用于处理服务器回复消息（需要后续模块复写）
    public virtual void HandleReply( RequestReplyResult result, ReplyMessage reply, object param)
    {

    }

    
    public virtual void Update() { }
}
