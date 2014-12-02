using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;

public class ResourceManageConfigTool  
{

    static EditorRoot s_root = null;

    [MenuItem("H3D/资源管理/资源管理配置工具")]
    public static void ShowConfigDialog()
    {
        EditorRoot root = EditorManager.GetInstance().FindEditor("资源管理配置工具");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("资源管理配置工具", false, InitControls);
        }
    }

    public static void InitControls(EditorRoot editorRoot)
    {
        s_root = editorRoot;

        s_root.position = new Rect(100f, 100f, 1024, 768f);

        {
            s_root.onDestroy = OnDestroy;
            s_root.onEnable = OnEnable;
        }

        HSpliterCtrl hspliter = new HSpliterCtrl();
        hspliter.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30f, true);

        s_root.RootCtrl = hspliter;

        //上方树状视图
        VBoxCtrl vb0 = new VBoxCtrl();
        hspliter.Add(vb0);

        //下方工具条
        HBoxCtrl hb0 = new HBoxCtrl();
        hspliter.Add(hb0);


        TreeViewCtrl treeView = new TreeViewCtrl();
        treeView.Name = "_MainTreeView";
        treeView.onValueChange = OnTreeViewNodeToggle;
        vb0.Add(treeView);

        Rect btnRect = new Rect(0, 0, 60, 20);
        ButtonCtrl okBtn = new ButtonCtrl();
        okBtn.Caption = "确定";
        okBtn.Name = "_OkButton";
        okBtn.Size = btnRect;
        okBtn.onClick = OnOkButtonClick;

        ButtonCtrl cancelBtn = new ButtonCtrl();
        cancelBtn.Caption = "取消";
        cancelBtn.Name = "_CancelButton";
        cancelBtn.Size = btnRect;
        cancelBtn.onClick = OnCancelButtonClick;


        hb0.Add(okBtn);
        hb0.Add(cancelBtn);
    }


    static void UpdateTreeView()
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if( treeView == null )
        {
            return;
        }

        treeView.Clear();

        string[] allAssetPaths = ResourceManageToolUtility.GetAllAssetPaths();

        foreach( var path in allAssetPaths )
        {
            if( ResourceManageToolUtility.PathIsFolder(path) )
            {

                AddAssetToResourceTreeView(path);
            } 
        }
    }

    static void AddAssetToResourceTreeView( string path)
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;

        if (treeView == null)
        {
            return;
        }

        string totalPath = path;
        string currPath = path;
        List<TreeViewNode> currLevelNodeList = treeView.Roots;
        TreeViewNode parentNode = null;
        int len = 0;
        while (currPath != "")
        {
            int i = currPath.IndexOf('/');
            if (i < 0)
            {
                i = currPath.Length;
            }
            len += i + 1;
            string pathNodeName = currPath.Substring(0, i);
            string currNodeFullPath = totalPath.Substring(0, len - 1);
            if (i + 1 < currPath.Length)
            {
                currPath = currPath.Substring(i + 1);
            }
            else
            {
                currPath = "";
            }


            bool findNode = false;
            foreach (var treeNode in currLevelNodeList)
            {
                if (treeNode.name == pathNodeName)
                {
                    findNode = true;
                    parentNode = treeNode;
                    currLevelNodeList = treeNode.children;
                    break;
                }
            }

            if (!findNode)
            { 
                TreeViewNode newNode = new TreeViewNode();
                newNode.name = pathNodeName;
                newNode.image = ResourceManageToolUtility.GetCachedIcon(path);
                newNode.state.IsExpand = true;
                TreeViewNodeUserParam userParam = new TreeViewNodeUserParam();

                bool toggleState = false;
                foreach( var p in ResourceManageConfig.GetInstance().Paths )
                {
                    if (p.Equals(currNodeFullPath))
                    {
                        toggleState = true;
                    }
                } 
                userParam.param = toggleState;
                newNode.state.userParams.Add(userParam);

                if (parentNode == null)
                {//说明需要作为根节点插入树视图中
                    currLevelNodeList.Add(newNode);
                }
                else
                {
                    parentNode.Add(newNode);
                } 
                parentNode = newNode;
                currLevelNodeList = newNode.children;
            } 
        }
    }

    static void OnTreeViewNodeToggle(EditorControl c, float value)
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if (treeView == null)
        {
            return;
        }

        RecalcTreeNodes(); 
        RequestRepaint();
    }

    static void RecalcTreeNodes()
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if (treeView == null)
        {
            return;
        }

        foreach (var root in treeView.Roots)
        {
            TreeViewCtrl.PreorderTraverse(root, RecalcTreeNodeVisitCallback);
        } 
    }

    static bool RecalcTreeNodeVisitCallback(TreeViewNode n)
    {
        bool parentAlreadyToggled = false;
        TreeViewNode parent = n.parent;
        while(parent != null)
        {
            if( (bool)parent.state.userParams[0].param )
            {
                parentAlreadyToggled = true;
            }
            parent = parent.parent;
        }

        if( parentAlreadyToggled )
        {
            n.state.userParams[0].param = false;
        }
        return true;
    }

    static void CollectAllPaths()
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if (treeView == null)
        {
            return;
        }

        ResourceManageConfig.GetInstance().Paths.Clear();

        foreach (var root in treeView.Roots)
        {
            TreeViewCtrl.PreorderTraverse(root, CollectAllPathsTreeNodeVisitCallback);
        } 
    }

    static bool CollectAllPathsTreeNodeVisitCallback(TreeViewNode n)
    {
        if( (bool)n.state.userParams[0].param )
        {
            string path = n.GetPathString();
            ResourceManageConfig.GetInstance().Paths.Add(path);
        }
        return true;
    }

    static void OnOkButtonClick(EditorControl c)
    {
        RecalcTreeNodes();
        CollectAllPaths();
        ResourceManageConfig.GetInstance().Save();
        s_root.ShutDown();
    }

    static void OnCancelButtonClick(EditorControl c)
    {
        s_root.ShutDown();
    }

    static void OnEnable(EditorRoot root)
    {
        ResourceManageConfig.GetInstance().Init();
        UpdateTreeView();
    }

    static void OnDestroy(EditorRoot root)
    {
        ResourceManageConfig.DestoryInstance();
    }

    static void RequestRepaint()
    {
        s_root.RequestRepaint();
    }

	
}
