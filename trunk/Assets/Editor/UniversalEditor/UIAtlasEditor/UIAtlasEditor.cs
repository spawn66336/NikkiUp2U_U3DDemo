using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEditor;
using System.IO;
using System;
using System.Windows.Forms;

public class UIAtlasEditor 
{//Atlas编辑器

    static EditorRoot s_root = null;                        //根控件

    static public ListViewCtrl m_projTreeView = null;       //工程List窗口
    static public ListViewCtrl m_spriteListView = null;     //小图List窗口
    static public InspectorViewCtrl m_inspector = null;     //Inspector视图
    static public MainViewCtrl m_preview = null;            //预览区域

    static private string m_projOutputPath = null;          //Atlas输出路径
    static private string m_spriteWidthStr = null;          //当前选择小图宽度(字符串)
    static private string m_spriteHeightStr = null;         //当前选择小图高度(字符串)
    static private int m_spriteWidth = 0;                   //当前选择小图宽度(pix)
    static private int m_spriteHeight = 0;                  //当前选择小图高度(pix)
    static private float m_aspecet = 0f;                    //当前选择小图宽高比

    static private GameObject m_previewObj = null;          //预览区域GameObject
    static private GameObject m_Counter = null;             //命令计数器
    static private UIAtlasCommandCounter m_CommandCounter = null;   
    
