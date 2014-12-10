


using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using H3DEditor;

 
static public class LayoutMenu
{
    [MenuItem("H3D/UI布局编辑/Layout编辑器")]
    static public void StartEditor()
    {
        LayoutEditorWindow.Init();
    }

    [MenuItem("H3D/UI布局编辑/导出Layout...")]
    static public void ExportLayout()
    {
        string prefab = EditorTool.GetCurrentSelectedAssetPath();
        string layout_file = LayoutTool.GetLayoutFullPath(prefab);

        if (string.IsNullOrEmpty(layout_file))
        {
            return;
        }
        if (File.Exists(layout_file))
        {
            if (!EditorUtility.DisplayDialog("", "\"" + layout_file + "\"已经存在，是否覆盖？", "确定", "取消"))
            {
                return;
            }
        }

        GameObject inst = Object.Instantiate(EditorTool.GetCurrentSelectedAssetObj()) as GameObject;
        if (LayoutTool.HasUI(inst))
        {
            if (!LayoutTool.HasAnchorUI(inst))
            {
                if (!LayoutTool.HasUIRootOrPanel(inst))
                {
                    GameObject objRoot = new GameObject("UIRootTempPanel");
                    UIPanel uPanel = objRoot.AddComponent<UIPanel>();
                    if (uPanel != null)
                    {
                        uPanel.depth = 1;
                        string sName = EditorTool.GetCurrentSelectedAssetObj().name;
                        inst.transform.parent = objRoot.transform;
                        inst.name = sName;
                        inst = objRoot;
                    }
                    else
                    {
                        Debug.Log("Add UIPanel Component Failed");
                    }
                }

                Camera camera = LayoutTool.CreateCamera();
                int max_try = 5;
                int try_count = 0;
                bool need_reset_pos = LayoutTool.NeedResetPos(inst);
                if (need_reset_pos)
                {
                    inst.transform.localPosition = Vector3.zero;
                }
                inst.transform.localEulerAngles = Vector3.zero;
                inst.transform.localScale = Vector3.one;
                LayoutTool.SetCamera(inst, camera);
                while (try_count < max_try && LayoutTool.RemoveNoUINode(inst))
                {
                    ++try_count;
                }

                if (LayoutTool.ProcessBeforeExport(inst))
                {
                    LayoutTool.SaveLayout(layout_file, inst);
                    EditorUtility.DisplayDialog("", "导出成功", "确定");
                }
                LayoutTool.ReleaseCamera(camera);
            }
            else
            {
                EditorUtility.DisplayDialog("", "有含有Anchor的UI节点，无法导出", "确定");
            }
        }
        else
        {
            EditorUtility.DisplayDialog("", "没有可调整的UI，无需导出", "确定");
        }
        Object.DestroyImmediate(inst);
    }

