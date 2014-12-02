using UnityEngine;
using System.Collections;
using UnityEditor;

public class TabViewRenderStrategy : EditorRenderStrategy
{
    public TabViewRenderStrategy()
    {
        int fontSize = 12;

        tabMidBtnPressedStyle = new GUIStyle( EditorStyles.miniButtonMid);
        tabMidBtnPressedStyle.onNormal = tabMidBtnPressedStyle.onActive;
        tabMidBtnPressedStyle.onHover = tabMidBtnPressedStyle.onActive;
        tabMidBtnPressedStyle.onFocused = tabMidBtnPressedStyle.onActive;
        tabMidBtnPressedStyle.normal = tabMidBtnPressedStyle.active;
        tabMidBtnPressedStyle.hover = tabMidBtnPressedStyle.active; 
        tabMidBtnPressedStyle.focused = tabMidBtnPressedStyle.active;
        tabMidBtnPressedStyle.margin.bottom = -2;
        tabMidBtnPressedStyle.fontSize = fontSize; 


        tabMidBtnNormalStyle = new GUIStyle(EditorStyles.miniButtonMid);
        tabMidBtnNormalStyle.onActive = tabMidBtnNormalStyle.onNormal;
        tabMidBtnNormalStyle.onHover = tabMidBtnNormalStyle.onNormal;
        tabMidBtnNormalStyle.onFocused = tabMidBtnNormalStyle.onNormal;
        tabMidBtnNormalStyle.active = tabMidBtnNormalStyle.normal;
        tabMidBtnNormalStyle.hover = tabMidBtnNormalStyle.normal;
        tabMidBtnNormalStyle.focused = tabMidBtnNormalStyle.normal;
        tabMidBtnNormalStyle.margin.bottom = -2;
        tabMidBtnNormalStyle.fontSize = fontSize;

        tabLeftBtnPressedStyle = new GUIStyle(EditorStyles.miniButtonLeft);
        tabLeftBtnPressedStyle.onNormal = tabLeftBtnPressedStyle.onActive;
        tabLeftBtnPressedStyle.onHover = tabLeftBtnPressedStyle.onActive;
        tabLeftBtnPressedStyle.onFocused = tabLeftBtnPressedStyle.onActive;
        tabLeftBtnPressedStyle.normal = tabLeftBtnPressedStyle.active;
        tabLeftBtnPressedStyle.hover = tabLeftBtnPressedStyle.active;
        tabLeftBtnPressedStyle.focused = tabLeftBtnPressedStyle.active;
        tabLeftBtnPressedStyle.margin.bottom = -2;
        tabLeftBtnPressedStyle.fontSize = fontSize;

        tabLeftBtnNormalStyle = new GUIStyle(EditorStyles.miniButtonLeft);
        tabLeftBtnNormalStyle.onActive = tabLeftBtnNormalStyle.onNormal;
        tabLeftBtnNormalStyle.onHover = tabLeftBtnNormalStyle.onNormal;
        tabLeftBtnNormalStyle.onFocused = tabLeftBtnNormalStyle.onNormal;
        tabLeftBtnNormalStyle.active = tabLeftBtnNormalStyle.normal;
        tabLeftBtnNormalStyle.hover = tabLeftBtnNormalStyle.normal;
        tabLeftBtnNormalStyle.focused = tabLeftBtnNormalStyle.normal;
        tabLeftBtnNormalStyle.margin.bottom = -2;
        tabLeftBtnNormalStyle.fontSize = fontSize;

        tabRightBtnPressedStyle = new GUIStyle(EditorStyles.miniButtonRight);
        tabRightBtnPressedStyle.onNormal = tabRightBtnPressedStyle.onActive;
        tabRightBtnPressedStyle.onHover = tabRightBtnPressedStyle.onActive;
        tabRightBtnPressedStyle.onFocused = tabRightBtnPressedStyle.onActive;
        tabRightBtnPressedStyle.normal = tabRightBtnPressedStyle.active;
        tabRightBtnPressedStyle.hover = tabRightBtnPressedStyle.active;
        tabRightBtnPressedStyle.focused = tabRightBtnPressedStyle.active;
        tabRightBtnPressedStyle.margin.bottom = -2;
        tabRightBtnPressedStyle.fontSize = fontSize;
        
        tabRightBtnNormalStyle = new GUIStyle(EditorStyles.miniButtonRight);
        tabRightBtnNormalStyle.onActive = tabRightBtnNormalStyle.onNormal;
        tabRightBtnNormalStyle.onHover = tabRightBtnNormalStyle.onNormal;
        tabRightBtnNormalStyle.onFocused = tabRightBtnNormalStyle.onNormal;
        tabRightBtnNormalStyle.active = tabRightBtnNormalStyle.normal;
        tabRightBtnNormalStyle.hover = tabRightBtnNormalStyle.normal;
        tabRightBtnNormalStyle.focused = tabRightBtnNormalStyle.normal;
        tabRightBtnNormalStyle.margin.bottom = -2;
        tabRightBtnNormalStyle.fontSize = fontSize;
    }

