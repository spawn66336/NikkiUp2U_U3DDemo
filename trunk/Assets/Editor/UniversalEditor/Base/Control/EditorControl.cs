using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class TriggerInfo
{
    //本帧是否点击
    public bool isClick = false;
    public bool isHover = false;
    public bool isValueChanged = false;
    public bool isScroll = false;
     

    public bool isDraggingObjs = false;
    public bool isDropObjs = false;
     
    public int lastSelectItem = -1;
    //add by liteng for atlas start
    public int lastSelectItemR = -1;
    public int lastSelectItemRU = -1;
    //add by liteng for atlas end
    public Vector2 scrollPos = new Vector2();
    public void Reset()
    {
        isClick = false;
        isHover = false;
        isValueChanged = false;
        isScroll = false;  
        lastSelectItem = -1;
        //add by liteng for atlas start
        lastSelectItemR = -1;
        lastSelectItemRU = -1;
        //add by liteng for atlas end
        scrollPos.Set(0f, 0f);

        isDraggingObjs = false;
        isDropObjs = false;
    }
}


public class EditorControl 
{
    public EditorRoot Root
    {
        get { return root; }
        set { root = value; }
    }

    public EditorControl Parent
    {
        get { return parent; }
        set { parent = value; }
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public string Caption
    {
        get { return caption; }
        set { caption = value; }
    }

    public bool IsRoot
    {
        get { return parent == null; }
    }

    public Rect Size
    {
        get { return sizeRect; }
        set 
        { 
            sizeRect = value; 
        }
    }

    public Rect LastRect
    {
        get { return lastRect; }
    }

    public float CurrValue
    {
        get { return currValue;}
        set { currValue = value; }
    }

    public float ValueEpsilon
    {
        get { return valueEpsilon; }
        set { valueEpsilon = value; }
    }

    public Vector2 ValueRange
    {
        get { return valueRange; }
        set { valueRange = value; }
    }

    public bool Enable
    {
        get { return enable; }
        set { enable = value; }
    }

    public delegate void VoidDelegate( EditorControl c );
    public delegate void BoolDelegate( EditorControl c , bool state);
    public delegate void FloatDelegate( EditorControl c , float value);
    public delegate void IntDelegate(EditorControl c, int value);
    public delegate void Vec2Delegate(EditorControl c, Vector2 scrollPos);
    public delegate void DragObjsDelegate(EditorControl c, Object[] objs, string[] paths);
    public delegate bool AcceptDragObjsDelegate(EditorControl c, Object[] objs, string[] paths); 

    //记录本帧的操作信息
    public TriggerInfo frameTriggerInfo = new TriggerInfo();

    //布局约束
    public LayoutConstraint layoutConstraint = new LayoutConstraint();
    
    //消息回调
    public VoidDelegate  onClick = null;
    public VoidDelegate  onHover = null;
    public FloatDelegate onValueChange = null;
    public IntDelegate   onItemSelected = null;
    //Add by liteng for atlas start
    public IntDelegate onItemSelectedR = null;
    public IntDelegate onItemSelectedRU = null;
    //Add by liteng for atlas end
    public Vec2Delegate  onScroll = null;

    public IntDelegate onDragItemBegin = null;
    public IntDelegate onDragItem = null;
    public IntDelegate onDragItemEnd = null;

    //用于外部向编辑器拖拽资源
    public DragObjsDelegate onDragingObjs = null;
    public DragObjsDelegate onDropObjs = null;
    //用于判断是否接收拖拽物体
    public AcceptDragObjsDelegate onAcceptDragObjs = null;

    public virtual void Add(EditorControl c) { }
    public virtual void Remove(EditorControl c) { }
    public virtual EditorControl GetAt(int i) { return null; }
    public virtual int GetChildCount() { return 0; }
    public virtual void Traverse(EditorCtrlVisitor v) { v.Visit(this); }

  
    public virtual GUIStyle GetStyle() { return null; }
    public virtual GUILayoutOption[] GetOptions() 
    { 
        if( layoutConstraint != null )
             return layoutConstraint.GetOptions();
        return null;
    }

    public void UpdateLastRect()
    { 
        SpecialEffectEditorUtility.GetLastRect( ref lastRect );
    }
    
    public Vector2 CalcLocalPos( Vector2 p )
    {
        return  p - new Vector2(lastRect.x, lastRect.y);
    }

    public void RequestRepaint()
    {
        if (null != Root)
        {
            Root.renderer.RequestRepaint();
        }
    }

    //用于控件发送消息
    public void PostMessage( ControlMessage.Message msg , object p0 = null, object p1 = null )
    {
        if( null != Root )
        {
            ControlMessage newMsg = new ControlMessage(this, msg, p0, p1);
            Root.EnqueueMessage(newMsg);
        }
    }

    //控件所属编辑器窗口
    protected EditorRoot root;
    //控件名
    private string name;
    //控件标题
    private string caption;
    //控件的大小
    private Rect sizeRect = new Rect();
    //控件最终绘制大小
    private Rect lastRect = new Rect();
    //控件当前值
    private float currValue = 0.0f;
    //控件变更阈值
    private float valueEpsilon = 0.001f;
    //控件值域
    private Vector2 valueRange = new Vector2();
    //父控件
    private EditorControl parent;
    //控件是否为启用状态
    private bool enable = true;
}

public class EditorCtrlComposite : EditorControl
{


    public override void Add(EditorControl c) 
    { 
        //若找到相同控件则不加入列表
        if (children.Contains(c))
            return;

        c.Root = this.Root;
        c.Parent = this;
        children.Add(c); 
    }

    public override void Remove(EditorControl c) 
    {
        if( children.Remove(c) )
        {
            c.Parent = null;
        }
    }

    public override EditorControl GetAt(int i) 
    {
        if (i < 0 || i >= children.Count)
            return null;
 
        return children[i];
    }

    public override int GetChildCount() 
    { 
        return children.Count; 
    }

    public override void Traverse(EditorCtrlVisitor v) 
    {
        //若在预访问阶段判断不通过，则不访问此子树
        if (!v.PreVisit(this))
            return;

        v.Visit(this);
        v.AfterVisit(this);

        if (!v.PreVisitChildren(this))
            return;

        int i = 0;
        foreach( var child in children )
        {
            if (v.PreVisitChild(this, i))
            {
                child.Traverse(v);
                v.AfterVisitChild(this, i);
            }
            i++;
        }
        v.AfterVisitChildren(this);
    }

    //子控件
    protected List<EditorControl> children = new List<EditorControl>();
}