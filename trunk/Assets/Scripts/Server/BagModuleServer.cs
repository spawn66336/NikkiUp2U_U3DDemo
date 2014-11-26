using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class BagModuleServer : ModuleServer 
{
    public static string fileName = "bagItem.xml"; 
    private List<BagItemInfo> bagItemList = new List<BagItemInfo>();
    public override void Init()
    {
        base.Init();
        bagItemList = (List<BagItemInfo>)XMLTools.readXml(fileName, bagItemList.GetType());
    }
    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_GetBagItemInfos)
        {
            BagItemInfosRequestMsg msg = (BagItemInfosRequestMsg)request.msg;
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            ReplyToClient(replyMsg);
        }
    }
}
