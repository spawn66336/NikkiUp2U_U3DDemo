using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaMapModule : GameLogicModule , IAreaMap
{
    public void GetAreaMapList(GetAreaMapIdListCallback callback) 
	{
		SendRequest(new RequestAreaMapIdListMessage(), callback);
	}

    public void GetAreaMapInfo(int id, GetAreaMapInfoCallback callback)
    {
		RequestAreaMapInfoMessage req = new RequestAreaMapInfoMessage();
        req.id = id;
        SendRequest(req, callback);
    }
    //响应
	public override void HandleReply( RequestReplyResult result, ReplyMessage reply, object param)
	{
		if(param is GetAreaMapIdListCallback)
		{
			List<int> mapIdList = reply.resultObject as System.Collections.Generic.List<int>;
        	((GetAreaMapIdListCallback)param)(mapIdList);
		}else if(param is GetAreaMapInfoCallback)
		{
			AreaMapInfo info = reply.resultObject as AreaMapInfo;
			((GetAreaMapInfoCallback)param)(info);
		}
	}
}

//获取区域地图id列表
public class RequestAreaMapIdListMessage : RequestMessage
{
	public override int Message
	{
		get { return (int)RequestMessageDef.Request_AreaMapIdList; }
	}
}
//获取指定id的区域地图详细信息
public class RequestAreaMapInfoMessage : RequestMessage
{
	public int id;
	public override int Message
	{
		get { return (int)RequestMessageDef.Request_AreaMapInfo; }
	}
}
