using UnityEngine;
using System.Collections;
using UnityEditor;

public class MainViewRenderStrategy : EditorRenderStrategy
{
     

	public override void Visit(EditorControl c)
    {
        currCtrl = c as MainViewCtrl;
        if (currCtrl == null)
            return;

        currCtrl.Init();

        _HandleInput();

        _RenderScene(); 

        Rect mainViewRect = new Rect(c.LastRect);
        EditorGUILayout.BeginHorizontal(c.GetStyle(), c.GetOptions()); 
            GUI.DrawTexture(mainViewRect, currCtrl.mainViewTexture, ScaleMode.StretchToFill, false); 
        EditorGUILayout.EndHorizontal();
        c.UpdateLastRect();

        //主视图大小有变
        if( 
                c.LastRect.width != mainViewRect.width ||
                c.LastRect.height != mainViewRect.height
          )
        {
            currCtrl.Resize(c.LastRect); 
            c.RequestRepaint();
        }
    }

    void _HandleInput()
    {
        if (currCtrl == null)
            return;

        Vector2 currMousePos = FrameInputInfo.GetInstance().currPos;
        Vector2 localMousePos = currCtrl.CalcLocalPos(currMousePos);
        Vector2 mouseMoveDelta = FrameInputInfo.GetInstance().posOffset;
        Vector2 wheelScrollDelta = FrameInputInfo.GetInstance().delta;

        //2D视图
        bool is2DView = currCtrl.Is2DView;

        //若当前鼠标不在视口区域内直接忽略
        if( !currCtrl.LastRect.Contains( currMousePos ))
        {
            currCtrl.rotateDragging = false;
            currCtrl.moveDragging = false;
            return;
        }

        if( FrameInputInfo.GetInstance().dragingObjs  )
        {
            bool accept = false;

            if (currCtrl.onAcceptDragObjs != null)
            {
                accept = currCtrl.onAcceptDragObjs(
                                        currCtrl,
                                        FrameInputInfo.GetInstance().dragObjs,
                                        FrameInputInfo.GetInstance().dragObjsPaths);

                if (accept)
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Link;

                    if (!FrameInputInfo.GetInstance().dragObjsPerform)
                    {
                        currCtrl.frameTriggerInfo.isDraggingObjs = true;
                    }
                    else
                    {
                        currCtrl.frameTriggerInfo.isDropObjs = true;
                    }
                }
                else
                {
                    DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
                }
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }
        }

        //根据输入分析控件的推拽状态
        _UpdateDraggingState();
         
        if( Mathf.Abs( mouseMoveDelta.x ) < Mathf.Epsilon &&
            Mathf.Abs( mouseMoveDelta.y ) < Mathf.Epsilon &&
            !FrameInputInfo.GetInstance().scroll 
            ) 
        {//无明显移动直接返回
            return;
        }

        Transform rootTrans = currCtrl.mainViewRoot.transform;
        Transform camTrans = currCtrl.camObj.transform;
        Camera cam = currCtrl.mainCam;

        if( currCtrl.rotateDragging )
        {
            if (!is2DView)
            {//只有3D视图才响应旋转拖拽
                float angleAroundUp = mouseMoveDelta.x * 0.1f;
                float angleAroundRight = mouseMoveDelta.y * 0.1f;

                Vector3 localPos = (camTrans.localPosition - currCtrl.center).normalized * currCtrl.radius;


                Quaternion q0 = Quaternion.AngleAxis(angleAroundUp, camTrans.up);
                camTrans.localPosition = q0 * localPos;
                camTrans.Rotate(Vector3.up, angleAroundUp, Space.Self);

                Quaternion q1 = Quaternion.AngleAxis(angleAroundRight, camTrans.right);
                camTrans.Rotate(Vector3.right, angleAroundRight, Space.Self);
                camTrans.localPosition = q1 * camTrans.localPosition;

                camTrans.localPosition += currCtrl.center;
                currCtrl.RequestRepaint();
            }
        }

        if( currCtrl.moveDragging )
        {
            float moveX = -mouseMoveDelta.x * 0.01f;
            float moveY = mouseMoveDelta.y * 0.01f;

            if (!is2DView)
            {//3D视图
                Vector3 localPos = camTrans.localPosition - currCtrl.center;
                currCtrl.center += camTrans.up * moveY + camTrans.right * moveX;
                camTrans.localPosition = localPos + currCtrl.center;
            }
  
            currCtrl.RequestRepaint();
        }

        float zoom = 0.0f;
        if( currCtrl.zoomWheelScroll )
        {
            zoom = wheelScrollDelta.y * 0.2f;
        }else if( currCtrl.zoomDragging )
        {
            zoom = mouseMoveDelta.y * 0.01f; 
        }

        if (currCtrl.zoomWheelScroll || currCtrl.zoomDragging)
        {
            if (!is2DView)
            {
                currCtrl.radius += zoom;
                currCtrl.radius = Mathf.Clamp(currCtrl.radius, currCtrl.minRadius, currCtrl.maxRadius);

                Vector3 localPos = (camTrans.localPosition - currCtrl.center).normalized * currCtrl.radius;
                camTrans.localPosition = localPos + currCtrl.center;
            }
            else
            {
                if (currCtrl.zoomDragging)
                {//缩放拖拽在2D视图中是移动
                    float offsetX = (-mouseMoveDelta.x / cam.pixelWidth) * cam.orthographicSize * 2.0f;
                    float offsetY = (mouseMoveDelta.y / cam.pixelHeight) * cam.orthographicSize * 2.0f;
                    camTrans.localPosition += new Vector3(offsetX, offsetY);
                }
                else
                { 
                    if (zoom < 0.0f)
                    {
                        cam.orthographicSize *= 0.9f;
                    }
                    else
                    {
                        cam.orthographicSize *= 1.1f;
                    }
                }
            }
            currCtrl.RequestRepaint();
        }

        
    }

    void _UpdateDraggingState()
    {
        if (FrameInputInfo.GetInstance().leftButtonDown)
        {
            if (FrameInputInfo.GetInstance().leftBtnPress)
            {
                currCtrl.rotateDragging = true;
            }
        }
        else
        {
            currCtrl.rotateDragging = false;
        }

        if (FrameInputInfo.GetInstance().midButtonDown)
        {
            if (FrameInputInfo.GetInstance().midBtnPress)
            {
                currCtrl.moveDragging = true;
            }
        }
        else
        {
            currCtrl.moveDragging = false;
        }

        if (FrameInputInfo.GetInstance().rightButtonDown)
        {
            if (FrameInputInfo.GetInstance().rightBtnPress)
            {
                currCtrl.zoomDragging = true;
            }
        }
        else
        {
            currCtrl.zoomDragging = false;
        }

        if( FrameInputInfo.GetInstance().scroll )
        {
            currCtrl.zoomWheelScroll = true;
        }
        else
        {
            currCtrl.zoomWheelScroll = false;
        }
    }
 
    void _RenderScene()
    {
        currCtrl.mainCam.Render();
    }

    MainViewCtrl currCtrl = null;
}
