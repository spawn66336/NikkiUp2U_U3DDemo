using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ListCtrlItem
{
    //列表项名
    public string name;
    //列表项图标
    public Texture image;
    //项中的用户数据
    public object userObj;
    //正常情况下颜色显示
    public Color color = Color.green;
    //列表项被选择时的颜色
    public Color onSelectColor = Color.blue; 
    //最近一次绘制矩形区域
    public Rect lastRect = new Rect(); 
}

public class ListViewCtrl : EditorControl 
{
    public Vector2 ScrollPos
    {
        get { return scrollPos; }
        set { scrollPos = value; }
    }

    public List<ListCtrlItem> Items
    {
        get { return items; }
    }

    public int LastSelectItem
    {
        get { return lastSelectItem; }
        set 
        { 
            lastSelectItem = value;
            if (lastSelectItem >= items.Count)
                lastSelectItem = items.Count - 1; 
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

    public void AddItem(ListCtrlItem item)
    {
        items.Add(item);
        lastSelectItem = -1;
    }

    public ListCtrlItem GetItemAt(int i)
    {
        return items[i];
    }

    public int IndexOfItem(ListCtrlItem item)
    {
        return items.IndexOf(item);
    }

    public void RemoveItem(ListCtrlItem item)
    {
        items.Remove(item);
        lastSelectItem = -1;
    }

    public void ClearItems()
    {
        items.Clear();
        lastSelectItem = -1;
    }

    public int GetItemCount()
    {
        return items.Count;
    }

    private Vector2 scrollPos = new Vector2(0,0); 
    private List<ListCtrlItem> items = new List<ListCtrlItem>();
    private int lastSelectItem = -1;
}
