using UnityEngine;
using System.Collections;


//请求消息定义
public enum RequestMessageDef
{
    Request_Null = 0,
	Request_AreaMapIdList,
	Request_AreaMapInfo ,
    Request_GetBagItemInfos,
	Request_GetLevelInfo,
    Request_PlayerInfo,
    Request_GameItemInfoById,
    Request_ChangeCurrLevel,
    Request_Rating
}
