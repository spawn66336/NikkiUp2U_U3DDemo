using UnityEngine;
using System.Collections;

public class LevelModuleServer : ModuleServer 
{
	public override void Init()
	{
		base.Init();
	}
	public override void HandleRequest( ServerRequestMessage request )
	{
		//获取区域地图id列表
		if (request.msg.Message == (int)RequestMessageDef.Request_GetLevelInfo)
		{
			int id = ((RequestLevelInfoMessage)request.msg).id;
			LevelInfo levInfo = FakeServer.GetInstance().GetAreaMapModuleServer().getInfo(id);
			ServerReplyMessage rpl = new ServerReplyMessage();
			rpl.serial = request.serial;
			rpl.resultObject = levInfo;
			ReplyToClient(rpl);
		}
	}
	//由关卡Id返回所在地图信息
}
