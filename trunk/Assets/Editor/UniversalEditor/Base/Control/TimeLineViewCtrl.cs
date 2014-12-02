using UnityEngine;
using System.Collections;
using System.Collections.Generic;


//标尺上的时间标签
public class TimeTag
{
    public string name = "";
    public float time = 0f;
    public Color color = Color.yellow;
    public Color onDragColor = Color.red;
    public bool dragable = true;
    public bool isDragging = false;

    public Rect lastRect = new Rect();

    public TimeTag Copy()
    {
        return this.MemberwiseClone() as TimeTag;
    }
}

public class TimeLineItem
{
    //起始时间
    public float startTime = 0.0f;
    //播放时长
    public float length = 0.0f;
    //是否循环播放
    public bool loop = false;
    //最近一次绘制区域
    public Rect lastRect = new Rect();

    public Rect leftDragLastRect = new Rect();

    public Rect rightDragLastRect = new Rect(); 

    public Color color = Color.black;

    public Color onSelectedColor = Color.red;

    public Color dragBoxColor = Color.gray;

    public Color dragBoxSelectedColor = Color.yellow;

    public TimeLineItem Copy()
    {
        return this.MemberwiseClone() as TimeLineItem;
    }
}

public class TimeLineViewCtrl : EditorControl 
{ 
    public float TotalFrameNum
    {
        get
        {
            return Mathf.Floor(totalTime / secondsPerFrame);
        }
        set
        {
            totalTime = Mathf.Floor(value * secondsPerFrame);
        }
    }

    public float TotalTime
    {
        get { return totalTime; }
        set { totalTime = value; }
    }

    public float CurrFrame
    {
        get
        {
            return Mathf.Floor(currPlayTime / secondsPerFrame);
        }
        set
        {
            currPlayTime = value * secondsPerFrame;
        }
    }

    public float CurrPlayTime
    {
        get { return currPlayTime; }
        set
        {
            currPlayTime = value;
            if (currPlayTime > totalTime)
                currPlayTime = totalTime;
            if (currPlayTime < 0f)
                currPlayTime = 0f;
        }
    }

    public List<TimeLineItem> Items
    {
        get { return items; }
    }

    public List<TimeTag> Tags
    {
        get { return tags; }
    }

    public Vector2 ScrollPos
    {
        get { return scrollPos; }
        set { scrollPos = value; }
    }
     
    public float SecondsPerPixel
    {
        get
        {
            return secondsPerPixel * zoom;
        }

        set
        {
            zoom = value / secondsPerPixel;
            if (zoom < 1.0f)
                zoom = 1.0f;
        }
    }

    public float Zoom
    {
        get { return zoom; }
        set 
        {
            float tmp = zoom;
            zoom = value;
            if (zoom < 1.0f)
            {
                zoom = 1.0f;
            }else if (GetCurrRulerFrameNum() >= rulerMaxFrameNum )
            {
                zoom = tmp;
            } 
        }
    }

    //最近一次选中项
    public int LastSelectedItem
    {
        get { return lastSelectedItem; }
        set { 
            lastSelectedItem = value;
            if (lastSelectedItem >= Items.Count)
                lastSelectedItem = Items.Count - 1;
        }
    }

    //最近一次选中的标记
    public int LastSelectedTag
    {
        get { return lastSelectTag; }
        set
        {
            lastSelectTag = value;
            if (lastSelectTag >= Tags.Count)
                lastSelectTag = Tags.Count - 1;
        }
    }


    public override GUIStyle GetStyle()
    {
        return SpecialEffectEditorStyle.PanelBox;
    }

    public override GUILayoutOption[] GetOptions()
    {
        return new GUILayoutOption[] {  
            GUILayout.ExpandHeight(true), 
            GUILayout.ExpandWidth(true) };
    }
     
    //小刻度像素跨度
    public float GetSmallScalePixelLength()
    {
        float minPixelLen = secondsPerFrame / secondsPerPixel;
        float maxPixelLen = smallScaleMaxPixels;
        float currPixelLen = (secondsPerFrame * framesPerSmallScale) / SecondsPerPixel;

        while (currPixelLen > maxPixelLen)
        {
            framesPerSmallScale /= 2;
            currPixelLen = (secondsPerFrame * framesPerSmallScale) / SecondsPerPixel;

            if (framesPerSmallScale - 1.0f < Mathf.Epsilon)
            {
                framesPerSmallScale = 1.0f;
                break;
            }
        }

        while( currPixelLen < minPixelLen )
        {

            framesPerSmallScale *= 2f;
            currPixelLen = (secondsPerFrame * framesPerSmallScale) / SecondsPerPixel;

            if (framesPerSmallScale - 1.0f < Mathf.Epsilon)
            {
                framesPerSmallScale = 1.0f;
                break;
            }
        } 
        
        return currPixelLen;
    }

    public float GetCurrRulerFrameNum()
    {
        float currPixelLen = (secondsPerFrame * framesPerSmallScale) / SecondsPerPixel;
        float totalSmallScaleCount = rulerTotalPixelLength / currPixelLen;
        return totalSmallScaleCount * framesPerSmallScale;
    }

    //大刻度像素跨度
    public float GetBigScalePixelLength()
    {
        return GetSmallScalePixelLength() * smallScalesPerBigScale;
    }

    //根据像素长度计算可容纳小刻度数
    public float CalcSmallScaleCount( float pixelLength )
    {
        return pixelLength / GetSmallScalePixelLength();
    }
    
    //给定帧数计算像素数
    public float CalcPixelLength( float frames )
    {
        return CalcPixelLengthByTime(frames * secondsPerFrame);
    }

