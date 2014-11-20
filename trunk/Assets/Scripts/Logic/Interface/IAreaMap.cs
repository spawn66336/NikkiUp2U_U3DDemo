using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaMapInfo
{
    //区域地图Id
    public int id;
    //区域地图名
    public string name;
    //当前区域地图是否解锁（true表示已解锁）
    public bool isAvaliable; 
    //当前区域地图预览图标路径
    public string mapIconImgPath;
    //当前区域地图纹理路径
    public string[] mapImgPaths;
    //当前区域地图所对应关卡id列表
    public int[] levels;
    //如果当前区域地图未解锁，则用于显示的锁定原因
    public string lockReason;
}

//用于获取区域地图id列表回调
public delegate void GetAreaMapIdListCallback( List<int> mapIdList );

//用于获取某个区域地图详细信息
public delegate void GetAreaMapInfoCallback( AreaMapInfo info );

public interface IAreaMap 
{
    //获取区域地图id列表
    void GetAreaMapList(GetAreaMapIdListCallback callback );

    //获取指定id的区域地图详细信息
    void GetAreaMapInfo(int id , GetAreaMapInfoCallback callback );
}
