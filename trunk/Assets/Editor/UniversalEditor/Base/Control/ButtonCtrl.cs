using UnityEngine;
using System.Collections;

public class ButtonCtrl : EditorControl 
{ 
    public Color BtnColor
    {
        get { return color; }
        set { color = value; }
    }

    public override GUILayoutOption[] GetOptions() 
    { 
        if( layoutConstraint.expandWidth == true )
        {
            return new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(Size.height) };  
        }
        return new GUILayoutOption[] { GUILayout.Width(Size.width), GUILayout.Height(Size.height) }; 
    }

    private Color color = Color.white;
}
