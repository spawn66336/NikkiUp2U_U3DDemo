using UnityEngine;
using System.Collections;

public class LevelInGradeAction : ConditionAction
{

    public override string complete(string target)
    {
        if (!target.Contains(","))
        {
            return "-1";
        }
        string[] strs = target.Split(',');
        if (strs.Length != 2)
        {
            return "-1";
        }
        int levelId = int.Parse(strs[0]);
        int targetRank = int.Parse(strs[1]);
        int rank = PlayerInfoManager.getInstance().getLevelgradeById(levelId);
        if (rank>=targetRank)
        {
            return "";
        }
        return "关卡" + levelId + "达到" + targetRank;
    }
}