    //给定时间计算在视口中的像素跨度
    public float CalcPixelLengthByTime(float sec)
    {
        return sec / SecondsPerPixel;
    }

    //传入像素长度，返回对应时间
    public float CalcTime( float pixels )
    {
        return SecondsPerPixel * pixels;
    }

    //给定像素位置转换至真实时间，会考虑到
    //标尺像素偏移
    public float Trans2RealTime( float pos )
    {
        return CalcTime(pos - rulerHorizonPixelOffset);
    }


    /*
     * 用于多选的工具函数
     */

    public void RecalcDragMutiSelectRect()
    {
        Vector2 leftTop = new Vector2
            (
                Mathf.Min(mutiSelectRectStartPos.x,mutiSelectRectEndPos.x),
                Mathf.Min(mutiSelectRectStartPos.y,mutiSelectRectEndPos.y)
            );

        float width = Mathf.Abs(mutiSelectRectEndPos.x - mutiSelectRectStartPos.x);
        float height = Mathf.Abs(mutiSelectRectEndPos.y - mutiSelectRectStartPos.y);

        lastMutiSelectRect = new Rect(leftTop.x, leftTop.y, width, height);
    }
    
    public bool IsHighLightBox( int itemIndx , int side )
    { 
        foreach( var info in selectItemInfos )
        {
            if (itemIndx == info.indx && side == info.side)
                return true;
        }
        return false;
    }

    public void AddSelectItemInfo( int i , int side )
    {
        ItemSelectInfo info = new ItemSelectInfo(i, side);
        selectItemInfos.Add(info);
    }

    public void ClearSelectItemInfo()
    {
        selectItemInfos.Clear(); 
    }

    public void UpdateSelectItemInfoListWithMutiSelectRect()
    {
        ClearSelectItemInfo();
        int i = 0; 
        foreach (var item in Items)
        {
            int selState = 0;

            if (lastMutiSelectRect.Overlaps(item.leftDragLastRect))
            {
                selState |= TimeLineViewCtrl.SIDE_LEFT;
            }
            if (lastMutiSelectRect.Overlaps(item.lastRect))
            {
                selState |= TimeLineViewCtrl.SIDE_MID;
            }
            if (lastMutiSelectRect.Overlaps(item.rightDragLastRect))
            {
                selState |= TimeLineViewCtrl.SIDE_RIGHT;
            }


            if ((selState & TimeLineViewCtrl.SIDE_LEFT) > 0)
            {
                AddSelectItemInfo(i, TimeLineViewCtrl.SIDE_LEFT); 
            }
            else if ((selState & TimeLineViewCtrl.SIDE_RIGHT) > 0)
            {
                AddSelectItemInfo(i, TimeLineViewCtrl.SIDE_RIGHT);  
            }
            else if ((selState & TimeLineViewCtrl.SIDE_MID) > 0)
            {
                AddSelectItemInfo(i, TimeLineViewCtrl.SIDE_MID);   
            }  

            i++;
        }
    }

    private EditorControl timeLineListView = null;
    private float totalTime = 10f;
    private float currPlayTime;
    //特效子元素时间轴
    private List<TimeLineItem> items = new List<TimeLineItem>();
    private Vector2 scrollPos = new Vector2();
    private static float secondsPerFrame = 1.0f/30f;

    //时间标记
    private List<TimeTag> tags = new List<TimeTag>();
     
    //最近一次选中的项
    private int lastSelectedItem = -1;
    //最近一次选中的标记
    private int lastSelectTag = -1;
     
    //用于绘图的参数(100像素容纳10帧时间)
    private float secondsPerPixel = 10f * secondsPerFrame / 100.0f;
    //缩放
    private float zoom = 1.0f;
    //小刻度代表多少帧
    public float framesPerSmallScale = 1;
    //标尺起始点偏移像素数
    public float rulerHorizonPixelOffset = 10f;
    //每个大刻度代表多少小刻度
    public float smallScalesPerBigScale = 10f;

    public float smallScaleMinPixels = 2f;
    public float smallScaleMaxPixels = 15f;

    //标尺总像素长度
    public float rulerTotalPixelLength = 3000f;
    //标尺最大能表示的帧数
    public float rulerMaxFrameNum = 10000f;

    //如果鼠标在时间线控件区域内记录
    public Vector2 mousePos = new Vector2();
     
    public bool showMouseTag = false;

    public bool horizonScrollDragging = false;

    ////对多选支持 
    //public bool dragingMutiSelectRect = false;
    ////是否进入多选状态
    //public bool mutiSelectMode = false;

    public enum MutiSelectState
    {
        STATE_NULL = 0,
        STATE_DRAGGING_MUTI_SEL_RECT = 1,
        STATE_MUTI_SEL_CHOOSE_DRAGGER = 2,
        STATE_MUTI_SEL_DRAGGING = 3
    }

    public MutiSelectState mutiSelectState = MutiSelectState.STATE_NULL;
    public Vector2 mutiSelectRectStartPos = Vector2.zero; 
    public Vector2 mutiSelectRectEndPos = Vector2.zero;
    public Rect lastMutiSelectRect = new Rect();


    public class ItemSelectInfo
    {
        private ItemSelectInfo() { }
        public ItemSelectInfo(int i, int s) { indx = i; this.side = s; }

        public int indx = -1;
        public int side = 0;
    }

    //每次选择只能选择时间线上一类
    //拖拽盒：左侧(1)中(2)右(4) 未选中(0)
    public List<ItemSelectInfo> selectItemInfos = new List<ItemSelectInfo>();

    public const int SIDE_LEFT = 1;
    public const int SIDE_MID = 2;
    public const int SIDE_RIGHT = 4;
}

