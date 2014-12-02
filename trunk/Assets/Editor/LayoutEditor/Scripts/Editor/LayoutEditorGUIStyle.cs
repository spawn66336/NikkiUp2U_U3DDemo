using UnityEngine;
using UnityEditor;
using System.Collections;

public static class LayoutEditorGUIStyle
{
    private static GUIStyle _hDivider;
    private static GUIStyle _vDivider;
    private static GUIStyle _panelBox;
    private static bool _stylesInitialized = false;

    public static void Init()
    {
        if (!_stylesInitialized)
        {
            _stylesInitialized = true;
            _hDivider = new GUIStyle(GUI.skin.box);
            _hDivider.name = "hDivider";
            _hDivider.margin = new RectOffset(0, 0, 0, 0);
            _hDivider.padding = new RectOffset(0, 0, 0, 0);
            _hDivider.border = new RectOffset(0, 0, 0, 0);
            _hDivider.normal.background = null;
            _vDivider = new GUIStyle(GUI.skin.box);
            _vDivider.name = "vDivider";
            _vDivider.margin = new RectOffset(0, 0, 0, 0);
            _vDivider.padding = new RectOffset(0, 0, 0, 0);
            _vDivider.border = new RectOffset(0, 0, 0, 0);
            _vDivider.normal.background = null;
            _panelBox = new GUIStyle(GUI.skin.box);
            _panelBox.name = "panelBox";
            _panelBox.padding = new RectOffset(1, 1, 1, 1);
            _panelBox.margin = new RectOffset(0, 0, 0, 0);
        }
    }

    public static GUIStyle hDivider
    {
        get
        {
            return _hDivider;
        }
    }

    public static GUIStyle vDivider
    {
        get
        {
            return _vDivider;
        }
    }

    public static GUIStyle panelBox
    {
        get
        {
            return _panelBox;
        }
    }
}