
//定义NGUI版本
#define NGUI_3_5_8

using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using H3DEditor;

public class Layout
{
    GameObject m_root;
    List<UIElement> m_sel_elements = new List<UIElement>();
    Dictionary<int, List<UIElement>> m_format_uilist_cache = new Dictionary<int, List<UIElement>>();
    bool m_dirty;
    string m_layout_file;
    string m_name;
    bool m_visible = true;

    public enum FormatType
    {
        FT_AlignLeft,
        FT_AlignVCenter,
        FT_AlignRight,
        FT_AlignTop,
        FT_AlignHCenter,
        FT_AlignBottom,
        FT_SameSize,
        FT_SameXSpace,
        FT_SameYSpace,
    }

    public bool Load(string layout_file)
    {
        GameObject root = LayoutTool.LoadLayout(layout_file, true);

        if (root != null)
        {
            Release();
            m_root = root;
            m_dirty = false;
            m_layout_file = layout_file;
            m_name = EditorTool.GetFileName(layout_file, false);
            UpdateHierarchyInfo(m_root, 0);
        }

        return (root != null);
    }
    public bool Reload()
    {
        if (!File.Exists(FileName))
        {
            return false;
        }

        return Load(FileName);
    }
    public void Dispose()
    {
        Release();
    }
    void Release()
    {
        if (m_root != null)
        {
            Object.DestroyImmediate(m_root);
            m_root = null;
        }
        ClearSel();
    }
    void ClearFormatCache()
    {
        m_format_uilist_cache.Clear();
    }

    public bool Visible
    {
        get { return m_visible; }
        set
        {
            if (m_root != null && m_visible != value)
            {
                m_visible = value;
                m_root.SetActive(m_visible);
            }
        }
    }
    public string Name
    {
        get { return m_name; }
    }
    public string FileName
    {
        get { return m_layout_file; }
    }
    public UIElement Root
    {
        get { return (m_root != null ? m_root.GetComponent<UIElement>() : null); }
    }
    public List<UIElement> SelElements
    {
        get { return m_sel_elements; }
    }
    public UIElement FirstUI
    {
        get
        {
            for (int i = 0; i < m_sel_elements.Count; ++i)
            {
                if (m_sel_elements[i].IsUI || m_sel_elements[i].CanEdit)
                {
                    return m_sel_elements[i];
                }
            }
            return null;
        }
    }
    private void UpdateHierarchyInfo(GameObject root, int nPos)
    {
        UIElement ui = root.GetComponent<UIElement>();
        if(ui != null)
        {
            ui.HierarchyPos = nPos;
        }

        GameObject[] child = EditorTool.GetSubChildren(root);
        foreach(GameObject go in child)
        {
            UpdateHierarchyInfo(go, nPos + 1);
        }
    }
    private int ComparisonFunc(UIElement ui_element_1, UIElement ui_element_2)
    {
        if (ui_element_1 == ui_element_2)
            return 0;

        if (ui_element_1.HierarchyPos != ui_element_2.HierarchyPos)
        {
            return ui_element_2.HierarchyPos.CompareTo(ui_element_1.HierarchyPos);
        }

        UIWidget w1 = ui_element_1.GetComponent<UIWidget>();
        UIWidget w2 = ui_element_2.GetComponent<UIWidget>();
        if(w1 != null && w2 != null)
        {
            if (w2.depth != w1.depth)
            {
                return w2.depth.CompareTo(w1.depth);
            }

            return MathTool.CompareRect(w1.worldCorners, w2.worldCorners);
        }

        return 0;
    }

