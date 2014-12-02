using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpecialEffectMenuItems
{

    [MenuItem("H3D/特效编辑/创建/创建特效调试物体")]
    static void CreateSpecialEffectDebugObject()
    {
        GameObject speDebugObj  = GameObject.Find("_SPE_DEBUG_OBJ") as GameObject;
        if( speDebugObj != null )
        {
            EditorUtility.DisplayDialog("警告", "您已经创建一个\"特效调试物体\"无需再次创建!", "确认");
            return;
        }
        speDebugObj = new GameObject("_SPE_DEBUG_OBJ");
        speDebugObj.AddComponent<SpecialEffectDebugObject>();
    }

    [MenuItem("H3D/特效编辑/创建/创建特效")]
    static void CreateSpecialEffect()
    {
        GameObject newSpeEffect = new GameObject("New SpecialEffect");
        newSpeEffect.AddComponent<SpecialEffect>();
        Selection.activeGameObject = newSpeEffect; 
    }
}
