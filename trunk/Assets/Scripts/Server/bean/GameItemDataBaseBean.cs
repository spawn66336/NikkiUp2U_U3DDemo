using System;
using UnityEngine;
using System.Xml.Serialization;
using System.Collections.Generic;
using System.Linq;
using System.Text;

[XmlRoot("GameItemData")]
public class GameItemDataBaseBean
{
    [XmlAttribute("ItemId")]
    //物品id
    public int id;
    [XmlAttribute("ItemName")]
    //物品名称
    public string name;
    [XmlAttribute("ItemType", typeof(int))]
    //物品类型
    public GameItemType type;
    //衣服类型
    [XmlAttribute("ItemDressType", typeof(int))]
    public DressType dressType;
    //物品评级
    [XmlAttribute("Rareness")]
    public int rareness;
    //物品描述
    [XmlAttribute("Desc")]
    public string desc;
    //bodypart
    [XmlAttribute("BodyPart")]
    public int bodypart;
    //图标路径
    [XmlIgnore]
    public string iconPath;
    //物品图片路径
    [XmlIgnore]
    public List<string> imgs = new List<string>();
    [XmlAttribute("ShowPoition",typeof(string))]
    //衣服显示位置
    public Vector2 showPos = Vector2.zero;
    //衣服显示缩放比例
    [XmlAttribute("ShowScale", typeof(string))]
    public Vector2 showScale = Vector2.one;
    //物品价格(金币)
    [XmlAttribute("PriceGold")]
    public int priceGold;
    //物品价格（钻石）
    [XmlAttribute("PriceDiamond")]
    public int priceDiamond;
    [XmlArray("StyleList"),XmlArrayItem("ratio")]
    public List<double> styleList = new List<double>();
    [XmlArray("AttributeList"),XmlArrayItem("Attribute")]
    public List<Attribute> attributeList = new List<Attribute>();
}

[XmlRoot("Attribute")]
public class Attribute{
    [XmlAttribute("Type")]
    public  int type;
    [XmlAttribute("Id")]
    public int id;
}

