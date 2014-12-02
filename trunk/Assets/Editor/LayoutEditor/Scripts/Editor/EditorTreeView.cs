using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;

public class EditorTreeView
{
    public event DragEventHandler OnDrag;
    public event DropEventHandler OnDrop;
    public event NodeToggleChangeEventHandler OnNodeToggleChange;
    public event NodeRenameChangeEventHandler OnNodeNameChange;
    public event NodeSelChangeEventHandler OnNodeSelChange;
    public delegate void MenuFunc(TreeNode curSelectNode);

    public object DataKey = null;
    public bool CanRenameNode = true;
    public bool HideFoldOutWhenAllChildrenHide = true;

    private List<TreeNode> RootNode = new List<TreeNode>();

    bool multiSelEnd = true;
    bool mutilSelectDown = false;
    bool ctrlDown = false;
    bool shiftDown = false;
    bool inShiftSelRange = false;
    bool leftDown = false;
    bool leftDrag = false;
    bool leftUp = false;
    bool rightUp = false;
    TreeNode curNode = null;
    TreeNode dragDestNode = null;
    TreeNode menuNode = null;
    private Dictionary<string, MenuFunc> menuItemDic = new Dictionary<string, MenuFunc>();

    private List<TreeNode> nodeToScroll = null;
    private TreeNode renamingNode = null;
    private TreeNode nodeToRename = null;
    private List<TreeNode> nodeToRenameList = new List<TreeNode>();
    private bool startRename = true;
    private bool dragged = false;
    private bool endRenaming = false;
    private string renameStr = "";
    private readonly string renameTextField = "renameTextField";
    Rect renameRect = new Rect();
    private Dictionary<TreeNode, bool> selectChangeDic = new Dictionary<TreeNode, bool>();

    float yToScroll = 0f;
    bool hasFoundNodeToScroll = false;
    bool findNodeToScroll = false;
    Vector2 scrollViewPosition = new Vector2();
    Vector2 innerPos = new Vector2();

    public void AddRootNode(TreeNode node)
    {
        RootNode.Add(node);
        RootNode.Sort(TreeNodeComparer.Instance);
    }

    public void RemoveRootNode(TreeNode node)
    {
        RootNode.Remove(node);
    }

    public int GetRootNodeCount()
    {
        return RootNode.Count;
    }

    public TreeNode GetRootNode(int index)
    {
        if (index >= RootNode.Count)
            return null;
        else
            return RootNode[index];
    }

    public List<TreeNode> GetSelectedNodes()
    {
        List<TreeNode> nodeList = new List<TreeNode>();

        foreach (TreeNode node in RootNode)
        {
            getSelNodes(node, nodeList);
        }

        return nodeList;
    }

    public void ScrollToNode(List<TreeNode> nodeList)
    {
        if (nodeList.Count > 0)
            nodeToScroll = nodeList;
        else
            nodeToScroll = null;
        
        foreach (TreeNode node in nodeList)
        {
            TreeNode parentNode = node.Parent;
            while (parentNode != null)
            {
                parentNode.Foldout = true;
                parentNode = parentNode.Parent;
            }
        }
    }

    private void getSelNodes(TreeNode node, List<TreeNode> nodeList)
    {
        if (node.selected)
            nodeList.Add(node);

        foreach (TreeNode subNode in node.Children)
        {
            getSelNodes(subNode, nodeList);
        }
    }

