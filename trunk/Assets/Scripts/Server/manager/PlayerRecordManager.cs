using System;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class PlayerRecordManager : IDataManager
{
    public Dictionary<int, PlayerLevelRecordForCondition> dicLevelRecordCon = new Dictionary<int, PlayerLevelRecordForCondition>();
    public Dictionary<int, PlayerMapRecordForCondition> dicMapRecordCon = new Dictionary<int, PlayerMapRecordForCondition>();
    public Hashtable tableMapOpenTime = new Hashtable();
    static PlayerRecordManager _instance;
    public static PlayerRecordManager getInstance()
    {
        if (_instance == null)
        {
            _instance = new PlayerRecordManager();
        }
        return _instance;
    }
    public string addLevelCondition(int levelId)
    {
        if (dicLevelRecordCon.ContainsKey(levelId))
        {
            return "";
        }
        Level levelInfo = DataManager.GetInstance().getLevelInfo(levelId);
        if (levelInfo == null)
        {
            return "";
        }
        int i = 0;
        Dictionary<ConditionType, bool> conditionDic = new Dictionary<ConditionType, bool>();
        string unlockRes = "";
        foreach (Unlock con in levelInfo.Cond)
        {
            ConditionAction action = null;
            if ((ConditionType)con.Type == ConditionType.Type_CollectCloth)
            {
                action = new CollectionColthAction();
               
            }
            else if ((ConditionType)con.Type == ConditionType.Type_CollectBodyIndexCloth)
            {
                action = new CollectionBodyIndexClothAction();
            }
            else if ((ConditionType)con.Type == ConditionType.Type_LevelInGrade)
            {
               action = new LevelInGradeAction();
            }
            if (action != null)
            {
                string str = action.complete(con.Value);
                bool flag = false;
                if (str.Equals(""))
                {
                    flag = true;
                    i++;
                }
                else
                {
                    unlockRes += str;
                }
                conditionDic.Add((ConditionType)con.Type, flag);
            }
            
        }
        if (i == levelInfo.Cond.Count)
        {
            return "";
        }
        
        PlayerLevelRecordForCondition record = new PlayerLevelRecordForCondition();
        record.levelInfo = levelInfo;
        record.conditionDic = conditionDic;
        dicLevelRecordCon.Add(levelId, record);
        return unlockRes;
    }

    public string addMapCondition(Map map)
    {
        if (dicMapRecordCon.ContainsKey(map.Id))
        {
            return "";
        }
        int i = 0;
        Dictionary<ConditionType, bool> conditionDic = new Dictionary<ConditionType, bool>();
        string unlockRes = "";
        foreach (Unlock con in map.UnlockCond)
        {
            if ((ConditionType)con.Type == ConditionType.Type_CollectCloth)
            {
                CollectionColthAction action = new CollectionColthAction();
                string str = action.complete(con.Value);
                bool flag = false;
                if (str.Equals(""))
                {
                    flag = true;
                    i++;
                }
                else
                {
                    unlockRes += str;
                }
                conditionDic.Add((ConditionType)con.Type, flag);
            }
            else if ((ConditionType)con.Type == ConditionType.Type_TimeOpen)
            {
                bool flag = false;
                string[] timestr = con.Value.Split('-');
                if (timestr.Length != 6)
                {
                    continue;
                }
                DateTime datetime = new DateTime(int.Parse(timestr[0]), int.Parse(timestr[1]), int.Parse(timestr[2]), int.Parse(timestr[3]), int.Parse(timestr[4]), int.Parse(timestr[5]));
                if (DateTime.Now >= datetime)
                {
                    i++;
                    flag = true;
                }
                else
                {
                    tableMapOpenTime.Add(map.Id, datetime);
                    unlockRes += datetime.ToString() + "开启";
                }
                conditionDic.Add((ConditionType)con.Type, flag);
            }
        }
        if (i == map.UnlockCond.Count)
        {
            return "";
        }

        PlayerMapRecordForCondition record = new PlayerMapRecordForCondition();
        record.mapInfo = map;
        record.mapConditionDic = conditionDic;
        dicMapRecordCon.Add(map.Id, record);
        return unlockRes;
    }
    public override void destroy()
    {
        dicLevelRecordCon.Clear();
        dicMapRecordCon.Clear();
    }
    public DateTime updateTime;
    public void updateMapTime()
    {
        DateTime now = DateTime.Now;
        if (updateTime == null)
        {
            updateTime = DateTime.Now;
        }
        else
        {
            updateTime.AddSeconds(5);
            if (now < updateTime)
            {
                return;
            }
            updateTime = now;
        }
        List<int> mapIdList = new List<int>();
        foreach (DictionaryEntry entry in tableMapOpenTime)
        {
            string[] timestr = ((string)entry.Value).Split('-');
            if (timestr.Length != 6)
            {
                continue;
            }
            DateTime datetime = new DateTime(int.Parse(timestr[0]), int.Parse(timestr[1]), int.Parse(timestr[2]), int.Parse(timestr[3]), int.Parse(timestr[4]), int.Parse(timestr[5]));
            if (updateTime >= datetime)
            {
                mapIdList.Add((int)entry.Key);
            }
        }
        foreach (int mapId in mapIdList)
        {
            tableMapOpenTime.Remove(mapId);
            if (!dicMapRecordCon.ContainsKey(mapId))
            {
                continue;
            }
            PlayerMapRecordForCondition info = dicMapRecordCon[mapId];
            info.mapConditionDic[ConditionType.Type_TimeOpen]=true;
            int flag=0;
            foreach (ConditionType type in info.mapConditionDic.Keys)
            {
                if (info.mapConditionDic[type])
                {
                    flag++;
                }
            }
            if(flag==info.mapConditionDic.Count){
                dicMapRecordCon.Remove(mapId);
                PlayerInfoManager.getInstance().openMap(mapId);
            }
        }
    }
}

public class PlayerLevelRecordForCondition
{
    public Level levelInfo;
    public Dictionary<ConditionType, bool> conditionDic = new Dictionary<ConditionType, bool>();
}

public class PlayerMapRecordForCondition{
    public Map mapInfo;
    public Dictionary<ConditionType,bool> mapConditionDic = new Dictionary<ConditionType,bool>();
}