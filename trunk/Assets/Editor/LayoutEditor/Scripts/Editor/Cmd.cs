using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Xml;

// cmd接口 注意：在add到manager时会execute一次!!!
public interface ICmd
{
    string CmdName { get; }
    void execute();
    void unexecute();
}

public interface ICmdDispose
{
    void Dispose();
}

public class CmdManager
{
    static string s_config_file = "Assets/Editor/LayoutEditor/Config/global_config.xml";

    private int cmdCountLimit = 10000;
    private int configLimit = 10000;

    public static void Init()
    {
        s_Instance = new CmdManager();
        

        XmlDocument doc = new XmlDocument();
        doc.Load(s_config_file);
        XmlNode root = doc.SelectSingleNode("GlobalConfig");
        if (root != null)
        {
            XmlNode limit_node = root.SelectSingleNode("CmdCountLimit");
            if (limit_node != null)
            {
                int.TryParse(limit_node.InnerText, out s_Instance.configLimit);
                if (s_Instance.configLimit >= 10)
                {
                    s_Instance.cmdCountLimit = s_Instance.configLimit;
                }
                else
                {
                    s_Instance.configLimit = 10000;
                }
            }
        }
    }

    private static CmdManager s_Instance = null;
    public static CmdManager Instance
    {
        get
        {
            if (null == s_Instance)
            {
                Init();
            }
            return s_Instance;
        }
    }

    // 编辑器cmdlist当前位置
    private int cmdCur = 0;
    // cmdlist
    private List<ICmd> cmdList = new List<ICmd>();
    // 由u3d控制的用户当前undo,redo位置
    private CmdCounter cmdCounter
    {
        get
        {
            CmdCounter c = H3DEditor.LayoutTool.Root.GetComponent<CmdCounter>();
            if(c == null)
            {
                H3DEditor.LayoutTool.Root.AddComponent<CmdCounter>();
            }
            return c;
        }
    }
    // 添加cmd 并执行一次
    public void AddCmd(ICmd cmd)
    {
        if (cmdCur != cmdCounter.Cur - cmdOffset)
            syncCmd();

#if !UNITY_4_3 && !UNITY_4_5
        Undo.IncrementCurrentEventIndex();
#endif

        // 清除当前位置之后的cmd
        if (cmdList.Count > cmdCur)
        {
            cmdList.RemoveRange(cmdCur, cmdList.Count - cmdCur);
        }

        cmdList.Add(cmd);
        cmd.execute();
        cmdCur++;

#if UNITY_4_3 || UNITY_4_5
        Undo.RegisterCompleteObjectUndo(cmdCounter, cmd.CmdName);
#else
        Undo.RegisterUndo(cmdCounter, cmd.CmdName);
#endif
        cmdCounter.Cur++;
    }
    // 同步cmd，即用户执行undo,redo，编辑器同步cmdlist到对应的位置，同时在cmd数量超过限制时进行清理
    public bool syncCmd()
    {
        if (cmdCur >= cmdCountLimit)
            limitCmdToHalf();

        if (cmdCur == (cmdCounter.Cur - cmdOffset))
            return false;

        // redo
        if (cmdCounter.Cur - cmdOffset > cmdCur && cmdList.Count >= cmdCounter.Cur - cmdOffset)
        {
            for (int i = cmdCur; i < cmdCounter.Cur - cmdOffset; i++)
            {
                cmdList[i].execute();
            }

            cmdCur = cmdCounter.Cur - cmdOffset;

        } // undo
        else if (cmdCounter.Cur - cmdOffset < cmdCur && cmdList.Count >= cmdCur)
        {
            for (int i = cmdCur - 1; i >= cmdCounter.Cur - cmdOffset; i--)
            {
                cmdList[i].unexecute();
            }

            cmdCur = cmdCounter.Cur - cmdOffset;
        }

        return true;
    }

    private List<ManagedCmd> managedCmdToDispose = new List<ManagedCmd>();
    private int cmdOffset = 0;
    private void limitCmdToHalf()
    {
        for (int i = cmdOffset; i < configLimit / 2; i++)
        {
            ICmd cmd = cmdList[i];
            if (cmd is ManagedCmd)
            {
                managedCmdToDispose.Add(cmd as ManagedCmd);
                continue;
            }
            if (cmd is ICmdDispose)
            {
                (cmd as ICmdDispose).Dispose();
            }
        }

        cmdList.RemoveRange(cmdOffset, configLimit / 2);

        cmdOffset += configLimit / 2;
        cmdCountLimit += cmdOffset;
    }

