using UnityEngine;
using System.Collections;

//用来存放所有特效编辑器用的Style
public class SpecialEffectEditorStyle 
{
    private static bool s_initFlag = false;
    private static GUIStyle s_hDivider = null;
    private static GUIStyle s_vDivider = null;
    private static GUIStyle s_panelBox = null;
    private static GUIStyle s_rulerNum = null;
    private static GUIStyle s_timeLineMouseTag = null;

    public static void Init()
    {
        if (s_initFlag)
            return;
        s_initFlag = true;

        s_hDivider = new GUIStyle(GUI.skin.box);
        s_hDivider.name = "hDivider";
        s_hDivider.margin = new RectOffset(0, 0, 0, 0);
        s_hDivider.padding = new RectOffset(0, 0, 0, 0);
        s_hDivider.border = new RectOffset(0, 0, 0, 0);
        s_hDivider.normal.background = null;

        s_vDivider = new GUIStyle(GUI.skin.box);
        s_vDivider.name = "vDivider";
        s_vDivider.margin = new RectOffset(0, 0, 0, 0);
        s_vDivider.padding = new RectOffset(0, 0, 0, 0);
        s_vDivider.border = new RectOffset(0, 0, 0, 0);
        s_vDivider.normal.background = null;


        s_panelBox = new GUIStyle(GUI.skin.box);
        s_panelBox.name = "panelBox";
        s_panelBox.padding = new RectOffset(1, 1, 1, 1);
        s_panelBox.margin = new RectOffset(0, 0, 0, 0);

        s_rulerNum = new GUIStyle(GUI.skin.box);
        s_rulerNum.name = "rulerNum";
        s_rulerNum.margin = new RectOffset(0, 0, 0, 0);
        s_rulerNum.padding = new RectOffset(0, 0, 0, 0);
        s_rulerNum.border = new RectOffset(0, 0, 0, 0);
        s_rulerNum.normal.background = null;
        s_rulerNum.alignment = TextAnchor.UpperLeft;


        s_timeLineMouseTag = new GUIStyle(GUI.skin.box);
        s_timeLineMouseTag.name = "timeLineMouseTag";  
        s_timeLineMouseTag.alignment = TextAnchor.UpperLeft; 
    }

    public static GUIStyle HDivider
    {
        get
        {
            return s_hDivider;
        }
    }

    public static GUIStyle VDivider
    {
        get
        {
            return s_vDivider;
        }
    }

    public static GUIStyle PanelBox
    {
        get 
        {
            return s_panelBox;
        }
    }

    public static GUIStyle RulerNum
    {
        get
        {
            return s_rulerNum;
        }
    }

    public static GUIStyle TimeLineMouseTag
    {
        get
        {
            return s_timeLineMouseTag;
        }
    }
}
