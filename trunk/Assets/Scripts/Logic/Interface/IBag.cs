using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BagItemInfo
{
    public int itemId;
    public int itemCount;
}

public delegate void GetBagItemInfoCallback( List<BagItemInfo> infos );

public interface IBag 
{
    void GetBagItemInfos( GetBagItemInfoCallback callback );

    void GetBagItemInfosRange(int start, int count, GetBagItemInfoCallback callback);
}
