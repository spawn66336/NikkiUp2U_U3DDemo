using UnityEngine;
using System.Collections;
using UnityEditor;
using YamlDotNet.RepresentationModel;
using System.IO;
using System;
using System.Globalization;
using System.Collections.Generic;

public class ResourceRefTool 
{
    static EditorRoot s_root = null;

    static Guid s_select_asset = Guid.Empty;

    [MenuItem("H3D/资源管理/资源检查工具")]
    static void Init()
    {
        EditorRoot root =
        EditorManager.GetInstance().FindEditor("资源检查工具");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("资源检查工具", false, InitControls);
        } 
    }

    public static void InitControls(EditorRoot editorRoot)
    {
        s_root = editorRoot;

        s_root.position = new Rect(100f, 100f, 1024, 768f);

        {//对编辑器全局消息响应
            EditorApplication.projectWindowItemOnGUI += OnProjectWindowItem;
            EditorApplication.projectWindowChanged += OnProjectWindowChanged; 
        }
        {//协程回调
            s_root.onCoroutineMessage = OnCoroutineMessage;
            s_root.onCoroutineTaskFinished = OnCoroutineTaskFinished;
            s_root.onDestroy = OnDestroy;
            s_root.onEnable = OnEnable;
        }

        {//注册数据库变化回调
            ResourceManageToolModel.GetInstance().onResourceDBUpdate = OnAssetDatabaseUpdate;
            ResourceManageToolModel.GetInstance().onResrouceDBStateChange = OnAssetDatabaseStateChange;
        }
         
        HSpliterCtrl hspliter = new HSpliterCtrl();
        s_root.RootCtrl = hspliter;
        hspliter.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30f);

        //用来放置上方菜单
        HBoxCtrl hb0 = new HBoxCtrl(); 
        VSpliterCtrl vspliter = new VSpliterCtrl();
        vspliter.layoutConstraint = LayoutConstraint.GetSpliterConstraint(300f);
        vspliter.Dragable = true;

        hspliter.Add(hb0);
        hspliter.Add(vspliter);

        //用来存放 资源列表、无引用资源列表
        VBoxCtrl vb0 = new VBoxCtrl();
        //用来存放 资源依赖项、反向引用
        VBoxCtrl vb1 = new VBoxCtrl();

        vspliter.Add(vb0);
        vspliter.Add(vb1);

        //左侧TabView
        TabViewCtrl leftTabView = new TabViewCtrl();
        //右侧TabView
        TabViewCtrl rightTabView = new TabViewCtrl();
        vb0.Add(leftTabView);
        vb1.Add(rightTabView);

        //资源列表
        TreeViewCtrl resTreeList = new TreeViewCtrl();
        resTreeList.Caption = "资源列表";
        resTreeList.Name = "_ResTreeList";
        resTreeList.onItemSelected = OnResourcesTreeViewSelectChanged;

        //无引用资源列表
        ListViewCtrl unUsedList = new ListViewCtrl();
        unUsedList.Caption = "无引用资源";
        unUsedList.Name = "_UnUsedResList";
        unUsedList.onItemSelected = OnUnUsedResourcesListViewSelectChanged;

        leftTabView.Add(resTreeList);
        leftTabView.Add(unUsedList);

        //资源依赖项
        TreeViewCtrl resRefTreeView = new TreeViewCtrl();
        resRefTreeView.Caption = "资源依赖项";
        resRefTreeView.Name = "_ResRefTreeView";
       

        //反向引用
        TreeViewCtrl resReverseRefTreeView = new TreeViewCtrl();
        resReverseRefTreeView.Caption = "反向引用";
        resReverseRefTreeView.Name = "_ResReverseRefTreeView";

        rightTabView.Add(resRefTreeView);
        rightTabView.Add(resReverseRefTreeView);


        Rect btnRect = new Rect(0, 0, 120, 20);
        ButtonCtrl searchBtn = new ButtonCtrl();
        searchBtn.Name = "_SearchAllResources";
        searchBtn.Caption = "扫描资源!";
        searchBtn.Size = btnRect;
        searchBtn.onClick += OnSearchAllAssets;
        searchBtn.BtnColor = Color.green;
        searchBtn.Enable = true;

        Rect comboBoxRect = new Rect(0, 0, 100, 20);

        ComboBoxCtrl assetTypeCombo = new ComboBoxCtrl();
        assetTypeCombo.Size = comboBoxRect;
        assetTypeCombo.Caption = "资源类型过滤";
        assetTypeCombo.Name = "_AssetTypeCombo";
        assetTypeCombo.onItemSelected = OnFilterComboSelectChanged;

        List<IAssetFilter> assetFilters = 
        ResourceManageToolModel.GetInstance().AssetFilterList;
        foreach( var filter in assetFilters )
        {
            AssetTypeFilter f = filter as AssetTypeFilter;
            assetTypeCombo.AddItem(f.DisplayTypeName, f.TypeIndex);
        }

        assetTypeCombo.CurrOption = 0;

        TextBoxCtrl searchTextBox = new TextBoxCtrl();
        searchTextBox.Size = comboBoxRect;
        searchTextBox.Icon = UnityInternalIconCache.GetInstance().GetCacheIcon("d_ViewToolZoom");
        searchTextBox.Name = "_SearchBox";
        searchTextBox.Caption = "搜索";
        searchTextBox.Enable = true;
        searchTextBox.onValueChange = OnSearchTextBoxChange;

        ButtonCtrl helpBtn = new ButtonCtrl();
        helpBtn.Name = "_HelpButton";
        helpBtn.Caption = "帮助文档";
        helpBtn.onClick = OnHelp;
        helpBtn.Size = btnRect;
        helpBtn.Enable = true;

        LabelCtrl stateLabel = new LabelCtrl();
        stateLabel.Name = "_StateLabel";
        stateLabel.textColor = Color.red;
        stateLabel.Caption = "数据库没有更新，无法查看无用资源列表与反向引用！";
        stateLabel.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();

        hb0.Add(searchBtn);
        hb0.Add(assetTypeCombo);
        hb0.Add(searchTextBox);
        hb0.Add(helpBtn);
        hb0.Add(stateLabel);
        
    }

    static void OnEnable( EditorRoot root)
    {
        ResourceManageConfig.GetInstance().Init();
        ResourceManageToolModel.GetInstance().Init();
        RebuildResourcesTreeView();
    }
 
    static void OnSearchAllAssets( EditorControl c )
    {
       IEditorCoroutineTask newTask = ResourceManageToolModel.GetInstance().NewUpdateTask();
       if (newTask != null)
       {
           s_root.GetCoroutine().AddTask(newTask);
       }
    }

    static void OnCoroutineTaskFinished(Guid taskID, object resultObj)
    { 
        if(ResourceManageToolModel.GetInstance().UpdateAssetDatabase(taskID,resultObj))
        {
            EditorUtility.ClearProgressBar();
        } 
    }

    static void OnHelp( EditorControl c )
    { 
    }

    static void OnFilterComboSelectChanged( EditorControl c , int sel )
    {
        ResourceManageToolModel.GetInstance().SetFilter(sel);
    }

    static void OnCoroutineMessage(EditorCoroutineMessage msg)
    {
        if( msg.msg == EditorCoroutineMessage.Message.PROGRESS_UI )
        {
            EditorUIProgressInfo progressInfo = msg.param0 as EditorUIProgressInfo; 
            float progress = ((float)progressInfo.curr)/((float)progressInfo.total); 
            EditorUtility.DisplayProgressBar("正在扫描",progressInfo.msg,progress);
        }
    }


     
    static void OnProjectWindowItem(string guid, Rect selectionRect)
    {   
    }

    static void OnProjectWindowChanged()
    { 
    }

    static void OnDestroy(EditorRoot root)
    {
        ResourceManageToolModel.GetInstance().onResourceDBUpdate = null;
        ResourceManageToolModel.DestroyInstance();
        ResourceManageConfig.DestoryInstance();

        s_root = null;
    }

    /*
     * 搜索框 
     */

    static void OnSearchTextBoxChange( EditorControl c , float value)
    {
        TextBoxCtrl searchBox = s_root.FindControl("_SearchBox") as TextBoxCtrl;
        ComboBoxCtrl typeFilterComboBox = s_root.FindControl("_AssetTypeCombo") as ComboBoxCtrl;

        typeFilterComboBox.CurrOption = 0; 
        if( searchBox.Text.Length == 0)
        {
            ResourceManageToolModel.GetInstance().SetFilter(0);
        }
        else
        {
            AssetNameFilter nameFilter = new AssetNameFilter();
            nameFilter.SearchText = searchBox.Text;
            ResourceManageToolModel.GetInstance().SetFilter(nameFilter);
        }
        RequestRepaint();
    }

    /*
     * 资源数据库状态标签
     */

    static void UpdateAssetDBStateLabel()
    {
        LabelCtrl dbStateLabel = s_root.FindControl("_StateLabel") as LabelCtrl;
        dbStateLabel.textColor = Color.green;
        dbStateLabel.Caption = "您使用的是\""+ResourceManageToolModel.GetInstance().GetAssetDatabaseUpdateTime()+"\"更新的数据库";
    }


    /*
     *  资源树相关函数
     */

    static void OnResourcesTreeViewSelectChanged( EditorControl c , int i )
    {
        TreeViewCtrl resTreeList = c as TreeViewCtrl;
        if (resTreeList == null)
            return;

        if (resTreeList.currSelectNode != null)
        {
            RebuildResourceRefTreeView((Guid)resTreeList.currSelectNode.userObject);
            RebuildReferencedTreeView((Guid)resTreeList.currSelectNode.userObject);
            RequestRepaint();
        }
    }

    static void RebuildResourcesTreeView()
    {
        TreeViewCtrl resTreeList = s_root.FindControl("_ResTreeList") as TreeViewCtrl;
        resTreeList.Clear();
         
        foreach( var assetInfo in ResourceManageToolModel.GetInstance().AssetList )
        {
            AddAssetToResourceTreeView(assetInfo); 
        }
        GC.Collect();
        RequestRepaint();
    }

    static void AddAssetToResourceTreeView( U3DAssetInfo assetInfo )
    {
        TreeViewCtrl resTreeList = s_root.FindControl("_ResTreeList") as TreeViewCtrl;

        if( resTreeList == null )
        {
            return;
        }

        bool expandTreeNode = false;
        if ((ResourceManageToolModel.GetInstance().CurrFilter as NullTypeFilter) == null)
        {//非过滤器为全部文件则节点都展开
            expandTreeNode = true;
        }

        string assetPath = assetInfo.path;
        string currPath = assetPath;
        List<TreeViewNode> currLevelNodeList = resTreeList.Roots;
        TreeViewNode parentNode = null;
        int len = 0;
        while( currPath != "")
        {
            int i = currPath.IndexOf('/');
            if (i < 0)
            {
                i = currPath.Length; 
            } 
            len += i + 1; 
            string pathNodeName = currPath.Substring(0,i);
            string currNodeFullPath = assetPath.Substring(0, len-1);
            if (i + 1 < currPath.Length)
            {
                currPath = currPath.Substring(i + 1);
            }
            else
            {
                currPath = "";
            }
              

            bool findNode = false;
            foreach( var treeNode in currLevelNodeList )
            {
                if( treeNode.name == pathNodeName )
                {
                    findNode = true;
                    parentNode = treeNode;
                    currLevelNodeList = treeNode.children;
                    break;
                }
            }

            if ( !findNode )
            {
                U3DAssetInfo info = null;
                TreeViewNode newNode = new TreeViewNode();
                newNode.name = pathNodeName;
                ResourceManageToolModel.GetInstance().Find(currNodeFullPath, out info);
                newNode.image = info.icon; 
                newNode.userObject = info.guid;
                newNode.state.IsExpand = expandTreeNode;

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

    /*
     * 未引用资源列表相关函数 
     */

    static void OnUnUsedResourcesListViewSelectChanged( EditorControl c , int i )
    {
        ListViewCtrl unUsedListView = c as ListViewCtrl;
        if (unUsedListView == null)
            return;
        
        if( unUsedListView.LastSelectItem != -1 )
        {
            Guid selResID = 
            (Guid)unUsedListView.GetItemAt(unUsedListView.LastSelectItem).userObj;
            RebuildResourceRefTreeView(selResID);
            RebuildReferencedTreeView(selResID);
            RequestRepaint();
        }
    }

    static void RebuildUnUsedResourcesListView()
    {
        ListViewCtrl unUsedResListView = s_root.FindControl("_UnUsedResList") as ListViewCtrl;

        if (unUsedResListView == null)
            return;

        unUsedResListView.ClearItems();

        List<U3DAssetInfo> unUsedAssets = ResourceManageToolModel.GetInstance().UnUsedAssetList;
        foreach( var asset in unUsedAssets )
        {
            ListCtrlItem newItem = new ListCtrlItem();
            newItem.name = Path.GetFileName(asset.path);
            newItem.image = asset.icon;
            newItem.userObj = asset.guid;
            newItem.color = Color.white;
            newItem.onSelectColor = Color.red;
            unUsedResListView.AddItem(newItem);
        }

        RequestRepaint();
    }

    /*
     * 资源引用树视图
     */

    static void ClearResourceRefTreeView()
    {
        TreeViewCtrl resRefTreeView = s_root.FindControl("_ResRefTreeView") as TreeViewCtrl;
        if (resRefTreeView == null)
        {
            return;
        }
        resRefTreeView.Clear();
    }

    static void RebuildResourceRefTreeView( Guid resID )
    {
        TreeViewCtrl resRefTreeView = s_root.FindControl("_ResRefTreeView") as TreeViewCtrl; 
        if (resRefTreeView == null)
        {
            return;
        } 
        resRefTreeView.Clear();
         
        List<U3DAssetInfo> depAssets = new List<U3DAssetInfo>(); 
        ResourceManageToolModel.GetInstance().GetAssetDependencies(resID,out depAssets); 
        _BuildResourceRefTreeView(resRefTreeView,null,depAssets); 
    }

    static void _BuildResourceRefTreeView(TreeViewCtrl resRefTreeView , TreeViewNode parent , List<U3DAssetInfo> deps )
    {
        if (deps.Count == 0)
            return;



        foreach( var asset in deps )
        {
            TreeViewNode newNode = resRefTreeView.CreateNode( Path.GetFileName(asset.path) );
            newNode.image = asset.icon;
            newNode.tooptip = asset.path;
            newNode.state.IsExpand = false;
            if (parent == null)
            {
                resRefTreeView.Roots.Add(newNode);
            }
            else
            {
                parent.Add(newNode);
            }

            //只构建第一层直接引用
            //List<U3DAssetInfo> nextLevelDeps = new List<U3DAssetInfo>();
            //ResourceManageToolModel.GetInstance().GetAssetDependencies(asset.guid, out nextLevelDeps);
            //_BuildResourceRefTreeView(resRefTreeView, newNode, nextLevelDeps);
        }
    }

    /*
     * 资源反向引用树视图 
     */

    static void ClearReferencedTreeView()
    {
        TreeViewCtrl referencedTreeView = s_root.FindControl("_ResReverseRefTreeView") as TreeViewCtrl;
        if (referencedTreeView == null)
        {
            return;
        }
        referencedTreeView.Clear();
    }

    static void RebuildReferencedTreeView( Guid resID )
    {
        TreeViewCtrl referencedTreeView = s_root.FindControl("_ResReverseRefTreeView") as TreeViewCtrl;
        if( referencedTreeView == null )
        {
            return;
        }
        referencedTreeView.Clear();

        List<U3DAssetInfo> referencedAssets = new List<U3DAssetInfo>();
        ResourceManageToolModel.GetInstance().GetAssetReferenced(resID, out referencedAssets);
        _BuildReferencedTreeView(referencedTreeView, null, referencedAssets);

    }

    static void _BuildReferencedTreeView( TreeViewCtrl referencedTreeView , TreeViewNode parent , List<U3DAssetInfo> assets )
    {
        if (assets.Count == 0)
            return;

        foreach( var a in assets )
        {
            TreeViewNode newNode = referencedTreeView.CreateNode(Path.GetFileName(a.path));
            newNode.image = a.icon;
            newNode.tooptip = a.path;
            newNode.state.IsExpand = false;
            if (parent == null)
            {
                referencedTreeView.Roots.Add(newNode);
            }
            else
            {
                parent.Add(newNode);
            }

            List<U3DAssetInfo> nextLevelAssets = new List<U3DAssetInfo>();
            ResourceManageToolModel.GetInstance().GetAssetReferenced(a.guid, out nextLevelAssets);
            _BuildReferencedTreeView(referencedTreeView, newNode, nextLevelAssets);
        }
    }

    /*
     * 资源数据库发生变化响应函数
     */

    static void OnAssetDatabaseUpdate( ResourceManageToolModel m )
    {
        RebuildResourcesTreeView();
        RebuildUnUsedResourcesListView();
        ClearResourceRefTreeView();
        ClearReferencedTreeView(); 
    }

    static void OnAssetDatabaseStateChange( ResourceManageToolModel m)
    {
        LabelCtrl stateLabel = s_root.FindControl("_StateLabel") as LabelCtrl;
        stateLabel.Name = "_StateLabel";
        

        switch( m.CurrDBState )
        {
            case ResourceManageToolModel.State.STATE_INIT:
                stateLabel.textColor = Color.red;
                stateLabel.Caption = "数据库没有更新，无法查看无用资源列表与反向引用！";
                break;
            case ResourceManageToolModel.State.STATE_BUILD:
                stateLabel.textColor = Color.green; 
                stateLabel.Caption = "您使用的是\"" + m.GetAssetDatabaseUpdateTime() + "\"更新的数据库";
                break;
            default:
                break;
        }

        RequestRepaint();
    }

    /*
     * 工具函数
     */

    static void RequestRepaint()
    {
        s_root.RequestRepaint();
    }
}
