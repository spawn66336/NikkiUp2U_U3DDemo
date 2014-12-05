using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using H3DEditor;

public class MainView
{
    Controller m_view_controller;
    LayoutManager m_layout_mng;

    bool m_show_border = true;

    bool m_bSetCameraToAnchor = false;
    Camera m_camera;
    bool m_normal_camera = true;
    GameObject m_background;
    RenderTexture m_preview_tex;
    Rect m_view_rect;
    Vector3[] m_target_view_rect;

    int nResWidth = 0;
    int nResHeight = 0;

    public void Init(LayoutManager layout_mng)
    {
        m_camera = LayoutTool.CreateCamera(true);
        m_normal_camera = true;

        m_background = LayoutTool.CreateBackground(true);

        m_preview_tex = new RenderTexture(ConfigTool.Instance.target_width, ConfigTool.Instance.target_height, 32);
        m_camera.targetTexture = m_preview_tex;
        m_view_rect = new Rect(0, 0, ConfigTool.Instance.target_width, ConfigTool.Instance.target_height);

        m_target_view_rect = new Vector3[4];
        m_target_view_rect[0] = new Vector3(LayoutTool.s_editor_default_x - ConfigTool.Instance.target_width / 2, LayoutTool.s_editor_default_y - ConfigTool.Instance.target_height / 2);
        m_target_view_rect[1] = new Vector3(LayoutTool.s_editor_default_x - ConfigTool.Instance.target_width / 2, LayoutTool.s_editor_default_y + ConfigTool.Instance.target_height / 2);
        m_target_view_rect[2] = new Vector3(LayoutTool.s_editor_default_x + ConfigTool.Instance.target_width / 2, LayoutTool.s_editor_default_y + ConfigTool.Instance.target_height / 2);
        m_target_view_rect[3] = new Vector3(LayoutTool.s_editor_default_x + ConfigTool.Instance.target_width / 2, LayoutTool.s_editor_default_y - ConfigTool.Instance.target_height / 2);

        m_view_controller = new Controller(this);
        m_layout_mng = layout_mng;

        BackColor = LayoutTool.DefaultBackColor;
        BackTexture = EditorPrefs.GetString("BackTexture");

        nResWidth = ConfigTool.Instance.target_width;
        nResHeight = ConfigTool.Instance.target_height;
    }
    public void Dispose()
    {
        LayoutTool.ReleaseCamera(m_camera);
        LayoutTool.ReleaseBackground(m_background);
    }

    public Rect ViewRect
    {
        get { return m_view_rect; }
    }

    public bool ShowBorder
    {
        get { return m_show_border; }
        set { m_show_border = value; }
    }
    public Color BackColor
    {
        get { return (m_camera != null ? m_camera.backgroundColor : Color.white); }
        set
        {
            if (m_camera != null)
            {
                m_camera.backgroundColor = value;
                m_camera.Render();
                EditorPrefs.SetString("BackColor", value.ToString());
            }
        }
    }
    public string BackTexture
    {
        set
        {
            EditorPrefs.SetString("BackTexture", value);
            if (m_background != null)
            {
                m_background.renderer.sharedMaterial.mainTexture = ConfigTool.Instance.GetBackTexture(value);
                m_background.SetActive(m_background.renderer.sharedMaterial.mainTexture != null);
            }
        }
    }
    public bool NormalCamera
    {
        get { return m_normal_camera; }
    }

    public void Render(Rect position)
    {
        if (m_camera == null)
        {
            return;
        }

        float aspect = position.width / position.height;

        if (aspect > m_camera.aspect)
        {
            m_view_rect.height = position.height;
            m_view_rect.width = m_view_rect.height * m_camera.aspect;
            m_view_rect.x = position.x + (position.width - m_view_rect.width) / 2;
            m_view_rect.y = position.y;
        }
        else
        {
            m_view_rect.width = position.width;
            m_view_rect.height = m_view_rect.width / m_camera.aspect;
            m_view_rect.x = position.x;
            m_view_rect.y = position.y + (position.height - m_view_rect.height) / 2;
        }
        GUI.DrawTexture(m_view_rect, m_preview_tex, ScaleMode.StretchToFill, false);
        if (!m_normal_camera && ShowBorder)
        {
            RenderWorldRect(m_target_view_rect, Color.red);
        }

        Layout cur_layout = m_layout_mng.CurEditLayout;

        if (cur_layout != null)
        {
            if (!m_bSetCameraToAnchor)
            {
                cur_layout.SetCamera(m_camera);
                m_bSetCameraToAnchor = true;
            }
            else
            {
                cur_layout.DisableAnchor();
            }
            

            m_view_controller.OnGUI(cur_layout);

            List<UIElement> all_uis = cur_layout.GetAllUIs(true);

            for (int i = 0; i < all_uis.Count; ++i)
            {
                UIElement w = all_uis[i];

                if (cur_layout.IsElementSelected(w))
                {
                    RenderWorldRect(w.worldCorners, Color.white);
                    if (cur_layout.FirstUI == w)
                    {
                        RenderKnob(w.worldCorners, m_view_controller.m_knob_sel_res.m_knob_index);
                    }
                }
                else if (ShowBorder)
                {
                    RenderWorldRect(w.worldCorners, Color.green);
                }
            }
        }
    }

