using UnityEngine;
using System.Collections;
using UnityEditor;

public class ListViewRenderStrategy : EditorRenderStrategy
{ 

    public ListViewRenderStrategy()
    {
        itemStyle = new GUIStyle();
        itemStyle.padding.top = -1;
        itemStyle.padding.bottom = 1;
        itemStyle.normal.textColor = itemTextColor;
    }

    public override void Visit(EditorControl c)
    {
        ListViewCtrl list = c as ListViewCtrl;

        if (list == null)
            return;


        EditorGUILayout.BeginHorizontal();

            EditorGUILayout.BeginVertical(c.GetStyle(), c.GetOptions());
            Vector2 newScrollPos = EditorGUILayout.BeginScrollView(list.ScrollPos,false,true,GUIStyle.none,GUI.skin.verticalScrollbar,GUIStyle.none);

            if (!newScrollPos.Equals(list.ScrollPos))
            {
                c.frameTriggerInfo.isScroll = true;
                c.frameTriggerInfo.scrollPos = newScrollPos;
            } 
            list.ScrollPos = newScrollPos;

            int count = 0;
            foreach( var item in list.Items)
            {  
                if (list.LastSelectItem == count)
                {
                    GUI.color = item.onSelectColor;
                    GUI.Box(item.lastRect, GUIContent.none);
                    GUI.color = Color.white;
                }

                GUIContent itemContent = new GUIContent();
                itemContent.text = item.name; 
                if( item.image != null )
                {
                    itemContent.image = item.image; 
                }  
                
                //add by liteng for atlas begin
                if ((item.tooltip != "") && (item.tooltip != null))
                {
                    itemContent.tooltip = item.tooltip;
                }

                //add by liteng end
                EditorGUILayout.BeginHorizontal();

                EditorGUILayout.LabelField(itemContent, itemStyle);

                EditorGUILayout.EndHorizontal();
                  

                SpecialEffectEditorUtility.GetLastRect(ref item.lastRect);


                Vector2 localMousePos = c.CalcLocalPos(FrameInputInfo.GetInstance().currPos);
                localMousePos += list.ScrollPos;
                if (
                        FrameInputInfo.GetInstance().leftBtnPress &&
                        item.lastRect.Contains(localMousePos)
                    )
                {
                    list.LastSelectItem = count;
                    c.frameTriggerInfo.lastSelectItem = count;

                    c.RequestRepaint();
                }
                //add by liteng for atlas start
                else if (
                        FrameInputInfo.GetInstance().rightButtonDown &&
                        item.lastRect.Contains(localMousePos)
                   )
                {
                    list.LastSelectItem = count;
                    c.frameTriggerInfo.lastSelectItemR = count;

                    c.RequestRepaint();
                }
                else if(
                        FrameInputInfo.GetInstance().rightBtnPressUp &&
                        item.lastRect.Contains(localMousePos)                    
                    )
                {
                    list.LastSelectItem = count;
                    c.frameTriggerInfo.lastSelectItemRU = count;

                    c.RequestRepaint();
                }
                else { 
                    //do nothing
                }
                //add by liteng end
                count++;
            }
            EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();

        //为了顶住右侧边框，使ScrollView显示完全
        //GUILayout.Space(10f);
        EditorGUILayout.EndHorizontal();

        c.UpdateLastRect(); 

    }

    GUIStyle itemStyle;
    Color itemTextColor =
       new Color(179f / 255f, 179f / 255f, 179f / 255f); // dark style下foldout文字颜色
}
