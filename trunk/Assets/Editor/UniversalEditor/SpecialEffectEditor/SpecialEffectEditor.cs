using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;

public class SpecialEffectEditor
{

    static EditorRoot s_root = null;

    static private string m_helpURL = "http://192.168.2.121:8090/pages/viewpage.action?pageId=6619470";

    [MenuItem("H3D/特效编辑/特效编辑器")]
    static void Init()
    {
        EditorRoot root =
        EditorManager.GetInstance().FindEditor("特效编辑器");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("特效编辑器", false, InitControls);
        }

    }

    //构建控件
    public static void InitControls(EditorRoot editorRoot)
    {

        //注册命令管理器变更通知回调
        EditorCommandManager.GetInstance().callback += OnCmdMgrChange;

        s_root = editorRoot;

        //设置默认大小
        s_root.position = new Rect(200f, 100f, 1024f, 900f);

        {//注册Model变化回调
            SpecialEffectEditorModel.GetInstance().onSetNewEditTarget = OnSetNewEditTarget;
            SpecialEffectEditorModel.GetInstance().onEditTargetDestroy = OnSpeDestroy;
            SpecialEffectEditorModel.GetInstance().onEditTargetValueChange = OnSpeValueChange;
            SpecialEffectEditorModel.GetInstance().onEditTargetSaved = OnSpeSaved;
            SpecialEffectEditorModel.GetInstance().onEditTargetDirty = OnSpeSetDirty;
            SpecialEffectEditorModel.GetInstance().onCurrPlayTimeChange = OnCurrPlayTimeChange;
            SpecialEffectEditorModel.GetInstance().onActionListChange = OnActionListChange;
            SpecialEffectEditorModel.GetInstance().onRefSpeListChange = OnRefSpeListChange;

            SpecialEffectEditorModel.GetInstance().onRefModelOpen = OnRefModelOpen;
            SpecialEffectEditorModel.GetInstance().onRefModelDestroy = OnRefModelDestroy;
            SpecialEffectEditorModel.GetInstance().onItemSelectChange = OnSpeItemSelectChanged;
        }

        {//注册窗体回调
            editorRoot.onEnable = OnEditorEnable;
            editorRoot.onDisable = OnEditorDisable;
            editorRoot.onUpdate = OnEditorUpdate;
            editorRoot.onDestroy = OnEditorDestroy;
            editorRoot.onGUI = OnEditorGUI;
            editorRoot.onMessage = OnEditorMessage;
        }

        editorRoot.RootCtrl = new HSpliterCtrl();

        (editorRoot.RootCtrl as SpliterCtrl).Dragable = true;

        HSpliterCtrl h1 = new HSpliterCtrl();
        HSpliterCtrl h2 = new HSpliterCtrl();
        HSpliterCtrl h3 = new HSpliterCtrl();

        HSpliterCtrl h4 = new HSpliterCtrl();

        VSpliterCtrl v1 = new VSpliterCtrl();
        VSpliterCtrl v2 = new VSpliterCtrl();

        //Inspector右侧垫条
        VSpliterCtrl v3 = new VSpliterCtrl();

        editorRoot.RootCtrl.layoutConstraint = LayoutConstraint.GetSpliterConstraint(700);
        h1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(10);
        h2.layoutConstraint = LayoutConstraint.GetSpliterConstraint(20);
        h3.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30);

        h4.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30, true);

        //分割右侧Inspector的分割器
        v1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(300, true);
        v2.layoutConstraint = LayoutConstraint.GetSpliterConstraint(200);

        //Inspector右侧垫条
        v3.layoutConstraint = LayoutConstraint.GetSpliterConstraint(20f, true);

        editorRoot.RootCtrl.Add(h1);
        editorRoot.RootCtrl.Add(h3);

        //用于上方状态条
        EditorControl vb0 = new VBoxCtrl();
        vb0.Size = new Rect(0, 0, 1000, 20);
        vb0.layoutConstraint = LayoutConstraint.GetToolBarConstraint(30f);
        h1.Add(vb0);
        h1.Add(v1);

        //右侧Inspector
        EditorControl vb1 = new VBoxCtrl();
        vb1.Size = new Rect(0, 0, 200, 500);
        vb1.layoutConstraint = LayoutConstraint.GetInspectorViewConstraint(200, 200);
        v1.Add(h2);
        v1.Add(v3);
        v3.Add(vb1);
        EditorControl inspectorRightVb = new VBoxCtrl();
        v3.Add(inspectorRightVb);
        //v1.Add(vb1);
        v1.Dragable = true;

        //用于菜单
        EditorControl hb0 = new HBoxCtrl();
        hb0.Size = new Rect(0, 0, 1000, 30);
        hb0.layoutConstraint = LayoutConstraint.GetToolBarConstraint(30f);

        //用于主视图
        EditorControl hb1 = new HBoxCtrl();
        hb1.Size = new Rect(0, 0, 600, 600);
        hb1.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();

        h2.Add(hb0);
        h2.Add(hb1);

        //播放控制区域
        EditorControl hb2 = new HBoxCtrl();
        hb2.Size = new Rect(0, 0, 1000, 30);
        hb2.layoutConstraint = LayoutConstraint.GetToolBarConstraint(30f);

        h3.Add(hb2);
        //h3.Add(v2);
        h3.Add(h4);

        //用来放置底部状态条
        HBoxCtrl hb3 = new HBoxCtrl();
        h4.Add(v2);
        h4.Add(hb3);

        //动画元素列表区域
        EditorControl vb2 = new VBoxCtrl();
        vb2.Size = new Rect(0, 0, 200, 300);
        vb2.layoutConstraint = LayoutConstraint.GetListViewConstraint(200, 200);

        //动画时间线
        EditorControl vb3 = new VBoxCtrl();
        vb3.Size = new Rect(0, 0, 800, 300);
        vb3.layoutConstraint = LayoutConstraint.GetListViewConstraint(200, 200);

        v2.Add(vb2);
        v2.Add(vb3);


        _BuildMenuButtons(hb0);

        _BuildFrameControlBar(hb2);

        EditorControl stateLabel = new LabelCtrl();
        stateLabel.Caption = "请打开特效!";
        //vb0.Add(stateLabel); 

        EditorControl mainView = new MainViewCtrl();
        mainView.onAcceptDragObjs = OnMainViweAcceptDragingObjs;
        mainView.onDragingObjs = OnMainViewDragingObjs;
        mainView.onDropObjs = OnMainViewDropObjs;

        hb1.Add(mainView);


        TabViewCtrl tabView = new TabViewCtrl();
        vb1.Add(tabView);



        InspectorViewCtrl inspectorView = new InspectorViewCtrl();
        inspectorView.Name = "_SpeElemInspector";
        inspectorView.Caption = "特效元素";
        inspectorView.editTarget = new SpeElemInspectorTarget();
        inspectorView.onInspector = SpecialEffectEditorInspectorRenderDelegate.OnSpeElemInspector;
        inspectorView.onValueChange = OnSpeElemInspectorValueChange;
        tabView.Add(inspectorView);

        TreeViewCtrl treeTabView = new TreeViewCtrl();
        treeTabView.Name = "_RefModelTreeView";
        treeTabView.Caption = "模型";
        treeTabView.onValueChange = OnRefModelTreeViewValueChanged;
        tabView.Add(treeTabView);


        InspectorViewCtrl speTabView = new InspectorViewCtrl();
        speTabView.Name = "_SpeInspector";
        speTabView.Caption = "特效属性";
        speTabView.onInspector = SpecialEffectEditorInspectorRenderDelegate.OnSpeInspector;
        speTabView.onValueChange = OnSpeInspectorValueChange;
        tabView.Add(speTabView);

        VBoxCtrl actionListVBox = new VBoxCtrl();
        actionListVBox.Caption = "动作列表";
        actionListVBox.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();
        ListViewCtrl actionListListView = new ListViewCtrl();
        actionListListView.Name = "_RefModelActionList";
        actionListListView.Caption = "动作列表";
        actionListListView.onItemSelected = OnActionListSelectionChange;
        actionListVBox.Add(actionListListView);
        tabView.Add(actionListVBox);

        Rect clearActionBtnRect = new Rect(0, 0, 60, 20);
        ButtonCtrl clearActionBtnCtrl = new ButtonCtrl();
        clearActionBtnCtrl.Name = "_ClearActionBtn";
        clearActionBtnCtrl.Caption = "清空动作";
        clearActionBtnCtrl.BtnColor = Color.yellow;
        clearActionBtnCtrl.Size = clearActionBtnRect;
        clearActionBtnCtrl.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();
        actionListVBox.Add(clearActionBtnCtrl);
        clearActionBtnCtrl.onClick = OnClearAction;


        VBoxCtrl refSpeListVBox = new VBoxCtrl();
        refSpeListVBox.Caption = "参考特效";
        refSpeListVBox.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();
        ListViewCtrl refSpeListListView = new ListViewCtrl();
        refSpeListListView.Name = "_RefSpeList";
        refSpeListListView.Caption = "参考特效";
        refSpeListListView.onItemSelected = OnRefSpeListSelectionChange;
        refSpeListVBox.Add(refSpeListListView);
        tabView.Add(refSpeListVBox);

        Rect clearSpeBtnRect = new Rect(0, 0, 60, 20);
        ButtonCtrl clearSpeBtnCtrl = new ButtonCtrl();
        clearSpeBtnCtrl.Name = "_ClearSpeBtn";
        clearSpeBtnCtrl.Caption = "清空参考特效";
        clearSpeBtnCtrl.BtnColor = Color.yellow;
        clearSpeBtnCtrl.Size = clearSpeBtnRect;
        clearSpeBtnCtrl.layoutConstraint = LayoutConstraint.GetExtensibleViewConstraint();
        refSpeListVBox.Add(clearSpeBtnCtrl);
        clearSpeBtnCtrl.onClick = OnClearRefSpe;

        ListViewCtrl listView = new ListViewCtrl();
        listView.Name = "_SpeItemList";
        vb2.Add(listView);
        listView.onItemSelected += OnTimeLineListViewSelectChange;
        listView.onScroll += OnTimeLineListViewScroll;


        TimeLineViewCtrl timelineView = new TimeLineViewCtrl();  
        timelineView.onItemSelected += OnTimeLineSelectChange; 

        vb3.Add(timelineView);
    }


    static void _BuildFrameControlBar(EditorControl parent)
    {

        PlayCtrl playCtrl = new PlayCtrl();
        playCtrl.TotalTime = 10f;
        playCtrl.onValueChange += OnPlayCtrlValueChange;
        parent.Add(playCtrl);

    }

    static void _BuildMenuButtons(EditorControl parent)
    {
        Rect btnRect = new Rect(0, 0, 60, 20);
        Rect undoRect = new Rect(0, 0, 120, 20);
        Rect colorCtrlRect = new Rect(0, 0, 200, 20);
        Rect returnBtnRect = new Rect(0, 0, 120, 20);

        //EditorControl openBtn = new ButtonCtrl();
        //openBtn.Caption = "打开特效";
        //openBtn.Size = btnRect;
        //openBtn.onClick += OnOpenSpe;

        //EditorControl saveBtn = new ButtonCtrl();
        //saveBtn.Caption = "保存特效";
        //saveBtn.Size = btnRect;
        //saveBtn.onClick += OnSaveSpe;

        ButtonCtrl returnBtn = new ButtonCtrl();
        returnBtn.Name = "_FinishEditBtn";
        returnBtn.Caption = "编辑完成";
        returnBtn.Size = returnBtnRect;
        returnBtn.onClick += OnReturnSpe;
        returnBtn.BtnColor = Color.green;
        returnBtn.Enable = false;

        //ButtonCtrl openModelBtn = new ButtonCtrl();
        //openModelBtn.Caption = "打开模型";
        //openModelBtn.Size = btnRect;
        //openModelBtn.onClick += OnOpenModel;

        //ButtonCtrl setActionBtn = new ButtonCtrl();
        //setActionBtn.Caption = "设置动作";
        //setActionBtn.Size = btnRect;
        //setActionBtn.onClick += OnSetAction;

        ButtonCtrl resetCameraBtn = new ButtonCtrl();
        resetCameraBtn.Caption = "相机复位";
        resetCameraBtn.Size = btnRect;
        resetCameraBtn.onClick += OnResetCamera;

        ColorCtrl setBkColorBtn = new ColorCtrl();
        setBkColorBtn.Caption = "设背景色";
        setBkColorBtn.Size = colorCtrlRect;
        setBkColorBtn.onValueChange += OnSetBkColorValueChanged;

        ButtonCtrl helpBtn = new ButtonCtrl();
        helpBtn.Caption = "帮助";
        helpBtn.Size = btnRect;
        helpBtn.onClick += OnHelp;

        //EditorControl undoBtn = new ButtonCtrl();
        //undoBtn.Name = "UndoBtn";
        //undoBtn.Caption = "撤消/"+EditorCommandManager.GetInstance().GetNextUndoCmdName();
        //undoBtn.Size = undoRect;
        //undoBtn.onClick += OnUndoBtnClick;

        //EditorControl redoBtn = new ButtonCtrl();
        //redoBtn.Name = "RedoBtn";
        //redoBtn.Caption = "重做/"+EditorCommandManager.GetInstance().GetNextRedoCmdName();
        //redoBtn.Size = undoRect;
        //redoBtn.onClick += OnRedoBtnClick;

        //parent.Add(openBtn);
        //parent.Add(saveBtn);
        parent.Add(returnBtn);
        //parent.Add(openModelBtn);
        //parent.Add(setActionBtn);
        parent.Add(resetCameraBtn);
        parent.Add(setBkColorBtn);
        parent.Add(helpBtn);
        //parent.Add(undoBtn);
        //parent.Add(redoBtn);
    }


    static void OnEditorMessage( ControlMessage msg )
    {
        
        switch( msg.msg )
        {
            case ControlMessage.Message.TIMELINECTRL_BEGIN_DRAG_TAG: 
            case ControlMessage.Message.TIMELINECTRL_DRAG_TAG: 
            case ControlMessage.Message.TIMELINECTRL_END_DRAG_TAG:
                if ((int)msg.param0 != 0)
                    break;
 
                TimeLineViewCtrl timelineView = msg.sender as TimeLineViewCtrl;
                if (timelineView != null)
                { 
                    SpecialEffectEditorModel.GetInstance().SetEditTargetStartTime(timelineView.Tags[0].time);
                }

                break;
            case ControlMessage.Message.TIMELINECTRL_BEGIN_DRAG_ITEMS:
                OnTimeLineDragItemsBegin(msg.sender, msg.param0 as List<int>); 
                break;
            case ControlMessage.Message.TIMELINECTRL_DRAG_ITEMS:
                OnTimeLineDragItems(msg.sender, msg.param0 as List<int>); 
                break;
            case ControlMessage.Message.TIMELINECTRL_END_DRAG_ITEMS:
                OnTimeLineDragItemsEnd(msg.sender,msg.param0 as List<int>); 
                break;
            default:
                break;
        }
    }


    static void OnOpenSpe(EditorControl c)
    { 
    }

    static void OnSaveSpe(EditorControl c)
    { 
    }

    static void OnReturnSpe(EditorControl c)
    {
        SpecialEffectEditorModel.GetInstance().RetargetSpeToOldTarget();
    }

    static void OnOpenModel(EditorControl c)
    {
        SpecialEffectEditorModel.GetInstance().OpenRefModelFromFile();
    }

    static void OnSetAction(EditorControl c)
    {
        SpecialEffectEditorModel.GetInstance().LoadActionFromFile();
    }

    static void OnClearAction(EditorControl c )
    {
        SpecialEffectEditorModel.GetInstance().ClearActionList();
    }

    static void OnClearRefSpe(EditorControl c )
    {
        SpecialEffectEditorModel.GetInstance().ClearRefSpeList();
    }

    static void OnResetCamera(EditorControl c)
    {
        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            {//初始化相机位置

                mainView.center = Vector3.zero;
                mainView.radius = 10.0f;

                Transform camTrans = mainView.camObj.transform;
                camTrans.localPosition = new Vector3(0f, 0f, -5f);
                camTrans.localRotation = Quaternion.identity;


                float angleRotateAroundUp = 135.0f;
                float angleRotateAroundRight = 45.0f;

                Vector3 localPos = (camTrans.localPosition - mainView.center).normalized * mainView.radius;

                Quaternion q0 = Quaternion.AngleAxis(angleRotateAroundUp, camTrans.up);
                camTrans.localPosition = q0 * localPos;
                camTrans.Rotate(Vector3.up, angleRotateAroundUp, Space.Self);

                Quaternion q1 = Quaternion.AngleAxis(angleRotateAroundRight, camTrans.right);
                camTrans.Rotate(Vector3.right, angleRotateAroundRight, Space.Self);
                camTrans.localPosition = q1 * camTrans.localPosition;
                camTrans.localPosition += mainView.center;
            }

            mainView.RequestRepaint();
        }
    }

    static void OnSetBkColor(EditorControl c)
    {

    }

    static void OnSetBkColorValueChanged(EditorControl c, float v)
    {
        ColorCtrl colorCtrl = c as ColorCtrl;
        if (colorCtrl == null)
            return;

        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            mainView.mainCam.backgroundColor = colorCtrl.currColor;
        }
    }

    static void OnHelp(EditorControl c)
    {
        System.Diagnostics.Process.Start(m_helpURL);
    }

    static void OnUndoBtnClick(EditorControl c)
    {
        EditorCommandManager.GetInstance().PerformUndo();
    }

    static void OnRedoBtnClick(EditorControl c)
    {
        EditorCommandManager.GetInstance().PerformRedo();
    }

    static void OnCmdMgrChange()
    {
        //if (s_root == null)
        //    return;

        //EditorControl undoBtn = s_root.FindControl("UndoBtn");
        //EditorControl redoBtn = s_root.FindControl("RedoBtn");

        //if( null != undoBtn )
        //{
        //    undoBtn.Caption = "撤消/" + EditorCommandManager.GetInstance().GetNextUndoCmdName();
        //}
        //if( null != redoBtn )
        //{
        //    redoBtn.Caption = "重做/" + EditorCommandManager.GetInstance().GetNextRedoCmdName();
        //}

        //UpdateSpeInspector();
        //undoBtn.RequestRepaint();
    }


    /*
     * 参照模型树状控件响应函数
     */


    //当参照模型树的绑定选项有变化时
    static void OnRefModelTreeViewValueChanged(EditorControl c, float v)
    {
        TreeViewCtrl refModelTreeView = c as TreeViewCtrl;

        if (refModelTreeView == null)
            return;

        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();

        if (spe == null)
            return;

        string newPath = refModelTreeView.lastValueChangeNodePath;

        SpeBindingTargetChangeCmd cmd = new SpeBindingTargetChangeCmd();
        cmd.oldPath = spe.BindingTargetPath;
        cmd.newPath = newPath;
        EditorCommandManager.GetInstance().Add(cmd);
    }

    static void ForceBindSpeToDefaultTarget()
    {
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();
         
        GameObject defaultTarget = null;

        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            defaultTarget = mainView.GetBindingTarget();
            mainView.RequestRepaint();
        }

        if (defaultTarget == null)
            return;

        if (spe != null)
        {
            spe.ForceBindTarget(defaultTarget);
        } 
    }

     

    static void TryBindSpeToRefModel()
    {
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();

        if (spe == null)
            return;

        GameObject refModel = SpecialEffectEditorModel.GetInstance().GetRefModel();

        if (spe.BindingTargetPath.Equals("") || refModel == null)
        {
            ForceBindSpeToDefaultTarget(); 
            return;
        }

        if (!spe.BindTarget(refModel))
        {
            ForceBindSpeToDefaultTarget();
        }
         
    }

    static void TryBindRefSpeToRefModel()
    { 
        GameObject defaultTarget = null;

        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            defaultTarget = mainView.GetBindingTarget();
            mainView.RequestRepaint();
        }

        if (defaultTarget == null)
            return;

        GameObject refModel = SpecialEffectEditorModel.GetInstance().GetRefModel(); 
        SpecialEffectEditorModel.GetInstance().TryBindToTarget(refModel, defaultTarget);
    }

    static void ForceBindRefSpeToDefaultTarget()
    { 
        GameObject defaultTarget = null;

        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            defaultTarget = mainView.GetBindingTarget();
            mainView.RequestRepaint();
        }

        if (defaultTarget == null)
            return;

        SpecialEffectEditorModel.GetInstance().ForceBindRefSpeToTarget(defaultTarget);
    }

    //参照特效绑定物体，同步TreeView的绑定选项
    static void SyncTreeViewBindingSelection()
    {
        TreeViewCtrl refModelTreeView = s_root.FindControl<TreeViewCtrl>();

        if (refModelTreeView == null)
        {
            return;
        }

        if (refModelTreeView.Roots.Count == 0)
        {
            return;
        }

        TreeViewCtrl.PreorderTraverse(refModelTreeView.Roots[0], SyncTreeViewBindingSelectionVisitCallBack);
    }


    //参照特效绑定物体，同步TreeView的绑定选项
    static bool SyncTreeViewBindingSelectionVisitCallBack(TreeViewNode n)
    {
        string bindTargetPath = "";
        if (!SpecialEffectEditorModel.GetInstance().HasRefModel())
        {//此时说明TreeView没有创建
            return false;
        }

        if (SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            bindTargetPath = SpecialEffectEditorModel.GetInstance().GetEditTarget().BindingTargetPath;
        }

        if (n.GetPathString().Equals(bindTargetPath))
        {
            n.state.userParams[0].param = true;
        }
        else
        {
            n.state.userParams[0].param = false;
        }
        return true;
    }

    //根据Model中的参考模型构建模型树视图
    //此函数会删除当前树，重新构建
    static void ReBuildTreeView()
    {
        GameObject refModel =
        SpecialEffectEditorModel.GetInstance().GetRefModel();

        TreeViewCtrl refModelTreeView = s_root.FindControl<TreeViewCtrl>();

        if (refModelTreeView == null)
            return;

        //若没有载入参考模型
        if (refModel == null)
        {
            refModelTreeView.Clear();
            return;
        }

        Transform rootTrans = refModel.transform;
        TreeViewNode rootNode = refModelTreeView.CreateNode(rootTrans.gameObject.name);

        TreeViewNodeUserParam rootParam = new TreeViewNodeUserParam();
        rootParam.name = "bind";
        rootParam.desc = "绑定";
        rootParam.param = false;

        rootNode.state.userParams.Add(rootParam);

        Queue<Transform> q = new Queue<Transform>();
        Queue<TreeViewNode> q2 = new Queue<TreeViewNode>();


        q.Enqueue(rootTrans);
        q2.Enqueue(rootNode);
        while (q.Count > 0)
        {
            Transform t = q.Dequeue();
            TreeViewNode n = q2.Dequeue();
            for (int i = 0; i < t.childCount; i++)
            {
                TreeViewNode nn = refModelTreeView.CreateNode(t.GetChild(i).gameObject.name);

                TreeViewNodeUserParam newParam = new TreeViewNodeUserParam();
                newParam.name = "bind";
                newParam.desc = "绑定";
                newParam.param = false;
                nn.state.userParams.Add(newParam);

                n.Add(nn);

                q.Enqueue(t.GetChild(i));
                q2.Enqueue(nn);
            }
        }

        refModelTreeView.Clear(); 
        refModelTreeView.Roots.Add(rootNode); 

        SyncTreeViewBindingSelection();

        refModelTreeView.RequestRepaint();
    }


    /*
     * 动作列表视图函数
     */
    
   static void UpdateActionListView()
   {
       ListViewCtrl actionListView = s_root.FindControl("_RefModelActionList") as ListViewCtrl;

       if (actionListView == null)
           return;

       actionListView.ClearItems();

       List<SpeEditorAction> actionList = SpecialEffectEditorModel.GetInstance().RefModelActionList();

       foreach( var action in actionList )
       {
           ListCtrlItem item = new ListCtrlItem();
           item.name = action.AnimClip.name;
           actionListView.AddItem(item);
       }

       RequestRepaint();
   } 

   static void OnActionListSelectionChange(EditorControl c, int i)
   {
       RequestRepaint();
   }
     

    /*
     * 参照特效列表视图响应函数
     */

    static void UpdateRefSpeListView()
    {
        ListViewCtrl refSpeListView = s_root.FindControl("_RefSpeList") as ListViewCtrl;

        if (refSpeListView == null)
            return;

        refSpeListView.ClearItems();

        int refSpeCount = SpecialEffectEditorModel.GetInstance().GetRefSpeCount();
        for (int i = 0; i < refSpeCount; i++)
        {
            ListCtrlItem item = new ListCtrlItem();
            string speName = "";
            SpecialEffectEditorModel.GetInstance().GetRefSpeName(i, ref speName);
            item.name = speName;
            refSpeListView.AddItem(item);
        }

        RequestRepaint();
    }

    static void OnRefSpeListSelectionChange(EditorControl c , int i)
    {
        RequestRepaint();
    }

    /*
    * 播放器控件响应函数
    */
    //当播放控制器值发生变化
    static void OnPlayCtrlValueChange(EditorControl c, float v)
    {
        PlayCtrl playCtrl = c as PlayCtrl;

        if (playCtrl == null)
            return;

        SpecialEffectEditorModel.GetInstance().SetCurrPlayTime(playCtrl.PlayTime);
    }


    /*
    * Inspector控件响应函数
    */
    static void OnSpeElemInspectorValueChange(EditorControl c, float v)
    {
        InspectorViewCtrl inspectorCtrl = c as InspectorViewCtrl;

        if (inspectorCtrl == null)
            return;

        SpeElemInspectorTarget target =
        inspectorCtrl.editTarget as SpeElemInspectorTarget;

        if (target == null)
            return;

        SpeElemInspectorTarget oldValue = new SpeElemInspectorTarget();
        oldValue.Set(SpecialEffectEditorModel.GetInstance().GetEditTarget() , target.selectItem);

        SpeElemInspectorValueChangeCmd cmd = new SpeElemInspectorValueChangeCmd();
        cmd.oldValue = oldValue;
        cmd.newValue = target.Copy();
        EditorCommandManager.GetInstance().Add(cmd);
    }

    static void OnSpeInspectorValueChange(EditorControl c, float v)
    {
        InspectorViewCtrl inspectorCtrl = c as InspectorViewCtrl;

        if (inspectorCtrl == null)
            return;

        if (inspectorCtrl.editTarget == null)
            return;

        SpeInspectorTarget target = inspectorCtrl.editTarget as SpeInspectorTarget;

        if (target == null)
            return;

        SpeInspectorTarget oldValue = new SpeInspectorTarget();
        oldValue.Set(SpecialEffectEditorModel.GetInstance().GetEditTarget());

        SpeInspectorValueChangeCmd cmd = new SpeInspectorValueChangeCmd();
        cmd.oldValue = oldValue;
        cmd.newValue = target.Copy();
        EditorCommandManager.GetInstance().Add(cmd);

    }

    //指定索引项更新Inspector
    static void UpdateSpeElemInspector(int sel)
    {
        InspectorViewCtrl inspectorCtrl = s_root.FindControl("_SpeElemInspector") as InspectorViewCtrl;

        if (inspectorCtrl == null)
            return;
        
        SpeElemInspectorTarget inspectorTarget = inspectorCtrl.editTarget as SpeElemInspectorTarget;
        if (sel != -1)
        {
            int speElemsNum = 0;
            if (SpecialEffectEditorModel.GetInstance().HasEditTarget())
            {
                speElemsNum = SpecialEffectEditorModel.GetInstance().GetEditTarget().GetItemCount();
                if (sel < speElemsNum && sel >= 0)
                {
                    inspectorTarget.Set(SpecialEffectEditorModel.GetInstance().GetEditTarget(), sel);
                    SpecialEffectEditorModel.GetInstance().GetEditTarget().ShowSelectElemInspector(sel);
                    RequestRepaint();
                    return;
                }
            }
            SpecialEffectEditorModel.GetInstance().ShowSelectRefSpeInspector(sel - speElemsNum);
        }
        else
        {
            inspectorTarget.Set(null, -1);
        }
        RequestRepaint();
    }

    static void UpdateSpeInspector()
    {
        InspectorViewCtrl inspectorCtrl = s_root.FindControl("_SpeInspector") as InspectorViewCtrl;
        if (inspectorCtrl != null)
        {
            if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
            {
                inspectorCtrl.editTarget = null;
            }
            else
            {
                inspectorCtrl.editTarget = new SpeInspectorTarget();
                SpeInspectorTarget inspectorTarget = inspectorCtrl.editTarget as SpeInspectorTarget;
                inspectorTarget.Set(SpecialEffectEditorModel.GetInstance().GetEditTarget());
            }
        }
    }

    /*
     * 时间线视图控件响应函数
     */

    static void OnTimeLineDragItemsBegin(EditorControl c, List<int> indxList )
    {
        TimeLineViewCtrl timeLineCtrl = c.Root.FindControl<TimeLineViewCtrl>(); 
        if (timeLineCtrl == null)
            return;

        SpeItemChangeCmd.ClearTmpItems();
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();
        int speElems = 0;

        if (spe != null)
        {
            speElems = spe.GetItemCount();
            foreach (var i in indxList)
            {
                if (i >= 0 && i < speElems)
                {
                    SpeItemChangeCmd.tmpItems.Add(timeLineCtrl.Items[i].Copy());
                }
            }
        }

        RequestRepaint();
    }

    //时间线在拖拽中
    static void OnTimeLineDragItems(EditorControl c, List<int> indxList)
    {
        TimeLineViewCtrl timeLineCtrl = c.Root.FindControl<TimeLineViewCtrl>();

        if (timeLineCtrl == null)
            return;

        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();
        int speElems = 0;
        if (spe != null)
        {
            speElems = spe.GetItemCount(); 
            foreach( var i in indxList )
            {
                if (i >= 0 && i < speElems)
                {
                    spe.SetItemTimeInfo(i, timeLineCtrl.Items[i].startTime, timeLineCtrl.Items[i].length);
                    spe.SetItemStateInfo(i, timeLineCtrl.Items[i].loop);
                }
            }
            
        }

        foreach( var i in indxList )
        {
            if( i >= speElems )
            { 
                //选中时间线为参考特效
                SpecialEffectEditorModel.GetInstance().SetRefSpeStartTime(i - speElems, timeLineCtrl.Items[i].startTime);
            }
        }

       SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();

       RequestRepaint();
    }

    static void OnTimeLineDragItemsEnd(EditorControl c, List<int> indxList)
    {
        TimeLineViewCtrl timeLineCtrl = c.Root.FindControl<TimeLineViewCtrl>();

        if (timeLineCtrl == null)
            return;

        int speElems = 0;
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();
        if( spe != null )
        {
            speElems = spe.GetItemCount();
        }

        //更新参考特效
        foreach (var i in indxList)
        {
            if (i >= speElems)
            {
                //选中时间线为参考特效
                SpecialEffectEditorModel.GetInstance().SetRefSpeStartTime(i - speElems, timeLineCtrl.Items[i].startTime);
            }
        }
        
        if (spe != null)
        { 

            SpeItemChangeCmd cmd = new SpeItemChangeCmd();
            cmd.indices = indxList; 
            cmd.oldTimeLineItems = SpeItemChangeCmd.tmpItems; 
            foreach (var i in indxList)
            {
                if (i >= 0 && i < speElems)
                {
                    cmd.newTimeLineItems.Add(timeLineCtrl.Items[i].Copy());  
                }
            }


            int indx = 0;
            foreach( var old in cmd.oldTimeLineItems )
            {
                int i = indxList[indx];
                if (i >= 0 && i < speElems)
                {
                    spe.SetItemTimeInfo(i, old.startTime, old.length);
                    spe.SetItemStateInfo(i, old.loop);
                }
                indx++;
            }


            EditorCommandManager.GetInstance().Add(cmd);
        }

        RequestRepaint();
    }


    static void OnTimeLineSelectChange(EditorControl c, int i)
    {
        SpeItemSelectChangeCmd cmd = new SpeItemSelectChangeCmd();
        cmd.oldSelection = SpecialEffectEditorModel.GetInstance().selectItem;
        cmd.newSelection = i;
        EditorCommandManager.GetInstance().Add(cmd);
    }



    /*
    * 时间线列表控件响应函数
    */
    static void OnTimeLineListViewScroll(EditorControl c, Vector2 pos)
    {
        List<EditorControl> list = c.Root.FindControls<TimeLineViewCtrl>();

        if (list.Count > 0)
        {
            TimeLineViewCtrl timeLineCtrl = (list[0] as TimeLineViewCtrl);
            if (timeLineCtrl != null)
            {
                Vector2 oldPos = timeLineCtrl.ScrollPos;
                timeLineCtrl.ScrollPos = new Vector2(oldPos.x, pos.y);
                timeLineCtrl.RequestRepaint();
            }
        }
    }
    static void OnTimeLineListViewSelectChange(EditorControl c, int i)
    {
        SpeItemSelectChangeCmd cmd = new SpeItemSelectChangeCmd();
        cmd.oldSelection = SpecialEffectEditorModel.GetInstance().selectItem;
        cmd.newSelection = i;
        EditorCommandManager.GetInstance().Add(cmd);
    }



    /*
     * Model变化回调
     */

    static void OnRefModelOpen(SpecialEffectEditorModel m)
    {
        if (!m.HasRefModel())
            return;

        MainViewCtrl mainView = s_root.FindControl<MainViewCtrl>();
        if (mainView != null)
        {
            m.GetRefModel().transform.parent = mainView.GetRefModelBindingTarget().transform;
            m.GetRefModel().transform.localPosition = Vector3.zero;
            mainView.RequestRepaint();
        }

        ReBuildTreeView();

        //同步树视图绑定选项
        SyncTreeViewBindingSelection();

        //重新绑定模型
        TryBindSpeToRefModel();

        //重新绑定参照特效
        TryBindRefSpeToRefModel();
    }

    static void OnRefModelDestroy(SpecialEffectEditorModel m)
    {
        //当引用模型销毁之前，将所有特效绑定到
        //默认绑定点，防止被模型销毁。
        ForceBindSpeToDefaultTarget();
        ForceBindRefSpeToDefaultTarget();
    }

    static void OnSpeSetDirty(SpecialEffectEditorModel m)
    {
        //string title = m.GetEditTargetFilePath();
        //if (m.IsDirty)
        //    title += "*";

        //s_root.title = title;
    }


    static void OnSetNewEditTarget(SpecialEffectEditorModel m)
    {
        EditorCommandManager.GetInstance().Clear();

        PlayCtrl playCtrl = s_root.FindControl<PlayCtrl>();
        TimeLineViewCtrl timeLineViewCtrl = s_root.FindControl<TimeLineViewCtrl>();
        ListViewCtrl listViewCtrl = s_root.FindControl("_SpeItemList") as ListViewCtrl; 
        
        playCtrl.PlayTime = 0.0f;
        playCtrl.TotalTime = m.GetPreviewTotalTime(); 
        
        listViewCtrl.Items.Clear();
        timeLineViewCtrl.Items.Clear();
        timeLineViewCtrl.Tags.Clear();
        timeLineViewCtrl.TotalTime = m.GetPreviewTotalTime();

        SpecialEffectEditProxy spe = m.GetEditTarget();

        if (spe != null)
        {
            //待编辑特效
            for( int i = 0 ; i < spe.GetItemCount() ; i++)
            {
                string name = "";
                float startTime = 0f;
                float length = 0f;
                bool isLoop = false;

                spe.GetItemName(i,ref name);
                spe.GetItemTimeInfo(i, ref startTime, ref length);
                spe.GetItemStateInfo(i, ref isLoop);

                ListCtrlItem newItem = new ListCtrlItem();
                TimeLineItem newTimeLineItem = new TimeLineItem();

                newItem.name = name;
                newTimeLineItem.startTime = startTime;
                newTimeLineItem.length = length;
                newTimeLineItem.loop = isLoop;

                listViewCtrl.Items.Add(newItem);
                timeLineViewCtrl.Items.Add(newTimeLineItem);
            }

            //为编辑特效加入起始时间标记
            TimeTag newTag = new TimeTag();
            newTag.time = spe.StartTime;
            timeLineViewCtrl.Tags.Add(newTag);


            SetFinishEditButtonEnable(true);
        }
        else
        {
            SetFinishEditButtonEnable(false);
        }

        //处理参考特效
        for (int i = 0; i < SpecialEffectEditorModel.GetInstance().GetRefSpeCount(); i++)
        {
            string name = "";
            float startTime = 0f;
            float length = 0f;

            SpecialEffectEditorModel.GetInstance().GetRefSpeName(i, ref name);
            SpecialEffectEditorModel.GetInstance().GetRefSpeStartTime(i, ref startTime);
            SpecialEffectEditorModel.GetInstance().GetRefSpeLength(i, ref length);

            ListCtrlItem newItem = new ListCtrlItem();
            TimeLineItem newTimeLineItem = new TimeLineItem();

            newItem.name = name + "(参考)";
            newItem.color = Color.gray; 

            newTimeLineItem.startTime = startTime;
            newTimeLineItem.length = length;
            newTimeLineItem.color = Color.blue;
            newTimeLineItem.onSelectedColor = Color.cyan;
            newTimeLineItem.dragBoxColor = Color.blue;
            newTimeLineItem.dragBoxSelectedColor = Color.cyan;

            listViewCtrl.Items.Add(newItem);
            timeLineViewCtrl.Items.Add(newTimeLineItem);
        }
         

        ForceBindSpeToDefaultTarget();

        ForceBindRefSpeToDefaultTarget();

        UpdateSpeInspector();

        //同步树视图绑定选项
        SyncTreeViewBindingSelection();

        //尝试绑定模型
        TryBindSpeToRefModel();

        //尝试绑定参照特效到模型
        TryBindRefSpeToRefModel();

        RequestRepaint();
    }

    static void SetFinishEditButtonEnable(bool enable)
    {
        ButtonCtrl finishEditBtn = s_root.FindControl("_FinishEditBtn") as ButtonCtrl;
        if (finishEditBtn != null)
        {
            finishEditBtn.Enable = enable;
        }
    }

    static void OnSpeValueChange(SpecialEffectEditorModel m)
    {
        PlayCtrl playCtrl = s_root.FindControl<PlayCtrl>();
        TimeLineViewCtrl timeLineViewCtrl = s_root.FindControl<TimeLineViewCtrl>();
        ListViewCtrl listViewCtrl = s_root.FindControl("_SpeItemList") as ListViewCtrl;

        SpecialEffectEditProxy spe = m.GetEditTarget();

        playCtrl.TotalTime = m.GetPreviewTotalTime();
        timeLineViewCtrl.TotalTime = m.GetPreviewTotalTime();

        int itemIndx = 0;

        if (spe != null)
        {//更新待编辑特效值
            for (int i = 0; i < spe.GetItemCount(); i++)
            {
                spe.GetItemTimeInfo(i, ref timeLineViewCtrl.Items[i].startTime, ref timeLineViewCtrl.Items[i].length);
                spe.GetItemStateInfo(i, ref timeLineViewCtrl.Items[i].loop);
                itemIndx++;
            }

            if (timeLineViewCtrl.Tags.Count > 0)
            {
                timeLineViewCtrl.Tags[0].time = spe.StartTime;
            }
        }

        {//处理参考特效

            for( int i = 0 ; i < m.GetRefSpeCount() ; i++ )
            {
                m.GetRefSpeStartTime(i, ref timeLineViewCtrl.Items[itemIndx].startTime);
                m.GetRefSpeLength(i, ref timeLineViewCtrl.Items[itemIndx].length);
                itemIndx++;
            }

        }


        OnPlayCtrlValueChange(playCtrl, 0.0f);

        UpdateSpeInspector();

        //同步树视图绑定选项
        SyncTreeViewBindingSelection();

        //重新绑定模型
        TryBindSpeToRefModel();

        //重新绑定参照特效到参照模型
        TryBindRefSpeToRefModel();

        RequestRepaint();
    }

    static void OnActionListChange(SpecialEffectEditorModel m)
    {
        UpdateActionListView();
    }

    static void OnRefSpeListChange(SpecialEffectEditorModel m )
    {
        UpdateRefSpeListView();
    }

    static void OnSpeDestroy(SpecialEffectEditorModel m)
    {
        EditorCommandManager.GetInstance().Clear();

        PlayCtrl playCtrl = s_root.FindControl<PlayCtrl>();
        TimeLineViewCtrl timeLineViewCtrl = s_root.FindControl<TimeLineViewCtrl>();
        ListViewCtrl listViewCtrl = s_root.FindControl("_SpeItemList") as ListViewCtrl;

        playCtrl.PlayTime = 0.0f;
        playCtrl.TotalTime = 5.0f;

        listViewCtrl.Items.Clear();
        timeLineViewCtrl.Items.Clear();
        timeLineViewCtrl.TotalTime = 5.0f;
        UpdateSpeInspector();
        RequestRepaint();
    }

    static void RequestRepaint()
    {
        s_root.RequestRepaint();
    }

    static void OnSpeSaved(SpecialEffectEditorModel m)
    {
        EditorCommandManager.GetInstance().Clear();
    }

    static void OnSpeItemSelectChanged(SpecialEffectEditorModel m)
    {
        ListViewCtrl listCtrl = s_root.FindControl("_SpeItemList") as ListViewCtrl;
        if (listCtrl != null)
        {
            listCtrl.LastSelectItem = m.selectItem;
            listCtrl.RequestRepaint();
        }

        TimeLineViewCtrl timeLineCtrl = s_root.FindControl<TimeLineViewCtrl>();
        if (timeLineCtrl != null)
        {
            timeLineCtrl.LastSelectedItem = m.selectItem;
            timeLineCtrl.RequestRepaint();
        }

        UpdateSpeElemInspector(m.selectItem);
    }


    static void OnCurrPlayTimeChange(SpecialEffectEditorModel m)
    {

        PlayCtrl playCtrl = s_root.FindControl<PlayCtrl>();
        if (playCtrl != null)
        {
            playCtrl.PlayTime = m.GetCurrPlayTime();
        }

        //在播放控制器值发生变化时需要通知时间线视口
        TimeLineViewCtrl timeLineCtrl = s_root.FindControl<TimeLineViewCtrl>();
        if (timeLineCtrl != null)
        {
            timeLineCtrl.CurrPlayTime = m.GetCurrPlayTime();
        }

        RequestRepaint();
    }

    static void OnEditorEnable(EditorRoot root)
    {
        {//对来自编辑器的事件进行监听
            Undo.postprocessModifications += _OnUndoPostProcessModification;
            Undo.undoRedoPerformed += _OnUndoRedo;
            Undo.undoRedoPerformed += SpecialEffectEditorModel.GetInstance().OnUndoRedo;

            EditorCommandManager.GetInstance().onBeforeCmdExecute += SpecialEffectEditorModel.GetInstance().OnBeforeCmdExecute;
            EditorCommandManager.GetInstance().onAfterCmdExecute += SpecialEffectEditorModel.GetInstance().OnAfterCmdExecute;
        }
    }

    static void OnEditorDisable(EditorRoot root)
    {
        {//对来自编辑器的事件进行监听
            Undo.postprocessModifications -= _OnUndoPostProcessModification;
            Undo.undoRedoPerformed -= _OnUndoRedo;
            Undo.undoRedoPerformed -= SpecialEffectEditorModel.GetInstance().OnUndoRedo;

            EditorCommandManager.GetInstance().onBeforeCmdExecute -= SpecialEffectEditorModel.GetInstance().OnBeforeCmdExecute;
            EditorCommandManager.GetInstance().onAfterCmdExecute -= SpecialEffectEditorModel.GetInstance().OnAfterCmdExecute;
        }

        //if (SpecialEffectEditorModel.GetInstance().IsDirty)
        //{
        //    if (EditorUtility.DisplayDialog("警告!", "当前编辑的特效已被修改，是否保存?", "是", "否"))
        //    {
        //        SpecialEffectEditorModel.GetInstance().SaveSpeChange();
        //    }
        //}
        SpecialEffectEditorModel.GetInstance().Destroy();
        EditorCommandManager.GetInstance().Destroy();
    }

    static void OnEditorGUI(EditorRoot root)
    {
    }

    static void OnEditorUpdate(EditorRoot root)
    {

    }

    static void OnEditorDestroy(EditorRoot root)
    {

    }



    static void OnMainViewDragingObjs(EditorControl c, Object[] objs, string[] paths)
    {
    }

    static void OnMainViewDropObjs(EditorControl c, Object[] objs, string[] paths)
    {//目前只支持单一物体拖放
        if (c == null)
            return;

        if (objs == null)
            return;

        if (objs.Length == 0)
            return;

        GameObject go = objs[0] as GameObject;
        if (go == null)
            return;

        SpecialEffect spe = go.GetComponent<SpecialEffect>();
        if (spe != null)
        {
            if (SpecialEffectEditorModel.GetInstance().HasEditTarget())
            {//如果已经有编辑目标则作为参考特效加载
                SpecialEffectEditorModel.GetInstance().AddRefSpe(go);
            }
            else
            {
                SpecialEffectEditorModel.GetInstance().SetEditTarget(go);
            }
        }
        else
        {
            if (paths.Length == 0)
                return;

            string resPath = paths[0].Substring(paths[0].IndexOf("Assets"));
            AnimationClip animClip = Resources.LoadAssetAtPath<AnimationClip>(resPath);
            if (animClip == null)
            {//为模型 
                SpecialEffectEditorModel.GetInstance().OpenRefModelFromFile(resPath);
            }
            else
            {//为动作  
                SpecialEffectEditorModel.GetInstance().AddRefModelAction(animClip);
            }
        }

    }

    //测试拖拽进编辑器的物体编辑器是否接受
    static bool OnMainViweAcceptDragingObjs(EditorControl c, Object[] objs, string[] paths)
    {
        if (c == null)
            return false;

        if (objs == null)
            return false;

        if (objs.Length == 0)
            return false;

        GameObject spe = objs[0] as GameObject;
        if (spe == null)
            return false;

        //查看是否为特效
        if (null != spe.GetComponent<SpecialEffect>())
        {
            return true;
        }
        else
        {//查看是否为动作或模型
            if (paths.Length == 0)
                return false;
            return true;          
        }

        return false;
    }

    static void _OnHierarchyWndItemOnGUI(int instanceID, Rect selectionRect)
    {
    }

    static void _OnUndoRedo()
    {
        RequestRepaint();
    }

    static UndoPropertyModification[] _OnUndoPostProcessModification(UndoPropertyModification[] modifications)
    {
        //当Inspector值发生变化时重绘
        RequestRepaint();

        //Debug.Log("ModProcess");
        //foreach( var m in modifications )
        //{
        //    PropertyModification propMod = m.propertyModification;

        //    string objRefName = propMod.objectReference == null ? "null" : propMod.objectReference.name;
        //    string targetName = propMod.target == null ? "null" : propMod.target.name; 

        //    Debug.Log(
        //        "objRefName=" + objRefName +
        //        " target=" + targetName +
        //        " value="+propMod.value+
        //        " propPath="+propMod.propertyPath
        //        );
        //}
        return modifications;
    }
}
