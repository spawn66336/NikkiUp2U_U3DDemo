using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GameItemType
{
    Dress = 1,
    Other
}

public enum DressType
{
    Hair = 1,
    Tops = 2,
    Coat = 3,
    Dress = 4,
    Bottoms = 5,
    Sock = 6,
    Shoe = 7,
    Acc = 8
}

public class GameItemInfo
{
    //物品id
    public int id;
    //物品类型
    public GameItemType type;
    //衣服类型
    public DressType dressType;
    //物品评级
    public int rank;
    //物品描述
    public string desc;
    //bodypart
    public int bodypart;
    //图标路径
    public string iconPath;
    //物品图片路径
    public List<string> imgs = new List<string>();
    //衣服显示位置
    public Vector2 showPos = Vector2.zero;
    //物品价格(金币)
    public int priceGold;
    //物品价格（钻石）
    public int priceDiamond;

}

public delegate void GetGameItemInfoByIdCallback( GameItemInfo info);

public delegate void GetGameItemInfosCallback( List<GameItemInfo> infos );

public interface IGameItemDatabase 
{
    void GetGameItemInfoById( int id , GetGameItemInfoByIdCallback callback );

    void GetGameItemsByType(GameItemType type, GetGameItemInfosCallback callback);
	 
}