    public void clearCmd()
    {
        foreach (ICmd cmd in cmdList)
        {
            if (cmd is ICmdDispose)
            {
                (cmd as ICmdDispose).Dispose();
            }
        }
        
        foreach (ManagedCmd cmd in managedCmdToDispose)
        {
            cmd.Dispose();
        }

        cmdOffset = 0;
        cmdList.Clear();
        cmdCur = 0;
        cmdCounter.Cur = 0;
        cmdCountLimit = configLimit; 

        Undo.ClearUndo(cmdCounter);  
    }

    public void BeginCmd(List<UIElement> elementList, string name)
    { 
        List<Component> comList = SetSnapshotTarget(elementList, name);
        CreateSnapshot();
        foreach (Component com in comList)
        {
            if (com is Transform)
            {
                (com as Transform).localPosition = (com as Transform).localPosition;
            }
        }
        RegisterSnapshot(); 
    }

    public void EndCmd()
    {
#if !UNITY_4_3
        Undo.ClearSnapshotTarget();  
#endif
    }

    private UnityEngine.Object[] snapshotTarget;
    private string targetCmdName;
    public void SetSnapshotTarget(UnityEngine.Object obj, string name)
    {
        //Debug.Log("SetSnapShorTarget:" + name);
        if (cmdCur != cmdCounter.Cur - cmdOffset)
            syncCmd();

        UnityEngine.Object[] objs = new UnityEngine.Object[2];
        objs[0] = obj;
        objs[1] = cmdCounter;
        snapshotTarget = objs;
        targetCmdName = name;

#if UNITY_4_3 || UNITY_4_5
        Undo.RegisterCompleteObjectUndo(objs, name);
#else
        Undo.SetSnapshotTarget(objs, name);
#endif
    }

    public void SetSnapshotTarget(UnityEngine.Object[] objs, string name)
    {
        if (cmdCur != cmdCounter.Cur - cmdOffset)
            syncCmd();

        UnityEngine.Object[] cmdObjs = new UnityEngine.Object[objs.Length + 1];
        for (int i = 0; i < objs.Length; i++)
        {
            cmdObjs[i] = objs[i];
        }

        cmdObjs[cmdObjs.Length - 1] = cmdCounter; 

        snapshotTarget = cmdObjs;
        targetCmdName = name;

#if UNITY_4_3 || UNITY_4_5
        Undo.RegisterCompleteObjectUndo(objs, name);
#else
        Undo.SetSnapshotTarget(objs, name);
#endif
    }

    public List<Component> SetSnapshotTarget(List<UIElement> elementList, string name)
    {
        List<Component> monoList = new List<Component>();
        foreach (UIElement element in elementList)
        {
            monoList.AddRange(element.gameObject.GetComponents<Component>());
        }
         
        SetSnapshotTarget(monoList.ToArray(), name);
        return monoList;
    }

    public void CreateSnapshot()
    {
#if !UNITY_4_3
        Undo.CreateSnapshot();
#endif
    }

    public void RegisterSnapshot()
    {
        LayoutEditorWindow.Instance.SetCurLayoutDirty();
#if !UNITY_4_3 && !UNITY_4_5
        Undo.IncrementCurrentEventIndex();
#endif
        ManagedCmd cmd = new ManagedCmd(snapshotTarget, targetCmdName); 
        cmdList.Add(cmd);
        cmdCur++;
        cmdCounter.Cur++;

#if !UNITY_4_3
        Undo.RegisterSnapshot();
#endif
    }

    public void RegisterUndo(UnityEngine.Object obj, string name)
    {
        UnityEngine.Object[] objs = new UnityEngine.Object[1];
        objs[0] = obj;
        RegisterUndo(objs, name);
    }

    public void RegisterUndo(UnityEngine.Object[] objs, string name)
    {
        if (cmdCur != cmdCounter.Cur - cmdOffset)
            syncCmd();

#if !UNITY_4_3 && !UNITY_4_5
        Undo.IncrementCurrentEventIndex();
#endif

        Array.Resize<UnityEngine.Object>(ref objs, objs.Length + 1);
        objs[objs.Length - 1] = cmdCounter;

        ManagedCmd cmd = new ManagedCmd(objs, name);

        cmdList.Add(cmd);
        cmdCur++;

#if UNITY_4_3 || UNITY_4_5
        Undo.RegisterCompleteObjectUndo(objs, name);
#else
        Undo.RegisterUndo(objs, name);
#endif

        cmdCounter.Cur++;
    }

    public void RegisterUndo(UIElement element, string name)
    {
        RegisterUndo(element.gameObject.GetComponents<MonoBehaviour>(), name);
    }

