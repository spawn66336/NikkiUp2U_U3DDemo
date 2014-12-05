using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;
public class PackageExportTool 
{
    static EditorRoot s_root = null;

    private static string s_configFileName = "Assets/Editor/UniversalEditor/ResourceManageTool/Config/PackageExportTool.xml";
    private static List<string> s_ExportPaths = new List<string>();
    private static string s_ExportFileName = "";
    private static string s_Version = "";

    [MenuItem("H3D/资源管理/资源导出工具")]
    public static void Show()
    {
        EditorRoot root = EditorManager.GetInstance().FindEditor("资源导出工具");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("资源导出工具", false, InitControls);
        }
    }
    public static void InitControls(EditorRoot editorRoot)
    {
        s_root = editorRoot;

        s_root.position = new Rect(100f, 100f, 1024, 768f);
        s_root.onDestroy = OnDestroy;
        s_root.onEnable = OnEnable;

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
        vb0.Add(treeView);

        TextBoxCtrl tbVersion = new TextBoxCtrl();
        tbVersion.Size = new Rect(0, 0, 30, 20);
        tbVersion.Name = "_VersionBox";
        tbVersion.Caption = "版本号";
        tbVersion.Enable = true;
        tbVersion.layoutConstraint = LayoutConstraint.GetInspectorViewConstraint(10, 50);

        TextBoxCtrl tbFileName = new TextBoxCtrl();
        tbFileName.Size = new Rect(0, 0, 60, 20);
        tbFileName.Name = "_SaveFileName";
        tbFileName.Caption = "输出文件";
        tbFileName.Enable = true;
        tbFileName.layoutConstraint = LayoutConstraint.GetInspectorViewConstraint(10, 50);

        Rect btnRect = new Rect(0, 0, 60, 20);

        ButtonCtrl ChooseFileBtn = new ButtonCtrl();
        ChooseFileBtn.Caption = "选择";
        ChooseFileBtn.Name = "_ChooseFileName";
        ChooseFileBtn.Size = btnRect;
        ChooseFileBtn.onClick = OnChooseFileNameButtonClick;

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

        hb0.Add(tbVersion);
        hb0.Add(tbFileName);
        hb0.Add(ChooseFileBtn);
        hb0.Add(okBtn);
        hb0.Add(cancelBtn);
    }

    static void UpdateTreeView()
    {
        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if (treeView == null)
        {
            return;
        }

        treeView.Clear();

        string[] allAssetPaths = ResourceManageToolUtility.GetAllAssetPaths();

        foreach (var path in allAssetPaths)
        {
            if (ResourceManageToolUtility.PathIsFolder(path))
            {
                AddAssetToResourceTreeView(path);
            }
        }
    }

    static void AddAssetToResourceTreeView(string path)
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
                if (string.Equals(pathNodeName.ToLower(), "assets"))
                    newNode.state.IsExpand = true;
                else
                    newNode.state.IsExpand = false;

                TreeViewNodeUserParam userParam = new TreeViewNodeUserParam();

                bool toggleState = false;
                foreach (string p in s_ExportPaths)
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

    static bool CanExport()
    {
        TextBoxCtrl versioinTB = s_root.FindControl("_VersionBox") as TextBoxCtrl;
        TextBoxCtrl filenameTB = s_root.FindControl("_SaveFileName") as TextBoxCtrl;

        if (versioinTB == null || filenameTB == null)
        {
            return false;
        }
        s_Version = versioinTB.Text;
        if (s_Version.Length == 0)
        {
            EditorUtility.DisplayDialog("", "请输入版本号", "确定");
            return false;
        }
        string sPathName = filenameTB.Text;
        if (sPathName.Length == 0)
        {
            EditorUtility.DisplayDialog("", "请输入输出文件路径", "确定");
            return false;
        }
        string sPath = Path.GetDirectoryName(sPathName);
        string sName = Path.GetFileNameWithoutExtension(sPathName);
        s_ExportFileName = sPath + "/" + sName + "V" + s_Version + ".unitypackage";

        return true;
    }

    static void OnChooseFileNameButtonClick(EditorControl c)
    {
        string filename = EditorUtility.SaveFilePanel("保存文件", "", "H3DUnityEditor", "unitypackage");
        TextBoxCtrl filenameTB = s_root.FindControl("_SaveFileName") as TextBoxCtrl;
        if (filenameTB != null && filename.Length > 0)
        {
            filenameTB.Text = filename;
            RequestRepaint();
        }
    }
    static void OnOkButtonClick(EditorControl c)
    {
        if(CanExport())
        {
            RecalcTreeNodes();
            CollectAllPaths();

            SaveConfig();
            ExportPackage();
            s_root.ShutDown();
        }
    }

    static void OnCancelButtonClick(EditorControl c)
    {
        s_root.ShutDown();
    }

    static void LoadConfig()
    {
        s_ExportPaths.Clear();
        if(File.Exists(s_configFileName))
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(s_configFileName);
            XmlNode root = doc.SelectSingleNode("ExportPackageConfig");
            if (root != null)
            {
                XmlNode version_node = root.SelectSingleNode("Version");
                if (version_node != null)
                {
                    TextBoxCtrl versioinTB = s_root.FindControl("_VersionBox") as TextBoxCtrl;
                    if (versioinTB != null)
                    {
                        versioinTB.Text = version_node.InnerText;
                    }
                }

                XmlNode path_node = root.SelectSingleNode("Path");
                if (path_node != null)
                {
                    foreach (XmlNode n in path_node.ChildNodes)
                    {
                        if(n.Name == "Item")
                        {
                            string s = n.InnerText;
                            if(s.Length > 0)
                            {
                                s_ExportPaths.Add(s);
                            }
                        }
                    }
                }
            }

            RequestRepaint();
        }
    }
    static void SaveConfig()
    {
        XmlDocument docment = new XmlDocument();
        XmlElement root = docment.CreateElement("ExportPackageConfig");
        docment.AppendChild(root);

        XmlElement nodeVersion = docment.CreateElement("Version");
        nodeVersion.InnerText = s_Version;
        root.AppendChild(nodeVersion);

        XmlElement nodePath = docment.CreateElement("Path");
        root.AppendChild(nodePath);
        foreach (string sPath in s_ExportPaths)
        {
            XmlElement node = docment.CreateElement("Item");
            node.InnerText = sPath;
            nodePath.AppendChild(node);
        }

        try
        {
            docment.Save(s_configFileName);
        }
        catch(System.Exception e)
        {
            Debug.Log("保存打包工具配置文件失败: " + e.Message);
        }
    }

    static void OnEnable(EditorRoot root)
    {
        s_ExportFileName = "";
        s_Version = "";
        LoadConfig();
        UpdateTreeView();
    }

    static void OnDestroy(EditorRoot root)
    {
        s_ExportPaths.Clear();
    }

    static void RequestRepaint()
    {
        s_root.RequestRepaint();
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
        while (parent != null)
        {
            if ((bool)parent.state.userParams[0].param)
            {
                parentAlreadyToggled = true;
            }
            parent = parent.parent;
        }

        if (parentAlreadyToggled)
        {
            n.state.userParams[0].param = false;
        }
        return true;
    }

    static void CollectAllPaths()
    {
        s_ExportPaths.Clear();

        TreeViewCtrl treeView = s_root.FindControl("_MainTreeView") as TreeViewCtrl;
        if (treeView == null)
        {
            return;
        }

        foreach (var root in treeView.Roots)
        {
            TreeViewCtrl.PreorderTraverse(root, CollectAllPathsTreeNodeVisitCallback);
        }
    }

    static bool CollectAllPathsTreeNodeVisitCallback(TreeViewNode n)
    {
        if ((bool)n.state.userParams[0].param)
        {
            string path = n.GetPathString();
            s_ExportPaths.Add(path);
        }
        return true;
    }

    static void ExportPackage()
    {
        if (s_ExportPaths.Count > 0 && s_ExportFileName.Length > 0)
        {
            string[] sAssets = s_ExportPaths.ToArray();

            try
            {
                AssetDatabase.ExportPackage(sAssets, s_ExportFileName, ExportPackageOptions.Recurse);
            }
            catch (System.Exception e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
