using UnityEngine;
using System.Collections;
using UnityEditor;

public class LabelCtrlRenderStrategy : EditorRenderStrategy 
{
    public LabelCtrlRenderStrategy()
    {
        labelStyle = new GUIStyle();
        labelStyle.fontSize = 16;
    }

    public override void Visit(EditorControl c) 
    {
        LabelCtrl label = c as LabelCtrl;

        Color oldColor =labelStyle.normal.textColor;
        labelStyle.normal.textColor = label.textColor;
         
        if( _IsExtensible(c))
        {
            GUILayout.Label(c.Caption,labelStyle, new GUILayoutOption[] { GUILayout.Height(20f) , GUILayout.ExpandWidth(true) });
            return;
        } 
        GUILayout.Label(c.Caption ,labelStyle,new GUILayoutOption[]{ GUILayout.Height(20f)});

        labelStyle.normal.textColor = oldColor;
    }

    private bool _IsExtensible(EditorControl c)
    {
        if (c.layoutConstraint.expandWidth == true &&
            c.layoutConstraint.expandHeight == true)
        {
            return true;
        }
        return false;
    }

    GUIStyle labelStyle;
    	 
}
