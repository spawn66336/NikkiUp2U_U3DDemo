using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerInfoManager : IDataManager
{
    private static PlayerInfoManager _instance;
    public static PlayerInfoManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new PlayerInfoManager();
        }
        return _instance;
    }
    private static string fileName = "playerInfo.xml";
    PlayerInfo playerInfo;

    public PlayerInfo PlayerInfo
    {
        get
        {
            return playerInfo;
        }
    }


    public override void init()
    {
        playerInfo = (PlayerInfo)XMLTools.readXml(fileName, typeof(PlayerInfo));
        checkLevel();
        LevelInfo currLevel = DataManager.GetInstance().LevelDic[playerInfo.currLevelId];
        Debug.Log(currLevel);
        playerInfo.currAreaMapId = currLevel.areaMapId;
        initMap();
    }
    private void initMap()
    {
        List<Map> areaMapList = DataManager.GetInstance().getMapList();
        List<PlayerAreaMapRecordInfo> mapRecodeList = new List<PlayerAreaMapRecordInfo>();
        bool isChangeMap = false;
        int i = 0;
        foreach (Map map in areaMapList)
        {
            i++;
            // todo 判断解锁条件
            PlayerAreaMapRecordInfo mapRecoder = new PlayerAreaMapRecordInfo();
            mapRecoder.areaMapId = map.Id;
            string str = PlayerRecordManager.getInstance().addMapCondition(map);
            if (str.Equals(""))
            {
                mapRecoder.isLocked = false;
            }
            else
            {
                mapRecoder.isLocked = true;
                mapRecoder.lockReason = str;
                if (map.Id == playerInfo.currAreaMapId)
                {
                    isChangeMap = true;
                }
            }
            mapRecodeList.Add(mapRecoder);
            // todo 如果当前地图被锁住，则更改当前地图及当前关卡id，疑问：重新选定的地图规则
            if (isChangeMap)
            {
                i--;
            }
        }
        playerInfo.areaMapRecordList.AddRange(mapRecodeList);
        if (isChangeMap)
        {
            for (int j = i; j >= 0; j--)
            {
                PlayerAreaMapRecordInfo record = mapRecodeList[j];
                if (record.isLocked)
                {
                    continue;
                }
                Dictionary<int, List<Level>> dic = DataManager.GetInstance().getMapInfo(record.areaMapId);
                playerInfo.currAreaMapId = record.areaMapId;
                playerInfo.currLevelId=dic[record.areaMapId][0].Id;
                break;
            }
        }
        
    }
    private void checkLevel()
    {
        foreach (PlayerLevelRecordInfo record in playerInfo.levelRecordList)
        {
            if (record.state == LevelState.Locked)
            {
                string str = PlayerRecordManager.getInstance().addLevelCondition(record.levelId);
                if (str.Equals(""))
                {
                    record.state = LevelState.Unlocked;
                }
                else
                {
                    record.lockReason = str;
                }
            }
        }
    }

    public void finishLevel(int levelId, List<int> dressIdList, RatingInfo ratingInfo)
    {
        bool isOpenLevel = false;
        int rank=-1;
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
                        recordInfo.highestRank = ratingInfo.levelRank;
                        recordInfo.highestScore = ratingInfo.score;
                        recordInfo.highestDressSet.dressList.Clear();
                        recordInfo.highestDressSet.dressList.AddRange(dressIdList);

                    }
                    rank = (int)recordInfo.highestRank;
                }
                break;
            }
        }
        if (isOpenLevel)
        {
            PlayerLevelRecordInfo newRecord = new PlayerLevelRecordInfo();
            Level nexLevel = DataManager.GetInstance().getNextLevel(levelId);
            newRecord.levelId = nexLevel.Id;
            string str = PlayerRecordManager.getInstance().addLevelCondition(nexLevel.Id);
            if (str.Equals(""))
            {
                newRecord.state = LevelState.Unlocked;
            }
            else
            {
                newRecord.state = LevelState.Locked;
                newRecord.lockReason = str;
            }
            playerInfo.levelRecordList.Add(newRecord);

        }

        EventCenter.onLevelInGrade(levelId, rank);
    }

    internal void changeLevelState(int levelId)
    {
        foreach (PlayerLevelRecordInfo info in playerInfo.levelRecordList)
        {
            if (info.levelId == levelId && info.state == LevelState.Locked)
            {
                info.state = LevelState.Unlocked;
                break;
            }
        }
    }
    public int getLevelCountByRank(int rank)
    {
        int count = 0;
        List<PlayerLevelRecordInfo> recordList = PlayerInfoManager.getInstance().PlayerInfo.levelRecordList;
        foreach (PlayerLevelRecordInfo record in recordList)
        {
            if ((int)record.highestRank == rank)
            {
                count += 1;
            }
        }
        return count;
    }

    public int getLevelgradeById(int levelId)
    {
        foreach (PlayerLevelRecordInfo info in playerInfo.levelRecordList)
        {
            if (levelId == info.levelId)
            {
                return (int)info.highestRank;
            }
        }
        return -1;
    }
}
