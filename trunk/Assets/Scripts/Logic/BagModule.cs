using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BagModule : GameLogicModule , IBag
{

    public void GetBagItemInfos(GetBagItemInfoCallback callback) {
        BagItemInfosRequestMsg reqMsg = new BagItemInfosRequestMsg();
        SendRequest(reqMsg, callback);
    }

    public void GetBagItemInfosRange(int start, int count, GetBagItemInfoCallback callback) { }

    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        if (result == RequestReplyResult.Ok)
        {
            List<BagItemInfo> infos = (List<BagItemInfo>)reply.resultObject;
            GetBagItemInfoCallback callBack = (GetBagItemInfoCallback)param;
            callBack(infos);
        }
        
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
