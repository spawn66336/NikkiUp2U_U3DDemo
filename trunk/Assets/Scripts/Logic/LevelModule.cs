using UnityEngine;
using System.Collections;

public class LevelModule : GameLogicModule , ILevel
{

    //获取关卡信息
    public void GetLevelInfo(int id, GetLevelInfoCallback callback) 
	{
		RequestLevelInfoMessage req = new RequestLevelInfoMessage();
		req.id = id;
		SendRequest(req, callback);
	}

	//响应
	public override void HandleReply( RequestReplyResult result, ReplyMessage reply, object param)
	{
		if(param is GetLevelInfoCallback)
		{
			LevelInfo levelInfo = reply.resultObject as LevelInfo;
			((GetLevelInfoCallback)param)(levelInfo);
		}
	}
}

//获取指定id的关卡信息
public class RequestLevelInfoMessage : RequestMessage
{
	public int id;
	public override int Message
	{
		get { return (int)RequestMessageDef.Request_GetLevelInfo; }
	}
}
