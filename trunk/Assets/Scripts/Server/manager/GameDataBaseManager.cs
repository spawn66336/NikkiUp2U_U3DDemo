using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GameDataBaseManager : IDataManager
{
    private static GameDataBaseManager _instance;
    public static GameDataBaseManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new GameDataBaseManager();
        }
        return _instance;
    }
    private static string fileName = "gameItemData.xml";
    public List<GameItemDataBaseBean> beanList = new List<GameItemDataBaseBean>();

    public override void init()
    {
        beanList = (List<GameItemDataBaseBean>)XMLTools.readXml(fileName, beanList.GetType());
        foreach (GameItemDataBaseBean bean in beanList)
        {
            string str = bean.showPos.Substring(1, (bean.showPos.Length - 2));
            string[] strs = str.Split(',');
            bean.showPosVector2.Set(float.Parse(strs[0]), float.Parse(strs[1]));
        }
    }
    public GameItemDataBaseBean getGameItemBean(int itemId)
    {
        foreach (GameItemDataBaseBean bean in beanList)
        {
            if (itemId == bean.id)
            {
                return bean;
            }
        }
        return null;
    }
}
