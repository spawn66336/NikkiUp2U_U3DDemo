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
    [XmlAttribute("ItemType")]
    //物品类型
    public GameItemType type;
    //物品稀有度
    [XmlAttribute("Rareness")]
    public int rareness;
    //物品描述
    [XmlAttribute("Desc")]
    public string desc;
    //bodypart
    [XmlAttribute("DressType")]
    public DressType dressType;
    //物品基础分
    [XmlAttribute("Score")]
    public int score;
    //图标路径
    [XmlIgnore]
    public string iconPath;
    //物品图片路径
    [XmlIgnore]
    public List<string> imgs = new List<string>();
    [XmlAttribute("ShowPoition")]
    public string showPos;
    //衣服显示位置
    [XmlIgnore]
    public Vector2 showPosVector2;
    ////衣服显示缩放比例
    //[XmlAttribute("ShowScale", typeof(string))]
    //public Vector2 showScale = Vector2.one;
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
    [XmlIgnore]
    public List<Attribute> attStyleList = new List<Attribute>();
    [XmlIgnore]
    public List<Attribute> attMatrialList = new List<Attribute>();
}

[XmlRoot("Attribute")]
public class Attribute{
    [XmlAttribute("Type")]
    public  int type;
    [XmlAttribute("Id")]
    public int id;
}

