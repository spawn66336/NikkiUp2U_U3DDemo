using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerModuleServer : ModuleServer 
{

    private static string fileName = "playerInfo.xml";
    public PlayerInfo playerInfo;
    public override void Init()
    {
        base.Init();
        playerInfo = (PlayerInfo)XMLTools.readXml(fileName, typeof(PlayerInfo));
        Debug.Log("playerInfo......");
        
    }

    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_PlayerInfo)
        {
            // mapId和levelIdList需要调用接口
            LevelInfo currLevel = DataManager.GetInstance().LevelDic[playerInfo.currLevelId];
            Debug.Log(currLevel);
            int mapId=currLevel.areaMapId;
            List<PlayerLevelRecordInfo> recordList = playerInfo.levelRecordList;
            foreach (PlayerLevelRecordInfo record in recordList)
            {
                if (record.state == LevelState.Unlocked)
                {
                    // todo 加入条件模块之后去寻找
                    Level levelInfo = DataManager.GetInstance().getLevelInfo(record.levelId);
                    record.lockReason = "奏是锁住了。咬我啊咬我啊！！！！";
                }
            }

            List<Map> areaMapList = DataManager.GetInstance().getMapList();
            List<PlayerAreaMapRecordInfo> mapRecodeList = new List<PlayerAreaMapRecordInfo>();
            foreach (Map map in areaMapList)
            {
                // todo 判断解锁条件
                PlayerAreaMapRecordInfo mapRecoder = new PlayerAreaMapRecordInfo();
                mapRecoder.areaMapId = map.Id;
                mapRecoder.isLocked = false;
                mapRecoder.lockReason = "我佛慈悲~~~~~~~~";
                mapRecodeList.Add(mapRecoder);
                // todo 如果当前地图被锁住，则更改当前地图及当前关卡id，疑问：重新选定的地图规则
            }
            playerInfo.areaMapRecordList.AddRange(mapRecodeList);
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = playerInfo;
            replyMsg.message = RequestMessageDef.Request_PlayerInfo;
            ReplyToClient(replyMsg);
        }
        else if (request.msg.Message == (int)RequestMessageDef.Request_ChangeCurrLevel)
        {
            ChangeCurrLevelRequestMsg msg = (ChangeCurrLevelRequestMsg)request.msg;
            playerInfo.currLevelId = msg.currLevelId;
            playerInfo.currAreaMapId = msg.currMapId;
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = playerInfo;
            replyMsg.message = RequestMessageDef.Request_ChangeCurrLevel;
            ReplyToClient(replyMsg);
        }
    }
    public int getLevelCountByRank(int rank)
    {
        int count = 0;
        List<PlayerLevelRecordInfo> recordList = playerInfo.levelRecordList;
        foreach (PlayerLevelRecordInfo record in recordList)
        {
            if ((int)record.highestRank == rank)
            {
                count += 1;
            }
        }
        return count;
    }

    public void finishLevel(int levelId, List<int> dressIdList,RatingInfo ratingInfo)
    {
        bool isOpenLevel=false;
        foreach (PlayerLevelRecordInfo recordInfo in playerInfo.levelRecordList)
        {
            if (recordInfo.levelId == levelId)
            {
                if (recordInfo.state == LevelState.Locked)
                {
                    // TODO 判断是否可以通过
                    LevelInfo levelInfo = new LevelInfo();

                }
                else if (recordInfo.state == LevelState.Finished)
                {
                    recordInfo.latestRank = ratingInfo.levelRank;
                    recordInfo.latestScore = ratingInfo.score;
                    recordInfo.latestDressSet.dressList.Clear();
                    recordInfo.latestDressSet.dressList.AddRange(dressIdList);
                    if (recordInfo.highestScore <= ratingInfo.score)
                    {
                        recordInfo.highestRank = ratingInfo.levelRank ;
                        recordInfo.highestScore = ratingInfo.score;
                        recordInfo.highestDressSet.dressList.Clear();
                        recordInfo.highestDressSet.dressList.AddRange(dressIdList);
                        
                    }
                }
                break;
            }
        }
        if (isOpenLevel)
        {
            // TODO 获得下一个level的的info,判断是否解锁
            Level nexLevel = DataManager.GetInstance().getNextLevel(levelId);
            PlayerLevelRecordInfo newRecord = new PlayerLevelRecordInfo();
            
        }


    }
   
}


