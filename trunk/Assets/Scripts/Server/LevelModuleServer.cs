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
		//获取关卡信息
		if (request.msg.Message == (int)RequestMessageDef.Request_GetLevelInfo)
		{
			int id = ((RequestLevelInfoMessage)request.msg).id;
			ServerReplyMessage rpl = new ServerReplyMessage();
			rpl.serial = request.serial;
			if(DataManager.GetInstance().LevelDic !=null && DataManager.GetInstance().LevelDic.ContainsKey(id))
				rpl.resultObject = DataManager.GetInstance().LevelDic[id];
			else
				rpl.resultObject = null;
			ReplyToClient(rpl);
		}
	}
}
