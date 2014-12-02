using UnityEngine;
using System.Collections;

public class VBoxCtrl : EditorCtrlComposite
{
    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[]{
            GUILayout.ExpandWidth(true),
            GUILayout.ExpandHeight(true)
        };
    }

}
