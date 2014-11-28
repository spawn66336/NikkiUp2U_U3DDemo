using UnityEngine;
using System.Collections;

public class RatingSystemModule : GameLogicModule , IRatingSystem
{
    public void Rate(int id, DressSetInfo dressSet, RatingResultCallback callback) {
        RatingRequestMsg msg = new RatingRequestMsg();
        msg.dressSet = dressSet;
        msg.levelId = id;
        SendRequest(msg, callback);
    }

    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        if (result == RequestReplyResult.Ok)
        {
            if (reply.messageType == RequestMessageDef.Request_Rating)
            {
                RatingInfo info = (RatingInfo)reply.resultObject;
                RatingResultCallback callBack = (RatingResultCallback)param;
                callBack(info);
            }
        }
    }
}

public class RatingRequestMsg : RequestMessage
{
    public override int Message
    {
        get
        {
            return (int)RequestMessageDef.Request_Rating;
        }
    }

    public int levelId;
    public DressSetInfo dressSet;
}
