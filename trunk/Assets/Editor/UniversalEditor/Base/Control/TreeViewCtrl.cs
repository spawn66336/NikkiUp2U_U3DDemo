using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class TreeViewNodeUserParam
{
    //参数名
    public string name;
    //参数描述
    public string desc;
    //参数对象
    public object param;
}

public class TreeViewNodeState
{
   
    public bool IsExpand
    {
        get { return expand; }
        set { expand = value; }
    }

    public bool IsSelected
    {
        get { return selected; }
        set { selected = value; }
    }

    private bool expand = true;
    private bool selected = false;

    public List<TreeViewNodeUserParam> userParams = new List<TreeViewNodeUserParam>();
}

public class TreeViewNode
{

    public void Add( TreeViewNode node )
    {
        if (children.Contains(node))
            return;

        
        if( node.parent != null )
        {
            node.parent.Remove(node);
        }

        node.parent = this;
        node.control = control;
        children.Add(node);
    }

    public void Remove( TreeViewNode node)
    {
        if (!children.Contains(node))
            return;

        children.Remove(node);
    }

    public bool IsRoot()
    {
        return parent == null;
    }

    public bool IsLeaf()
    {
        return children.Count == 0;
    }

    //当前节点所属层级
    public int Level()
    {
        if (parent == null)
            return 0;
        return parent.Level() + 1;
    }

    //获得从根节点到当前节点的路径描述字符串
    public string GetPathString()
    {
        if( parent == null )
        {
            return name;
        } 
        return parent.GetPathString() + "/" + name;
    }

    
    //树节点名称
    public string name;

    //树节点图标
    public Texture image;

    //树节点的标记
    public string tooptip;

    //用户自定义数据
    public object userObject;

    //所属控件
    public TreeViewCtrl control;

    //父节点
    public TreeViewNode parent;

    //子节点
    public List<TreeViewNode> children = new List<TreeViewNode>();

    //树节点状态
    public TreeViewNodeState state = new TreeViewNodeState();

    
    public Rect lastRect = new Rect();

    public Rect lastLabelRect = new Rect();
}

public class TreeViewCtrl : EditorCtrlComposite 
{
    //遍历控件树结构回调,返回true则继续遍历此节点的子树
    public delegate bool VisitCallBack(TreeViewNode n);


    public static void PreorderTraverse(TreeViewNode root, VisitCallBack visitProc)
    {
        Stack<TreeViewNode> visitStack = new Stack<TreeViewNode>();
        visitStack.Push(root);

        while (visitStack.Count > 0)
        {
            TreeViewNode node = visitStack.Pop();
            if( !visitProc(node) )
            {//略过此节点子树
                continue;
            }
             
            int num = node.children.Count;

            for (int i = num - 1; i >= 0; i--)
            {
                visitStack.Push(node.children[i]);
            }
        }
    }


    public TreeViewNode CreateNode( string name )
    {
        TreeViewNode newNode = new TreeViewNode();
        newNode.name = name;
        newNode.control = this;
        return newNode;
    }

    public void Clear()
    {
        roots.Clear();
        scrollPos.Set(0f, 0f);
        currSelectNode = null;
        currSelectPath = "";
        lastValueChangeNodePath = "";
    }
     

    public List<TreeViewNode> Roots
    {
        get { return roots; }
    }
    

    public List<TreeViewNode> roots = new List<TreeViewNode>();
    
    public Vector2 scrollPos = new Vector2();

    //当前选中节点
    public TreeViewNode currSelectNode = null; 

    //用于高亮显示节点
    public string currSelectPath = "";

    //最近一次值变更节点路径
    public string lastValueChangeNodePath = "";
     
}
