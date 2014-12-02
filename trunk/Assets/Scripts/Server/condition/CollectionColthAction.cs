using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

class CollectionColthAction : ConditionAction
{
    public override string complete(string target)
    {
        int clothNum = int.Parse(target);
        if (clothNum <= BagInfoManager.getInstance().totalColthCount)
        {
            return "";
        }
        return "获得"+clothNum+"件衣服";
    }
}

