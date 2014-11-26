using UnityEngine;
using System.Collections;

public class BagModule : GameLogicModule , IBag
{

    public void GetBagItemInfos(GetBagItemInfoCallback callback) {
        BagItemInfosRequestMsg reqMsg = new BagItemInfosRequestMsg();
        SendRequest(reqMsg, callback);
    }

    public void GetBagItemInfosRange(int start, int count, GetBagItemInfoCallback callback) { }

    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        base.HandleReply(result, reply, param);
        GetBagItemInfoCallback call = (GetBagItemInfoCallback)param;
        //call()
        
    }
}

public class BagItemInfosRequestMsg:RequestMessage
{
    public override int Message
    {
        get
        {
            return (int)RequestMessageDef.Request_GetBagItemInfos;
        }
    }
}
