using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Collections;
using H3DEditor;

public class LayoutEditorWindow : EditorWindow
{
    LayoutManager m_layout_mng = new LayoutManager();
    MainView m_view = new MainView();
    private MouseRegion _mouseRegionUpdate;
    private bool m_MouseDown;
    private float curMouseX = 0f;
    private float curMouseY = 0f;
    private int lastMouseX = 0;
    private int lastMouseY = 0;
    private EditorTreeView uiTree = null;
    private EditorTreeView layoutTree = null;
    public Dictionary<MouseRegion, Rect> _mouseRegionRect = new Dictionary<MouseRegion, Rect>();
    private MouseRegion m_MouseDownRegion;
    private MouseRegion _mouseRegion;
    private static bool _wasRepaintRequested;
    private List<UIElement> curSelUI = null;

    public static void Init()
    {
        s_Instance = EditorWindow.GetWindow<LayoutEditorWindow>("Layout编辑器", true);
    }

    private static LayoutEditorWindow s_Instance = null;
    public static LayoutEditorWindow Instance
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

    public void SetCurLayoutDirty()
    {
        if (m_layout_mng.CurEditLayout != null)
            m_layout_mng.CurEditLayout.SetDirty();
    }

    void OnEnable()
    {
        wantsMouseMove = true;
        m_view.Init(m_layout_mng);
        ResetLayoutTree();
        ResetUITree();

        RequestRepaint();
    }

    void ClearUndos()
    {
        CmdManager.Instance.clearCmd();
        Undo.ClearUndo(this);
    }

    void OnDestroy()
    {
        Layout cur_layout = m_layout_mng.CurEditLayout;
        if (cur_layout != null && cur_layout.Dirty)
        {
            if (EditorUtility.DisplayDialog("", "当前编辑的layout已被修改，是否保存?", "是", "否"))
            {
                cur_layout.Save();
            }
        }

        ClearUndos();
        m_view.Dispose();
        m_layout_mng.Dispose();
        LayoutTool.ReleaseRoot();
    }

    void Update()
    {
        curSelUI = m_layout_mng.CurEditLayout == null ? null : new List<UIElement>(m_layout_mng.CurEditLayout.SelElements);

        bool changed = CmdManager.Instance.syncCmd();
        if (changed && m_layout_mng.CurEditLayout != null)
            m_layout_mng.CurEditLayout.SetDirty();

        this._mouseRegion = this._mouseRegionUpdate;

        OnMouseMove();

        if (_wasRepaintRequested)
        {
            _wasRepaintRequested = false;
            Repaint();
        }
    }

    public void OnMouseMove()
    {
        int num = (int)curMouseX - lastMouseX;
        int num2 = (int)curMouseY - lastMouseY;
        lastMouseX = (int)curMouseX;
        lastMouseY = (int)curMouseY;
        /*  计算主视窗中鼠标的相对主视窗的位置
        this.m_MouseMoveArgs.X -= (int)this._canvasRect.x;
        this.m_MouseMoveArgs.Y -= (int)this._canvasRect.y;
        Detox.Windows.Forms.Cursor.Position.X = this.m_MouseMoveArgs.X;
        Detox.Windows.Forms.Cursor.Position.Y = this.m_MouseMoveArgs.Y;
        if (this._mouseRegion == MouseRegion.Canvas)
        {
            this.m_ScriptEditorCtrl.OnMouseMove(this.m_MouseMoveArgs);
        }
        this.m_MouseMoveArgs.X += (int)this._canvasRect.x;
        this.m_MouseMoveArgs.Y += (int)this._canvasRect.y;*/
        //if (num != 0 && num2 != 0)
        //Debug.Log("num:" + num + " num2:" + num2 + " handle:" + this.m_MouseDownRegion);
        if (GUI.enabled && this.m_MouseDown)
        {
            if ((this.m_MouseDownRegion == MouseRegion.HandleTreeAndFile) && (num2 != 0))
            {
                LayoutEditorGUI.panelLayoutTreeHeight += num2;
                RequestRepaint();
            }
            else if ((this.m_MouseDownRegion == MouseRegion.HandleTreeAndMain) && (num != 0))
            {
                LayoutEditorGUI.panelLayoutListWidth += num;
                RequestRepaint();
            }
            else if ((this.m_MouseDownRegion == MouseRegion.HandleMainAndProp) && (num != 0))
            {
                LayoutEditorGUI.panelProperWidth -= num;
                RequestRepaint();
            }
        }
    }

