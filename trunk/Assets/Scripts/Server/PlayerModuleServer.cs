using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerModuleServer : ModuleServer 
{

    private static string fileName = "playInfo.xml";
    public PlayerInfo playerInfo;
    public override void Init()
    {
        base.Init();
        playerInfo = (PlayerInfo)XMLTools.readXml(fileName, typeof(PlayerInfo));

    }

    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_PlayerInfo)
        {
            // mapId和levelIdList需要调用接口
            Dictionary<int,List<Level>> dic = FakeServer.GetInstance().GetAreaMapModuleServer().getMapInfo(playerInfo.currLevelId);
            int mapId=0;
            foreach(int id in dic.Keys){
                mapId=id;
            }
            List<Level> levelInfoList = dic[mapId];
            List<PlayerLevelRecordInfo> finalLevelList = new List<PlayerLevelRecordInfo>();
            List<PlayerLevelRecordInfo> recordList = playerInfo.levelRecordList;
            foreach (PlayerLevelRecordInfo record in recordList)
            {
                foreach (Level levelInfo in levelInfoList)
                {
                    if (levelInfo.Id == record.levelId)
                    {
                        if (record.state == LevelState.Unlocked)
                        {
                            // todo 加入条件模块之后去寻找
                            record.lockReason = "奏是锁住了。咬我啊咬我啊！！！！";
                        }
                        finalLevelList.Add(record);
                        break;
                    }
                }
            }
            PlayerInfo finalInfo = new PlayerInfo();
            finalInfo.gold = playerInfo.gold;
            finalInfo.energy = playerInfo.energy;
            finalInfo.diamond = playerInfo.diamond;
            finalInfo.currLevelId = playerInfo.currLevelId;
            finalInfo.currAreaMapId = mapId;
            finalInfo.levelRecordList.AddRange(finalLevelList);
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.serial = request.serial;
            replyMsg.resultObject = finalInfo;
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
            PlayerLevelRecordInfo newRecord = new PlayerLevelRecordInfo();
            
        }


    }
   
}