    public override bool PreVisit(EditorControl c) 
    { 
        EditorGUILayout.BeginHorizontal();
        return true;
    }

    public override void Visit(EditorControl c) 
    {
        currCtrl = c as TabViewCtrl;

        if (currCtrl == null)
            return;

        if (currCtrl.GetChildCount() <= 1)
        {
            currCtrl.selectTab = 0;
            return;
        }

        int newSelect = -1;
        int selectTab = currCtrl.selectTab;

        for( int i = 0 ; i < currCtrl.GetChildCount() ; i++ )
        {
            GUIStyle currBtnStyle = null;

            if( i == 0 )
            {
                if( selectTab == i )
                {
                    currBtnStyle = tabLeftBtnPressedStyle;
                }
                else
                {
                    currBtnStyle = tabLeftBtnNormalStyle;
                }
            }else if( i == currCtrl.GetChildCount() - 1)
            {
                if( selectTab == i )
                {
                    currBtnStyle = tabRightBtnPressedStyle;
                }
                else
                {
                    currBtnStyle = tabRightBtnNormalStyle;
                }
            }
            else
            {
                if( selectTab == i )
                {
                    currBtnStyle = tabMidBtnPressedStyle;
                }
                else
                {
                    currBtnStyle = tabMidBtnNormalStyle;
                }
            }

            if( GUILayout.Button( currCtrl.GetAt(i).Caption , currBtnStyle, tabBtnOptions) )
            {
                newSelect = i;
            }
           
        }

        if( newSelect != -1 )
        { 
            if (currCtrl.selectTab != newSelect)
            {
                currCtrl.frameTriggerInfo.lastSelectItem = newSelect;
                currCtrl.selectTab = newSelect;
                currCtrl.RequestRepaint();
            }
        }  
    }

    public override void AfterVisit(EditorControl c) 
    { 
        EditorGUILayout.EndHorizontal();
    }

    public override bool PreVisitChild(EditorControl c, int ichild) 
    {
        currCtrl = c as TabViewCtrl;

        if (currCtrl == null)
            return true;

        if( currCtrl.selectTab == ichild )
        {
            return true;
        }
        return false;
    }

    private TabViewCtrl currCtrl = null;

    private GUIStyle tabMidBtnPressedStyle;
    private GUIStyle tabMidBtnNormalStyle;


    private GUIStyle tabLeftBtnPressedStyle;
    private GUIStyle tabLeftBtnNormalStyle;


    private GUIStyle tabRightBtnPressedStyle;
    private GUIStyle tabRightBtnNormalStyle;

    private GUILayoutOption[] tabBtnOptions = new GUILayoutOption[] { 
        GUILayout.MaxHeight(20f), GUILayout.MaxWidth(70),GUILayout.ExpandWidth(false) };
   
}