    public UIElement RayTest(Vector3 mousePos)
    {
        List<UIElement> uiele_list = new List<UIElement>();
        UIElement[] eles = m_root.GetComponentsInChildren<UIElement>();
        for (int i = 0; i < eles.Length; ++i)
        {
            UIElement ui = eles[i];
            UIWidget w = ui.GetComponent<UIWidget>();
            BoxCollider box = ui.GetComponent<BoxCollider>();
            if(w != null || box != null)
            {
                if(!ui.Freeze)
                {
                    uiele_list.Add(ui);
                }
            }
        }
        //widget_list.Sort(delegate(UIWidget w1, UIWidget w2) { return w2.depth.CompareTo(w1.depth); });
        uiele_list.Sort(ComparisonFunc);

        for (int i = 0; i < uiele_list.Count; ++i)
        {
            UIElement ui_element = uiele_list[i];
            UIElement ui = ui_element;
            UIWidget w = ui_element.GetComponent<UIWidget>();

            Vector3[] corners = null;
            if(w != null)
            {
                corners = w.worldCorners;
            }
            else
            {
                corners = ui_element.worldCorners;
            }
            if (corners == null)
                continue;

            if (MathTool.IsPointInConvexPoly(mousePos, corners))
            {
                UIElement topmost_lock_ui = null;
                while (ui_element != null)
                {
                    if (ui_element.Lock)
                    {
                        topmost_lock_ui = ui_element;
                    }
                    ui_element = (ui_element.transform.parent != null ? ui_element.transform.parent.GetComponent<UIElement>() : null);
                }

                return (topmost_lock_ui != null ? topmost_lock_ui : ui);
            }
        }

        return null;
    }
    public List<UIElement> GetAllUIs(bool include_freeze)
    {
        List<UIElement> all_ui_list = new List<UIElement>();

        if (m_root != null)
        {
            Stack<GameObject> stack = new Stack<GameObject>();

            stack.Push(m_root);
            while (stack.Count > 0)
            {
                GameObject cur_go = stack.Pop();
                UIElement ui_element = cur_go.GetComponent<UIElement>();

                if (ui_element != null)
                {
                    if (!ui_element.Lock)
                    {
                        GameObject[] children = EditorTool.GetSubChildren(cur_go);

                        for (int i = 0; i < children.Length; ++i)
                        {
                            stack.Push(children[i]);
                        }
                    }
                    if (ui_element.CanEdit && (include_freeze || !ui_element.Freeze) && ui_element.VisbleGolbal)
                    {
                        all_ui_list.Add(ui_element);
                    }
                }
            }
        }

        return all_ui_list;
    }
    public List<UIElement> GetMoveUIs()
    {
        List<UIElement> ui_list = new List<UIElement>();

        for (int i = 0; i < m_sel_elements.Count; ++i)
        {
            UIElement w = m_sel_elements[i];

            if (w.CanEdit)
            {
                bool has_parent_in_list = false;
                Transform parent = w.transform.parent;

                while (parent != null)
                {
                    if (IsElementSelected(parent.GetComponent<UIElement>()))
                    {
                        has_parent_in_list = true;
                        break;
                    }
                    parent = parent.transform.parent;
                }
                if (!has_parent_in_list)
                {
                    ui_list.Add(w);
                }
            }
        }

        return ui_list;
    }
    private List<UIElement> GetFormatUIs(FormatType format_type)
    {
        int need_count = GetFormatNeedCount(format_type);

        if (m_format_uilist_cache.ContainsKey(need_count))
        {
            return m_format_uilist_cache[need_count];
        }

        List<UIElement> ui_list = new List<UIElement>();

        for (int i = 0; i < m_sel_elements.Count; ++i)
        {
            UIElement w = m_sel_elements[i];

            if (w.IsUI)
            {
                if (ui_list.Count == 0 || ui_list[0].transform.parent == w.transform.parent)
                {
                    ui_list.Add(w);
                }
            }
        }
        if (ui_list.Count < need_count)
        {
            ui_list.Clear();
        }
        m_format_uilist_cache.Add(need_count, ui_list);

        return ui_list;
    }
    private int GetFormatNeedCount(FormatType format_type)
    {
        switch (format_type)
        {
            case FormatType.FT_AlignLeft:
            case FormatType.FT_AlignVCenter:
            case FormatType.FT_AlignRight:
            case FormatType.FT_AlignTop:
            case FormatType.FT_AlignHCenter:
            case FormatType.FT_AlignBottom:
            case FormatType.FT_SameSize:
                return 2;
            case FormatType.FT_SameXSpace:
            case FormatType.FT_SameYSpace:
                return 3;
        }

        return 2;
    }
    private UIWidget.Pivot GetPivotForFormat(FormatType format_type)
    {
        switch (format_type)
        {
            case FormatType.FT_AlignLeft:
                return UIWidget.Pivot.Left;
            case FormatType.FT_AlignRight:
                return UIWidget.Pivot.Right;
            case FormatType.FT_AlignTop:
                return UIWidget.Pivot.Top;
            case FormatType.FT_AlignBottom:
                return UIWidget.Pivot.Bottom;
            case FormatType.FT_AlignVCenter:
            case FormatType.FT_AlignHCenter:
                return UIWidget.Pivot.Center;
            case FormatType.FT_SameSize:
            case FormatType.FT_SameXSpace:
            case FormatType.FT_SameYSpace:
                return UIWidget.Pivot.BottomLeft;
        }

        return UIWidget.Pivot.BottomLeft;
    }

    public void ClearSel()
    {
        m_sel_elements.Clear();
        ClearFormatCache();
    }
    public bool SelElement(UIElement ui_element)
    {
        if (ui_element != null && !IsElementSelected(ui_element) && !ui_element.Freeze && ui_element.VisbleGolbal)
        {
            m_sel_elements.Add(ui_element);
            ClearFormatCache();
            return true;
        }
        return false;
    }
    public void UnSelElement(UIElement ui_element)
    {
        if (ui_element != null)
        {
            m_sel_elements.Remove(ui_element);
            ClearFormatCache();
        }
    }
    public bool IsElementSelected(UIElement ui_element)
    {
        if (ui_element == null)
        {
            return false;
        }
        return (m_sel_elements.IndexOf(ui_element) != -1);
    }

