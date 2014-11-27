using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameItemDBModuleServer : ModuleServer 
{
    private static string fileName = "gameItemData.xml";
    public List<GameItemDataBaseBean> beanList = new List<GameItemDataBaseBean>();
    public override void Init()
    {
        beanList = (List<GameItemDataBaseBean>)XMLTools.readXml(fileName, beanList.GetType());
        foreach (GameItemDataBaseBean bean in beanList)
        {
            string str = bean.showPos.Substring(1, (bean.showPos.Length - 2));
            string[] strs = str.Split(',');
            bean.showPosVector2.Set(float.Parse(strs[0]), float.Parse(strs[1]));
        }
    }

    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_GameItemInfoById)
        {
            GameItemInfoByIdRequestMsg requestMsg = (GameItemInfoByIdRequestMsg)request.msg;
            int itemId = requestMsg.itemId;
            GameItemInfo info = new GameItemInfo();
            foreach (GameItemDataBaseBean bean in beanList)
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
    public GameItemDataBaseBean getGameItemBean(int itemId)
    {
        return new GameItemDataBaseBean();
    }
	 
}
