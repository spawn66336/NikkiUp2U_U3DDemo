using UnityEngine;
using System.Collections;

public class InspectorViewCtrl : EditorCtrlComposite 
{
    public delegate void InspectorDrawDelegate(EditorControl c, object obj);

    public object editTarget = null;
    public InspectorDrawDelegate onInspector = null;

    public override GUIStyle GetStyle()
    {
        return SpecialEffectEditorStyle.PanelBox;
        //return "AS TextArea";
    }

    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[] 
        {   
            GUILayout.ExpandHeight(true), 
            GUILayout.ExpandWidth(true) 
        };
    }
}
