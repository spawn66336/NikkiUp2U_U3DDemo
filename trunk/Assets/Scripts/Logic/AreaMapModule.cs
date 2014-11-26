using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AreaMapModule : GameLogicModule , IAreaMap
{
    //获取区域地图id列表
    public class RequestAreaMapIdList : RequestMessage
    {
        public override int Message
        {
            get { return (int)RequestMessageDef.Request_AreaMapIdList; }
        }
    }
    public void GetAreaMapList(GetAreaMapIdListCallback callback) 
	{
		SendRequest(new RequestAreaMapIdList(), callback);
	}

    //获取指定id的区域地图详细信息
    public class RequestAreaMapInfo : RequestMessage
    {
        public int id;
        public override int Message
        {
            get { return (int)RequestMessageDef.Request_AreaMapInfo; }
        }
    }
    public void GetAreaMapInfo(int id, GetAreaMapInfoCallback callback)
    {
        RequestAreaMapInfo req = new RequestAreaMapInfo();
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
