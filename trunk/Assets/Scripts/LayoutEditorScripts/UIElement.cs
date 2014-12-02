using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class UIElement : MonoBehaviour
{
    [SerializeField]
    bool _Hide;
    [SerializeField]
    bool _Removed;
    [SerializeField]
    public bool Freeze;
    [SerializeField]
    public bool Lock;

    UIWidget.Pivot m_pivot = UIWidget.Pivot.BottomLeft;
    Vector3[] m_Corners = new Vector3[4];

    public string FullPathName;
    
    public string Name
    {
        get { return gameObject.name; }
        set { gameObject.name = value; }
    }
    public bool Hide
    {
        get { return _Hide; }
        set
        {
            _Hide = value;
            gameObject.SetActive(Visible);
        }
    }
    public bool Removed
    {
        get { return _Removed; }
        set
        {
            _Removed = value;
            gameObject.SetActive(Visible);
        }
    }
    public bool IsUI
    {
        get { return (Lock ? !CanRemove() : GetWidget() != null); }
    }
    public int ChildrenCount
    {
        get
        {
            return transform.GetChildCount();
        }
    }

    // 为了实现锁定控件，对UIWidget接口的包装
    public UIWidget.Pivot pivot
    {
        get
        {
            if (Lock)
            {
                return m_pivot;
            }
            else
            {
                UIWidget w = GetWidget();

                return (w != null ? w.pivot : UIWidget.Pivot.BottomLeft);
            }
        }
        set
        {
            if (Lock)
            {
                m_pivot = value;
            }
            else
            {
                UIWidget w = GetWidget();

                if (w != null)
                {
                    w.pivot = value;
                }
            }
        }
    }
    public float width
    {
        get
        {
            if (Lock)
            {
                Vector3[] world_Corners = worldCorners;

                return world_Corners[2].x - world_Corners[0].x;
            }
            else
            {
                UIWidget w = GetWidget();

                return (w != null ? w.width : 0);
            }
        }
        set
        {
            if (Lock)
            {
                //
            }
            else
            {
                UIWidget w = GetWidget();

                if (w != null)
                {
                    if (value < w.minWidth)
                    {
                        value = w.minWidth;
                    }
                    w.width = (int)(value + 0.5f);
                }
            }
        }
    }
    public float height
    {
        get
        {
            if (Lock)
            {
                Vector3[] world_Corners = worldCorners;

                return world_Corners[2].y - world_Corners[0].y;
            }
            else
            {
                UIWidget w = GetWidget();

                return (w != null ? w.height : 0);
            }
        }
        set
        {
            if (Lock)
            {
                //
            }
            else
            {
                UIWidget w = GetWidget();

                if (w != null)
                {
                    if (value < w.minHeight)
                    {
                        value = w.minHeight;
                    }
                    w.height = (int)(value + 0.5f);
                }
            }
        }
    }
    public Vector3[] worldCorners
    {
        get
        {
            if (Lock)
            {
                if (m_Corners == null)
                {
                    m_Corners = new Vector3[4];
                    m_Corners[0] = new Vector3();
                    m_Corners[1] = new Vector3();
                    m_Corners[2] = new Vector3();
                    m_Corners[3] = new Vector3();
                }

                float x0 = float.MaxValue;
                float y0 = float.MaxValue;
                float x1 = float.MinValue;
                float y1 = float.MinValue;
                UIWidget[] widgets = GetComponentsInChildren<UIWidget>(true);

                for (int i = 0; i < widgets.Length; ++i)
                {
                    ExpandRect(widgets[i].worldCorners, ref x0, ref y0, ref x1, ref y1);
                }

                m_Corners[0].x = x0;
                m_Corners[0].y = y0;
                m_Corners[1].x = x0;
                m_Corners[1].y = y1;
                m_Corners[2].x = x1;
                m_Corners[2].y = y1;
                m_Corners[3].x = x1;
                m_Corners[3].y = y0;

                return m_Corners;
            }
            else
            {
                UIWidget w = GetWidget();

                return (w != null ? w.worldCorners : m_Corners);
            }
        }
    }
    public Vector3 Pos
    {
        get
        {
            if (Lock)
            {
                Vector3[] world_Corners = worldCorners;
                float x = 0;
                float y = 0;

                switch (pivot)
                {
                    case UIWidget.Pivot.TopLeft:
                    case UIWidget.Pivot.Left:
                    case UIWidget.Pivot.BottomLeft:
                        x = world_Corners[0].x;
                        break;
                    case UIWidget.Pivot.Top:
                    case UIWidget.Pivot.Center:
                    case UIWidget.Pivot.Bottom:
                        x = (world_Corners[0].x + world_Corners[2].x) / 2;
                        break;
                    case UIWidget.Pivot.TopRight:
                    case UIWidget.Pivot.Right:
                    case UIWidget.Pivot.BottomRight:
                        x = world_Corners[2].x;
                        break;
                }
                switch (pivot)
                {
                    case UIWidget.Pivot.BottomLeft:
                    case UIWidget.Pivot.Bottom:
                    case UIWidget.Pivot.BottomRight:
                        y = world_Corners[0].y;
                        break;
                    case UIWidget.Pivot.Left:
                    case UIWidget.Pivot.Center:
                    case UIWidget.Pivot.Right:
                        y = (world_Corners[0].y + world_Corners[2].y) / 2;
                        break;
                    case UIWidget.Pivot.TopLeft:
                    case UIWidget.Pivot.Top:
                    case UIWidget.Pivot.TopRight:
                        y = world_Corners[2].y;
                        break;
                }

                return new Vector3(x, y);
            }
            else
            {
                return transform.position;
            }
        }
        set
        {
            if (Lock)
            {
                Vector3 move_delta = value - Pos;
                transform.position += move_delta;
            }
            else
            {
                transform.position = value;
            }
        }
    }
    public Vector3 LocalPos
    {
        get
        {
            if (Lock)
            {
                Transform parent = transform.parent;

                if (parent != null)
                {
                    return Pos - parent.position;
                }
                return Pos;
            }
            else
            {
                return transform.localPosition;
            }
        }
        set
        {
            if (Lock)
            {
                Vector3 move_delta = value - LocalPos;
                transform.localPosition += move_delta;
            }
            else
            {
                transform.localPosition = value;
            }
        }
    }
    public void Move(Vector3 move_delta)
    {
        transform.position += GetRealMoveDelta(GetParentUI(), this, move_delta);
    }
    public bool SetWidthDelta(int scale, float delta)
    {
        bool res = false;

        if (Lock)
        {
            //
        }
        else
        {
            UIWidget w = GetWidget();

            if (w != null)
            {
                int new_width = GetRealWidth(GetParentUI(), w, scale, delta);

                if (new_width != w.width)
                {
                    res = true;
                    if (scale < 0)
                    {
                        Move(new Vector3(w.width - new_width, 0));
                    }
                    w.width = new_width;
                }
            }
        }

        return res;
    }
    public bool SetHeightDelta(int scale, float delta)
    {
        bool res = false;

        if (Lock)
        {
            //
        }
        else
        {
            UIWidget w = GetWidget();

            if (w != null)
            {
                int new_height = GetRealHeight(GetParentUI(), w, scale, delta);

                if (new_height != w.height)
                {
                    res = true;
                    if (scale < 0)
                    {
                        Move(new Vector3(0, w.height - new_height));
                    }
                    w.height = new_height;
                }
            }
        }

        return res;
    }
    //

    // 节点身上的UI元素（UIWidget），可能返回空
    public UIWidget GetWidget()
    {
        return (Lock ? null : GetComponent<UIWidget>());
    }
    public UIElement GetChild(int index)
    {
        if (index < 0 || index >= ChildrenCount)
        {
            return null;
        }
        return transform.GetChild(index).GetComponent<UIElement>();
    }

    // 增加子节点, 返回值为新增节点。锁定的节点不能增加子节点
    public UIElement AddChildNode()
    {
        if (Lock)
        {
            return null;
        }

        GameObject new_go = new GameObject();
        new_go.transform.parent = transform;

        return new_go.AddComponent("UIElement") as UIElement;
    }
    public bool SetParent(UIElement ui_element)
    {
        if (ui_element == null || ui_element.Lock)
        {
            return false;
        }
        transform.parent = ui_element.transform;
        return true;
    }
    public bool RemoveNode()
    {
        if (!CanRemove())
        {
            return false;
        }
        Object.DestroyImmediate(gameObject);

        return true;
    }
    // 是否可以删除该节点，含有UI元素（UIWidget）的节点不能删除
    public bool CanRemove()
    {
        return (GetComponentsInChildren<UIWidget>(true).Length == 0);
    }

    public bool VisbleGolbal
    {
        get { return Visible && gameObject.activeInHierarchy; }
    }

    bool Visible
    {
        get { return (!_Hide && !_Removed); }
    }

    void ExpandRect(Vector3[] corners, ref float x0, ref float y0, ref float x1, ref float y1)
    {
        if (corners[0].x < x0)
        {
            x0 = corners[0].x;
        }
        if (corners[0].y < y0)
        {
            y0 = corners[0].y;
        }
        if (corners[2].x > x1)
        {
            x1 = corners[2].x;
        }
        if (corners[2].y > y1)
        {
            y1 = corners[2].y;
        }
    }
    UIElement GetParentUI()
    {
        UIElement ui_element = (transform.parent != null ? transform.parent.GetComponent<UIElement>() : null);

        while (ui_element != null)
        {
            if (ui_element.IsUI)
            {
                break;
            }
            ui_element = (ui_element.transform.parent != null ? ui_element.transform.parent.GetComponent<UIElement>() : null);
        }

        return ui_element;
    }
    int GetRealWidth(UIElement parent_ui, UIWidget ui, int scale, float move_delta)
    {
        float delta = scale * move_delta;

        if (delta > 0 && parent_ui != null)
        {
            delta = Mathf.Min(delta, Mathf.Max(0, (scale > 0 ? parent_ui.Pos.x + parent_ui.width - (ui.transform.position.x + ui.width) : ui.transform.position.x - parent_ui.Pos.x)));
        }

        float new_width = ui.width + delta;

        if (new_width < ui.minWidth)
        {
            new_width = ui.minWidth;
        }

        return (int)(new_width + 0.5f);
    }
    int GetRealHeight(UIElement parent_ui, UIWidget ui, int scale, float move_delta)
    {
        float delta = scale * move_delta;

        if (delta > 0 && parent_ui != null)
        {
            delta = Mathf.Min(delta, Mathf.Max(0, (scale > 0 ? parent_ui.Pos.y + parent_ui.height - (ui.transform.position.y + ui.height) : ui.transform.position.y - parent_ui.Pos.y)));
        }

        float new_height = ui.height + delta;

        if (new_height < ui.minHeight)
        {
            new_height = ui.minHeight;
        }

        return (int)(new_height + 0.5f);
    }
    Vector3 GetRealMoveDelta(UIElement parent_ui, UIElement ui, Vector3 move_delta)
    {
        if (parent_ui == null)
        {
            return move_delta;
        }

        float x_delta = parent_ui.Pos.x - ui.Pos.x + (move_delta.x > 0 ? parent_ui.width - ui.width : 0);
        float y_delta = parent_ui.Pos.y - ui.Pos.y + (move_delta.y > 0 ? parent_ui.height - ui.height : 0);

        x_delta = (move_delta.x > 0 ? Mathf.Min(move_delta.x, Mathf.Max(x_delta, 0)) : Mathf.Max(move_delta.x, Mathf.Min(x_delta, 0)));
        y_delta = (move_delta.y > 0 ? Mathf.Min(move_delta.y, Mathf.Max(y_delta, 0)) : Mathf.Max(move_delta.y, Mathf.Min(y_delta, 0)));

        return new Vector3(x_delta, y_delta);
    }
}
