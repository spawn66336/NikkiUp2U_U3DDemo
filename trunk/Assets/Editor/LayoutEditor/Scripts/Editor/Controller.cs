using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using H3DEditor;

public class Controller
{
    public KnobSelResult m_knob_sel_res = new KnobSelResult();

    MainView m_view;

    bool m_drag = false;
    bool m_camera_move = false;
    bool m_bound_selecting = false;
    bool m_knob_move = false;
    bool m_sel_move = false;
    Vector3 m_lastMousePos;
    Vector3 m_draggingMousePos;
    UIElement m_last_sel;

    Dictionary<UIElement, bool> m_selDic = new Dictionary<UIElement, bool>();

    Vector3[] m_pre_sel_rec = new Vector3[4];
    Vector3[] m_cur_sel_rect = new Vector3[4];

    public Controller(MainView view)
    {
        m_view = view;
    }

    public void OnGUI(Layout cur_layout)
    {
        HandleInput(cur_layout);
        if (m_bound_selecting)
        {
            m_view.RenderWorldRect(m_pre_sel_rec, Color.white);
        }
    }

    private void HandleInput(Layout cur_layout)
    {
        Event e = Event.current;

        switch (e.type)
        {
            case EventType.MouseDown:
                if (m_view.ViewRect.Contains(e.mousePosition))
                {
                    m_lastMousePos = m_view.GUIToWorld(new Vector3(e.mousePosition.x, e.mousePosition.y));
                    if (e.button == 0) // 左键
                    {
                        m_drag = true;

                        if (m_knob_sel_res.m_knob_index != -1)
                        {
                            m_knob_move = true;
                        }
                        else
                        {
                            UIElement sel_widget = cur_layout.RayTest(m_lastMousePos);

                            BeginSelChange(cur_layout);
                            if (sel_widget != null)
                            {
                                if (!cur_layout.IsElementSelected(sel_widget))
                                {
                                    if (!e.control)
                                    {
                                        cur_layout.ClearSel();
                                    }
                                    cur_layout.SelElement(sel_widget);
                                }
                                else
                                {
                                    m_last_sel = sel_widget;
                                }
                            }
                            else
                            {
                                if (!e.control)
                                {
                                    cur_layout.ClearSel();
                                }
                                m_draggingMousePos = m_lastMousePos;
                                BuildSelRect(m_pre_sel_rec, m_draggingMousePos, m_lastMousePos);
                                m_bound_selecting = true;
                            }
                            EndSelChange(cur_layout);
                            BeginSelChange(cur_layout);
                        }
                    }
                    else if (e.button == 2) // 中键
                    {
                        m_camera_move = true;
                    }
                }
                break;
            case EventType.MouseUp:
                if (m_last_sel != null && !m_sel_move)
                {
                    if (e.control)
                    {
                        cur_layout.UnSelElement(m_last_sel);
                    }
                    else
                    {
                        cur_layout.ClearSel();
                        cur_layout.SelElement(m_last_sel);
                    }
                }

                if (m_drag)
                {
                    if (!m_knob_move)
                    {
                        EndSelChange(cur_layout);
                    }
                    if (!m_bound_selecting && m_sel_move)
                    { 
                        CmdManager.Instance.EndCmd(); 
                    }
                }

                m_drag = false;
                m_camera_move = false;
                m_bound_selecting = false;
                m_knob_move = false;
                m_sel_move = false;
                m_last_sel = null;
                SelKnob(cur_layout, e.mousePosition);
                break;
            case EventType.ScrollWheel:
                if (m_view.ViewRect.Contains(e.mousePosition))
                {
                    m_view.ScaleCamera(e.delta.y > 0);
                }
                break;
            case EventType.KeyDown:
                if (e.keyCode == KeyCode.UpArrow || e.keyCode == KeyCode.DownArrow || e.keyCode == KeyCode.LeftArrow || e.keyCode == KeyCode.RightArrow)
                {
                    List<UIElement> sel_list = cur_layout.GetMoveUIs();

                    if (sel_list.Count > 0 && EditorGUIUtility.keyboardControl == 0)
                    {
                        Vector3 move_delta = new Vector3((e.keyCode == KeyCode.LeftArrow ? -1 : (e.keyCode == KeyCode.RightArrow ? 1 : 0)), (e.keyCode == KeyCode.UpArrow ? 1 : (e.keyCode == KeyCode.DownArrow ? -1 : 0)), 0);
                         
                        CmdManager.Instance.BeginCmd(sel_list, "Move Widgets"); 
                        for (int i = 0; i < sel_list.Count; ++i)
                        {
                            sel_list[i].Move(move_delta);
                        } 
                        CmdManager.Instance.EndCmd(); 
                        LayoutEditorWindow.RequestRepaint();

                        cur_layout.SetDirty();
                    }
                }
                break;
            case EventType.MouseMove:
            case EventType.MouseDrag:
                Vector3 curMousePos = m_view.GUIToWorld(new Vector3(e.mousePosition.x, e.mousePosition.y));

                if (m_camera_move)
                {
                    Vector3 camera_move_delta = m_lastMousePos - curMousePos;

                    m_view.MoveCamera(camera_move_delta.x, camera_move_delta.y);
                    m_lastMousePos = m_view.GUIToWorld(new Vector3(e.mousePosition.x, e.mousePosition.y));
                }
                else if (m_drag) // 鼠标左键被按下
                {
                    if (m_bound_selecting) // 框选
                    {
                        BuildSelRect(m_cur_sel_rect, m_draggingMousePos, curMousePos);

                        List<UIElement> widgets = cur_layout.GetAllUIs(false);

                        for (int i = 0; i < widgets.Count; ++i)
                        {
                            UIElement w = widgets[i];
                            Vector3[] corners = w.worldCorners;
                            bool in_pre_rect = SelUI(corners, m_pre_sel_rec);
                            bool in_cur_rect = SelUI(corners, m_cur_sel_rect);

                            if (in_pre_rect != in_cur_rect)
                            {
                                if (in_cur_rect)
                                {
                                    if (!cur_layout.IsElementSelected(w))
                                    {
                                        cur_layout.SelElement(w);
                                    }
                                    else if (e.control)
                                    {
                                        cur_layout.UnSelElement(w);
                                    }
                                }
                                else
                                {
                                    if (cur_layout.IsElementSelected(w))
                                    {
                                        cur_layout.UnSelElement(w);
                                    }
                                    else if (e.control)
                                    {
                                        cur_layout.SelElement(w);
                                    }
                                }
                            }
                        }
                        BuildSelRect(m_pre_sel_rec, m_draggingMousePos, curMousePos);
                        m_lastMousePos = curMousePos;
                    }
                    else
                    {
                        bool real_move = false;
                        Vector3 move_delta = curMousePos - m_lastMousePos;
                        List<UIElement> sel_list = cur_layout.GetMoveUIs();

                        if (!m_sel_move)
                        { 
                            CmdManager.Instance.BeginCmd(sel_list, (m_knob_move ? "Resize Widgets" : "Move Widgets")); 
                        }
                        if (m_knob_move)
                        {
                            UIWidget.Pivot pivot = GetKnobPivot();
                            int x_scale = GetKnobXScale(pivot);
                            int y_scale = GetKnobYScale(pivot);

                            for (int i = 0; i < sel_list.Count; ++i)
                            {
                                UIElement ui_element = sel_list[i];

                                if (ui_element.SetWidthDelta(x_scale, move_delta.x))
                                {
                                    real_move = true;
                                }
                                if (ui_element.SetHeightDelta(y_scale, move_delta.y))
                                {
                                    real_move = true;
                                }
                            }
                        }
                        else
                        {
                            for (int i = 0; i < sel_list.Count; ++i)
                            {
                                // 拖拽移动
                                sel_list[i].Move(move_delta);
                                real_move = true;
                            }
                        }
                        m_sel_move = true;
                        if (real_move)
                        {
                            m_lastMousePos = curMousePos;
                        }

                        cur_layout.SetDirty();
                    }
                }
                else
                {
                    SelKnob(cur_layout, e.mousePosition);
                }
                break;
        }
    }

