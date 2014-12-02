using UnityEngine;
using System.Collections;

public class CollectionBodyIndexClothAction : ConditionAction
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
        int bodyIndex = int.Parse(strs[0]);
        int num = int.Parse(strs[1]);
        if (num <= BagInfoManager.getInstance().getDressCount4DType(bodyIndex))
        {
            return "";
        }
        return "获得" + bodyIndex + "衣服" + num + "件";
    }
}