    public void Draw(Rect rect, EditorWindow window)
    {
        Event e = Event.current;

        if (e.isMouse)
        {
            leftDown = false;
            leftDrag = false;
            leftUp = false;
            rightUp = false;
        }

        ctrlDown = e.control;
        shiftDown = !ctrlDown && e.shift;

        if (e.type == EventType.Repaint)
            innerPos = new Vector2(e.mousePosition.x - rect.x + scrollViewPosition.x, e.mousePosition.y - rect.y + scrollViewPosition.y);

        if (e.rawType == EventType.MouseDown && e.button == 0)
        {
            if (rect.Contains(e.mousePosition) && rect.xMax - 16 > e.mousePosition.x) // 避免在有滚动条的情况下穿透点击
            {
                leftDown = true;
            }
            if (renamingNode != null && !renameRect.Contains(innerPos))
            {
                endRenaming = true;
                leftDown = false;
            }
        }
        else if (e.rawType == EventType.MouseDrag && e.button == 0 && renamingNode == null)
        {
            leftDrag = true;
            dragged = true;

            if (curNode != null && OnDrag != null)
            {
                DragEventArgs args = new DragEventArgs(curNode);
                OnDrag(this, args);
                if (args.Cancel)
                    curNode = null;
            }
        }
        else if (e.rawType == EventType.MouseUp && e.button == 0)
        {
            mutilSelectDown = false;
            leftUp = true;
        }
        else if (e.rawType == EventType.MouseUp && e.button == 1)
        {
            if (rect.Contains(Event.current.mousePosition))
            {
                rightUp = true;
                menuNode = null;
            }
        }
        
        if (leftDown && shiftDown && !mutilSelectDown && Event.current.type == EventType.repaint)
        {
            multiSelEnd = false;
            multiSelCache.Clear();
            inShiftSelRange = false;
            //clearSelect();
        }

        if (leftUp && dragged)
        {
            nodeToRename = null;
        }
        if (CanRenameNode && leftUp && !dragged && nodeToRename != null && nodeToRename.CanRenameByUI)
        {
            TreeNode temNode = nodeToRename;
            new System.Threading.Thread(delegate()
            {
                System.Threading.Thread.Sleep(100);
                renamingNode = temNode;
                LayoutEditorWindow.RequestRepaint();
            }).Start();

            nodeToRename = null;
        }

        if (hasFoundNodeToScroll)
        {
            float nodeHeight = 18f;
            if (rect.height + scrollViewPosition.y < yToScroll + nodeHeight)
            {
                scrollViewPosition.y = yToScroll - rect.height + nodeHeight;
            }
            else if (yToScroll < scrollViewPosition.y)
            {
                scrollViewPosition.y = yToScroll;
            }

            hasFoundNodeToScroll = false;
            yToScroll = 0.0f;
        }

        scrollViewPosition = EditorGUILayout.BeginScrollView(scrollViewPosition, GUIStyle.none, GUI.skin.verticalScrollbar);
        
        EditorGUILayout.BeginVertical();
        SetGeneralStyles();

        if (nodeToScroll != null)
        {
            findNodeToScroll = true;
            hasFoundNodeToScroll = false;
        }

        foreach (TreeNode node in RootNode)
        {
            if (!node.Hide)
                DrawNode(node);
        }
        EditorGUILayout.EndVertical();

        if (findNodeToScroll)
        {
            nodeToScroll = null;
            findNodeToScroll = false;
        }

        if (leftUp)
        {
            nodeToRenameList = GetSelectedNodes();
            dragged = false;
        }

        if (renamingNode != null)
        {
            showRenameTextbox();
        }

        if (leftDrag && curNode != null)
        {
            Vector2 size = GUI.skin.GetStyle("Label").CalcSize(new GUIContent(curNode.NodeName));
            EditorGUI.LabelField(new Rect(Event.current.mousePosition.x, Event.current.mousePosition.y, size.x, size.y), curNode.NodeName);
        }

        EditorGUILayout.EndScrollView();

        if (leftDown && shiftDown && selectChangeDic.Count > 0 && !mutilSelectDown)
        {
            multiSelEnd = true;
            mutilSelectDown = true;
            inShiftSelRange = false;
            if (OnNodeSelChange != null)
            {
                OnNodeSelChange(this, new NodeSelChangeEventArgs(selectChangeDic));
            }
            selectChangeDic.Clear();
            multiSelCache.Clear();
        }

        if (leftUp && curNode != null)
        {
            if (shiftDown)
                curNode = null;
        }

        if (leftUp && curNode != null)
        {            
            if (curNode != dragDestNode)
            {
                performDrag(rect);
            }

            dragDestNode = null;
            curNode = null;
        }

        if (rightUp)
        {
            ShowMenu();
            e.Use();
        }

        if (Event.current.isMouse)
            window.Repaint();
    }