    void OnGUI()
    {
        LayoutEditorGUIStyle.Init();
        this._mouseRegionUpdate = MouseRegion.Outside;

        bool mouseDown = this.m_MouseDown;

        HandleInput();


        EditorGUILayout.BeginVertical();
        RenderToolBar();

        Layout cur_layout = m_layout_mng.CurEditLayout;
        //if (cur_layout != null)
        {
            EditorGUILayout.BeginHorizontal();

            Rect realRect = EditorGUILayout.BeginVertical(GUILayout.Width(LayoutEditorGUI.panelLayoutListWidth));
            if (realRect.width > LayoutEditorGUI.panelLayoutListWidth)
                LayoutEditorGUI.panelLayoutListWidth = (int)realRect.width;

            RenderCurUIHierarchy();
            DrawGUIHorizontalDivider();
            SetMouseRegion(MouseRegion.HandleTreeAndFile);
            RenderLayoutList();

            EditorGUILayout.EndVertical();

            DrawGUIVerticalDivider();
            SetMouseRegion(MouseRegion.HandleTreeAndMain);

            RenderView();

            DrawGUIVerticalDivider();
            SetMouseRegion(MouseRegion.HandleMainAndProp);

            OnInspector();

            EditorGUILayout.EndHorizontal();

            CalculateMouseRegion();
            if (!mouseDown && this.m_MouseDown)
            {
                this.m_MouseDownRegion = this._mouseRegion;
            }
        }

        EditorGUILayout.EndVertical();
    }

    private void HandleInput()
    {
        Event current = Event.current;
        switch (current.type)
        {
            case EventType.MouseDown:
                //GUI.FocusControl("");
                if (current.button != 1)
                {
                    if (!this.m_MouseDown)
                    {
                        this.m_MouseDown = true;
                    }
                    curMouseX = current.mousePosition.x;
                    curMouseY = current.mousePosition.y;
                }
                return;

            case EventType.MouseUp:
                this.m_MouseDown = false;
                return;

            case EventType.MouseMove:
            case EventType.MouseDrag:
                curMouseX = current.mousePosition.x;
                curMouseY = current.mousePosition.y;
                return;

            case EventType.keyDown:
                if (current.control && current.keyCode == KeyCode.E)
                {
                    Debug.Log(current.keyCode.ToString());

                    if (m_layout_mng.CurEditLayout != null)
                    {
                        m_layout_mng.CurEditLayout.Save();
                    }
                }
                return;

            default:
                return;
        }
    }

