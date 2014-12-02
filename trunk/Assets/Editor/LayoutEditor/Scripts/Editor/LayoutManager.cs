using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using H3DEditor;

public class LayoutManager
{
    List<Layout> m_layout_list = new List<Layout>();
    int m_cur_index = -1;

    public void Dispose()
    {
        foreach (Layout layout in m_layout_list)
        {
            layout.Dispose();
        }
        m_layout_list.Clear();
        m_cur_index = -1;
    }

    // 导入Layout
    public bool ImportLayout(string filename)
    {
        if (IsOpened(filename))
        {
            EditorUtility.DisplayDialog("", "该文件已打开", "确定");
            return false;
        }

        Layout new_layout = new Layout();

        if (!new_layout.Load(filename))
        {
            return false;
        }
        m_layout_list.Add(new_layout);

        return true;
    }
    bool IsOpened(string filename)
    {
        foreach (Layout layout in m_layout_list)
        {
            if (layout.FileName == filename)
            {
                return true;
            }
        }
        return false;
    }
    // 移除Layout
    public bool RemoveLayout(int index)
    {
        if (!ValidIndex(index))
        {
            return false;
        }
        if (index == m_cur_index)
        {
            if (m_cur_index < LayoutCount - 1)
            {
                ++m_cur_index;
            }
            else
            {
                --m_cur_index;
            }
        }
        else if (index < m_cur_index)
        {
            --m_cur_index;
        }
        m_layout_list[index].Dispose();
        m_layout_list.RemoveAt(index);
        return true;
    }
    public void Clear()
    {
        Dispose();
    }

    public int LayoutCount
    {
        get { return m_layout_list.Count; }
    }
    public int CurEditLayoutIndex
    {
        get { return m_cur_index; }
    }
    public Layout CurEditLayout
    {
        get
        {
            if (!ValidIndex(m_cur_index))
            {
                return null;
            }
            return m_layout_list[m_cur_index];
        }
    }
    public bool SetCurEditLayout(int index)
    {
        if (!ValidIndex(index))
        {
            return false;
        }
        m_cur_index = index;

        return true;
    }
    public bool IsLayoutVisible(int index)
    {
        if (!ValidIndex(index))
        {
            return false;
        }
        return m_layout_list[index].Visible;
    }
    public void SetLayoutVisible(int index, bool b)
    {
        if (ValidIndex(index))
        {
            m_layout_list[index].Visible = b;
        }
    }
    public string GetLayoutName(int index)
    {
        if (!ValidIndex(index))
        {
            return "";
        }
        return m_layout_list[index].Name;
    }

    private bool ValidIndex(int index)
    {
        return EditorTool.ValidIndex(index, LayoutCount);
    }
}