    public bool ValidFormat(FormatType format_type)
    {
        return (GetFormatUIs(format_type).Count > 0);
    }
    public void Format(FormatType format_type)
    {
        List<UIElement> list = GetFormatUIs(format_type); 
        CmdManager.Instance.BeginCmd(list, "Format Widgets"); 
        if (format_type != FormatType.FT_SameXSpace && format_type != FormatType.FT_SameYSpace)
        {
            UIWidget.Pivot pivot = GetPivotForFormat(format_type);

            for (int i = 0; i < list.Count; ++i)
            {
                list[i].pivot = pivot;
                if (i > 0)
                {
                    switch (format_type)
                    {
                        case FormatType.FT_AlignLeft:
                        case FormatType.FT_AlignRight:
                        case FormatType.FT_AlignHCenter:
                            list[i].Pos = new Vector3(list[0].Pos.x, list[i].Pos.y, list[i].Pos.z);
                            break;
                        case FormatType.FT_AlignVCenter:
                        case FormatType.FT_AlignTop:
                        case FormatType.FT_AlignBottom:
                            list[i].Pos = new Vector3(list[i].Pos.x, list[0].Pos.y, list[i].Pos.z);
                            break;
                        case FormatType.FT_SameSize:
                            list[i].width = list[0].width;
                            list[i].height = list[0].height;
                            break;
                        case FormatType.FT_SameXSpace:
                            break;
                        case FormatType.FT_SameYSpace:
                            break;
                    }
                }
            }
            for (int i = 0; i < list.Count; ++i)
            {
                list[i].pivot = UIWidget.Pivot.BottomLeft;
            }
        }
        else
        {
            switch (format_type)
            {
                case FormatType.FT_SameXSpace:
                    list.Sort(delegate(UIElement w1, UIElement w2)
                    {
                        int c = w1.Pos.x.CompareTo(w2.Pos.x);
                        if (c != 0)
                        {
                            return c;
                        }
                        else
                        {
                            return w1.Pos.y.CompareTo(w2.Pos.y);
                        }
                    });

                    float total_width = 0; // 所有元素宽度总和
                    float cur_width = 0; // 目前的x方向跨度
                    float x_space = 0;

                    for (int i = 0; i < list.Count; ++i)
                    {
                        total_width += list[i].width;
                    }
                    cur_width = list[list.Count - 1].width + list[list.Count - 1].Pos.x - list[0].Pos.x;
                    if (cur_width > total_width)
                    {
                        x_space = (cur_width - total_width) / (list.Count - 1);
                    }
                    for (int i = 1; i < list.Count; ++i)
                    {
                        list[i].Pos = new Vector3(list[i - 1].Pos.x + list[i - 1].width + x_space, list[i].Pos.y, list[i].Pos.z);
                    }

                    break;
                case FormatType.FT_SameYSpace:
                    list.Sort(delegate(UIElement w1, UIElement w2)
                    {
                        int c = w1.Pos.y.CompareTo(w2.Pos.y);
                        if (c != 0)
                        {
                            return c;
                        }
                        else
                        {
                            return w1.Pos.x.CompareTo(w2.Pos.x);
                        }
                    });

                    float total_height = 0; // 所有元素高度总和
                    float cur_height = 0; // 目前的y方向跨度
                    float y_space = 0;

                    for (int i = 0; i < list.Count; ++i)
                    {
                        total_height += list[i].height;
                    }
                    cur_height = list[list.Count - 1].height + list[list.Count - 1].Pos.y - list[0].Pos.y;
                    if (cur_height > total_height)
                    {
                        y_space = (cur_height - total_height) / (list.Count - 1);
                    }
                    for (int i = 1; i < list.Count; ++i)
                    {
                        list[i].Pos = new Vector3(list[i].Pos.x, list[i - 1].Pos.y + list[i - 1].height + y_space, list[i].Pos.z);
                    }

                    break;
            }
        }
         
        CmdManager.Instance.EndCmd(); 
    }

    public void SetDirty()
    {
        m_dirty = true;
    }
    public bool Dirty
    {
        get { return m_dirty; }
    }
    public bool Save()
    {
        if (Dirty && m_root != null)
        {
            if (LayoutTool.SaveLayout(FileName, m_root))
            {
                m_dirty = false;
                return true;
            }
        }
        return false;
    }

    public void SetCamera(Camera cam)
    {
        if (m_root != null)
        {
            UIAnchor[] ui_anchors = m_root.GetComponentsInChildren<UIAnchor>(true);
            for (int i = 0; i < ui_anchors.Length; ++i)
            {
                ui_anchors[i].enabled = true;
                ui_anchors[i].uiCamera = cam;
                ui_anchors[i].SendMessage("Start");
            }
        }
    }

    public void DisableAnchor()
    {
        if(m_root != null)
        {
            UIAnchor[] ui_anchors = m_root.GetComponentsInChildren<UIAnchor>(true);
            for (int i = 0; i < ui_anchors.Length; ++i)
            {
                ui_anchors[i].enabled = false;
            }
        }
    }
}