    private class ManagedCmd : ICmd, ICmdDispose
    {
        /*
        public ManagedCmd(UnityEngine.Object obj, string CmdName)
        {
            cmdName = CmdName;
            Objs.Add(obj);
        }*/

        public ManagedCmd(UnityEngine.Object[] obj, string CmdName)
        {
            cmdName = CmdName;
            Objs.AddRange(obj); 
        }

        public void execute()
        {
            LayoutEditorWindow.RequestRepaint();

#if NGUI_3_5_8
            foreach( var p in UIPanel.list )
            { 
                p.RebuildAllDrawCalls();
            }
#else
            UIPanel.SetDirty();
#endif
            // sprite 实时刷新

            foreach (UnityEngine.Object obj in Objs)
            {
                if (obj is UIWidget)
                    (obj as UIWidget).MarkAsChanged();
            }
            if (CmdName == "Sprite Change" && Objs[0] is UISprite)
            {
                UISprite sprite = Objs[0] as UISprite;
#if NGUI_3_5_8 
                sprite.MarkAsChanged();
#else
                if (sprite.isValid) sprite.UpdateUVs(true);
#endif
            }
        }

        public void unexecute()
        {
            LayoutEditorWindow.RequestRepaint();
#if NGUI_3_5_8
            foreach (var p in UIPanel.list)
            {
                p.RebuildAllDrawCalls();
            }
#else
            UIPanel.SetDirty();
#endif
            // sprite 实时刷新
            foreach (UnityEngine.Object obj in Objs)
            {
                if (obj is UIWidget)
                    (obj as UIWidget).MarkAsChanged();
            }
            if (CmdName == "Sprite Change" && Objs[0] is UISprite)
            {
                UISprite sprite = Objs[0] as UISprite;
#if NGUI_3_5_8

                sprite.MarkAsChanged();
#else
                if (sprite.isValid) sprite.UpdateUVs(true);
#endif
            }
        }

        public void Dispose()
        {
            for (int i = 0; i < Objs.Count - 1; i++)
                Undo.ClearUndo(Objs[i]); 
        }

        private string cmdName;
        public string CmdName { get { return cmdName; } }
        public List<UnityEngine.Object> Objs = new List<UnityEngine.Object>(); 
    }
}

// uitreeview节点拖拽cmd
public class UITreeNodeDragCmd : ICmd
{
    public UITreeNodeDragCmd(TreeNode node, TreeNode destnode, EditorTreeView tree)
    {
        m_node = node;
        m_oldParent = node.Parent;
        m_newParent = destnode;
        m_tree = tree;

        m_element = node.DataKey as UIElement;
        m_oldEleParent = (m_oldParent == null ? tree.DataKey : m_oldParent.DataKey) as UIElement;
        m_newEleParent = (m_newParent == null ? tree.DataKey : m_newParent.DataKey) as UIElement;
    }
    public void execute()
    {
        if (!executed)
        {
            executed = true;
            m_element.SetParent(m_newEleParent);
            return;
        }

        //Debug.Log("in execute!!" + "  node:" + m_node.NodeName + "  oldpar:" + m_oldParent.NodeName + "  newpar:" + m_newParent.NodeName);

        if (m_oldParent != null)
            m_oldParent.RemoveChild(m_node);
        else
            m_tree.RemoveRootNode(m_node);

        if (m_newParent != null)
            m_newParent.AddChild(m_node);
        else
            m_tree.AddRootNode(m_node);

        m_element.SetParent(m_newEleParent);

        LayoutEditorWindow.RequestRepaint();
    }
    public void unexecute()
    {
        //Debug.Log("in unexecute!!" + "  node:" + m_node.NodeName + "  oldpar:" + m_oldParent.NodeName + "  newpar:" + m_newParent.NodeName);

        //Debug.Log("tree:" + (m_node.Tree == null ? "null" : "not null"));

        if (m_newParent != null)
            m_newParent.RemoveChild(m_node);
        else
            m_tree.RemoveRootNode(m_node);

        if (m_oldParent != null)
            m_oldParent.AddChild(m_node);
        else
            m_tree.AddRootNode(m_node);

        m_element.SetParent(m_oldEleParent);

        LayoutEditorWindow.RequestRepaint();
    }
    public string CmdName { get { return "move node"; } }
    private bool executed = false;
    private TreeNode m_node;
    private TreeNode m_oldParent;
    private TreeNode m_newParent;
    private UIElement m_element;
    private UIElement m_newEleParent;
    private UIElement m_oldEleParent;
    private EditorTreeView m_tree;
}

