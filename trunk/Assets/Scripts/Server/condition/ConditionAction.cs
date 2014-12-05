using UnityEngine;
using System.Collections;

public class ConditionAction
{

    public virtual string complete(string target){
        return "";
    }
}

public enum ConditionType
{
    Type_null=0,
    Type_CollectCloth,
    Type_CollectBodyIndexCloth,
    Type_LevelInGrade,
    Type_TimeOpen
}