    private void SelKnob(Layout layout, Vector2 mousePosition)
    {
        m_knob_sel_res.m_knob_index = -1;

        UIElement w = layout.FirstUI;

        if (w != null)
        {
            Vector3[] gui_knobs = m_view.BuildKnobs(w.worldCorners);
            Vector3 p = new Vector3(mousePosition.x, mousePosition.y);

            for (int i = 0; i < gui_knobs.Length; ++i)
            {
                Vector3[] rect = MathTool.GetRect(gui_knobs[i], MathTool.s_knob_half_size);
                if (MathTool.IsPointInConvexPoly(p, rect))
                {
                    m_knob_sel_res.m_knob_index = i;
                    break;
                }
            }
        }
    }
    private UIWidget.Pivot GetKnobPivot()
    {
        switch (m_knob_sel_res.m_knob_index)
        {
            case 0:
                return UIWidget.Pivot.TopRight;
            case 1:
                return UIWidget.Pivot.BottomRight;
            case 2:
                return UIWidget.Pivot.BottomLeft;
            case 3:
                return UIWidget.Pivot.TopLeft;
            case 4:
                return UIWidget.Pivot.Right;
            case 5:
                return UIWidget.Pivot.Bottom;
            case 6:
                return UIWidget.Pivot.Left;
            case 7:
                return UIWidget.Pivot.Top;
        }

        return UIWidget.Pivot.BottomLeft;
    }
    private int GetKnobXScale(UIWidget.Pivot pivot)
    {
        switch (pivot)
        {
            case UIWidget.Pivot.TopLeft:
            case UIWidget.Pivot.Left:
            case UIWidget.Pivot.BottomLeft:
                return 1;
            case UIWidget.Pivot.Top:
            case UIWidget.Pivot.Bottom:
                return 0;
            case UIWidget.Pivot.TopRight:
            case UIWidget.Pivot.Right:
            case UIWidget.Pivot.BottomRight:
                return -1;
        }

        return 0;
    }
    private int GetKnobYScale(UIWidget.Pivot pivot)
    {
        switch (pivot)
        {
            case UIWidget.Pivot.BottomLeft:
            case UIWidget.Pivot.Bottom:
            case UIWidget.Pivot.BottomRight:
                return 1;
            case UIWidget.Pivot.Left:
            case UIWidget.Pivot.Right:
                return 0;
            case UIWidget.Pivot.TopLeft:
            case UIWidget.Pivot.Top:
            case UIWidget.Pivot.TopRight:
                return -1;
        }

        return 0;
    }

