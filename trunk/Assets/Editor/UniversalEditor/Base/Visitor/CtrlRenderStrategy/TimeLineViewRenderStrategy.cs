using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class TimeLineViewRenderStrategy : EditorRenderStrategy
{
    
    public override void Visit(EditorControl c)
    {
        currCtrl = c as TimeLineViewCtrl;
        if (null == currCtrl)
            return;

        float itemWidth = c.LastRect.width;
        float itemHeight = 19.0f;
        float itemInterval = 3.0f;

        float rulerTotalLength = currCtrl.rulerTotalPixelLength;
        float smallScalesNum = currCtrl.CalcSmallScaleCount(rulerTotalLength);
        float rulerOffset = currCtrl.rulerHorizonPixelOffset;
        float rulerPixelLength = rulerTotalLength + rulerOffset;
        float smallScalePixelLen = currCtrl.GetSmallScalePixelLength();
        float bigScalePixelLen = currCtrl.GetBigScalePixelLength();
        float smallScaleHeight = itemHeight * 0.3f;
        float bigScaleHeight = itemHeight * 0.8f;
        float framesPerSmallScale = currCtrl.framesPerSmallScale;
        float framesPerBigScale = framesPerSmallScale * 10f;     
         

        //时间线款
        float itemTimeLineHeight = itemHeight * 0.4f;
        float itemOffsetHeight = (itemHeight - itemTimeLineHeight) / 2f;

         
        //根据时间轴列表的滚动条位置，移动可视区域
        viewRect = c.LastRect;
        //viewRect.x += currCtrl.ScrollPos.x;
        viewRect.y += currCtrl.ScrollPos.y;
        viewRect.width = rulerOffset + rulerTotalLength;


        Vector2 viewIndependentUpperLeft = new Vector2(c.LastRect.x, c.LastRect.y);
        Vector2 viewDependentUpperLeft = new Vector2(viewRect.x, viewRect.y);
         
        Vector2 localMousePos = c.CalcLocalPos(currCtrl.mousePos);

        EditorGUILayout.BeginVertical(c.GetStyle(), c.GetOptions());

            float scrollPosY = currCtrl.ScrollPos.y;
            currCtrl.ScrollPos =
            GUI.BeginScrollView(c.LastRect, currCtrl.ScrollPos , viewRect, GUI.skin.horizontalScrollbar, GUIStyle.none);

            currCtrl.ScrollPos = new Vector2(currCtrl.ScrollPos.x, scrollPosY);
            
            int i = 0;
            float x = c.LastRect.x;
            float y = c.LastRect.y + itemHeight;
                        
            foreach( var item in currCtrl.Items )
            {//绘制子时间轴
                y += itemInterval;
                y += itemOffsetHeight;

                float itemTimeLineWidth = currCtrl.CalcPixelLengthByTime(item.length);
                float itemTimeLineStartX = currCtrl.CalcPixelLengthByTime(item.startTime);

                Color timeLineItemColor;
                if (i == currCtrl.LastSelectedItem || currCtrl.IsHighLightBox(i,TimeLineViewCtrl.SIDE_MID))
                {
                    timeLineItemColor = item.onSelectedColor;
                    GUI.color = item.onSelectedColor;
                }
                else
                {
                    timeLineItemColor = item.color;
                    GUI.color = item.color;
                }

                item.lastRect = new Rect(x + itemTimeLineStartX + rulerOffset, y, itemTimeLineWidth, itemTimeLineHeight); 
                Drawing.DrawColorBox(item.lastRect, timeLineItemColor);

                float boxHeight = itemTimeLineHeight * 1.5f;
                float boxOffsetHeight = itemTimeLineHeight * 0.25f;
                Rect leftDragBoxRect = new Rect(item.lastRect.x , item.lastRect.y - boxOffsetHeight , boxHeight , boxHeight);
                Rect rightDragBoxRect = new Rect(item.lastRect.x + itemTimeLineWidth - boxHeight, item.lastRect.y - boxOffsetHeight, boxHeight, boxHeight);

                //用于碰撞检测
                item.leftDragLastRect = leftDragBoxRect;
                item.rightDragLastRect = rightDragBoxRect;

                Color leftDragBoxColor = item.dragBoxColor;
                Color rightDragBoxColor = item.dragBoxColor;

                if (currCtrl.IsHighLightBox(i, TimeLineViewCtrl.SIDE_LEFT))
                {
                    leftDragBoxColor = item.dragBoxSelectedColor;
                }

                if (currCtrl.IsHighLightBox(i, TimeLineViewCtrl.SIDE_RIGHT))
                {
                    rightDragBoxColor = item.dragBoxSelectedColor;
                }

                Drawing.DrawColorBox(leftDragBoxRect, leftDragBoxColor);
                Drawing.DrawColorBox(rightDragBoxRect, rightDragBoxColor);

                 
                //若当前时间轴属于拖动状态绘制标线
                if (
                    currCtrl.IsHighLightBox(i, TimeLineViewCtrl.SIDE_MID) ||
                    currCtrl.IsHighLightBox(i, TimeLineViewCtrl.SIDE_LEFT) || 
                    currCtrl.IsHighLightBox(i, TimeLineViewCtrl.SIDE_RIGHT)
                    )
                {
                    //绘制时间线前后两个基准线
                    DrawFullViewVLine(item.lastRect.x, Color.white, 1);
                    DrawFullViewVLine(item.lastRect.x + itemTimeLineWidth, Color.white, 1);

                    //绘制基准线时间标
                    GUI.Box(
                        new Rect(item.lastRect.x + 4, y - 18 , 60, 18),
                        currCtrl.Trans2RealTime(item.lastRect.x - currCtrl.LastRect.x).ToString("f2") + "s",
                        SpecialEffectEditorStyle.TimeLineMouseTag);

                    GUI.Box(
                        new Rect(item.lastRect.x + itemTimeLineWidth + 4,y - 18 , 60, 18),
                        currCtrl.Trans2RealTime(item.lastRect.x + itemTimeLineWidth - currCtrl.LastRect.x ).ToString("f2") + "s",
                        SpecialEffectEditorStyle.TimeLineMouseTag);
                } 
                 
                y += itemTimeLineHeight;
                y += itemOffsetHeight;
                i++;
            }//绘制子时间轴

            GUI.color = Color.white;


            {//绘制标尺
                //标尺主体
                GUI.Box(new Rect(c.LastRect.x, viewRect.y, rulerPixelLength, itemHeight), GUIContent.none  );
                
                //绘制标记
                foreach( var tag in currCtrl.Tags )
                {
                    float tagSize = itemHeight; 
                    float tagOffset = currCtrl.CalcPixelLengthByTime(tag.time);

                    float tagCenterX = viewIndependentUpperLeft.x + rulerOffset + tagOffset;
                    float tagX = tagCenterX - tagSize * 0.5f;

                    tag.lastRect.Set(tagX, viewDependentUpperLeft.y, tagSize, tagSize);

                    Color tagColor = tag.color;
                    if( tag.isDragging )
                    {
                        tagColor = tag.onDragColor;
                    } 
                    Drawing.DrawColorBox(tag.lastRect, tagColor);

                    DrawFullViewVLine(tagCenterX, tagColor, 1);
                }
                  
                //绘制起始线   
                DrawFullViewVLine(rulerOffset + c.LastRect.x , Color.red , 1);
                 
                float mouseLineX = localMousePos.x + viewRect.x + currCtrl.ScrollPos.x;

                //美术认为目前不需要鼠标时间线
                if (false)
                {
                    //绘制鼠标线
                    DrawFullViewVLine(mouseLineX, Color.yellow, 1);


                    //绘制鼠标辅助线所指时间 
                    if (currCtrl.showMouseTag)
                    {
                        GUI.Box(
                            new Rect(mouseLineX + 4,
                                (localMousePos.y - 18) + viewRect.y, 60, 18),
                            currCtrl.Trans2RealTime(localMousePos.x + currCtrl.ScrollPos.x).ToString("f2") + "s",
                            SpecialEffectEditorStyle.TimeLineMouseTag);
                    }
                }
                 
                //绘制当前帧标线
                DrawFullViewVLine(c.LastRect.x + rulerOffset + currCtrl.CalcPixelLengthByTime(currCtrl.CurrPlayTime), Color.gray, 1);
                
                //绘制当前帧标线 
                Vector2 p0 = new Vector2(), p1 = new Vector2();
                  
                //绘制小刻度
                for( int j = 0 ; j < smallScalesNum ; j++ )
                { 
                    float xStart = rulerOffset + c.LastRect.x + j * smallScalePixelLen;
                    float yStart = viewRect.y + (itemHeight - smallScaleHeight);
                    p0 = new Vector2( xStart, yStart);
                    p1 = new Vector2( xStart, yStart + smallScaleHeight);
                    Drawing.DrawLine(p0,p1,Color.black,1f); 

                    //绘制大刻度
                    if (0 == j % (int)currCtrl.smallScalesPerBigScale)
                    {
                        yStart = viewRect.y + (itemHeight - bigScaleHeight);
                        p0 = new Vector2( xStart, yStart);
                        p1 = new Vector2( xStart, yStart + bigScaleHeight);
                        Drawing.DrawLine(p0, p1, Color.black, 1f);

                        //标尺刻度值
                        GUI.Box(new Rect(p0.x+4, viewRect.y, 100, 18), (framesPerSmallScale*j).ToString(), SpecialEffectEditorStyle.RulerNum);
                    }
                }
                 

                //绘制总时间线
                Vector2 startTimePos = new Vector2(rulerOffset + c.LastRect.x, viewRect.y + itemHeight);
                Vector2 endTimePos = new Vector2(rulerOffset + c.LastRect.x + currCtrl.CalcPixelLengthByTime(currCtrl.TotalTime), startTimePos.y); 
                Drawing.DrawLine(startTimePos - new Vector2(0, bigScaleHeight), startTimePos, Color.red, 2);
                Drawing.DrawLine(endTimePos - new Vector2(0, bigScaleHeight), endTimePos, Color.red, 2);
                Drawing.DrawLine(startTimePos, endTimePos, Color.red, 2);
            }//绘制标尺

            {//绘制多选范围
                if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_DRAGGING_MUTI_SEL_RECT)
                {
                    GUI.Box(currCtrl.lastMutiSelectRect, GUIContent.none);
                }
            }//绘制多选范围

            GUI.EndScrollView();
         
        EditorGUILayout.EndVertical();

        //处理所有输入交互
        HandleInput();
         
        c.UpdateLastRect(); 
    }

    //处理所有输入
    void HandleInput()
    {
        Vector2 mousePos = FrameInputInfo.GetInstance().currPos;
        Vector2 mousePosOffset = FrameInputInfo.GetInstance().posOffset;
        Vector2 viewLocalMousePos = FrameInputInfo.GetInstance().currPos + currCtrl.ScrollPos;


        bool lBtnPress = FrameInputInfo.GetInstance().leftBtnPress;
        bool lBtnPressUp = FrameInputInfo.GetInstance().leftBtnPressUp;
        bool lBtnDown = FrameInputInfo.GetInstance().leftButtonDown;
        
        bool mBtnPress = FrameInputInfo.GetInstance().midBtnPress;
        bool mBtnDown = FrameInputInfo.GetInstance().midButtonDown;

        bool rBtnPress = FrameInputInfo.GetInstance().rightBtnPress;
        bool rBtnDown = FrameInputInfo.GetInstance().rightButtonDown;

        bool mouseScroll = FrameInputInfo.GetInstance().scroll;
        Vector2 scrollDelta = FrameInputInfo.GetInstance().delta;


        bool hasInteractSubCtrl = false;

        {//处理所有时间标签响应 
            int selTag = -1;
            int i = 0;
            //对控件的时间标签进行输入处理
            foreach (var tag in currCtrl.Tags)
            {
                if (tag.lastRect.Contains(viewLocalMousePos) && lBtnPress)
                {
                    tag.isDragging = true;
                    selTag = i;
                    currCtrl.LastSelectedTag = i;

                    hasInteractSubCtrl = true;
                    break;
                }
                i++;
            }
             
            if (!lBtnDown)
            {
                foreach (var tag in currCtrl.Tags)
                {
                    tag.isDragging = false;
                }
                selTag = -1;
                if( currCtrl.LastSelectedTag != -1 )
                {
                    currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_END_DRAG_TAG, currCtrl.LastSelectedTag);
                }
                currCtrl.LastSelectedTag = -1; 
            }

            if (selTag != -1)
            {//有新选中项 
                currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_BEGIN_DRAG_TAG, selTag); 
            }

            bool hasDraggingMsg = false;
            foreach (var tag in currCtrl.Tags)
            {
                if (tag.isDragging)
                { 
                    float pixelOffset = FrameInputInfo.GetInstance().posOffset.x; 
                    float timeOffset = currCtrl.CalcTime(pixelOffset);
                    tag.time += timeOffset;
                    if (tag.time < 0f)
                        tag.time = 0f;
                    currCtrl.RequestRepaint();

                    if( Mathf.Abs(pixelOffset) > 0f )
                    {
                        hasDraggingMsg = true;
                    }

                    hasInteractSubCtrl = true;
                }
            }

            if( hasDraggingMsg )
            {
                currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_DRAG_TAG, currCtrl.LastSelectedTag);
            }

        }//处理所有标签响应事件



        {//处理所有时间线响应事件

            //处理鼠标左键点击事件
            if (lBtnPress)
            { 
                if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_CHOOSE_DRAGGER)
                {//若进入多选模式

                    int i = 0;
                    //查看点击的项与矩形是否与多选记录相符
                    foreach (var item in currCtrl.Items)
                    {
                        int selSide = 0;
                        if (item.leftDragLastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_LEFT;
                        }
                        else if(item.rightDragLastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_RIGHT;
                        }
                        else if (item.lastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_MID;
                        }
                        if( selSide > 0)
                        {
                            if( currCtrl.IsHighLightBox(i,selSide) )
                            {//点中多选项中的一项
                                hasInteractSubCtrl = true;
                                //进入多选拖拽模式
                                currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_DRAGGING;
                                TriggerSelectChange(i);
                                TriggerDragItemsBegin();
                                break;
                            }
                            else
                            {//如果选中项与多选项不符
                                currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_NULL; 
                            }
                        }

                        i++;
                    } 
                }


                if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_NULL )
                {//单选点击  
                    currCtrl.ClearSelectItemInfo(); 
                    int i = 0;
                    foreach (var item in currCtrl.Items)
                    {
                        int selSide = 0;
                        if (item.leftDragLastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_LEFT;
                        }
                        else if (item.rightDragLastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_RIGHT;
                        }
                        else if (item.lastRect.Contains(viewLocalMousePos))
                        {
                            selSide = TimeLineViewCtrl.SIDE_MID;
                        }

                        if (selSide > 0)
                        {
                            hasInteractSubCtrl = true;
                            currCtrl.AddSelectItemInfo(i, selSide);
                            break;
                        } 
                        i++;
                    }

                    if( currCtrl.selectItemInfos.Count > 0 )
                    {
                        TriggerSelectChange(i);
                        TriggerDragItemsBegin();
                    }
                }

            }//处理鼠标左键点击事件
             

            if( lBtnDown && 
                ( 
                  currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_NULL ||
                  currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_DRAGGING
                )
              )
            {
                bool triggerDragging = false;
                foreach( var info in currCtrl.selectItemInfos )
                {
                    TimeLineItem item = currCtrl.Items[info.indx]; 

                    if( info.side == TimeLineViewCtrl.SIDE_LEFT )
                    {
                        float newStartX = item.lastRect.x + mousePosOffset.x;

                        if (Mathf.Abs(newStartX - item.lastRect.x) > Mathf.Epsilon)
                        {
                            float newStartTime = currCtrl.Trans2RealTime(newStartX - currCtrl.LastRect.x);
                            float oldEndTime = item.startTime + item.length;

                            if (newStartTime < 0.0f)
                                newStartTime = 0.0f;

                            if (newStartTime > oldEndTime)
                                newStartTime = oldEndTime;

                            float newLength = oldEndTime - newStartTime;

                            item.startTime = newStartTime;
                            item.length = newLength;

                            triggerDragging = true; 
                        }
                    }
                    else if( info.side == TimeLineViewCtrl.SIDE_RIGHT )
                    {
                        float newEndX = item.lastRect.x + item.lastRect.width + mousePosOffset.x;

                        if (Mathf.Abs(newEndX - (item.lastRect.x + item.lastRect.width)) > Mathf.Epsilon)
                        {
                            float newEndTime = currCtrl.Trans2RealTime(newEndX - currCtrl.LastRect.x);

                            if (newEndTime < 0.0f)
                                newEndTime = 0.0f;

                            if (newEndTime < item.startTime)
                                newEndTime = item.startTime;

                            item.length = newEndTime - item.startTime;

                            triggerDragging = true; 
                        }
                    }
                    else if( info.side == TimeLineViewCtrl.SIDE_MID )
                    {
                        float newStartX = item.lastRect.x + mousePosOffset.x;

                        if (Mathf.Abs(newStartX - item.lastRect.x) > Mathf.Epsilon)
                        {
                            float newStartTime = currCtrl.Trans2RealTime(newStartX - currCtrl.LastRect.x);

                            if (newStartTime < 0.0f)
                                newStartTime = 0.0f;
                              
                            item.startTime = newStartTime;

                            triggerDragging = true;
                        }
                    }

                    hasInteractSubCtrl = true;
                }

                if( triggerDragging )
                {
                    TriggerItemsDraging();
                }
            } 

        }//处理所有时间线响应事件


        {//对于松开鼠标左键处理
            if (lBtnPressUp)
            { 
                if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_DRAGGING)
                {
                    TriggerDragItemsEnd();
                    currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_CHOOSE_DRAGGER;
                }
                else if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_NULL)
                {
                    if (currCtrl.selectItemInfos.Count > 0)
                    {
                        TriggerDragItemsEnd();
                    }
                }
            }
        }

        {//处理在整个控件范围内的输入

            //若鼠标在控件区域内
            if (currCtrl.LastRect.Contains(mousePos))
            {
                //左键点下并且之前没有与其他子控件交互
                if( lBtnPress && !hasInteractSubCtrl)
                {//进入多选选择阶段
                    currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_DRAGGING_MUTI_SEL_RECT; 
                    currCtrl.mutiSelectRectStartPos = viewLocalMousePos;
                    currCtrl.ClearSelectItemInfo();
                }

                if( lBtnPressUp )
                {//左键松开
                    if (currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_DRAGGING_MUTI_SEL_RECT)
                    {//进入多选模式
                        if (currCtrl.selectItemInfos.Count > 0)
                        { 
                            if (currCtrl.selectItemInfos.Count > 0)
                            {
                                TriggerSelectChange(currCtrl.selectItemInfos[0].indx);
                            }
                            currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_MUTI_SEL_CHOOSE_DRAGGER;
                        }
                        else
                        { 
                            currCtrl.mutiSelectState = TimeLineViewCtrl.MutiSelectState.STATE_NULL;
                        }
                    }  
                }

                if ( lBtnDown && currCtrl.mutiSelectState == TimeLineViewCtrl.MutiSelectState.STATE_DRAGGING_MUTI_SEL_RECT)
                {
                    currCtrl.mutiSelectRectEndPos = viewLocalMousePos;
                    currCtrl.RecalcDragMutiSelectRect();
                    //根据多选拖拽矩形，更新选中项列表
                    currCtrl.UpdateSelectItemInfoListWithMutiSelectRect();
                    
                  
                }
                

                //控件的鼠标标线时间标签
                if (mBtnDown && mBtnPress)
                {
                    currCtrl.showMouseTag = !currCtrl.showMouseTag;
                }


                //对横向滚动响应
                if (rBtnPress && rBtnDown)
                {
                    currCtrl.horizonScrollDragging = true;
                }

                if (!rBtnDown)
                {
                    currCtrl.horizonScrollDragging = false;
                }


                //缩放时间轴视图
                if (mouseScroll)
                {
                    if (Mathf.Abs(scrollDelta.y) > Mathf.Epsilon)
                    {
                        if (scrollDelta.y > 0.0f)
                        {
                            currCtrl.Zoom *= 1.1f;
                        }
                        else
                        {
                            currCtrl.Zoom /= 1.1f;
                        }
                    }
                }

                currCtrl.mousePos = mousePos;
                currCtrl.RequestRepaint();
            }
            else
            {
                currCtrl.horizonScrollDragging = false; 
            }

            //水平拖动
            if (currCtrl.horizonScrollDragging)
            {
                currCtrl.ScrollPos -= new Vector2(mousePosOffset.x, 0);
            }
        }//处理在整个控件范围内的输入
    }
     
    void TriggerDragItemsBegin()
    { 
        List<int> l = new List<int>();
        foreach( var info in currCtrl.selectItemInfos )
        {
            l.Add(info.indx);
        } 
        currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_BEGIN_DRAG_ITEMS, l); 
        currCtrl.RequestRepaint();
    }

    void TriggerItemsDraging()
    { 
        List<int> l = new List<int>();
        foreach (var info in currCtrl.selectItemInfos)
        {
            l.Add(info.indx);
        }
        currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_DRAG_ITEMS, l);
        currCtrl.RequestRepaint();
    }

    void TriggerDragItemsEnd()
    {
        List<int> l = new List<int>();
        foreach (var info in currCtrl.selectItemInfos)
        {
            l.Add(info.indx);
        }
        currCtrl.PostMessage(ControlMessage.Message.TIMELINECTRL_END_DRAG_ITEMS, l);
        currCtrl.RequestRepaint();
    }

    void TriggerSelectChange( int sel )
    {
        currCtrl.frameTriggerInfo.lastSelectItem = sel;
    }
    void DrawFullViewVLine( float x , Color color , float width )
    { 
        Drawing.DrawLine(
                   new Vector2(x, viewRect.y),
                   new Vector2(x, viewRect.y + viewRect.height), color, 1
       );
    }

    private TimeLineViewCtrl currCtrl = null;
    private Rect viewRect; 
}
