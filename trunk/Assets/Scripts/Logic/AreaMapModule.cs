using UnityEngine;
using System.Collections;

public class AreaMapModule : GameLogicModule , IAreaMap
{

    //获取区域地图id列表
    public void GetAreaMapList(GetAreaMapIdListCallback callback) { }

    //获取指定id的区域地图详细信息
    public void GetAreaMapInfo(int id, GetAreaMapInfoCallback callback) { }
	 
}
