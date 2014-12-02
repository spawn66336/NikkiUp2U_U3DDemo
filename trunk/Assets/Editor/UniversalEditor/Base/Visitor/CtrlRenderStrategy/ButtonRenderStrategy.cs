using UnityEngine;
using System.Collections;
using UnityEditor;

public class ButtonRenderStrategy : EditorRenderStrategy
{
    public ButtonRenderStrategy()
    { 
        
    }

    public override void Visit(EditorControl c) 
    {
        currCtrl = c as ButtonCtrl;
        if (currCtrl == null)
            return;

        if (!currCtrl.Enable)
            return;

        Color oldColor = GUI.color;
        GUI.color = currCtrl.BtnColor;

        c.frameTriggerInfo.isClick = GUILayout.Button(c.Caption, c.GetOptions());
        c.UpdateLastRect();

        GUI.color = oldColor;
          
    }

    private ButtonCtrl currCtrl;
    private GUIStyle enableBtnStyle;
    private GUIStyle disableBtnStyle;
}
