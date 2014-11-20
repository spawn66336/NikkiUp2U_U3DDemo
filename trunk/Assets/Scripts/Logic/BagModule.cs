using UnityEngine;
using System.Collections;

public class BagModule : GameLogicModule , IBag
{

    public void GetBagItemInfos(GetBagItemInfoCallback callback) { }

    public void GetBagItemInfosRange(int start, int count, GetBagItemInfoCallback callback) { }
	 
}
