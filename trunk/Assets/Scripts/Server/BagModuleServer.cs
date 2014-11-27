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
            replyMsg.serial = request.serial;
            replyMsg.resultObject = bagItemList;
            ReplyToClient(replyMsg);
        }
    }

    public int getDressCount()
    {
        int count = 0;
        foreach (BagItemInfo info in bagItemList)
        {
            GameItemDataBaseBean item = FakeServer.GetInstance().getGameItemDBServer().getGameItemBean(info.itemId);
            if (item.type == GameItemType.Dress)
            {
                count += info.itemCount;
            }
        }
        return count;
    }

    public int getDressCount4DType(int dressType)
    {
        int count = 0;
        foreach (BagItemInfo info in bagItemList)
        {
            GameItemDataBaseBean item = FakeServer.GetInstance().getGameItemDBServer().getGameItemBean(info.itemId);
            if (item.type == GameItemType.Dress && (int)item.dressType == dressType)
            {
                count += info.itemCount;
            }
        }
        return count;
    }

    public void addItemToBag(int itemId, int count)
    {
        bool flag = false;
        foreach (BagItemInfo info in bagItemList)
        {
            if (info.itemId == itemId)
            {
                info.itemCount += count;
                flag = true;
            }
        }
        if (!flag)
        {
            BagItemInfo newInfo = new BagItemInfo();
            newInfo.itemId = itemId;
            newInfo.itemCount = count;
        }

        GameItemDataBaseBean itemBean = FakeServer.GetInstance().getGameItemDBServer().getGameItemBean(itemId);
        if (itemBean.type == GameItemType.Dress)
        {
            // todo 
        }
        
    }
}
