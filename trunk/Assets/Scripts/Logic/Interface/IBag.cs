using System.Xml.Serialization;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[XmlRoot("BagItemInfo")]
public class BagItemInfo
{
    [XmlAttribute("ItemId")]
    public int itemId;
    [XmlAttribute("ItemCount")]
    public int itemCount;
}

public delegate void GetBagItemInfoCallback( List<BagItemInfo> infos );

public interface IBag 
{
    
    void GetBagItemInfos( GetBagItemInfoCallback callback );
    
    void GetBagItemInfosRange(int start, int count, GetBagItemInfoCallback callback);
}
