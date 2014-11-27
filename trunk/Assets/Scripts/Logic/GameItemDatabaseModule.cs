using UnityEngine;
using System.Collections;

public class GameItemDatabaseModule : GameLogicModule , IGameItemDatabase
{
    public void GetGameItemInfoById(int id, GetGameItemInfoByIdCallback callback)
    {
        GameItemInfoByIdRequestMsg msg = new GameItemInfoByIdRequestMsg();
        msg.itemId = id;
        SendRequest(msg, callback);
    }

    public override void HandleReply(RequestReplyResult result, ReplyMessage reply, object param)
    {
        if (result == RequestReplyResult.Ok)
        {
            GameItemInfo info = (GameItemInfo)reply.resultObject;
            GetGameItemInfoByIdCallback callBack = (GetGameItemInfoByIdCallback)param;
            callBack(info);
        }
    }
    public void GetGameItemsByType(GameItemType type, GetGameItemInfosCallback callback) { }
}

public class GameItemInfoByIdRequestMsg : RequestMessage
{
    public override int Message
    {
        get
        {
            return (int)RequestMessageDef.Request_GameItemInfoById;
        }
    }

    public int itemId;
}