    private void showRenameTextbox()
    {
        if (startRename)
            renameStr = renamingNode.NodeName;

        if (Event.current.type == EventType.repaint)
            renameRect = new Rect(renamingNode.x - 3, renamingNode.y + 1, renamingNode.xMax - renamingNode.x, renamingNode.yMax - renamingNode.y);

        GUI.SetNextControlName(renameTextField);
        if (_textBoxStyle == null)
        {
            _textBoxStyle = new GUIStyle(GUI.skin.textField);
            _textBoxStyle.fontStyle = FontStyle.Bold;
        }
        renameStr = GUI.TextField(renameRect, renameStr, _textBoxStyle);

        if (startRename)
        {
            TextEditor te = (TextEditor)GUIUtility.GetStateObject(typeof(TextEditor), GUIUtility.keyboardControl);
            te.SelectAll();
            startRename = false;
            GUI.FocusControl(renameTextField);
            LayoutEditorWindow.RequestRepaint();
        }

        if (Event.current.keyCode == KeyCode.Escape)
        {
            renamingNode = null;
            startRename = true;
            LayoutEditorWindow.RequestRepaint();
        }
        else if ((!string.IsNullOrEmpty(GUI.GetNameOfFocusedControl()) && GUI.GetNameOfFocusedControl() != renameTextField) || Event.current.keyCode == KeyCode.Return || endRenaming == true)
        {
            endRenaming = false;
            if (renameStr != renamingNode.NodeName)
            {
                performNodeNameChange(renamingNode, renameStr);
            }
            renamingNode = null;
            startRename = true;
            LayoutEditorWindow.RequestRepaint();
        }
    }

    private void performDrag(Rect rect)
    {
        if (dragDestNode == null && curNode.Parent != null && rect.Contains(Event.current.mousePosition))
        {
            DropEventArgs dropArg = new DropEventArgs(curNode, dragDestNode);
            if (OnDrop != null)
            {
                OnDrop(this, dropArg);
            }
            if (!dropArg.Cancel)
            {
                curNode.Parent.RemoveChild(curNode);
                AddRootNode(curNode);
            }
        }
        else if (dragDestNode != null && CheckCanMove(dragDestNode, curNode))
        {
            DropEventArgs dropArg = new DropEventArgs(curNode, dragDestNode);
            if (OnDrop != null)
            {
                OnDrop(this, dropArg);
            }
            if (!dropArg.Cancel)
            {
                if (curNode.Parent != null)
                    curNode.Parent.RemoveChild(curNode);
                if (RootNode.Contains(curNode))
                    RootNode.Remove(curNode);
                dragDestNode.AddChild(curNode);
            }
        }
    }

    private void ShowMenu()
    {
        GenericMenu menu = new GenericMenu();
        if (menuItemDic.Count > 0)
        {
            foreach (KeyValuePair<string, MenuFunc> kvp in menuItemDic)
            {
                MenuFunc func = kvp.Value;
                menu.AddItem(new GUIContent(kvp.Key), false, delegate() { func(menuNode); });
            }
        }

        endRenaming = true;
        menu.ShowAsContext();

        curNode = null;
        rightUp = false;
    }