    void RenderToolBar()
    {
        Layout cur_layout = m_layout_mng.CurEditLayout;

        GUILayout.Label("版本号：" + ConfigTool.Instance.version);
        if (cur_layout == null)
            GUILayout.Label("请打开layout文件来编辑。");
        else
            GUILayout.Label(cur_layout.FileName + (cur_layout.Dirty ? " *" : ""));

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("打开"))
        {
            int res = 0;
            if (cur_layout != null && m_layout_mng.CurEditLayout.Dirty)
            {
                res = EditorUtility.DisplayDialogComplex("", "当前编辑的layout已被修改，是否保存?", "是", "否", "取消");
                switch (res)
                {
                    // 是
                    case 0:
                        if (cur_layout != null)
                        {
                            cur_layout.Save();
                        }

                        break;

                    // 否
                    case 1:
                        break;
                    // 取消
                    case 2:
                        break;
                    default:
                        break;
                }
            }
            if (res != 2)
            {
                string layout = LayoutTool.OpenLayoutDialog();

                if (!string.IsNullOrEmpty(layout))
                {
                    ClearUndos();

                    m_layout_mng.Clear();
                    /*
                    if (res == 1)
                        m_layout_mng.CurEditLayout.Reload();*/

                    //m_layout_mng.RemoveLayout(1);
                    m_layout_mng.ImportLayout(layout);
                    m_layout_mng.SetCurEditLayout(m_layout_mng.LayoutCount - 1);
                    m_layout_mng.SetLayoutVisible(m_layout_mng.LayoutCount - 1, true);
                    ResetLayoutTree();
                    ResetUITree();

                    RequestRepaint();
                }
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.Dirty);
        if (GUILayout.Button("保存"))
        {
            if (cur_layout != null)
            {
                cur_layout.Save();
            }
            //Debug.Log("counter:" + cmdCounter.Cur + "  cur:" + cmdCur);
        }
        GUI.enabled = true;
        if (GUILayout.Button("设置背景图"))
        {
            string file = LayoutTool.OpenBackImage();

            if (!string.IsNullOrEmpty(file))
            {
                m_view.BackTexture = file;
            }
        }
        if (GUILayout.Button("清除背景图"))
        {
            m_view.BackTexture = "";
        }
        GUILayout.Label("背景颜色");
        m_view.BackColor = EditorGUILayout.ColorField(m_view.BackColor);
        m_view.ShowBorder = GUILayout.Toggle(m_view.ShowBorder, "显示边框");
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignLeft));
        if (GUILayout.Button("左对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignLeft);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignVCenter));
        if (GUILayout.Button("纵向居中对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignVCenter);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignRight));
        if (GUILayout.Button("右对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignRight);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignTop));
        if (GUILayout.Button("顶对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignTop);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignHCenter));
        if (GUILayout.Button("横向居中对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignHCenter);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_AlignBottom));
        if (GUILayout.Button("底对齐"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_AlignBottom);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_SameSize));
        if (GUILayout.Button("使大小相同"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_SameSize);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_SameXSpace));
        if (GUILayout.Button("使x间距相同"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_SameXSpace);
            }
        }
        GUI.enabled = (cur_layout != null && cur_layout.ValidFormat(Layout.FormatType.FT_SameYSpace));
        if (GUILayout.Button("使y间距相同"))
        {
            if (cur_layout != null)
            {
                cur_layout.Format(Layout.FormatType.FT_SameYSpace);
            }
        }
        GUI.enabled = (cur_layout != null && !m_view.NormalCamera);
        if (GUILayout.Button("重置相机"))
        {
            m_view.ResetCamera();
        }
        GUI.enabled = true;
        if (GUILayout.Button("使用文档"))
        {
            Help.BrowseURL(ConfigTool.Instance.help_url);
        }
        GUILayout.FlexibleSpace();
        EditorGUILayout.EndHorizontal();
    }

    public void ResetUITree()
    {
        curSelUI = null;
        Layout cur_layout = m_layout_mng.CurEditLayout;
        if (cur_layout == null)
        {
            uiTree = null;
            return;
        }

        buildUITree(cur_layout.Root);
    }

    public void SyncUITreeSelectState(Dictionary<UIElement, bool> SelDic, bool reverse, bool needScroll)
    {
        List<TreeNode> nodeList = new List<TreeNode>();
        if (!reverse)
        {
            syncTreeSelect(SelDic, uiTree.GetRootNode(0), nodeList);
        }
        else
        {
            syncTreeSelectReverse(SelDic, uiTree.GetRootNode(0), nodeList);
        }
        if (needScroll)
            uiTree.ScrollToNode(nodeList);
    }

    private void syncTreeSelect(Dictionary<UIElement, bool> SelDic, TreeNode node, List<TreeNode> nodeList)
    {
        UIElement element = node.DataKey as UIElement;
        if (element == null)
            return;

        if (SelDic.ContainsKey(element))
        {
            node.selected = SelDic[element];
            if (node.selected)
                nodeList.Add(node);
        }

        foreach (TreeNode subNode in node.Children)
        {
            syncTreeSelect(SelDic, subNode, nodeList);
        }
    }

    private void syncTreeSelectReverse(Dictionary<UIElement, bool> SelDic, TreeNode node, List<TreeNode> nodeList)
    {
        UIElement element = node.DataKey as UIElement;
        if (element == null)
            return;

        if (SelDic.ContainsKey(element))
        {
            node.selected = !SelDic[element];
            if (node.selected)
                nodeList.Add(node);
        }

        foreach (TreeNode subNode in node.Children)
        {
            syncTreeSelectReverse(SelDic, subNode, nodeList);
        }
    }

    private void buildUITree(UIElement element)
    {
        uiTree = new EditorTreeView();
        uiTree.OnDrag += onDragNode;
        uiTree.OnDrop += onDropNode;
        uiTree.OnNodeToggleChange += onUINodeToggleChange;
        uiTree.OnNodeNameChange += onUINodeNameChange;
        uiTree.OnNodeSelChange += onUINodeSelChange;
        uiTree.AddMenuItem("添加节点", AddNode);
        uiTree.AddMenuItem("删除节点", RemoveNode);

        TreeNode node = TreeNodeFactory.CreateNewUITreeNode(element.Name, element.Hide, element.Freeze, element.Lock);
        node.DataKey = element;
        node.CanRenameByUI = !LayoutTool.HasUI(element.gameObject, false);
        uiTree.AddRootNode(node);

        buildUITreeRev(element, node);

    }

    private void buildUITreeRev(UIElement element, TreeNode node)
    {
        for (int i = 0; i < element.ChildrenCount; i++)
        {
            UIElement child = element.GetChild(i);

            TreeNode subnode = TreeNodeFactory.CreateNewUITreeNode(child.Name, child.Hide, child.Freeze, child.Lock);
            subnode.DataKey = child;
            subnode.CanRenameByUI = !LayoutTool.HasUI(child.gameObject, false);
            subnode.Hide = element.Lock;
            if (LayoutTool.HasUI(child.gameObject, false))
                subnode.SetColor(new Color(0.8f, 0.35f, 0.0f));
            node.AddChild(subnode);

            buildUITreeRev(child, subnode);
        }
    }

    public void ResetLayoutTree()
    {
        layoutTree = new EditorTreeView();
        layoutTree.OnDrag += onFileTreeDrag;
        layoutTree.OnNodeToggleChange += onLayoutNodeToggleChange;
        layoutTree.CanRenameNode = false;

        for (int i = 0; i < m_layout_mng.LayoutCount; i++)
        {
            TreeNode node = TreeNodeFactory.CreateNewLayoutTreeNode(m_layout_mng.GetLayoutName(i), m_layout_mng.IsLayoutVisible(i), m_layout_mng.CurEditLayoutIndex == i);
            node.DataKey = i;

            layoutTree.AddRootNode(node);
        }
    }

    void RenderCurUIHierarchy()
    {
        if (uiTree == null)
        {
            return;
        }


        Rect realRect = EditorGUILayout.BeginVertical(LayoutEditorGUIStyle.panelBox, GUILayout.Height(LayoutEditorGUI.panelLayoutTreeHeight));
        if (realRect.height > LayoutEditorGUI.panelLayoutTreeHeight)
            LayoutEditorGUI.panelLayoutTreeHeight = (int)realRect.height;

        uiTree.Draw(realRect, this);

        EditorGUILayout.EndVertical();
    }

    void RenderLayoutList()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandHeight(true));

        Rect treeRect = EditorGUILayout.BeginVertical(LayoutEditorGUIStyle.panelBox, GUILayout.ExpandHeight(true));
        if (layoutTree != null)
            layoutTree.Draw(treeRect, this);

        EditorGUILayout.EndVertical();

        EditorGUILayout.BeginHorizontal();

        GUILayout.FlexibleSpace();
        if (GUILayout.Button("导入"))
        {
            if (m_layout_mng.CurEditLayout == null)
            {
                EditorUtility.DisplayDialog("", "请先打开一个layout文件！", "确定");
                return;
            }

            string layout = LayoutTool.OpenLayoutDialog();

            if (!string.IsNullOrEmpty(layout))
            {
                m_layout_mng.ImportLayout(layout);
                m_layout_mng.SetLayoutVisible(m_layout_mng.LayoutCount - 1, true);
                ResetLayoutTree();

                RequestRepaint();
            }
        }
        if (GUILayout.Button("移除"))
        {
            List<TreeNode> selList = layoutTree.GetSelectedNodes();

            bool hasEdit = false;
            foreach (TreeNode node in selList)
            {
                if (node.ToggleList["编辑"])
                {
                    hasEdit = true;
                    break;
                }
            }

            if (!hasEdit && EditorUtility.DisplayDialog("移除layout", "确认要移除选中的layout？", "确认", "取消"))
            {
                foreach (TreeNode node in selList)
                {
                    layoutTree.RemoveRootNode(node);
                    m_layout_mng.RemoveLayout((int)node.DataKey);
                }

                ResetLayoutTree();
                RequestRepaint();
            }
        }
        GUILayout.FlexibleSpace();

        EditorGUILayout.EndHorizontal();
        EditorGUILayout.EndVertical();
    }
    void RenderView()
    {
        Rect viewRect = EditorGUILayout.BeginHorizontal(LayoutEditorGUIStyle.panelBox, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        m_view.Render(viewRect);
        EditorGUILayout.EndHorizontal();
    }
    void OnInspector()
    {
        Rect realRect = EditorGUILayout.BeginHorizontal(LayoutEditorGUIStyle.panelBox, GUILayout.Width(LayoutEditorGUI.panelProperWidth));
        if (realRect.width > LayoutEditorGUI.panelProperWidth)
            LayoutEditorGUI.panelProperWidth = (int)realRect.width;

        EditorGUILayout.BeginVertical(GUI.skin.box, GUILayout.ExpandHeight(true), GUILayout.ExpandWidth(true));

        if (curSelUI != null)
            UIInspector.DrawUIProp(curSelUI, realRect);

        EditorGUILayout.EndVertical();

        EditorGUILayout.EndHorizontal();
    }

    public void SetMouseRegion(MouseRegion region)
    {
        if (Event.current.type == EventType.Repaint)
        {
            this._mouseRegionRect[region] = GUILayoutUtility.GetLastRect();
        }
        if (this._mouseRegionRect.ContainsKey(region) && GUI.enabled)
        {
            switch (region)
            {
                case MouseRegion.HandleTreeAndFile:
                    EditorGUIUtility.AddCursorRect(this._mouseRegionRect[region], MouseCursor.ResizeVertical);
                    return;

                case MouseRegion.HandleTreeAndMain:
                case MouseRegion.HandleMainAndProp:
                    EditorGUIUtility.AddCursorRect(this._mouseRegionRect[region], MouseCursor.ResizeHorizontal);
                    return;
            }
        }
    }

    private void DrawGUIHorizontalDivider()
    {
        GUILayout.Box("", LayoutEditorGUIStyle.hDivider, new GUILayoutOption[] { GUILayout.Height((float)LayoutEditorGUI.panelDividerThickness), GUILayout.ExpandWidth(true) });
    }

    private void DrawGUIVerticalDivider()
    {
        GUILayout.Box("", LayoutEditorGUIStyle.vDivider, new GUILayoutOption[] { GUILayout.Width((float)LayoutEditorGUI.panelDividerThickness), GUILayout.ExpandHeight(true) });
    }

    private void CalculateMouseRegion()
    {
        foreach (KeyValuePair<MouseRegion, Rect> pair in this._mouseRegionRect)
        {
            if (pair.Value.Contains(Event.current.mousePosition))
            {
                this._mouseRegionUpdate = pair.Key;
                break;
            }
        }
    }

    public static void RequestRepaint()
    {
        _wasRepaintRequested = true;
    }

    private void onDragNode(object sender, DragEventArgs e)
    {
        if (e.Node.ToggleList["隐藏"] || e.Node.ToggleList["冻结"] || !e.Node.selected)
        {
            Debug.Log("不能移动处于冻结、隐藏状态下的节点！");
            e.Cancel = true;
            return;
        }
    }

    private void onDropNode(object sender, DropEventArgs e)
    {
        if (e.DestNode == null)
        {
            Debug.Log("拖拽操作需要指定一个父节点！");
            e.Cancel = true;
            return;
        }
        if (e.DestNode != null && (!(e.DestNode.DataKey as UIElement).VisbleGolbal || e.DestNode.ToggleList["锁定"]))
        {
            Debug.Log("不能移动到处于隐藏、锁定状态的节点下！");
            e.Cancel = true;
            return;
        }

        m_layout_mng.CurEditLayout.SetDirty();
        CmdManager.Instance.AddCmd(new UITreeNodeDragCmd(e.Node, e.DestNode, uiTree));
    }

    private void onUINodeToggleChange(object sender, NodeToggleChangeEventArgs e)
    {
        UIElement element = e.Node.DataKey as UIElement;

        if (element != null)
        {
            m_layout_mng.CurEditLayout.SetDirty();
            CmdManager.Instance.AddCmd(new UITreeNodePropChangeCmd(element, e.Node, e.Toggle, m_layout_mng.CurEditLayout));
        }
    }

    private void onLayoutNodeToggleChange(object sender, NodeToggleChangeEventArgs e)
    {
        int index = (int)e.Node.DataKey;

        switch (e.Toggle)
        {
            case "可见":
                if (m_layout_mng.CurEditLayoutIndex == index)
                {
                    Debug.Log("处于编辑状态的layout不可设置为不可见！");
                    e.Cancel = true;
                }
                else
                {
                    m_layout_mng.SetLayoutVisible(index, e.Node.ToggleList["可见"]);
                    RequestRepaint();
                    //CmdManager.Instance.AddCmd(new LayoutTreeNodeVisibleChangeCmd(index, e.Node, m_layout_mng));
                }
                break;
            case "编辑":
                if (m_layout_mng.CurEditLayoutIndex == index)
                {
                    e.Cancel = true;
                }
                else
                {
                    if (m_layout_mng.CurEditLayout.Dirty)
                    {
                        switch (EditorUtility.DisplayDialogComplex("", "当前编辑的layout已被修改，是否保存?", "是", "否", "取消"))
                        {
                            // 是
                            case 0:
                                Layout cur_layout = m_layout_mng.CurEditLayout;
                                if (cur_layout != null)
                                {
                                    cur_layout.Save();
                                }
                                ClearUndos();

                                m_layout_mng.SetCurEditLayout(index);
                                ResetLayoutTree();
                                ResetUITree();

                                RequestRepaint();
                                break;

                            // 否
                            case 1:
                                ClearUndos();

                                m_layout_mng.CurEditLayout.Reload();

                                m_layout_mng.SetCurEditLayout(index);
                                ResetLayoutTree();
                                ResetUITree();

                                RequestRepaint();
                                break;

                            // 取消
                            case 2:
                                e.Cancel = true;
                                break;
                            default:
                                break;
                        }
                    }
                    else
                    {
                        m_layout_mng.SetCurEditLayout(index);
                        ResetLayoutTree();
                        ResetUITree();

                        RequestRepaint();
                    }

                }
                break;
            default:
                break;
        }
    }

    private void onUINodeNameChange(object sender, NodeRenameChangeEventArgs e)
    {
        UIElement element = e.Node.DataKey as UIElement;

        if (element == null)
        {
            e.Cancel = true;
            return;
        }

        if (H3DEditor.LayoutTool.HasUI(element.gameObject, false))
        {
            Debug.Log("UI节点禁止改名！");
            e.Cancel = true;
            return;
        }

        m_layout_mng.CurEditLayout.SetDirty();
        CmdManager.Instance.AddCmd(new ChangeUITreeNodeCmd(e.Node, e.NewName));
    }

    private void onUINodeSelChange(object sender, NodeSelChangeEventArgs e)
    {
        Dictionary<UIElement, bool> selDic = new Dictionary<UIElement, bool>();
        foreach (KeyValuePair<TreeNode, bool> kvp in e.SelChangeDic)
        {
            UIElement element = kvp.Key.DataKey as UIElement;
            if (!m_layout_mng.CurEditLayout.SelElement(element) && kvp.Value)
            {
                kvp.Key.selected = false;
                continue;
            }

            selDic.Add(kvp.Key.DataKey as UIElement, kvp.Value);
        }
        if (selDic.Count == 0)
            return;

        CmdManager.Instance.AddCmd(new UISelectChangeCmd(selDic, m_layout_mng.CurEditLayout, true));
    }

    private void onFileTreeDrag(object sender, DragEventArgs e)
    {
        e.Cancel = true;
    }

    private void AddNode(TreeNode node)
    {
        if (node == null)
        {
            Debug.Log("新建操作必须选中一个父节点！");
            return;
        }
        m_layout_mng.CurEditLayout.SetDirty();
        CmdManager.Instance.AddCmd(new AddUITreeNodeCmd(node, m_layout_mng, uiTree));
    }

    private void RemoveNode(TreeNode node)
    {
        if (node == null)
        {
            Debug.Log("删除操作必须选中一个节点！");
            return;
        }
        if (!(node.DataKey as UIElement).CanRemove())
        {
            Debug.Log("UI节点禁止删除！");
            return;
        }
        m_layout_mng.CurEditLayout.SetDirty();
        CmdManager.Instance.AddCmd(new RemoveUITreeNodeCmd(node, m_layout_mng, uiTree));
    }
}

public static class TreeNodeFactory
{
    static TreeNodeFactory()
    {
        uiProp.Add("隐藏", false);
        uiProp.Add("冻结", false);
        uiProp.Add("锁定", false);

        layoutProp.Add("可见", false);
        layoutProp.Add("编辑", false);
    }

    public static TreeNode CreateNewUITreeNode(string name, bool hide, bool freeze, bool islock)
    {
        uiProp["隐藏"] = hide;
        uiProp["冻结"] = freeze;
        uiProp["锁定"] = islock;
        return new TreeNode(name, uiProp);
    }

    public static TreeNode CreateNewUITreeNode(string name)
    {
        return CreateNewUITreeNode(name, false, false, false);
    }

    public static TreeNode CreateNewLayoutTreeNode(string name, bool visible, bool edit)
    {
        layoutProp["可见"] = visible;
        layoutProp["编辑"] = edit;
        return new TreeNode(name, layoutProp);
    }

    static Dictionary<string, bool> uiProp = new Dictionary<string, bool>();
    static Dictionary<string, bool> layoutProp = new Dictionary<string, bool>();
}

public enum MouseRegion
{
    HandleTreeAndFile,
    HandleTreeAndMain,
    HandleMainAndProp,
    Outside
}