    public void MoveCamera(float x, float y)
    {
        m_camera.transform.position = new Vector3(m_camera.transform.position.x + x, m_camera.transform.position.y + y, m_camera.transform.position.z);
        m_normal_camera = false;
        m_camera.Render();
    }
    public void ScaleCamera(bool larger)
    {
        m_camera.orthographicSize *= (larger ? 1.1f : 0.9f);
        m_normal_camera = false;
        m_camera.Render();
        LayoutEditorWindow.RequestRepaint();
    }
    public void ResetCamera()
    {
        LayoutTool.ResetCamera(m_camera);
        m_normal_camera = true;
        m_camera.Render();
    }

    public void RenderKnob(Vector3[] corners, int sel_index)
    {
        Vector3[] gui_knobs = BuildKnobs(corners);

        for (int i = 0; i < gui_knobs.Length; ++i)
        {
            Vector3[] rect = MathTool.GetRect(gui_knobs[i], MathTool.s_knob_half_size);
            if (MathTool.ClipRect(m_view_rect, rect))
            {
                Color color = (i == sel_index ? Color.yellow : Color.green);

                Handles.DrawSolidRectangleWithOutline(rect, color, color);
            }
        }
    }
    public Vector3[] BuildKnobs(Vector3[] corners)
    {
        Vector3[] gui_knobs = new Vector3[8];

        gui_knobs[0] = WorldToGUI(corners[0]);
        gui_knobs[1] = WorldToGUI(corners[1]);
        gui_knobs[2] = WorldToGUI(corners[2]);
        gui_knobs[3] = WorldToGUI(corners[3]);

        gui_knobs[4] = (gui_knobs[0] + gui_knobs[1]) * 0.5f;
        gui_knobs[5] = (gui_knobs[1] + gui_knobs[2]) * 0.5f;
        gui_knobs[6] = (gui_knobs[2] + gui_knobs[3]) * 0.5f;
        gui_knobs[7] = (gui_knobs[0] + gui_knobs[3]) * 0.5f;

        return gui_knobs;
    }
    public void RenderWorldRect(Vector3[] corners, Color color)
    {
        for (int i = 0; i < corners.Length; ++i)
        {
            DrawLine(WorldToGUI(corners[i]), WorldToGUI(corners[(i + 1) % corners.Length]), color);
        }
    }
    public void DrawLine(Vector3 p1, Vector3 p2, Color color)
    {
        if (MathTool.ClipLine(m_view_rect, ref p1, ref p2))
        {
            Handles.color = color;
            Handles.DrawLine(p1, p2);
        }
    }

    // 视图的缩放比例（编辑器视图/场景视图）
    public float ViewScale
    {
        get { return m_view_rect.height / (2 * m_camera.orthographicSize); }
    }
    public Vector3 GUIToWorld(Vector3 p)
    {
        return new Vector3((p.x - m_view_rect.x - m_view_rect.width / 2) / ViewScale + m_camera.transform.position.x, (m_view_rect.y + m_view_rect.height / 2 - p.y) / ViewScale + m_camera.transform.position.y, 0);
    }
    public Vector3 WorldToGUI(Vector3 p)
    {
        return new Vector3(((p.x - m_camera.transform.position.x) * ViewScale + m_view_rect.x + m_view_rect.width / 2), (m_view_rect.y + m_view_rect.height / 2 - (p.y - m_camera.transform.position.y) * ViewScale), 0);
    }
    public Rect GetGUIRect(Vector3[] corners)
    {
        Vector3 lb = WorldToGUI(corners[0]);
        Vector3 rt = WorldToGUI(corners[2]);

        return new Rect(lb.x, rt.y, rt.x - lb.x, lb.y - rt.y);
    }

    public void ChangeResolution(int nW, int nH)
    {
        m_camera.orthographicSize = nH / 2;
        m_camera.aspect = (float)nW / nH;

        m_preview_tex = new RenderTexture(nW, nH, 32);
        m_camera.targetTexture = m_preview_tex;

        m_view_rect = new Rect(0, 0, nW, nH);

        m_target_view_rect[0] = new Vector3(LayoutTool.s_editor_default_x - nW / 2, LayoutTool.s_editor_default_y - nH / 2);
        m_target_view_rect[1] = new Vector3(LayoutTool.s_editor_default_x - nW / 2, LayoutTool.s_editor_default_y + nH / 2);
        m_target_view_rect[2] = new Vector3(LayoutTool.s_editor_default_x + nW / 2, LayoutTool.s_editor_default_y + nH / 2);
        m_target_view_rect[3] = new Vector3(LayoutTool.s_editor_default_x + nW / 2, LayoutTool.s_editor_default_y - nH / 2);

        m_bSetCameraToAnchor = false;
        nResWidth = nW;
        nResHeight = nH;
        LayoutEditorWindow.RequestRepaint();
    }

    public bool CheckResolutionBeforeSave()
    {
        if (nResWidth == ConfigTool.Instance.target_width && nResHeight == ConfigTool.Instance.target_height)
            return true;

        if (!EditorUtility.DisplayDialog("提示", "当前分辨率与默认配置分辨率不一致， 继续保存么？", "确定", "取消"))
        {
            return false;
        }
        return true;
    }
}

