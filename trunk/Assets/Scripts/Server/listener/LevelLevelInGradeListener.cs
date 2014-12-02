using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LevelLevelInGradeListener : IGameEventListener
{

    public LevelLevelInGradeListener()
    {
        registeredEvent();
    }

    public override string getName()
    {
        return base.getName();
    }

    public override void handlerEvent(IEventInterface eventBean)
    {
        LevelLevelInGradeEvent levelEvent = (LevelLevelInGradeEvent)eventBean;
        int levelId = levelEvent.levelId;
        int curRank = levelEvent.levelRank;
        List<PlayerLevelRecordForCondition> finishList = new List<PlayerLevelRecordForCondition>();
        List<int> listKeys = new List<int>();
        listKeys.AddRange(PlayerRecordManager.getInstance().dicLevelRecordCon.Keys);
        for (int i = 0; i < listKeys.Count; i++)
        {
            PlayerLevelRecordForCondition info = PlayerRecordManager.getInstance().dicLevelRecordCon[listKeys[i]];
            int num = 0;
            List<ConditionType> listTypeKeys = new List<ConditionType>();
            listTypeKeys.AddRange(info.conditionDic.Keys);
            for (int j = 0; j < listTypeKeys.Count; j++)
            {
                ConditionType type = listTypeKeys[j];
                if (info.conditionDic[type])
                {
                    num++;
                    continue;
                }
                if (type == ConditionType.Type_LevelInGrade)
                {
                    for (int m = 0; m < info.levelInfo.Cond.Count; m++)
                    {
                        Unlock unlock = info.levelInfo.Cond[m];
                        if ((ConditionType)unlock.Type == ConditionType.Type_LevelInGrade)
                        {
                            string[] strs = unlock.Value.Split(',');
                            int targetId = int.Parse(strs[0]);
                            int targetRank = int.Parse(strs[1]);
                            if (levelId == targetId && curRank >= targetRank)
                            {
                                num++;
                                info.conditionDic[type] = true;
                            }
                            break;
                        }
                    }
                }

                if (num == info.conditionDic.Count)
                {
                    finishList.Add(info);
                }
            }
        }
        foreach (PlayerLevelRecordForCondition finish in finishList)
        {
            PlayerRecordManager.getInstance().dicLevelRecordCon.Remove(finish.levelInfo.Id);
            PlayerInfoManager.getInstance().changeLevelState(finish.levelInfo.Id);
        }
    }
    public override void registeredEvent()
    {
        EventEngine.GetInstance().addListener(GameEventType.Event_LevelInGrade, this);
    }
}
