using UnityEngine;
using System.Collections;
using UnityEditor;

public static class LayoutEditorGUI
{
    public static int panelDividerThickness
    {
        get
        {
            return 4;
        }
    }

    private static int _panelLayoutTreeHeight = 500;
    public static int panelLayoutTreeHeight
    {
        get
        {
            return _panelLayoutTreeHeight;
        }
        set
        {
            _panelLayoutTreeHeight = value;
        }
    }

    private static int _panelLayoutListWidth = 278;
    public static int panelLayoutListWidth
    {
        get
        {
            return _panelLayoutListWidth;
        }
        set
        {
            _panelLayoutListWidth = value;
        }
    }

    private static int _panelProperWidth = 265;
    public static int panelProperWidth
    {
        get
        {
            return _panelProperWidth;
        }
        set
        {
            _panelProperWidth = value;
        }
    }
}
