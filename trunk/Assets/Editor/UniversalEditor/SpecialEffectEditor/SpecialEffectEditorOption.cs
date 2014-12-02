using UnityEngine;
using System.Collections;

public class SpecialEffectEditorOption 
{
    private static bool s_initFlag = false;
    private static GUILayoutOption s_toolBarBtnWidth = null;
    private static GUILayoutOption s_widthExpand = null;
    private static GUILayoutOption s_heightExpand = null;

    //Panel分割条属性
    private static GUILayoutOption s_dividerWidth = null;
    private static GUILayoutOption s_dividerHeight = null;
    private static GUILayoutOption[] s_hDividerOptions = null;
    private static GUILayoutOption[] s_vDividerOptions = null;


    public static void Init()
    {
        if (s_initFlag)
            return;

        s_initFlag = true;

        s_toolBarBtnWidth = GUILayout.MaxWidth(70f);
        s_widthExpand = GUILayout.ExpandWidth(true);
        s_heightExpand = GUILayout.ExpandHeight(true);

        s_dividerWidth = GUILayout.Width(4f);
        s_dividerHeight = GUILayout.Height(4f);
        s_hDividerOptions = new GUILayoutOption[] { s_dividerHeight, s_widthExpand };
        s_vDividerOptions = new GUILayoutOption[] { s_dividerWidth, s_heightExpand };
    }

    public static GUILayoutOption toolBarBtnWidth
    {
        get
        {
            return s_toolBarBtnWidth;
        }
    }

    public static GUILayoutOption widthExpand
    {
        get 
        {
            return s_widthExpand;
        }
    }

    public static GUILayoutOption heightExpand
    {
        get
        {
            return s_heightExpand;
        }
    }

    public static GUILayoutOption[] hDividerOptions
    {
        get
        {
            return s_hDividerOptions;
        }
    }

    public static GUILayoutOption[] vDividerOptions
    {
        get
        {
            return s_vDividerOptions;
        }
    }

    
}