    [MenuItem("H3D/UI布局编辑/导入Layout...")]
    static public void ImportLayout()
    {
        string prefab = EditorTool.GetCurrentSelectedAssetPath();
        string layout_file = LayoutTool.GetLayoutFullPath(prefab);

        if (string.IsNullOrEmpty(layout_file))
        {
            return;
        }
        if (!File.Exists(layout_file))
        {
            EditorUtility.DisplayDialog("", "\"" + layout_file + "\"不存在，无法导入", "确定");
            return;
        }

        GameObject layout_obj = LayoutTool.LoadLayout(layout_file);
        GameObject prefab_obj = Resources.LoadAssetAtPath(prefab, typeof(GameObject)) as GameObject;
        GameObject new_prefab_obj = Object.Instantiate(prefab_obj) as GameObject;
        Vector3 pos = new_prefab_obj.transform.localPosition;
        Vector3 rotate = new_prefab_obj.transform.localEulerAngles;
        Vector3 scale = new_prefab_obj.transform.localScale;
        bool need_reset_pos = LayoutTool.NeedResetPos(new_prefab_obj);

        if (!LayoutTool.ProcessBeforeImport(new_prefab_obj))
        {
            Object.DestroyImmediate(new_prefab_obj);
            Object.DestroyImmediate(layout_obj);
            return;
        }

        Camera camera = LayoutTool.CreateCamera();
        //UIWidget[] all_ui = layout_obj.GetComponentsInChildren<UIWidget>(true);
        UIElement[] all_element = layout_obj.GetComponentsInChildren<UIElement>(true);

        if (need_reset_pos)
        {
            new_prefab_obj.transform.localPosition = Vector3.zero;
        }
        new_prefab_obj.transform.localEulerAngles = Vector3.zero;
        new_prefab_obj.transform.localScale = Vector3.one;
        new_prefab_obj.name = prefab_obj.name;
        LayoutTool.SetCamera(new_prefab_obj, camera);
        //for (int i = 0; i < all_ui.Length; ++i)
        for (int i = 0; i < all_element.Length; ++i)
        {
            if (all_element[i].FullPathName == "UIRootTempPanel")
                continue;

            bool bHasUIWidget = true;
            UIWidget widget = all_element[i].GetWidget();
            if (widget == null)
                bHasUIWidget = false;

            List<GameObject> ui_list = null;
            if(bHasUIWidget)
                ui_list = EditorTool.FindGameObjectByName(new_prefab_obj, widget.name);
            else
                ui_list = EditorTool.FindGameObjectByName(new_prefab_obj, all_element[i].Name);

            if (ui_list.Count == 0)
            {
                if (bHasUIWidget)
                    Debug.LogError("找不到节点\"" + widget.name + "\"");
                else
                    Debug.LogError("找不到节点\"" + all_element[i].Name + "\"");
            }
            else if (ui_list.Count > 1)
            {
                GameObject child = null;
                string sFullPathName = all_element[i].FullPathName;
                if (sFullPathName.Length > 0)
                {
                    int nIndex = sFullPathName.IndexOf('/');
                    if (nIndex >= 0)
                        sFullPathName = sFullPathName.Substring(nIndex + 1);
                    child = EditorTool.FindGameObjectByPath(new_prefab_obj, sFullPathName);
                }

                if (child != null)
                {
                    if (bHasUIWidget)
                    {
                        if (!LayoutTool.LoadWidgetInfo(child, widget))
                        {
                            Debug.LogError("节点\"" + widget.name + "\"类型改变，无法导入数据");
                        }
                    }
                    else
                    {
                        LayoutTool.LoadNonWidgetTransformInfo(child, all_element[i]);
                    }
                }
                else
                {
                    if (bHasUIWidget)
                        Debug.LogError("节点名称\"" + widget.name + "\"不唯一");
                    else
                        Debug.LogError("节点名称\"" + all_element[i].Name + "\"不唯一");
                }
            }
            else
            {
                if (bHasUIWidget)
                {
                    if (!LayoutTool.LoadWidgetInfo(ui_list[0], widget))
                    {
                        Debug.LogError("节点\"" + widget.name + "\"类型改变，无法导入数据");
                    }
                }
                else
                {
                    LayoutTool.LoadNonWidgetTransformInfo(ui_list[0], all_element[i]);
                }
            }
        }

        if (need_reset_pos)
        {
            new_prefab_obj.transform.localPosition = pos;
        }
        new_prefab_obj.transform.localEulerAngles = rotate;
        new_prefab_obj.transform.localScale = scale;
        PrefabUtility.ReplacePrefab(new_prefab_obj, prefab_obj);
        AssetDatabase.SaveAssets();
        LayoutTool.ReleaseCamera(camera);
        Object.DestroyImmediate(new_prefab_obj);
        Object.DestroyImmediate(layout_obj);
        EditorUtility.DisplayDialog("", "导入成功", "确定");
    }

    [MenuItem("H3D/Export Layout...", true), MenuItem("H3D/Import Layout...", true)]
    static bool ValidPrefab()
    {
        Object obj = EditorTool.GetCurrentSelectedAssetObj();

        return (obj != null ? PrefabUtility.GetPrefabType(obj) != PrefabType.None : false);
    }
}

 

