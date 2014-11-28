using UnityEngine;
using System.Collections;

public class PlayerModule : GameLogicModule , IPlayer
{
    public void SetPlayerCurrLevelId(int id)
    {

    }

    public void GetPlayerInfo(GetPlayerInfoCallback callback)
    {
        PlayerInfoRequestMsg msg = new PlayerInfoRequestMsg();
        SendRequest(msg, callback);
    }

    public void ChangeCurrLevel(int mapId,int levelId,ChangeCurrentLevelCallBack callback)
    {
        ChangeCurrLevelRequestMsg msg = new ChangeCurrLevelRequestMsg();
        msg.currLevelId = levelId;
        msg.currMapId = mapId;
        SendRequest(msg, callback);
    }
    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        if (result == RequestReplyResult.Ok)
        {
            if (reply.messageType == RequestMessageDef.Request_PlayerInfo)
            {
                PlayerInfo info = (PlayerInfo)reply.resultObject;
                if (param == null)
                {
                    return;
                }
                GetPlayerInfoCallback callBack = (GetPlayerInfoCallback)param;
                callBack(info);
            }
            else if (reply.messageType == RequestMessageDef.Request_ChangeCurrLevel)
            {
                PlayerInfo info = (PlayerInfo)reply.resultObject;
                if (param == null)
                {
                    return;
                }
                ChangeCurrentLevelCallBack callBack = (ChangeCurrentLevelCallBack)param;
                callBack(info);
            }
            
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

public class ChangeCurrLevelRequestMsg : RequestMessage
{
    public override int Message
    {
        get
        {
            return (int)RequestMessageDef.Request_ChangeCurrLevel ;
        }
    }

    public int currMapId;
    public int currLevelId;
}