    private bool SelUI(Vector3[] corners, Vector3[] sel_rect)
    {
        return MathTool.IsConvexPolyIntersect(sel_rect, corners);
    }

    private void BuildSelRect(Vector3[] sel_rect, Vector3 p1, Vector3 p2)
    {
        float min_x = Mathf.Min(p1.x, p2.x);
        float max_x = Mathf.Max(p1.x, p2.x);
        float min_y = Mathf.Min(p1.y, p2.y);
        float max_y = Mathf.Max(p1.y, p2.y);

        sel_rect[0].x = min_x;
        sel_rect[0].y = min_y;
        sel_rect[1].x = min_x;
        sel_rect[1].y = max_y;
        sel_rect[2].x = max_x;
        sel_rect[2].y = max_y;
        sel_rect[3].x = max_x;
        sel_rect[3].y = min_y;
    }

    private void BeginSelChange(Layout cur_layout)
    {
        List<UIElement> sel_list = cur_layout.SelElements;

        m_selDic.Clear();
        foreach (UIElement ui_element in sel_list)
        {
            m_selDic.Add(ui_element, false);
        }
    }
    private void EndSelChange(Layout cur_layout)
    {
        List<UIElement> sel_list = cur_layout.SelElements;

        foreach (UIElement ui_element in sel_list)
        {
            if (m_selDic.ContainsKey(ui_element))
            {
                m_selDic.Remove(ui_element);
            }
            else
            {
                m_selDic.Add(ui_element, true);
            }
        }
        if (m_selDic.Count > 0)
        {
            CmdManager.Instance.AddCmd(new UISelectChangeCmd(m_selDic, cur_layout));
            m_selDic = new Dictionary<UIElement, bool>();
        }
    }
}

public class KnobSelResult
{
    public int m_knob_index;
}