    [UnityEditor.MenuItem("Assets/H3D/Atlas编辑器")]
    [UnityEditor.MenuItem("H3D/UI/Atlas编辑器")]
    static void Init()
    {//创建Atlas编辑器窗口

        EditorRoot root =
        EditorManager.GetInstance().FindEditor("Atlas编辑器");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("Atlas编辑器", false, InitControls);
        }
    }

    public static void InitControls(EditorRoot editorRoot)
    {//初始化窗口

        if (editorRoot == null)
        {
            //提示程序错误Message
            EditorUtility.DisplayDialog("运行错误",
                                         "窗口初始化失败",
                                         "确认");
            return;
        }

        s_root = editorRoot; 
        s_root.position = new Rect(100f, 100f, 1280, 768f);

        {
            s_root.onEnable = OnEnable;
            s_root.onDisable = OnDisable;
        }

        #region 创建布置窗口元素

        #region 第一级分割
        Rect btnRect = new Rect(0, 0, 80, 20);
        Rect labelRect = new Rect(0, 0, 80, 5);
        Rect hboxRect = new Rect(0, 0, 300, 5);

        HSpliterCtrl hs1 = new HSpliterCtrl();
        hs1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30f);
        HSpliterCtrl hs2= new HSpliterCtrl();
        hs2.layoutConstraint = LayoutConstraint.GetSpliterConstraint(30f, true);
      
        HBoxCtrl hb1 = new HBoxCtrl();      //布置上方菜单条
        HBoxCtrl hb2 = new HBoxCtrl();      //布置主窗口
        HBoxCtrl hb3 = new HBoxCtrl();      //布置下方状态栏
        #endregion

        #region 第二级分割
        VSpliterCtrl vs2_1 = new VSpliterCtrl();
        vs2_1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(300f);
        vs2_1.Dragable = true;
        VSpliterCtrl vs2_2 = new VSpliterCtrl();
        vs2_2.layoutConstraint = LayoutConstraint.GetSpliterConstraint(300f, true);
        vs2_2.Dragable = true;

        VBoxCtrl vb2_1 = new VBoxCtrl();               //布置工程目录/小图列表       
        VBoxCtrl vb2_2 = new VBoxCtrl();               //布置预览窗口       
        VBoxCtrl vb2_3 = new VBoxCtrl();               //布置Inspector窗口       
        #endregion

        #region 第三级分割
        HSpliterCtrl hs2_1_1 = new HSpliterCtrl();
        hs2_1_1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(260f, true);

        VBoxCtrl vb2_1_1 = new VBoxCtrl();                    //布置小图列表
        VBoxCtrl vb2_1_2 = new VBoxCtrl();                    //布置工程目录
        #endregion

        #region  第四级分割
        HSpliterCtrl hs2_1_2_1 = new HSpliterCtrl();
        hs2_1_2_1.layoutConstraint = LayoutConstraint.GetSpliterConstraint(240, true);

        VBoxCtrl vb2_1_2_1 = new VBoxCtrl();                    //布置小图列表
        VBoxCtrl vb2_1_2_2 = new VBoxCtrl();                    //布置工程目录
        #endregion
        #endregion

        #region 布置窗口（由高至低布置）

        #region 第四级分割

        #region Project View
        HBoxCtrl projectBtnBox = new HBoxCtrl();
        projectBtnBox.Size = hboxRect;

        LabelCtrl procLabel = new LabelCtrl();
        procLabel.Size = labelRect;
        procLabel.Caption = "Project View";

        ButtonCtrl newProjBtn = new ButtonCtrl();
        newProjBtn.Caption = "新建工程";
        newProjBtn.Name = "_NewProjBtn";
        newProjBtn.Size = btnRect;
        newProjBtn.onClick = OnNewProjBtn;

        ButtonCtrl openProjBtn = new ButtonCtrl();
        openProjBtn.Caption = "打开工程";
        openProjBtn.Name = "_OpenProjBtn";
        openProjBtn.Size = btnRect;
        openProjBtn.onClick = OnOpenProjBtn;

        ButtonCtrl saveProjBtn = new ButtonCtrl();
        saveProjBtn.Caption = "保存工程";
        saveProjBtn.Name = "_SaveProjBtn";
        saveProjBtn.Size = btnRect;
        saveProjBtn.onClick = OnSaveProjBtn;
        projectBtnBox.Add(newProjBtn);
        projectBtnBox.Add(saveProjBtn);
        projectBtnBox.Add(openProjBtn);

       // vb2_1_2_1.Add(procLabel);
        vb2_1_2_1.Add(projectBtnBox);
        #endregion

        m_projTreeView = new ListViewCtrl();          //工程目录
        m_projTreeView.Caption = "Project";
        m_projTreeView.Name = "Project";
        m_projTreeView.onItemSelected = OnSelectProjectListItem;


        vb2_1_2_2.Add(m_projTreeView);

        hs2_1_2_1.Add(vb2_1_2_1);
        hs2_1_2_1.Add(vb2_1_2_2);
        #endregion

        #region 第三级分割
        m_spriteListView = new ListViewCtrl();        //小图列表
        m_spriteListView.Caption = "小图";
        m_spriteListView.Name = "spriteTreeView";
        m_spriteListView.onItemSelected = OnSelectListItem;
        m_spriteListView.onItemSelectedR = OnSelectListItemR;
        vb2_1_1.Add(m_spriteListView);
        vb2_1_2.Add(hs2_1_2_1);

        hs2_1_1.Add(vb2_1_1);
        hs2_1_1.Add(vb2_1_2);
        #endregion

        #region 第二级分割
        m_preview = new MainViewCtrl();              //预览窗口
        m_preview.Name = "_Preview";
        m_preview.bkColor = Color.grey;
        m_preview.Is2DView = true;

        m_inspector = new InspectorViewCtrl();  //属性窗口
        m_inspector.Name = "_Inspector";
        m_inspector.onInspector = null;

        vb2_1.Add(hs2_1_1);
        vb2_2.Add(m_preview);
        vb2_3.Add(m_inspector);

        vs2_1.Add(vb2_1);
        vs2_1.Add(vs2_2);

        vs2_2.Add(m_preview);
        vs2_2.Add(vb2_3);
        #endregion

        #region 第一级分割
        #region 上方菜单条
        ButtonCtrl addImageBtn = new ButtonCtrl();
        addImageBtn.Caption = "添加小图";
        addImageBtn.Name = "_AddImageBtn";
        addImageBtn.Size = btnRect;
        addImageBtn.onClick = OnAddImageBtn;

        ButtonCtrl previewBtn = new ButtonCtrl();
        previewBtn.Caption = "生成预览";
        previewBtn.Name = "_PreviewBtn";
        previewBtn.Size = btnRect;
        previewBtn.onClick = OnPreviewBtn;

        ButtonCtrl makeAtlasBtn = new ButtonCtrl();
        makeAtlasBtn.Caption = "生成图集";
        makeAtlasBtn.Name = "_MakeAtlasBtn";
        makeAtlasBtn.Size = btnRect;
        makeAtlasBtn.onClick = OnMakeAtlasBtn;

        ButtonCtrl configImageBaseBtn = new ButtonCtrl();
        configImageBaseBtn.Caption = "配置图库路径";
        configImageBaseBtn.Name = "_ConfigImageBaseBtn";
        configImageBaseBtn.Size = btnRect;
        configImageBaseBtn.onClick = OnConfigImageBaseBtn;

        hb1.Add(addImageBtn);
        hb1.Add(previewBtn);
        hb1.Add(makeAtlasBtn);
        hb1.Add(configImageBaseBtn);
        #endregion

        hb2.Add(vs2_1);
        
        hs1.Add(hb1);
        hs1.Add(hs2);

        hs2.Add(hb2);
        hs2.Add(hb3);
        #endregion

        #endregion

        //设置窗口根控件
        s_root.RootCtrl = hs1;

        //注册UIAtlasEditorModel回调函数
        UIAtlasEditorModel.GetInstance().onNewProject = OnNewProject;
        UIAtlasEditorModel.GetInstance().onSpriteImageLoad = OnSpriteImageLoad;
        UIAtlasEditorModel.GetInstance().onAddSpriteImageCommand = OnAddSpriteImageCommand;
        UIAtlasEditorModel.GetInstance().onDeleteSpriteImageCommand = OnDeleteSpriteImageCommand;
        UIAtlasEditorModel.GetInstance().onMakeAtlasCommand = OnMakeAtlasCommand;
        UIAtlasEditorModel.GetInstance().onSpriteZoomChangedCommand = OnSpriteImageZoomChangedCommand;

        //注册编辑器窗口OnGui回调函数
        s_root.onGUI = OnEditorGUI;
        m_Counter = new GameObject();
        m_Counter.name = "AtlasCmdCounter";
        m_Counter.hideFlags = HideFlags.HideAndDontSave;
        m_Counter.AddComponent<UIAtlasCommandCounter>();
        m_CommandCounter = m_Counter.GetComponent<UIAtlasCommandCounter>();
    }

    static void OnEditorGUI(EditorRoot root)
    {//编辑器响应窗口OnGuI回调函数

        if (m_spriteListView == null)
        {
            return;
        }

        if(m_spriteListView.LastSelectItem < 0)
        {//如果当前未选中任何小图，更新小图资源变更

            if (m_inspector == null)
            {
                return;
            }

            //销毁预览GameObject
            GameObject.DestroyImmediate(m_previewObj);
            //Inspecetor试图清空
            if (m_inspector.onInspector == OnSpriteInspector)
            {
                 m_inspector.onInspector = null;
            }

            //更新小图资源
            UIAtlasEditorModel.GetInstance().UpdateSprite();
        }
    }

    static void OnNewProjBtn(EditorControl c)
    {//新建工程

        if(UIAtlasEditorModel.GetInstance().IsProjectExist())
        {
            if(EditorUtility.DisplayDialog("保存工程", "是否保存当前工程", "保存", "取消"))
            {
                OnSaveProjBtn(c);
            }
        }

        //显示新建工程对话框
        EditorWindow.GetWindow<AtlasDialog>(false, "Create Project", true);

        RequestRepaint();
    }

    static void OnOpenProjBtn(EditorControl c)
    {//打开工程

        if((m_spriteListView == null) || (m_projTreeView == null))
        {
            return;
        }

        //显示打开工程对话框
        string path = EditorUtility.OpenFilePanel("Load png Textures of Directory", "", "atlasproj");
        //清空小图List
        m_spriteListView.ClearItems();

        //读取工程
        if(UIAtlasEditorModel.GetInstance().LoadProject(path))
        {//读取成功

            //清空工程List
            m_projTreeView.ClearItems();

            //向工程List中添加新的工程项目
            ListCtrlItem newItem = new ListCtrlItem();
            newItem.name = UIAtlasEditorModel.GetInstance().GetProjectName();
            newItem.color = Color.white;
            newItem.onSelectColor = Color.blue;
            m_projTreeView.AddItem(newItem);

            //更新Inspector视图至工程属性
            m_projOutputPath = UIAtlasEditorModel.GetInstance().GetAtlasSavePath();

            //清空预览区域
            GameObject.DestroyImmediate(m_previewObj);
            m_spriteListView.LastSelectItem = -1;

            RequestRepaint();
        }
    }

    static void OnSaveProjBtn(EditorControl c)
    {//保存工程

        if (!UIAtlasEditorModel.GetInstance().IsProjectExist())
        {//工程不存在

            //提示创建工程的Message
            EditorUtility.DisplayDialog("保存失败",
                                         "请先创建工程",
                                         "确认");
        }
        else
        {//工程存在

            if (PROJECT_TYPE.PROJECT_TYPE_NEW == UIAtlasEditorModel.GetInstance().GetProjectType())
            {//当前是新创建的工程
                
                //显示选择保存工程路径对话框
                string fileNames = EditorUtility.SaveFilePanel("", "Assets/", UIAtlasEditorModel.GetInstance().GetProjectName(), "atlasproj");
                //保存工程
                UIAtlasEditorModel.GetInstance().SaveProject(fileNames);

                RequestRepaint();
            }
            else
            {//当前是已存在工程

                //保存工程
                UIAtlasEditorModel.GetInstance().SaveProject(UIAtlasEditorModel.GetInstance().GetProjectPath());
            }
        }

    }

    static void OnAddImageBtn(EditorControl c)
    {//添加小图

        if(!UIAtlasEditorModel.GetInstance().IsProjectExist())
        {//工程不存在

            //提示创建工程的Message
            EditorUtility.DisplayDialog("添加失败",
                                         "请先创建工程",
                                         "确认");
        }
        else
        {//工程存在

            //初始化添加小图对话框信息
            OpenFileDialog openfiledialog1 = new OpenFileDialog();
            openfiledialog1.Multiselect = true;//允许同时选择多个文件
            openfiledialog1.InitialDirectory = UIAtlasEditorConfig.ImageBasePath;
            openfiledialog1.Filter = "图片文件(*.jpg*.jpeg*.png*.bmp)|*.jpg;*.jpeg;*.png;*.bmp";
            openfiledialog1.FilterIndex = 2;
            openfiledialog1.RestoreDirectory = true;
            openfiledialog1.Title = "添加图片";

            //显示添加小图对话框
            if (openfiledialog1.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                for (int fi = 0; fi < openfiledialog1.FileNames.Length; fi++)
                {
                    AtlasAddSpriteImageCommand cmd = new AtlasAddSpriteImageCommand();
                    cmd.m_SpriteName = openfiledialog1.FileNames[fi].ToString();
                    EditorCommandManager.GetInstance().Add(cmd);
                }
            }

            RequestRepaint();
        }

    }

    static void OnAddSpriteImageCommand(bool bResult, string spriteName)
    {
        if(bResult)
        {//添加成功

            //更新小图LIst显示
            ListCtrlItem newItem = new ListCtrlItem();
            newItem.name = spriteName;
            newItem.color = Color.white;
            newItem.onSelectColor = Color.blue;
            m_spriteListView.AddItem(newItem);

            //初始化预览窗口、Inspector视图
            //清除Inspector视图信息
            m_inspector.onInspector = null;
            //销毁预览GameObject
            GameObject.DestroyImmediate(m_previewObj);
        }
        else
        {//添加失败

            if (PROJECT_FAILED_TYPE.PROJECT_FAILED_SPRITEIMAGE_PATH_ERROR
                == UIAtlasEditorModel.GetInstance().GetProjectFailedType())
            {//小图路径错误

                //提示正确路径的message
                EditorUtility.DisplayDialog("添加失败",
                                             "请重新配置图库路径",
                                             "确认");
            }
            else if (PROJECT_FAILED_TYPE.PROJECT_FAILED_SPRITE_EXIST_ERROR
                == UIAtlasEditorModel.GetInstance().GetProjectFailedType())
            {//小图已存在

                //提示小图已存在的message
                EditorUtility.DisplayDialog("添加失败",
                                             "该小图已存在",
                                             "确认");
            }

        }
        
        RequestRepaint();
    }
    static void OnPreviewBtn(EditorControl c)
    {//预览Atlas

        if (!UIAtlasEditorModel.GetInstance().IsProjectExist())
        {//工程未创建

            //提示创建工程Message
            EditorUtility.DisplayDialog("预览失败",
                                         "请先创建工程",
                                         "确认");
        }
        else
        {//工程已创建

            Texture2D tex = null;

            if (!UIAtlasEditorModel.GetInstance().PreViewAtlas(out tex))
            {//预览失败

                if (PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_IMAGE_ERROR
                   == UIAtlasEditorModel.GetInstance().GetProjectFailedType())
                {//未添加小图

                    //提示添加小图Message
                    EditorUtility.DisplayDialog("预览失败",
                                                 "请添加小图资源",
                                                 "确认");
                }
            }
            else
            {//预览成功

                //更新预览窗口
                UpdatePreViewWnd(tex);
            }
        }
        
        RequestRepaint();
    }

    static void OnMakeAtlasBtn(EditorControl c)
    {//生成Atlas

        if (!UIAtlasEditorModel.GetInstance().IsProjectExist())
        {//工程未创建

            //提示创建工程message
            EditorUtility.DisplayDialog("生成失败",
                                         "请先创建工程",
                                         "确认");
        }
        else
        {//工程已存在
            AtlasMakeAtlas cmd = new AtlasMakeAtlas();
            EditorCommandManager.GetInstance().Add(cmd);
        }
        
        RequestRepaint();
    }

    static void OnMakeAtlasCommand(bool bRet)
    {
        if(bRet)
        {
            //提示生成成功
            EditorUtility.DisplayDialog("生成成功",
                                         "Atlas、Prefab生成完",
                                         "确认");
        }
        else
        {
            if (PROJECT_FAILED_TYPE.PROJECT_FAILED_ATLASOUTPU_PATH_ERROR
                    == UIAtlasEditorModel.GetInstance().GetProjectFailedType())
            {//未指定Atlas输出路径

                //显示选择路径对话框
                string savePath = EditorUtility.SaveFolderPanel("SaveAtlas", "Assets/", "");

                if (savePath.Contains("/Assets"))
                {//路径合法(Unity相对路径)

                    //获取Unity相对路径
                    savePath = savePath.Substring(savePath.LastIndexOfAny("/Assets".ToCharArray()) - "Assets".Length + 1);
                    //设定Atlas输出路径
                    UIAtlasEditorModel.GetInstance().SetAtlasSavePath(savePath + "/");
                   
                    AtlasMakeAtlas cmd = new AtlasMakeAtlas();
                    EditorCommandManager.GetInstance().Add(cmd);
                }
                else
                {
                    //提示输出路径错误message
                    EditorUtility.DisplayDialog("生成失败",
                                                 "请选择Assets/下的路径",
                                                 "确认");
                }
            }
            else if (PROJECT_FAILED_TYPE.PROJECT_FAILED_NONE_IMAGE_ERROR
                    == UIAtlasEditorModel.GetInstance().GetProjectFailedType())
            {//未添加小图

                //提示添加小图message
                EditorUtility.DisplayDialog("生成失败",
                                             "请添加小图资源",
                                             "确认");
            }
            else
            {
                //do nothing
            }
        }

        RequestRepaint();
    }
    static void OnConfigImageBaseBtn(EditorControl c)
    {//配置小图路径

        //显示保存路径对话框
        string savePath = EditorUtility.SaveFolderPanel("Image Path Config", "", "");

        //将路径写入配置文件
        UIAtlasEditorModel.GetInstance().WriteImagePathConfig(savePath + "/");
    }

    static void OnNewProject(UIAtlasEditorModel mode)
    {//响应工程创建回调函数
 
        if ((m_projTreeView == null) || (m_spriteListView == null) || (m_inspector == null))
        {
            return;
        }

        //初始化窗口显示
        //清空工程List、小图List
        m_projTreeView.ClearItems();
        m_spriteListView.ClearItems();
        //清空预览窗口、Inspector窗口
        GameObject.DestroyImmediate(m_previewObj);
        m_inspector.onInspector = null;

        //添加新建工程至工程List
        ListCtrlItem newItem = new ListCtrlItem();
        newItem.name = mode.GetProjectName();
        newItem.color = Color.white;
        newItem.onSelectColor = Color.blue;
        m_projTreeView.AddItem(newItem);
        
        RequestRepaint();
    }

    static void OnSpriteImageLoad(string spriteName)
    {//小图读取完成回调函数

        //向List中添加新的小图
        ListCtrlItem newItem = new ListCtrlItem();
        newItem.name = spriteName;
        newItem.color = Color.white;
        newItem.onSelectColor = Color.blue;
        m_spriteListView.AddItem(newItem);
      
        RequestRepaint();
    }

    static void OnSelectProjectListItem(EditorControl c, int i)
    {//点击工程List处理函数

        if ((m_spriteListView == null) || (m_inspector == null))
        {
            return;
        }

        //更新Inspector视图至工程属性
        m_inspector.onInspector = OnProjectInspector;
        m_projOutputPath = UIAtlasEditorModel.GetInstance().GetAtlasSavePath();

        //清空预览区域
        GameObject.DestroyImmediate(m_previewObj);
        m_spriteListView.LastSelectItem = -1;

        RequestRepaint();
    }

    static void OnSelectListItem(EditorControl c, int i)
    {//点击小图List处理函数

        Texture2D tex = null;
        string SpriteName = null;

        if ((m_spriteListView == null) || (m_inspector == null) || (m_spriteListView.LastSelectItem < 0))
        {
            return;
        }

        //获取小图纹理
        ListCtrlItem deleteItem = m_spriteListView.Items[m_spriteListView.LastSelectItem];
        SpriteName = deleteItem.name;
        tex = UIAtlasEditorModel.GetInstance().LoadSpriteImage(SpriteName);

        if(tex == null)
        {
            return;
        }

        //更新预览区域
        UpdatePreViewWnd(tex);

        //更新Inspector视图至小图属性
        m_inspector.onInspector = OnSpriteInspector;

        //更新当前小图的宽、高、缩放比例信息
        m_spriteWidth = tex.width;
        m_spriteHeight = tex.height;
        m_aspecet = (float)m_spriteWidth / (float)m_spriteHeight;
        m_spriteWidthStr = m_spriteWidth.ToString();
        m_spriteHeightStr = m_spriteHeight.ToString();

        RequestRepaint();
    }

    static void OnSelectListItemR(EditorControl c, int i)
    {//右键单击小图List

        GenericMenu menu = new GenericMenu();
        //弹出删除小图下拉菜单
        menu.AddItem(new GUIContent("删除图片"), false, OnDeleteSpriteImage, "item 1");
        menu.ShowAsContext();

        RequestRepaint();
    }

    static void OnDeleteSpriteImage(object command)
    {//删除小图

        if ((m_spriteListView == null) || (m_spriteListView.LastSelectItem < 0))
        {
            return;
        }

        string deleteSpriteName = null;
        ListCtrlItem deleteItem = m_spriteListView.Items[m_spriteListView.LastSelectItem];
        deleteSpriteName = deleteItem.name;

        AtlasDeleteSpriteImageCommand cmd = new AtlasDeleteSpriteImageCommand();
        cmd.m_SpriteName = deleteSpriteName;
        EditorCommandManager.GetInstance().Add(cmd);

        RequestRepaint();
    }

    static void OnDeleteSpriteImageCommand(bool bRet)
    {
        if(bRet)
        {//删除成功
            if ((m_spriteListView == null) || (m_inspector == null) || (m_spriteListView.LastSelectItem < 0))
            {
                return;
            }

            ListCtrlItem deleteItem = m_spriteListView.Items[m_spriteListView.LastSelectItem];
            //从List中移除小图
            m_spriteListView.RemoveItem(deleteItem);
            //更新Inspector视图
            m_inspector.onInspector = null;
            //清空预览窗口
            GameObject.DestroyImmediate(m_previewObj);
            
            RequestRepaint();
        }
    }

    static void OnProjectInspector(EditorControl c, object target)
    {//工程属性视图显示
    
        GUILayout.Space(20f);
      
        GUILayout.BeginHorizontal();
        GUILayout.Label("Atlas输出路径", GUILayout.Width(80f));
        GUI.SetNextControlName("Atlas输出路径");
        m_projOutputPath = EditorGUILayout.TextField(m_projOutputPath, GUILayout.Width(150));
        
        //编辑完成
        if(Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.Used)
        {
            if (GUI.GetNameOfFocusedControl() == "Atlas输出路径")
            {
                //更新Atlas输出路径
                UIAtlasEditorModel.GetInstance().SetAtlasSavePath(m_projOutputPath);
            }
        }
        GUILayout.EndHorizontal();
        
        RequestRepaint();
    }

    static void OnSpriteInspector(EditorControl c, object target)
    {//小图属性窗口显示
        int w, h;
        string oldStr = null;
        string SpriteName = null;

        if ((m_spriteListView == null) || (m_spriteListView.LastSelectItem < 0))
        {
            return;
        }

        ListCtrlItem deleteItem = m_spriteListView.Items[m_spriteListView.LastSelectItem];
        SpriteName = deleteItem.name;

        GUILayout.Space(20f);

        GUILayout.BeginHorizontal();
        GUILayout.Label("宽高比", GUILayout.Width(60f));
        GUILayout.TextField(m_aspecet.ToString(), GUILayout.Width(150));
        GUILayout.EndHorizontal();
    
        GUILayout.BeginHorizontal();
        GUILayout.Label("宽度", GUILayout.Width(60f));
        GUI.SetNextControlName("宽度");
        oldStr = m_spriteWidthStr;
        m_spriteWidthStr = GUILayout.TextField(m_spriteWidthStr, GUILayout.Width(150));
        if ((GUI.GetNameOfFocusedControl() == "宽度") && (oldStr != m_spriteWidthStr))
        {
            int.TryParse(m_spriteWidthStr, out w);
            h = (int)(w / m_aspecet);
            m_spriteHeightStr = h.ToString();
        }
        GUILayout.Label("pix", GUILayout.Width(50f));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("高度", GUILayout.Width(60f));
        GUI.SetNextControlName("高度");
        oldStr = m_spriteHeightStr;
        m_spriteHeightStr = GUILayout.TextField(m_spriteHeightStr, GUILayout.Width(150));
        if ((GUI.GetNameOfFocusedControl() == "高度") && (oldStr != m_spriteHeightStr))
        {
            int.TryParse(m_spriteHeightStr, out h);
            w = (int)(h * m_aspecet);
            m_spriteWidthStr = w.ToString();
        }
        GUILayout.Label("pix", GUILayout.Width(50f));
        GUILayout.EndHorizontal();

        if(Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.Used)
        {
            if (GUI.GetNameOfFocusedControl() == "高度" || GUI.GetNameOfFocusedControl() == "宽度")
            {
                int.TryParse(m_spriteWidthStr, out w);
                int.TryParse(m_spriteHeightStr, out h);

                AtlasImageZoomCommand cmd = new AtlasImageZoomCommand();
                AtlasSpriteImage spriteImage = null;
                UIAtlasEditorModel.GetInstance().GetSpriteImage(SpriteName, out spriteImage);

                cmd.m_oldScaleFactor = spriteImage.ZoomScale;
                cmd.m_newScaleFactor = (float)w / (float)m_spriteWidth;
                cmd.m_SpriteName = SpriteName;
                EditorCommandManager.GetInstance().Add(cmd);
#if UNITY_4_5
                Undo.RecordObject(m_CommandCounter, "Zoom Image");
                //Undo.RecordObject(p, "Zoom Image");
                //Undo.RegisterCompleteObjectUndo(p, "Zoom Image");
                //Undo.RegisterCreatedObjectUndo(p, "Zoom Image");
#endif
                m_CommandCounter.CommandCounter++;
                int test = m_CommandCounter.CommandCounter;
            }
        }
        
        RequestRepaint();
    }

    static void OnSpriteImageZoomChangedCommand(string spritePath)
    {//小图缩放比例变更回调函数

        Texture2D tex = null;
        //获取小图纹理
        tex = UIAtlasEditorModel.GetInstance().GetSpriteTexture(spritePath);

        //更新预览窗口
        UpdatePreViewWnd(tex);
    }

    static void OnEnable(EditorRoot root)
    {
        UIAtlasEditorModel.GetInstance().ReadImagePathConfig();
        Undo.undoRedoPerformed += OnUndoRedo;
        
        RequestRepaint();
    }

    static void OnDisable(EditorRoot root)
    {
        UIAtlasTempTextureManager.DestroyInstance();
        UIAtlasEditorModel.GetInstance().DestoryInstance();
        EditorCommandManager.GetInstance().Clear();
        Undo.undoRedoPerformed -= OnUndoRedo;
        GameObject.DestroyImmediate(m_Counter);
        GameObject.DestroyImmediate(m_previewObj);
    }

    static void OnProjectOutputTextBoxChange(EditorControl c, float value)
    {

    }

    static GameObject _GenTexturePreviewObject( float width , float height , Texture tex )
    {
        GameObject previewObj = new GameObject();
        previewObj.transform.localScale = new Vector3(width, height,1.0f);
        MeshRenderer meshRenderer = previewObj.AddComponent<MeshRenderer>();
        MeshFilter   meshFilter   = previewObj.AddComponent<MeshFilter>();
        
        Mesh mesh = new Mesh();
        Vector3[] verts = new Vector3[4];
        verts[0].Set(-0.5f, 0.5f, 0.0f);
        verts[1].Set(0.5f, 0.5f, 0.0f);
        verts[2].Set(0.5f, -0.5f, 0.0f);
        verts[3].Set(-0.5f, -0.5f, 0.0f);

        Vector3[] norms = new Vector3[4];
        norms[0].Set(0.0f, 0.0f, -1.0f);
        norms[1].Set(0.0f, 0.0f, -1.0f);
        norms[2].Set(0.0f, 0.0f, -1.0f);
        norms[3].Set(0.0f, 0.0f, -1.0f);

        Vector2[] uv = new Vector2[4];
        uv[0].Set(0.0f, 1.0f);
        uv[1].Set(1.0f, 1.0f);
        uv[2].Set(1.0f, 0.0f);
        uv[3].Set(0.0f, 0.0f);

        Color[] vertcolor = new Color[4];
        vertcolor[0] = Color.white;
        vertcolor[1] = Color.white;
        vertcolor[2] = Color.white;
        vertcolor[3] = Color.white;

        int[] indices = new int[4];
        indices[0] = 0;
        indices[1] = 1;
        indices[2] = 2;
        indices[3] = 3;

        mesh.vertices = verts;
        mesh.normals = norms;
        mesh.uv = uv;
        mesh.colors = vertcolor;
        mesh.SetIndices(indices, MeshTopology.Quads,0);

        meshFilter.mesh = mesh;

        Shader shader = Resources.Load<Shader>("UIAtlasPreviewShader"); 
        Material mat = new Material(shader);
        mat.mainTexture = tex;
        tex.filterMode = FilterMode.Point;

        meshRenderer.material = mat;       

        return previewObj;
    }

   static private void MakeDialogProp(OpenFileName ofn)
    {
        ofn.structSize = Marshal.SizeOf(ofn);

        ofn.filter = "图片文件(*.jpg*.jpeg*.png*.bmp)\0*.jpg;*.jpeg;*.png;*.bmp";

        ofn.file = new string(new char[256]);

        ofn.maxFile = ofn.file.Length;

        ofn.fileTitle = new string(new char[64]);

        ofn.maxFileTitle = ofn.fileTitle.Length;

        ofn.initialDir = UnityEngine.Application.dataPath;//默认路径

        ofn.title = "Open Project";

        //ofn.defExt = "JPG";//显示文件的类型
        //注意 一下项目不一定要全选 但是0x00000008项不要缺少
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;//OFN_EXPLORER|OFN_FILEMUSTEXIST|OFN_PATHMUSTEXIST| OFN_ALLOWMULTISELECT|OFN_NOCHANGEDIR
    }

    static private void UpdatePreViewWnd(Texture2D tex)
    {
        if(tex == null)
        {
            return;
        }

        float aspect = (float)tex.width / (float)tex.height;
        float w1 = 10.0f;
        float h1 = w1 / aspect;        

        //更新预览窗口
        GameObject.DestroyImmediate(m_previewObj);
        m_previewObj = _GenTexturePreviewObject(w1, h1, tex);
        if (m_previewObj == null)
        {
            return;
        }

        m_previewObj.transform.parent = m_preview.GetBindingTarget().transform;
        m_previewObj.transform.localPosition = Vector3.zero;

        RequestRepaint();
    }

    static private void RequestRepaint() 
    {
        if(s_root != null)
        {
            s_root.RequestRepaint();
        }
    }

    static void OnUndoRedo()
    {
        int commandCount = 0;

        if (m_CommandCounter.IsRedo(out commandCount))
        {
            for(int i = 0; i < commandCount; i++)
            {
                EditorCommandManager.GetInstance().PerformRedo();
            }
        }
        else
        {
            for (int i = 0; i < commandCount; i++)
            {
                EditorCommandManager.GetInstance().PerformUndo();
            }
        }

        m_CommandCounter.PreCommandCounter = m_CommandCounter.CommandCounter;
    }
}
