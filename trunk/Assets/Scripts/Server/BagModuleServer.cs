using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BagModuleServer : ModuleServer 
{
   
    public override void Init()
    {
        base.Init();
        
    }
    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_GetBagItemInfos)
        {
            BagItemInfosRequestMsg msg = (BagItemInfosRequestMsg)request.msg;
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = BagInfoManager.getInstance().BagItemList;
            ReplyToClient(replyMsg);
        }
    }

    
}
