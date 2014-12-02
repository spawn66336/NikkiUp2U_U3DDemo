using UnityEngine;
using System.Collections;
using UnityEditor;

public class ComboBoxRenderStrategy : EditorRenderStrategy 
{
    public override void Visit(EditorControl c) 
    {
        currCtrl = c as ComboBoxCtrl;

        if (currCtrl == null)
            return;

        int newOption  = EditorGUILayout.IntPopup(
            currCtrl.CurrOption, 
            currCtrl.DisplayOptions, 
            currCtrl.OptionValues, new GUILayoutOption[]{ GUILayout.Width(currCtrl.Size.width),GUILayout.Height(currCtrl.Size.height)});
         
        if( currCtrl.CurrOption != newOption )
        {
            currCtrl.frameTriggerInfo.lastSelectItem = newOption;
            currCtrl.CurrOption = newOption;
        }
    }

    ComboBoxCtrl currCtrl;
}
