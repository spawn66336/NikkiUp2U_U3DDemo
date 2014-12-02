using UnityEngine;
using System.Collections;

public class HBoxRenderStrategy : EditorRenderStrategy
{
    public override bool PreVisitChildren(EditorControl c) 
    {
        GUILayoutOption[] v = new GUILayoutOption[]
        {
            GUILayout.Height(c.Size.height),
            GUILayout.ExpandWidth(true)
        };

        GUILayoutOption[] h = new GUILayoutOption[]
        {
            GUILayout.Width(c.Size.width),
            GUILayout.ExpandHeight(true)
        };

        if (_IsExtensible(c))
        {
            GUILayout.BeginHorizontal(
                new GUILayoutOption[]{ 
                    GUILayout.ExpandWidth(true),
                    GUILayout.ExpandHeight(true)
                });
        }
        else
        {
            GUILayout.BeginVertical(v);
            GUILayout.BeginHorizontal(h);
        }
        return true;
    }
    public override void AfterVisitChildren(EditorControl c) 
    {
        if (_IsExtensible(c))
        {
            GUILayout.EndHorizontal();
        }
        else
        {
            GUILayout.EndHorizontal();
            GUILayout.EndVertical();
        }
    }

    private bool _IsExtensible( EditorControl c )
    {
        if (c.layoutConstraint.expandWidth == true &&
            c.layoutConstraint.expandHeight == true)
        {
            return true;
        } 
        return false;
    }
	 
}