// uitreeview节点属性改变cmd
public class UITreeNodePropChangeCmd : ICmd
{
    public UITreeNodePropChangeCmd(UIElement element, TreeNode node, string propName, Layout layout)
    {
        m_element = element;
        m_node = node;
        m_prop = propName;
        m_layout = layout;
        m_new_value = m_node.ToggleList[m_prop];
        if (propName == "锁定")
        {
            selChildNode = new List<TreeNode>();
            regChildSelNode(node);
        }
        else if (propName == "隐藏" || propName == "冻结")
        {
            selChildNode = new List<TreeNode>();
            if (m_node.selected)
            {
                m_layout.UnSelElement(m_node.DataKey as UIElement);
                selChildNode.Add(m_node);
            }
            regChildSelNode(node);
        }
    }

    public void execute()
    {
        m_node.ToggleList[m_prop] = m_new_value;
        syncElementProp();
    }

    public void unexecute()
    {
        m_node.ToggleList[m_prop] = !m_new_value;
        syncElementProp();
    }

    private void syncElementProp()
    {
        switch (m_prop)
        {
            case "隐藏":
                m_element.Hide = m_node.ToggleList[m_prop];
                synSelect(!m_element.Hide);
                break;
            case "冻结":
                m_element.Freeze = m_node.ToggleList[m_prop];
                synSelect(!m_element.Freeze);
                break;
            case "锁定":
                m_element.Lock = m_node.ToggleList[m_prop];
                foreach (TreeNode node in m_node.Children)
                    node.Hide = m_element.Lock;
                if (m_new_value)
                    synSelect(!m_element.Lock);
                if (!m_new_value)
                    synSelect(false);
                break;
            default:
                break;
        }

        LayoutEditorWindow.RequestRepaint();
    }

    private void regChildSelNode(TreeNode node)
    {
        foreach (TreeNode subNode in node.Children)
        {
            if (subNode.selected)
            {
                m_layout.UnSelElement(subNode.DataKey as UIElement);
                selChildNode.Add(subNode);
            }

            regChildSelNode(subNode);
        }
    }

    private void synSelect(bool sel)
    {
        if (sel)
        {
            foreach (TreeNode node in selChildNode)
            {
                node.selected = true;
                m_layout.SelElement(node.DataKey as UIElement);
            }
        }
        else
        {
            foreach (TreeNode node in selChildNode)
            {
                node.selected = false;
                m_layout.UnSelElement(node.DataKey as UIElement);
            }
        }
    }

    public string CmdName { get { return "change ui node prop"; } }
    Layout m_layout;
    UIElement m_element;
    TreeNode m_node;
    string m_prop;
    bool m_new_value;
    List<TreeNode> selChildNode = null;
}

/* 导入移除不使用cmd 故可见属性变化也不可使用cmd
// layouttree可见属性变化cmd
public class LayoutTreeNodeVisibleChangeCmd : ICmd
{
    public LayoutTreeNodeVisibleChangeCmd(int index, TreeNode node, LayoutManager layoutMng)
    {
        m_index = index;
        m_node = node;
        m_layout_mng = layoutMng;
        m_visible = m_node.ToggleList["可见"];
    }

    public void execute()
    {
        Debug.Log(m_visible.ToString());
        m_node.ToggleList["可见"] = m_visible;
        m_layout_mng.SetLayoutVisible(m_index, m_node.ToggleList["可见"]);
        LayoutEditorWindow.RequestRepaint();
    }

    public void unexecute()
    {
        Debug.Log(m_visible.ToString());
        m_node.ToggleList["可见"] = !m_visible;
        m_layout_mng.SetLayoutVisible(m_index, m_node.ToggleList["可见"]);
        LayoutEditorWindow.RequestRepaint();
    }

    public string CmdName { get { return "change layout visible"; } }
    int m_index;
    TreeNode m_node;
    LayoutManager m_layout_mng;
    bool m_visible;
}*/

// 添加节点cmd
public class AddUITreeNodeCmd : ICmd
{
    public AddUITreeNodeCmd(TreeNode node, LayoutManager layoutmng, EditorTreeView tree)
    {
        m_node = node;
        m_layout_mng = layoutmng;
        m_tree = tree;
    }

    public void execute()
    {
        if (!executed)
        {
            executed = true;

            m_new_ui = (m_node == null ? m_layout_mng.CurEditLayout.Root.AddChildNode() : (m_node.DataKey as UIElement).AddChildNode());
            m_new_node = TreeNodeFactory.CreateNewUITreeNode(m_new_ui.Name);
            m_new_node.DataKey = m_new_ui;
        }
        else
        {
            (m_new_node.DataKey as UIElement).Removed = false;
        }

        if (m_node == null)
            m_tree.AddRootNode(m_new_node);
        else
            m_node.AddChild(m_new_node);

        LayoutEditorWindow.RequestRepaint();
    }