    bool firstSetStyle = false;
    bool lastProSkin = false;
    public EditorTreeView()
    {
        lastProSkin = EditorGUIUtility.isProSkin;
        firstSetStyle = true;
    }
    private void SetGeneralStyles()
    {
        if (_toggleStyle == null)
        {
            _toggleStyle = new GUIStyle(GUI.skin.toggle);
            _toggleStyle.alignment = TextAnchor.MiddleCenter;
            _toggleStyle.margin.top = 0;
            _toggleStyle.margin.bottom = 0;
            _toggleStyle.padding.top = 1;
            _toggleStyle.padding.bottom = 1;
            _toggleStyle.border.top = 0;
            _toggleStyle.border.bottom = 0;
        }
        if (_marginStyle == null)
        {
            _marginStyle = new GUIStyle();
            _marginStyle.normal.textColor = Color.white;
        }
        if (_labelStyle == null)
        {
            _labelStyle = new GUIStyle();
            _labelStyle.padding.top = -1;
            _labelStyle.padding.bottom = 1;
            _labelStyle.fontStyle = FontStyle.Bold;
        }
        if (_foldoutStyle == null)
        {
            _foldoutStyle = new GUIStyle(EditorStyles.foldout);
            _foldoutStyle.fontStyle = FontStyle.Bold;
        }
        
        if (lastProSkin != EditorGUIUtility.isProSkin || firstSetStyle)
        {
            firstSetStyle = false;
            lastProSkin = EditorGUIUtility.isProSkin;
            if (EditorGUIUtility.isProSkin)
            {
                _marginStyle.normal.background = darkBackground;

                _labelStyle.normal.textColor = darkLabelTextColor;
                _foldoutStyle.active.textColor = darkLabelTextColor;
                _foldoutStyle.normal.textColor = darkLabelTextColor;
                _foldoutStyle.onNormal.textColor = darkLabelTextColor;
                _foldoutStyle.onActive.textColor = darkLabelTextColor;
                _foldoutStyle.focused.textColor = darkLabelTextColor;
                _foldoutStyle.onFocused.textColor = darkLabelTextColor;
            }
            else
            {
                _marginStyle.normal.background = lightBackground;

                _labelStyle.normal.textColor = defaultLabelTextColor;
                _foldoutStyle.active.textColor = defaultLabelTextColor;
                _foldoutStyle.normal.textColor = defaultLabelTextColor;
                _foldoutStyle.onNormal.textColor = defaultLabelTextColor;
                _foldoutStyle.onActive.textColor = defaultLabelTextColor;
                _foldoutStyle.focused.textColor = defaultLabelTextColor;
                _foldoutStyle.onFocused.textColor = defaultLabelTextColor;
            }
        }
    }

    private void performNodeNameChange(TreeNode node, string newName)
    {
        NodeRenameChangeEventArgs e = new NodeRenameChangeEventArgs(node, newName);
        if (OnNodeNameChange != null)
        {
            OnNodeNameChange(this, e);
        }
        if (!e.Cancel)
        {
            node.NodeName = e.NewName;
        }
    }

    public void AddMenuItem(string name, MenuFunc callback)
    {
        if (menuItemDic.ContainsKey(name))
            throw new System.Exception("已存在相同名称的右键菜单！");

        menuItemDic.Add(name, callback);
    }

    private bool CheckCanMove(TreeNode destNode, TreeNode node)
    {
        if (node.Parent == destNode)
            return false;

        TreeNode parentNode = destNode.Parent;
        while (node != parentNode && parentNode != null)
        {
            parentNode = parentNode.Parent;
        }

        if (parentNode == node)
            return false;
        else
            return true;
    }

    private static GUIStyle _textBoxStyle;
    private static GUIStyle _toggleStyle;
    private static GUIStyle _labelStyle;
    private static GUIStyle _marginStyle;
    private static GUIStyle _foldoutStyle;
    private Color darkLabelTextColor = new Color(179f / 255f, 179f / 255f, 179f / 255f); // dark style下foldout文字颜色
    private Color defaultLabelTextColor = (new GUIStyle()).normal.textColor;
    private Texture2D lightBackground = MakeTex(1, 1, new Color(0.6f, 0.6f, 0.9f, 1.0f));
    private Texture2D darkBackground = MakeTex(1, 1, new Color(0.4f, 0.4f, 0.7f, 1.0f));
    private List<TreeNode> multiSelCache = new List<TreeNode>();

