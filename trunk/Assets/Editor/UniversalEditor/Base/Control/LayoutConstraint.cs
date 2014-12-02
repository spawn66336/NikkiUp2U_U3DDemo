using UnityEngine;
using System.Collections;

public class LayoutConstraint 
{
    public bool useMinWidth = false;
    public float minWidth = 0.0f;

    public bool useMinHeight = false;
    public float minHeight = 0.0f;

    public bool useMaxWidth = false;
    public float maxWidth = 0.0f;

    public bool useMaxHeight = false;
    public float maxHeight = 0.0f;

    public bool expandWidth = false;
    public bool expandHeight = false;

    public bool spliterOffsetInv = false;
    public float spliterOffset = 0.0f;

    public GUILayoutOption[] GetOptions()
    {
        ArrayList options = new ArrayList();
        
        if( useMinWidth )
        {
            options.Add(GUILayout.MinWidth(minWidth));
        }

        if( useMinHeight )
        {
            options.Add(GUILayout.MinHeight(minHeight));
        }

        if( useMaxWidth )
        {
            options.Add(GUILayout.MaxWidth(maxWidth));
        }

        if( useMaxHeight )
        {
            options.Add(GUILayout.MaxHeight(maxHeight));
        }

        if( expandWidth )
        {
            options.Add(GUILayout.ExpandWidth(true));
        }
        else
        {
            options.Add(GUILayout.ExpandWidth(false));
        }

        if( expandHeight )
        {
            options.Add(GUILayout.ExpandHeight(true));
        }
        else
        {
            options.Add(GUILayout.ExpandHeight(false));
        }

        return options.ToArray() as GUILayoutOption[];
    }

   public static LayoutConstraint GetToolBarConstraint( float h )
    {
        LayoutConstraint c = new LayoutConstraint();
        c.useMinHeight = true;
        c.minHeight = h;

        c.useMaxHeight = true;
        c.maxHeight = h;
        return c;
    }

    public static LayoutConstraint GetInspectorViewConstraint( float minWidth , float minHeight )
    {    
       LayoutConstraint c = new LayoutConstraint();
       c.useMinWidth = true;
       c.useMinHeight = true;
       c.minWidth = minWidth;
       c.minHeight = minHeight;
       return c;
    }

    public static LayoutConstraint GetListViewConstraint( float minWidth , float minHeight )
    {
        return GetInspectorViewConstraint(minWidth, minHeight);
    }

    public static LayoutConstraint GetExtensibleViewConstraint()
    {
        LayoutConstraint c = new LayoutConstraint();
        c.expandHeight = true;
        c.expandWidth = true;
        return c;
    }

    public static LayoutConstraint GetSpliterConstraint( float offset , bool inv = false)
    {
        LayoutConstraint c = new LayoutConstraint();
        c.spliterOffset = offset;
        c.spliterOffsetInv = inv;
        return c;
    }
}
