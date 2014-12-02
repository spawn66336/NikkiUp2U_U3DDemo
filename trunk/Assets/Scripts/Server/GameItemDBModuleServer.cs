using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameItemDBModuleServer : ModuleServer 
{


    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_GameItemInfoById)
        {
            GameItemInfoByIdRequestMsg requestMsg = (GameItemInfoByIdRequestMsg)request.msg;
            int itemId = requestMsg.itemId;
            GameItemInfo info = new GameItemInfo();
            foreach (GameItemDataBaseBean bean in GameDataBaseManager.getInstance().beanList)
            {
                if (bean.id == itemId)
                {
                    info.id = bean.id;
                    info.name = bean.name;
                    info.type = bean.type;
                    info.rareness = bean.rareness;
                    info.desc = bean.desc;
                    info.dressType = bean.dressType;
                    info.iconPath = bean.iconPath;
                    info.imgs.AddRange(bean.imgs);
                    info.showPos = bean.showPosVector2;
                    info.priceGold = bean.priceGold;
                    info.priceDiamond = bean.priceDiamond;
                }
            }

            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = info;
            ReplyToClient(replyMsg);
        }

    }
	 
}
