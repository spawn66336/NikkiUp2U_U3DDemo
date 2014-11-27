using UnityEngine;
using System.Collections;

public class PlayerModule : GameLogicModule , IPlayer
{
    public void GetPlayerInfo(GetPlayerInfoCallback callback)
    {
        PlayerInfoRequestMsg msg = new PlayerInfoRequestMsg();
        SendRequest(msg, callback);
    }

    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        if (result == RequestReplyResult.Error)
        {
            PlayerInfo info = (PlayerInfo)reply.resultObject;
            GetPlayerInfoCallback callBack = (GetPlayerInfoCallback)param;
            callBack(info);
        }
    }
}

public class PlayerInfoRequestMsg : RequestMessage
{
    public override int Message
    {
        get
        {
            return (int)RequestMessageDef.Request_PlayerInfo;
        }
    }
}