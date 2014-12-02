using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerModuleServer : ModuleServer 
{

    
    public override void Init()
    {
        base.Init();
        
    }

    
    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_PlayerInfo)
        {
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = PlayerInfoManager.getInstance().PlayerInfo;
            replyMsg.message = RequestMessageDef.Request_PlayerInfo;
            ReplyToClient(replyMsg);
        }
        else if (request.msg.Message == (int)RequestMessageDef.Request_ChangeCurrLevel)
        {
            ChangeCurrLevelRequestMsg msg = (ChangeCurrLevelRequestMsg)request.msg;
            PlayerInfoManager.getInstance().PlayerInfo.currLevelId = msg.currLevelId;
            PlayerInfoManager.getInstance().PlayerInfo.currAreaMapId = msg.currMapId;
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = PlayerInfoManager.getInstance().PlayerInfo;
            replyMsg.message = RequestMessageDef.Request_ChangeCurrLevel;
            ReplyToClient(replyMsg);
        }
    }
   

    

    
}


