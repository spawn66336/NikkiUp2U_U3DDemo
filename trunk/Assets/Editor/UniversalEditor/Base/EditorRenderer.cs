using UnityEngine;
using System.Collections;
using UnityEditor;


public class FrameInputInfo
{
    public Vector2 currPos = new Vector2();
    public Vector2 posOffset = new Vector2();
    //鼠标滚轮
    public Vector2 delta = new Vector2();

    public bool leftButtonDown = false;
    public bool midButtonDown = false;
    public bool rightButtonDown = false;

    public bool leftBtnPress = false;
    public bool midBtnPress = false;
    public bool rightBtnPress = false;

    public bool leftBtnPressUp = false;
    public bool midBtnPressUp = false;
    public bool rightBtnPressUp = false;

    public bool drag = false;

    //鼠标滚轮滚动
    public bool scroll = false;

    //用于外部向编辑器拖拽物体
    public bool dragingObjs = false;
    public bool dragObjsPerform = false;
    public Object[] dragObjs = null;
    public string[] dragObjsPaths = null;

     

    public void Update()
    {
        _FrameReset();

        Event currEvent = Event.current;
        Vector2 lastMousePos = currPos;
        currPos = currEvent.mousePosition;
        posOffset = currPos - lastMousePos;

        switch (currEvent.type)
        {
            case EventType.MouseDown:
                if (currEvent.button == 0)
                {
                    //若上一帧鼠标键没有按下
                    if (leftButtonDown == false)
                    {
                        leftBtnPress = true;
                    }
                    leftButtonDown = true;
                }

                //鼠标右键按下
                if( currEvent.button == 1)
                {
                    if(rightButtonDown == false )
                    {
                        rightBtnPress = true;
                    }
                    rightButtonDown = true;
                }

                //鼠标中键按下
                if( currEvent.button == 2)
                {
                    if (midButtonDown == false)
                    {
                        midBtnPress = true;
                    }
                    midButtonDown = true; 
                }
                break;

            case EventType.MouseUp:
                if (currEvent.button == 0)
                {
                    if( leftButtonDown )
                    {
                        leftBtnPressUp = true;
                    }
                    leftButtonDown = false;
                }

                if(currEvent.button == 1)
                {
                    if( rightButtonDown )
                    {
                        rightBtnPressUp = true;
                    }
                    rightButtonDown = false;
                }

                if(currEvent.button == 2)
                {
                    if( midButtonDown )
                    {
                        midBtnPressUp = true;
                    }
                    midButtonDown = false;
                } 
                break;

            case EventType.ScrollWheel:
                scroll = true;
                delta = currEvent.delta;
                break;
                 
            case EventType.MouseDrag:
                drag = true; 
                break; 
            case EventType.DragUpdated:
                dragingObjs = true;
                dragObjs = DragAndDrop.objectReferences;
                dragObjsPaths = DragAndDrop.paths; 
                break;
            case EventType.DragPerform:
                dragingObjs = true;
                dragObjsPerform = true;
                dragObjs = DragAndDrop.objectReferences;
                dragObjsPaths = DragAndDrop.paths; 
                break;
            case EventType.DragExited:
                dragingObjs = true;
                dragObjs = DragAndDrop.objectReferences;
                dragObjsPaths = DragAndDrop.paths; 
                break;

            default:
                break;
        }
    }

    void _PrintDragItems()
    {
        foreach( var obj in 
            DragAndDrop.objectReferences )
        {
            Debug.Log("obj = " + obj.name);
        }

        foreach (var path in
            DragAndDrop.paths)
        {
            Debug.Log("path = " + path);
        }
    }

    void _FrameReset()
    {
        leftBtnPress = false;
        midBtnPress = false;
        rightBtnPress = false;

        leftBtnPressUp = false;
        midBtnPressUp = false;
        rightBtnPressUp = false;

        drag = false;
        scroll = false;

        dragingObjs = false;
        dragObjsPerform = false;
        dragObjs = null;
        dragObjsPaths = null;
    }
     

    public static FrameInputInfo GetInstance()
    {
        return currInputInfo;
    }

    public static void SetCurrInputInfo( FrameInputInfo info )
    {
        currInputInfo = info;
    }

    private static FrameInputInfo currInputInfo = null;
}

public class EditorRenderer  
{
    private  LayoutUpdateVisitor layoutCalcVisitor = new LayoutUpdateVisitor();
    private  EditorCtrlVisitor renderVisitor = new EditorRenderVisitor();
    private  EditorCtrlVisitor triggerVisitor = new TriggerVisitor();
    private  EditorCtrlVisitor resetTriggerInfoVisitor = new ResetTriggerInfoVisitor();
    private  UpdateVisitor updateVisitor = new UpdateVisitor();
    private DestroyVisitor destroyVisitor = new DestroyVisitor();
   
    public void RequestRepaint()
    {
        repaint = true;
    }

    //是否重绘编辑器，在本帧被请求过
    public bool IsRepaintRequested()
    {
        return repaint;
    }

    //在OnGUI中调用
    public void Render( EditorControl root , Rect wndRect )
    {

        if (repaint)
        { 
            repaint = false;
        }

        
         
        layoutCalcVisitor.areaStack.Clear();
        layoutCalcVisitor.areaStack.Push(wndRect);
        //计算布局
        root.Traverse(layoutCalcVisitor);
        //渲染控件树
        root.Traverse(renderVisitor);
        //触发本帧的回调
        root.Traverse(triggerVisitor);
        //清除控件节点中本帧记录的操作
        root.Traverse(resetTriggerInfoVisitor); 

    }


    //每帧都需调用，用于执行控件每帧都需的更新任务
    public void Update( EditorControl root )
    {
        deltaTime = Time.realtimeSinceStartup - lastFrameTime;
        lastFrameTime = Time.realtimeSinceStartup;

        updateVisitor._InternalUpdate(deltaTime);
        root.Traverse(updateVisitor);  
    }

    public void Destroy( EditorControl root )
    {
        root.Traverse(destroyVisitor);
    }


    private bool repaint = false;
    private float lastFrameTime = 0.0f;
    private float deltaTime = 0.0f;
 
    	 
}