    private void DrawNode(TreeNode node)
    {
        if (node == null)
            return;

        GUIStyle foldoutStyle = null;
        GUIStyle labelStyle = null;

        if (!node.UseDefaultColor)
        {
            foldoutStyle = node.FoldoutStyle;
            labelStyle = node.LabelStyle;
        }
        else
        {
            foldoutStyle = _foldoutStyle;
            labelStyle = _labelStyle;
        }

        if (node.selected)
        {
            GUILayout.BeginHorizontal(_marginStyle);
        }
        else
        {
            GUILayout.BeginHorizontal();
        }


        bool childrenAllHide = true;
        foreach (TreeNode child in node.Children)
        {
            if (!child.Hide)
            {
                childrenAllHide = false;
                break;
            }
        }

        bool showAsLabel = node.Children.Count == 0 || (HideFoldOutWhenAllChildrenHide && childrenAllHide);

        GUILayout.Space(showAsLabel ? 12 + 16 * node.getLayerLevel() : 16 * node.getLayerLevel());

        if (showAsLabel)
        {
            EditorGUILayout.LabelField(node.NodeName, labelStyle, GUILayout.Width(labelStyle.CalcSize(new GUIContent(node.NodeName)).x));
        }
        else
        {
            node.Foldout = EditorGUILayout.Foldout(node.Foldout, node.NodeName, foldoutStyle);
        }

        if (Event.current.type == EventType.repaint)
            node.x = GUILayoutUtility.GetLastRect().x + (node.Children.Count == 0 ? 0 : 12);

        GUILayout.FlexibleSpace();

        if (Event.current.type == EventType.repaint)
            node.xMax = GUILayoutUtility.GetLastRect().width + GUILayoutUtility.GetLastRect().x;

        if (node.ToggleList != null && node.ToggleList.Count > 0)
        {
            List<string> toggles = new List<string>();
            foreach (string toggleName in node.ToggleList.Keys)
            {
                toggles.Add(toggleName);
            }
            foreach (string toggleName in toggles)
            {
                bool b = node.ToggleList[toggleName];
                node.ToggleList[toggleName] = GUILayout.Toggle(b, toggleName, _toggleStyle);

                NodeToggleChangeEventArgs arg = new NodeToggleChangeEventArgs(node, toggleName);
                if (b != node.ToggleList[toggleName] && OnNodeToggleChange != null)
                {
                    OnNodeToggleChange(this, arg);
                    if (arg.Cancel)
                        node.ToggleList[toggleName] = b;
                }
            }
        }

        if (Event.current.type == EventType.repaint)
            node.y = GUILayoutUtility.GetLastRect().y;
        if (Event.current.type == EventType.repaint)
            node.yMax = GUILayoutUtility.GetLastRect().yMax;

        bool inThisNode = Event.current.mousePosition.x < node.xMax && Event.current.mousePosition.y <= node.yMax && Event.current.mousePosition.y >= node.y;

        if (findNodeToScroll && !hasFoundNodeToScroll && nodeToScroll.Contains(node))
        {
            hasFoundNodeToScroll = true;
            yToScroll = node.y;
        }

        if (!multiSelEnd && leftDown && node.selected && Event.current.type == EventType.repaint && shiftDown && !mutilSelectDown)
        {
            if (!inShiftSelRange)
            {
                inShiftSelRange = true;
            }
            else
            {
                foreach (TreeNode n in multiSelCache)
                {
                    n.selected = true;
                    if (selectChangeDic.ContainsKey(n))
                        selectChangeDic.Remove(n);
                    else
                        selectChangeDic.Add(n, true);
                }
                multiSelCache.Clear();
            }
        }
        else if (!inThisNode && !multiSelEnd && leftDown && inShiftSelRange && !mutilSelectDown && shiftDown && Event.current.type == EventType.repaint)
        {
            if (!node.selected)
            {
                multiSelCache.Add(node);
            }
        }

        if (!inThisNode && multiSelEnd && inShiftSelRange && leftDown && /*node == lastSelNode*/ node.selected && Event.current.type == EventType.repaint && shiftDown && !mutilSelectDown)
        {
            if (selectChangeDic.ContainsKey(node))
                selectChangeDic[node] = false;
            else
                selectChangeDic.Add(node, false);
        }

        if ((leftDown || rightUp) && inThisNode && Event.current.type == EventType.repaint)
        {
            if (!ctrlDown && !shiftDown)
                singleSelectNode(node);
            else if (!mutilSelectDown && ctrlDown)
                multiSelectNode(node);
            else if (!mutilSelectDown && shiftDown && !multiSelEnd)
            {
                if (!node.selected)
                {
                    multiSelCache.Add(node);
                }

                if (inShiftSelRange)
                {
                    multiSelEnd = true;

                    foreach (TreeNode n in multiSelCache)
                    {
                        n.selected = true;
                        if (selectChangeDic.ContainsKey(n))
                            selectChangeDic.Remove(n);
                        else
                            selectChangeDic.Add(n, true);
                    }
                    multiSelCache.Clear();
                }
                else
                    inShiftSelRange = true;
                
            }

            if (!shiftDown)
                curNode = node;

            if (leftDown && nodeToRenameList.Contains(node) && Event.current.mousePosition.x > node.x && !ctrlDown && !shiftDown)
                nodeToRename = node;
        }
        if (rightUp && inThisNode)
        {
            menuNode = node;
        }
        if (leftUp && curNode != null)
        {
            if (inThisNode)
            {
                dragDestNode = node;
            }
        }

        GUILayout.EndHorizontal();

        if (node.Foldout)
        {
            foreach (TreeNode subNode in node.Children)
            {
                if (!subNode.Hide)
                    DrawNode(subNode);
            }
        }
    }

