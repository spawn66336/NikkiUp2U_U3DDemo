using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;


public class SliderCtrl : EditorControl
{
    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[]{
            GUILayout.Height(30f),
            GUILayout.MinWidth(500f),
            GUILayout.ExpandWidth(true)
        };
    }
} 
