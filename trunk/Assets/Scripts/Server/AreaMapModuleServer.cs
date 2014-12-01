using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;

public class AreaMapModuleServer : ModuleServer 
{

	public override void Init()
	{
		base.Init();
	}
	//处理请求
	public override void HandleRequest( ServerRequestMessage request )
	{
		//获取区域地图id列表
        if (request.msg.Message == (int)RequestMessageDef.Request_AreaMapIdList)
        {
			List<int> mapIdList = new List<int>();
			foreach(Map m in DataManager.GetInstance().AreaMapsData.Maps)
			{
				mapIdList.Add(m.Id);
			}
			ServerReplyMessage rpl = new ServerReplyMessage();
            rpl.serial = request.serial;
			rpl.resultObject = mapIdList;
            ReplyToClient(rpl);
        }
		//获取指定id的区域地图信息
        else if (request.msg.Message == (int)RequestMessageDef.Request_AreaMapInfo)
        {
			int id = ((RequestAreaMapInfoMessage)request.msg).id;
            ServerReplyMessage rpl = new ServerReplyMessage();
			rpl.serial = request.serial;
			rpl.resultObject = DataManager.GetInstance().MapDic[id];
			ReplyToClient(rpl);
        }
	}

    public List<Map> getMapList()
    {
		return DataManager.GetInstance().AreaMapsData.Maps;
    }
}
