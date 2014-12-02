using UnityEngine;
using System.Collections;
using UnityEditor;

public class HSpliterRenderStrategy : EditorRenderStrategy
{
    public override bool PreVisit(EditorControl c) 
    {
        GUILayout.BeginVertical();
        return true;
    }
    public override void Visit(EditorControl c) 
    {
        HSpliterCtrl spliter = c as HSpliterCtrl; 
        GUILayout.Box("", c.GetStyle(), c.GetOptions()); 

        c.UpdateLastRect();

        if (spliter.Dragable)
        {
            if (
               FrameInputInfo.GetInstance().leftBtnPress &&
               c.LastRect.Contains(FrameInputInfo.GetInstance().currPos)
             )
            {
                spliter.IsDragging = true;
            }
            else
            {

                if (FrameInputInfo.GetInstance().leftButtonDown == false)
                {
                    spliter.IsDragging = false;
                }

                if (spliter.IsDragging)
                {
                    if (null != c.layoutConstraint)
                    {
                        Vector2 mouseDelta = FrameInputInfo.GetInstance().posOffset;


                        if (Mathf.Abs(mouseDelta.y) > Mathf.Epsilon)
                        {
                            if (c.layoutConstraint.spliterOffsetInv)
                            {
                                c.layoutConstraint.spliterOffset -= mouseDelta.y;
                            }
                            else
                            {
                                c.layoutConstraint.spliterOffset += mouseDelta.y;
                            }

                            if (c.layoutConstraint.spliterOffset < 10.0f)
                            {
                                c.layoutConstraint.spliterOffset = 10.0f;
                            }

                            c.RequestRepaint();
                        }
                    } 
                }
            }
            EditorGUIUtility.AddCursorRect(c.LastRect, MouseCursor.ResizeVertical);
        }
         
    }
    public override void AfterVisit(EditorControl c) 
    {
        GUILayout.EndVertical();
    }
 
 
}
