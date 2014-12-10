using System;
using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class BagInfoManager : IDataManager
{
    private static BagInfoManager _instance;
    public static BagInfoManager getInstance()
    {
        if(_instance == null){
            _instance = new BagInfoManager();
        }
        return _instance;
    }
    public static string fileName = "bagItem.xml";
    List<BagItemInfo> bagItemList = new List<BagItemInfo>();
    public List<BagItemInfo> BagItemList
    {
        get
        {
            return bagItemList;
        }
    }
    public int totalColthCount = 0;
    public override void init()
    {
        int id=0;
        try
        {
            bagItemList = (List<BagItemInfo>)XMLTools.readXml(fileName, bagItemList.GetType());
            foreach (BagItemInfo info in bagItemList)
            {
                id=info.itemId;
                GameItemDataBaseBean bean = GameDataBaseManager.getInstance().getGameItemBean(info.itemId);
                if (bean.type == GameItemType.Dress)
                {
                    totalColthCount += info.itemCount;
                }
            }
        }
        catch
        {
            Debug.Log(id);
        }
        
    }
    public override void destroy()
    {
        bagItemList.Clear();
    }

    public int getDressCount()
    {
        int count = 0;
        foreach (BagItemInfo info in bagItemList)
        {
            GameItemDataBaseBean item = GameDataBaseManager.getInstance().getGameItemBean(info.itemId);
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
            GameItemDataBaseBean item = GameDataBaseManager.getInstance().getGameItemBean(info.itemId);
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
        int typenum = 0;
        foreach (BagItemInfo info in bagItemList)
        {
            if (info.itemId == itemId)
            {
                info.itemCount += count;
                typenum = info.itemCount;
                flag = true;
                break;
            }
        }
        if (!flag)
        {
            BagItemInfo newInfo = new BagItemInfo();
            newInfo.itemId = itemId;
            newInfo.itemCount = count;
            typenum = count;
        }

        GameItemDataBaseBean itemBean = GameDataBaseManager.getInstance().getGameItemBean(itemId);
        if (itemBean.type == GameItemType.Dress)
        {
            totalColthCount += count;
            EventCenter.onCollectColthForLevel(totalColthCount);
            EventCenter.onCollecTypeClothForLevel((int)itemBean.dressType, typenum);
        }

    }
}