    private void singleSelectNode(TreeNode node)
    {
        clearSelect();
        if (selectChangeDic.ContainsKey(node))
            selectChangeDic.Remove(node);
        else
            selectChangeDic.Add(node, true);

        node.selected = true;

        if (selectChangeDic.Count > 0 && OnNodeSelChange != null)
        {
            OnNodeSelChange(this, new NodeSelChangeEventArgs(selectChangeDic));
            selectChangeDic.Clear();
        }
    }

    private void multiSelectNode(TreeNode node)
    {
        selectChangeDic.Clear();
        selectChangeDic.Add(node, !node.selected);

        node.selected = !node.selected;

        if (OnNodeSelChange != null)
        {
            OnNodeSelChange(this, new NodeSelChangeEventArgs(selectChangeDic));
            selectChangeDic.Clear();
        }

        mutilSelectDown = true;
    }

    private void clearSelect()
    {
        selectChangeDic.Clear();
        foreach (TreeNode node in RootNode)
        {
            clearSelect(node);
        }
    }

    private void clearSelect(TreeNode node)
    {
        if (node.selected)
            selectChangeDic.Add(node, false);

        node.selected = false;
        foreach (TreeNode subNode in node.Children)
        {
            clearSelect(subNode);
        }
    }

    private static Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }

}

public class TreeNode
{
    public TreeNode(string name)
    {
        NodeName = name;
        initStyle();
    }

    public TreeNode(string name, Dictionary<string, bool> toggleList)
    {
        NodeName = name;
        ToggleList = new Dictionary<string, bool>(toggleList);
        initStyle();
    }

    private void initStyle()
    {
        foldoutStyle = new GUIStyle(EditorStyles.foldout);
        foldoutStyle.fontStyle = FontStyle.Bold;

        labelStyle = new GUIStyle();
        labelStyle.padding.top = -1;
        labelStyle.padding.bottom = 1;
        labelStyle.fontStyle = FontStyle.Bold;
    }

    public void AddChild(TreeNode node)
    {
        Children.Add(node);
        node.Parent = this;
        Children.Sort(TreeNodeComparer.Instance);
        //node.Tree = this.Tree;
    }

    public void RemoveChild(TreeNode node)
    {
        if (Children.Contains(node))
        {
            Children.Remove(node);
            node.Parent = null;
            //node.Tree = null;
        }
    }

