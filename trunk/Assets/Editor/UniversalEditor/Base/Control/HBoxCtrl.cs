using UnityEngine;
using System.Collections;

public class HBoxCtrl : EditorCtrlComposite 
{
    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[]{
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        };
    }  
	 
}
