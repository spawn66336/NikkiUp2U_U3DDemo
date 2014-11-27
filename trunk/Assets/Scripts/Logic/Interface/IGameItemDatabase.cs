using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum GameItemType
{
    [XmlEnum("0")]
    Dress = 0,
    [XmlEnum("1")]
    Other
}

public enum DressType
{
    [XmlEnum("0")]
    Hair = 0,
    [XmlEnum("1")]
    Tops,
    [XmlEnum("2")]
    Coat,
    [XmlEnum("3")]
    Dress,
    [XmlEnum("4")]
    Bottoms,
    [XmlEnum("5")]
    Socks,
    [XmlEnum("6")]
    Shoes,
    [XmlEnum("7")]
    AccHead,
    [XmlEnum("8")]
    AccEar,
    [XmlEnum("9")]
    AccNeck,
    [XmlEnum("10")]
    AccHand,
    [XmlEnum("11")]
    AccWaist,
    [XmlEnum("12")]
    AccLeg,
    [XmlEnum("13")]
    AccSpecial,
    [XmlEnum("14")]
    AccFace,        //14
    [XmlEnum("15")]
    AccBag,
    [XmlIgnore]
    DressTypeLength
}
public class GameItemInfo
{
    //物品id
    public int id;
    //物品名称
    public string name;
    //物品类型
    public GameItemType type;
    //物品评级
    public int rareness;
    //物品描述
    public string desc;
    //bodypart
    public DressType dressType;
    //图标路径
    public string iconPath;
    //物品图片路径
    public List<string> imgs = new List<string>();
    //衣服显示位置
    public Vector2 showPos = Vector2.zero;
    //衣服显示缩放比例
    public Vector2 showScale = Vector2.one;
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