    public void unexecute()
    {
        (m_new_node.DataKey as UIElement).Removed = true;

        if (m_node == null)
            m_tree.RemoveRootNode(m_new_node);
        else
            m_node.RemoveChild(m_new_node);

        LayoutEditorWindow.RequestRepaint();
    }

    public string CmdName { get { return "add treenode"; } }
    private bool executed = false;
    private TreeNode m_node;
    private LayoutManager m_layout_mng;
    private TreeNode m_new_node;
    private UIElement m_new_ui;
    private EditorTreeView m_tree;
}

// 删除节点cmd
public class RemoveUITreeNodeCmd : ICmd
{
    public RemoveUITreeNodeCmd(TreeNode node, LayoutManager layoutmng, EditorTreeView tree)
    {
        m_node = node;
        m_layout_mng = layoutmng;
        m_tree = tree;
        m_parent = m_node.Parent;
    }

    public void execute()
    {
        if (m_parent == null)
            m_tree.RemoveRootNode(m_node);
        else
            m_parent.RemoveChild(m_node);

        (m_node.DataKey as UIElement).Removed = true;
        LayoutEditorWindow.RequestRepaint();
    }

    public void unexecute()
    {
        if (m_parent == null)
            m_tree.AddRootNode(m_node);
        else
            m_parent.AddChild(m_node);

        (m_node.DataKey as UIElement).Removed = false;
        LayoutEditorWindow.RequestRepaint();
    }

    public string CmdName { get { return "remove treenode"; } }
    private bool executed = false;
    private TreeNode m_node;
    private LayoutManager m_layout_mng;
    private TreeNode m_parent;
    private EditorTreeView m_tree;
}

// 修改ui节点名cmd
public class ChangeUITreeNodeCmd : ICmd
{
    public ChangeUITreeNodeCmd(TreeNode node, string newName)
    {
        m_node = node;
        m_new_name = newName;
        m_old_name = (m_node.DataKey as UIElement).Name;
    }

    public void execute()
    {
        m_node.NodeName = m_new_name;
        (m_node.DataKey as UIElement).Name = m_new_name;
        LayoutEditorWindow.RequestRepaint();
    }

    public void unexecute()
    {
        m_node.NodeName = m_old_name;
        (m_node.DataKey as UIElement).Name = m_old_name;
        LayoutEditorWindow.RequestRepaint();
    }

    public string CmdName { get { return "rename treenode"; } }
    private TreeNode m_node;
    private string m_new_name;
    private string m_old_name;
    private bool executed = false;
}

// ui节点选择cmd
public class UISelectChangeCmd : ICmd
{
    public UISelectChangeCmd(Dictionary<UIElement, bool> SelDic, Layout layout)
    {
        m_selDic = new Dictionary<UIElement, bool>(SelDic);
        m_layout = layout;
    }

    public UISelectChangeCmd(Dictionary<UIElement, bool> SelDic, Layout layout, bool firstNoScroll)
    {
        m_selDic = new Dictionary<UIElement, bool>(SelDic);
        m_layout = layout;
        m_firstNoScroll = firstNoScroll;
    }

    public void execute()
    {
        foreach(KeyValuePair<UIElement, bool> kvp in m_selDic)
        {
            if (kvp.Value && !kvp.Key.Hide && !kvp.Key.Freeze)
                m_layout.SelElement(kvp.Key);
            else if (!kvp.Value)
                m_layout.UnSelElement(kvp.Key);
        }
        LayoutEditorWindow.Instance.SyncUITreeSelectState(m_selDic, false, !m_firstExe || !m_firstNoScroll);
        LayoutEditorWindow.RequestRepaint();
        m_firstExe = false;
    }

    public void unexecute()
    {
        foreach (KeyValuePair<UIElement, bool> kvp in m_selDic)
        {
            if (kvp.Value)
                m_layout.UnSelElement(kvp.Key);
            else if (!kvp.Key.Hide && !kvp.Key.Freeze)
                m_layout.SelElement(kvp.Key);
        }
        LayoutEditorWindow.Instance.SyncUITreeSelectState(m_selDic, true, true);
        LayoutEditorWindow.RequestRepaint();
    }

    public string CmdName { get { return "ui select change"; } }
    private Dictionary<UIElement, bool> m_selDic;
    private Layout m_layout;
    private bool m_firstNoScroll = false;
    private bool m_firstExe = true;
}