    public int getLayerLevel()
    {
        int level = 0;

        TreeNode tmp = this;
        while (tmp.Parent != null)
        {
            level++;
            tmp = tmp.Parent;
        }

        return level;
    }

    private void setChildrenTree(EditorTreeView tree)
    {
        //mTree = tree;
        foreach (TreeNode node in Children)
        {
            setChildrenTree(tree);
        }
    }

    public void SetColor(Color color)
    {
        this.color = color;

        foldoutStyle.onActive.textColor = color;
        foldoutStyle.onNormal.textColor = color;
        foldoutStyle.normal.textColor = color;
        foldoutStyle.active.textColor = color;
        foldoutStyle.focused.textColor = color;
        foldoutStyle.onFocused.textColor = color;
        labelStyle.normal.textColor = color;

        useDefaultColor = false;
    }

    public void ResetColor()
    {
        useDefaultColor = true;
    }

    public bool UseDefaultColor
    { get { return useDefaultColor; } }

    public Color TextColor
    { get { return color; } }

    private bool useDefaultColor = true;
    private Color color;

    public GUIStyle FoldoutStyle
    { get { return foldoutStyle; } }
    public GUIStyle LabelStyle
    { get { return labelStyle; } }

    private GUIStyle foldoutStyle = null;
    private GUIStyle labelStyle = null;

    public Dictionary<string, bool> ToggleList = null;
    public TreeNode Parent;
    public bool Foldout = true;
    public List<TreeNode> Children = new List<TreeNode>();
    public string NodeName = "";
    public bool selected = false;
    public object DataKey = null;
    public bool CanRenameByUI = true;
    public bool Hide = false;

    public float x = 0f;
    public float xMax = 0f;
    public float y = 0f;
    public float yMax = 0f;
}

public delegate void DragEventHandler(object sender, DragEventArgs e);
public class DragEventArgs
{
    public DragEventArgs(TreeNode node)
    {
        Node = node;
    }
    public readonly TreeNode Node;
    public bool Cancel = false;
}

public delegate void DropEventHandler(object sender, DropEventArgs e);
public class DropEventArgs
{
    public DropEventArgs(TreeNode node, TreeNode destNode)
    {
        Node = node;
        DestNode = destNode;
    }
    public readonly TreeNode Node;
    public readonly TreeNode DestNode;
    public bool Cancel = false;
}

public delegate void NodeToggleChangeEventHandler(object sender, NodeToggleChangeEventArgs e);
public class NodeToggleChangeEventArgs
{
    public NodeToggleChangeEventArgs(TreeNode node, string toggle)
    {
        Node = node;
        Toggle = toggle;
    }
    public readonly TreeNode Node;
    public readonly string Toggle;
    public bool Cancel = false;
}

public delegate void NodeRenameChangeEventHandler(object sender, NodeRenameChangeEventArgs e);
public class NodeRenameChangeEventArgs
{
    public NodeRenameChangeEventArgs(TreeNode node, string newName)
    {
        Node = node;
        NewName = newName;
    }
    public readonly TreeNode Node;
    public readonly string NewName;
    public bool Cancel = false;
}

public delegate void NodeSelChangeEventHandler(object sender, NodeSelChangeEventArgs e);
public class NodeSelChangeEventArgs
{
    public NodeSelChangeEventArgs(Dictionary<TreeNode, bool> selChangeDic)
    {
        SelChangeDic = selChangeDic;
    }
    public readonly Dictionary<TreeNode, bool> SelChangeDic;
}

public class TreeNodeComparer : IComparer<TreeNode>
{
    private static TreeNodeComparer _Instance = null;
    public static TreeNodeComparer Instance
    {
        get
        {
            if (_Instance == null)
                _Instance = new TreeNodeComparer();

            return _Instance;
        }
    }

    public int Compare(TreeNode x, TreeNode y)
    {
        return x.NodeName.CompareTo(y.NodeName);
    }
}