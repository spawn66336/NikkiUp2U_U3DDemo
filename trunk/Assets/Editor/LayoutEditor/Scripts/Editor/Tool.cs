using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Xml;

namespace H3DEditor
{
    public class EditorTool
    {
        static string s_Assets = "Assets/";

        public static Object GetCurrentSelectedAssetObj()
        {
            Object[] objs = Selection.GetFiltered(typeof(Object), SelectionMode.Assets);

            return (objs.Length == 1 ? objs[0] : null);
        }
        public static string GetCurrentSelectedAssetPath()
        {
            Object obj = GetCurrentSelectedAssetObj();

            return (obj != null ? AssetDatabase.GetAssetPath(obj) : "");
        }

        public static GameObject[] GetSubChildren(GameObject go)
        {
            GameObject[] children = new GameObject[go.transform.childCount];

            for (int i = 0; i < go.transform.childCount; ++i)
            {
                children[i] = go.transform.GetChild(i).gameObject;
            }
            return children;
        }
        public static List<GameObject> FindGameObjectByName(GameObject go, string name)
        {
            List<GameObject> list = new List<GameObject>();
            Stack<GameObject> stack = new Stack<GameObject>();

            stack.Push(go);
            while (stack.Count > 0)
            {
                GameObject cur_go = stack.Pop();
                GameObject[] children = EditorTool.GetSubChildren(cur_go);

                for (int i = 0; i < children.Length; ++i)
                {
                    stack.Push(children[i]);
                }
                if (cur_go.name.ToLower() == name.ToLower())
                {
                    list.Add(cur_go);
                }
            }

            return list;
        }

        public static string GetGameObjectFullPathName(GameObject obj, GameObject root)
        {
            if (obj == null || root == null)
                return "";

            string sName = obj.name;
            Transform parentTrans = obj.transform.parent;
            while(parentTrans != null)
            {
                sName = parentTrans.gameObject.name + "/" + sName;
                if (parentTrans == root.transform)
                    break;
                parentTrans = parentTrans.parent;
            }
            return sName;
        }

        public static GameObject FindGameObjectByPath(GameObject go, string path)
        {
            if (go == null)
            {
                return null;
            }

            string[] paths = path.Trim().Replace('\\', '/').Split('/');
            Transform res_trans = go.transform;

            for (int i = 0; i < paths.Length; ++i)
            {
                res_trans = res_trans.FindChild(paths[i]);
                if (res_trans == null)
                {
                    return null;
                }
            }
            return res_trans.gameObject;
        }

        public static bool ValidIndex(int index, int count)
        {
            return (index >= 0 && index < count);
        }
        public static int GetValidIndex(int index, int count)
        {
            return (index % count);
        }

        public static string SubString(string str, string begin, string end)
        {
            int begin_index = str.IndexOf(begin);
            if (begin_index == -1)
            {
                return str;
            }
            int end_index = str.IndexOf(end, begin_index + begin.Length);
            if (end_index == -1)
            {
                return str.Substring(begin_index + begin.Length);
            }
            else
            {
                return str.Substring(begin_index + begin.Length, end_index - begin_index - begin.Length);
            }
        }

        // 获得所在文件夹的名称
        public static string GetBaseFolder(string Path)
        {
            // xxx/xxxx/xxx
            int Index0 = Path.LastIndexOf('/');
            int Index1 = Path.LastIndexOf('/', Index0 - 1);
            return Path.Substring(Index1 + 1, Index0 - Index1 - 1);
        }
        // 获得文件夹的路径
        public static string GetFolder(string FilePath)
        {
            FilePath = FilePath.Replace('\\', '/');
            int DotIndex = FilePath.LastIndexOf('.');
            if (DotIndex != -1)
                return FilePath.Substring(0, FilePath.LastIndexOf('/')) + "/";
            return FilePath;
        }
        // 获得文件名
        public static string GetFileName(string FilePath, bool WithSuffix)
        {
            FilePath = FilePath.Replace('\\', '/');
            string FileName = FilePath.Substring(FilePath.LastIndexOf('/') + 1);
            if (!WithSuffix)
            {
                int DotIndex = FileName.LastIndexOf('.');
                if (DotIndex != -1)
                    FileName = FileName.Substring(0, DotIndex);
            }
            return FileName;
        }
        public static string GetProjectFullpath()
        {
            string data_folder = Application.dataPath + "/";

            return data_folder.Substring(0, data_folder.Length - s_Assets.Length);
        }
        public static string GetSystemFullpath(string AssetPath)
        {
            return Application.dataPath + "/" + GetUnityRelativePath(AssetPath);
        }
        public static void DeleteFolder(string Folder)
        {
            if (Directory.Exists(Folder))
                Directory.Delete(Folder, true);
        }
        public static string MakesureFolderExit(string FullPath)
        {
            FullPath = GetFolder(FullPath);
            DirectoryInfo Dir = new DirectoryInfo(FullPath);

            if (!Dir.Exists)
            {
                Dir.CreateSubdirectory(FullPath);
            }

            return Dir.FullName.Replace('\\', '/');
        }
        public static int FileWriteable(string filename)
        {
            MakesureFolderExit(filename);
            if (!File.Exists(filename))
                return -1;

            FileAttributes fatb = File.GetAttributes(filename);
            if ((fatb & FileAttributes.ReadOnly) != 0)
            {
                fatb &= ~FileAttributes.ReadOnly;
                File.SetAttributes(filename, fatb);
                return 1;
            }
            else
            {
                return 0;
            }
        }
        // 相对assets目录的路径，实现的不完全，确定没问题再用
        public static string GetUnityRelativePath(string Path)
        {
            Path = Path.Replace('\\', '/');
            if (!Path.Contains(".") && !Path.EndsWith("/")) Path += "/";
            int Index = Path.IndexOf(s_Assets, System.StringComparison.CurrentCultureIgnoreCase);
            if (Index != -1) return Path.Substring(Index + s_Assets.Length);
            return Path;
        }
    }

    public class MathTool
    {
        public static float s_knob_half_size = 4;

        // Liang-Barsky
        public static bool ClipLine(Rect rect, ref Vector3 point1, ref Vector3 point2)
        {
            float delta_x = point2.x - point1.x;
            float delta_y = point2.y - point1.y;
            float p = 0, q = 0, u = 0;
            float u1 = 0;
            float u2 = 1;

            for (int edge = 0; edge < 4; edge++)
            {   // Traverse through left, right, bottom, top edges.
                switch (edge)
                {
                    case 0:
                        p = -delta_x;
                        q = point1.x - rect.xMin;
                        break;
                    case 1:
                        p = delta_x;
                        q = rect.xMax - point1.x;
                        break;
                    case 2:
                        p = -delta_y;
                        q = point1.y - rect.yMin;
                        break;
                    case 3:
                        p = delta_y;
                        q = rect.yMax - point1.y;
                        break;
                }

                if (Mathf.Abs(p) < float.Epsilon)
                {
                    if (q < 0)
                    {
                        return false;   // Don't draw line at all. (parallel line outside)
                    }
                    else
                    {
                        continue;
                    }
                }
                u = q / p;

                if (p < 0)
                {
                    if (u > u2)
                    {
                        return false;         // Don't draw line at all.
                    }
                    else if (u > u1)
                    {
                        u1 = u;            // Line is clipped!
                    }
                }
                else if (p > 0)
                {
                    if (u < u1)
                    {
                        return false;      // Don't draw line at all.
                    }
                    else if (u < u2)
                    {
                        u2 = u;         // Line is clipped!
                    }
                }
            }

            float new_x1 = point1.x + u1 * delta_x;
            float new_y1 = point1.y + u1 * delta_y;
            float new_x2 = point1.x + u2 * delta_x;
            float new_y2 = point1.y + u2 * delta_y;

            point1.x = new_x1;
            point1.y = new_y1;
            point2.x = new_x2;
            point2.y = new_y2;

            return true;        // (clipped) line is drawn
        }
        public static bool ClipRect(Rect rect, Vector3[] rect_to_clip)
        {
            if (rect_to_clip.Length != 4)
            {
                return false;
            }

            float min_x = rect_to_clip[0].x;
            float max_x = rect_to_clip[2].x;
            float min_y = rect_to_clip[0].y;
            float max_y = rect_to_clip[2].y;

            if (min_x > rect.xMax || max_x < rect.xMin || min_y > rect.yMax || max_y < rect.yMin)
            {
                return false;
            }

            float new_min_x = Mathf.Max(min_x, rect.xMin);
            float new_max_x = Mathf.Min(max_x, rect.xMax);
            float new_min_y = Mathf.Max(min_y, rect.yMin);
            float new_max_y = Mathf.Min(max_y, rect.yMax);

            rect_to_clip[0].x = new_min_x;
            rect_to_clip[0].y = new_min_y;
            rect_to_clip[1].x = new_min_x;
            rect_to_clip[1].y = new_max_y;
            rect_to_clip[2].x = new_max_x;
            rect_to_clip[2].y = new_max_y;
            rect_to_clip[3].x = new_max_x;
            rect_to_clip[3].y = new_min_y;

            return true;
        }
        public static bool ClipRect(Rect rect, ref Rect rect_to_clip)
        {
            float min_x = rect_to_clip.xMin;
            float max_x = rect_to_clip.xMax;
            float min_y = rect_to_clip.yMin;
            float max_y = rect_to_clip.yMax;

            if (min_x > rect.xMax || max_x < rect.xMin || min_y > rect.yMax || max_y < rect.yMin)
            {
                return false;
            }

            rect_to_clip.xMin = Mathf.Max(min_x, rect.xMin);
            rect_to_clip.xMax = Mathf.Min(max_x, rect.xMax);
            rect_to_clip.yMin = Mathf.Max(min_y, rect.yMin);
            rect_to_clip.yMax = Mathf.Min(max_y, rect.yMax);

            return true;
        }

        public static bool IsConvexPolyIntersect(Vector3[] poly1, Vector3[] poly2)
        {
            for (int i = 0; i < poly1.Length; ++i)
            {
                if (IsPointInConvexPoly(poly1[i], poly2))
                {
                    return true;
                }
            }
            for (int i = 0; i < poly2.Length; ++i)
            {
                if (IsPointInConvexPoly(poly2[i], poly1))
                {
                    return true;
                }
            }
            for (int i = 0; i < poly1.Length; ++i)
            {
                // test line (poly1[i], poly1[(i + 1) % poly1.Length])
                for (int j = 0; j < poly2.Length; ++j)
                {
                    // test line (poly2[j], poly2[(j + 1) % poly2.Length])
                    if (IsLineIntersect(poly1[i], poly1[EditorTool.GetValidIndex((i + 1), poly1.Length)], poly2[j], poly2[EditorTool.GetValidIndex((j + 1), poly2.Length)]))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        public static bool IsPointInConvexPoly(Vector3 p, Vector3[] poly)
        {
            for (int i = 0; i < poly.Length; ++i)
            {
                float cwv = ClockWiseTest(poly[i], poly[EditorTool.GetValidIndex((i + 1), poly.Length)], poly[EditorTool.GetValidIndex((i + 2), poly.Length)]);

                if (cwv < float.Epsilon || ClockWiseTest(p, poly[i], poly[EditorTool.GetValidIndex((i + 1), poly.Length)]) * cwv < 0)
                {
                    return false;
                }
            }
            return true;
        }
        public static bool IsLineIntersect(Vector3 line1_p1, Vector3 line1_p2, Vector3 line2_p1, Vector3 line2_p2)
        {
            // line1的2点是否位于line2的同一边
            if (ClockWiseTest(line1_p1, line2_p1, line2_p2) * ClockWiseTest(line1_p2, line2_p1, line2_p2) > 0)
            {
                return false;
            }
            // line2的2点是否位于line1的同一边
            if (ClockWiseTest(line2_p1, line1_p1, line1_p2) * ClockWiseTest(line2_p2, line1_p1, line1_p2) > 0)
            {
                return false;
            }
            return true;
        }
        // 返回结果：>0 顺时针；<0 逆时针；==0 共线
        public static float ClockWiseTest(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            return ((p1.x - p2.x) * (p3.y - p2.y) - (p3.x - p2.x) * (p1.y - p2.y));
        }

        public static Vector3[] GetRect(Vector3 center, float half_size)
        {
            Vector3[] rect = new Vector3[4];

            rect[0] = new Vector3(center.x - half_size, center.y - half_size, 0);
            rect[1] = new Vector3(center.x - half_size, center.y + half_size, 0);
            rect[2] = new Vector3(center.x + half_size, center.y + half_size, 0);
            rect[3] = new Vector3(center.x + half_size, center.y - half_size, 0);

            return rect;
        }

        //比较两个矩形区域，忽略Z值。 1被2完全包含，返回-1， 1完全包含2，返回1，其他情况按点的包含个数决定
        public static int CompareRect(Vector3[] rc1, Vector3[] rc2)
        {
            if (rc1 == null || rc2 == null)
                return 0;
            if (rc1.Length != 4 || rc2.Length != 4)
                return 0;

            int n1In2 = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (IsPointInConvexPoly(rc1[i], rc2))
                    n1In2++;
            }
            if (n1In2 == 4)
                return -1;

            int n2In1 = 0;
            for (int i = 0; i < 4; ++i)
            {
                if (IsPointInConvexPoly(rc2[i], rc1))
                    n2In1++;
            }
            if (n2In1 == 4)
                return 1;

            return n2In1.CompareTo(n1In2);
        }
    }

    public class LayoutTool
    {
        static string s_prefab_postfix = ".prefab";
        static string s_layout_postfix = ".layout";
        static int s_editor_layer = 31;

        public static float s_editor_default_x = 100000;
        public static float s_editor_default_y = 100000;

        static GameObject s_root = null;

        public static GameObject Root
        {
            get { return CreateRoot(); }
        }
        public static void ReleaseRoot()
        {
            if (s_root != null)
            {
                Object.DestroyImmediate(s_root);
                s_root = null;
            }
        }
        static GameObject CreateRoot()
        {
            if (s_root == null)
            {
                s_root = new GameObject();
                s_root.hideFlags = HideFlags.HideAndDontSave;
                s_root.transform.localPosition = new Vector3(s_editor_default_x, s_editor_default_y);
                s_root.name = "EditorRoot";
                s_root.layer = s_editor_layer;
                s_root.AddComponent(typeof(CmdCounter));
            }

            return s_root;
        }

        public static bool IsRoot(UIElement ui_element)
        {
            if (ui_element == null)
            {
                return false;
            }
            return ui_element.transform.parent == Root.transform;
        }

        public static Camera CreateCamera()
        {
            return CreateCamera(false);
        }
        public static Camera CreateCamera(bool for_edit)
        {
            GameObject go = new GameObject();
            go.name = "Camera";
            if (for_edit)
            {
                go.transform.parent = Root.transform;
            }
            go.transform.localPosition = new Vector3(0, 0, -100);
            Camera camera = go.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.Color;
            camera.isOrthoGraphic = true;
            camera.far = 100000;
            camera.orthographicSize = ConfigTool.Instance.target_height / 2;
            camera.aspect = (float)ConfigTool.Instance.target_width / ConfigTool.Instance.target_height;
            //camera.cullingMask = 1 << s_editor_layer;

            return camera;
        }
        public static void ResetCamera(Camera camera)
        {
            if (camera != null)
            {
                camera.orthographicSize = ConfigTool.Instance.target_height / 2;
                camera.transform.localPosition = new Vector3(0, 0, camera.transform.localPosition.z);
            }
        }
        public static void ReleaseCamera(Camera camera)
        {
            if (camera != null)
            {
                Object.DestroyImmediate(camera.gameObject);
            }
        }

        public static Color DefaultBackColor
        {
            get
            {
                string[] colors = EditorTool.SubString(EditorPrefs.GetString("BackColor"), "(", ")").Split(',');

                if (colors.Length == 4)
                {
                    float r, g, b, a;
                    if (float.TryParse(colors[0], out r) && float.TryParse(colors[1], out g) && float.TryParse(colors[2], out b) && float.TryParse(colors[3], out a))
                    {
                        return new Color(r, g, b, a);
                    }
                }

                float gray = 71.0f / 255;

                return new Color(gray, gray, gray);
            }
        }

        public static GameObject CreateBackground(bool for_edit)
        {
            GameObject background = GameObject.CreatePrimitive(PrimitiveType.Plane);

            background.name = "Background";
            if (for_edit)
            {
                background.transform.parent = Root.transform;
            }
            background.layer = s_editor_layer;
            background.transform.localEulerAngles = new Vector3(90, 0, 180);
            background.transform.localPosition = new Vector3(0, 0, 100);
            background.transform.localScale = new Vector3(ConfigTool.Instance.target_width / 10, 1, ConfigTool.Instance.target_height / 10);
            background.renderer.sharedMaterial = new Material(Shader.Find("Unlit/Transparent Colored"));

            return background;
        }
        public static void ReleaseBackground(GameObject background)
        {
            if (background != null)
            {
                Object.DestroyImmediate(background);
            }
        }

        public static void SetCamera(GameObject go, Camera camera)
        {
            UIAnchor[] ui_anchors = go.GetComponentsInChildren<UIAnchor>(true);

            for (int i = 0; i < ui_anchors.Length; ++i)
            {
                ui_anchors[i].uiCamera = camera;
                ui_anchors[i].SendMessage("Start");
            }
        }
        public static bool RemoveNoUINode(GameObject go)
        {
            bool remove_sth = false;

            if (go != null)
            {
                GameObject[] children = EditorTool.GetSubChildren(go);
                for (int i = 0; i < children.Length; ++i)
                {
                    if (RemoveNoUINode(children[i]))
                    {
                        remove_sth = true;
                    }
                }
                bool has_ui = false;
                Component[] components = go.GetComponents<Component>();
                for (int i = 0; i < components.Length; ++i)
                {
                    if (components[i] != null)
                    {
                        if (components[i] is UIWidget)
                        {
                            has_ui = true;
                        }
                        else if (!(components[i] is Transform) && !(components[i] is UIPanel) && !(components[i] is Camera))
                        {
                            Object.DestroyImmediate(components[i]);
                            remove_sth = true;
                        }
                    }
                }
                if (!has_ui && go.transform.GetChildCount() == 0)
                {
                    Object.DestroyImmediate(go);
                    remove_sth = true;
                }
            }

            return remove_sth;
        }
        public static void RemoveInvalidNode(GameObject go)
        {
            if (go != null)
            {
                GameObject[] children = EditorTool.GetSubChildren(go);
                for (int i = 0; i < children.Length; ++i)
                {
                    UIElement ui_element = children[i].GetComponent<UIElement>();
                    if (ui_element != null)
                    {
                        if (ui_element.Removed)
                        {
                            Object.DestroyImmediate(ui_element.gameObject);
                        }
                        else
                        {
                            RemoveInvalidNode(ui_element.gameObject);
                        }
                    }
                }
            }
        }
        public static bool NeedResetPos(GameObject go)
        {
            if (go == null)
            {
                return false;
            }
            return !HasUI(go, false) && go.GetComponent<UIRoot>() != null;
        }
        public static bool HasUI(GameObject go)
        {
            return HasUI(go, true);
        }
        public static bool HasUI(GameObject go, bool recur)
        {
            if (go == null)
            {
                return false;
            }
            return (recur ? (go.GetComponentsInChildren<UIWidget>(true).Length > 0) : (go.GetComponent<UIWidget>() != null));
        }
        public static bool HasAnchorUI(GameObject go)
        {
            if (go == null)
            {
                return false;
            }
            UIAnchor[] anchors = go.GetComponentsInChildren<UIAnchor>(true);
            foreach (UIAnchor anchor in anchors)
            {
                if (HasUI(anchor.gameObject, false))
                {
                    return true;
                }
            }
            return false;
        }
        // 检测是否有重名UI元素，为每一个节点挂上UIElement
        public static bool ProcessBeforeExport(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            HashSet<string> name_dic = new HashSet<string>();
            HashSet<string> path_name_dic = new HashSet<string>();
            Stack<GameObject> stack = new Stack<GameObject>();

            stack.Push(go);
            while (stack.Count > 0)
            {
                GameObject cur_go = stack.Pop();
                GameObject[] children = EditorTool.GetSubChildren(cur_go);

                for (int i = 0; i < children.Length; ++i)
                {
                    stack.Push(children[i]);
                }

                string sPathName = "";
                if (HasUI(cur_go, false))
                {
                    sPathName = EditorTool.GetGameObjectFullPathName(cur_go, go);
                    
                    if (name_dic.Contains(cur_go.name))
                    {
                        if (path_name_dic.Contains(sPathName))
                        {
                            EditorUtility.DisplayDialog("", "节点名称\"" + cur_go.name + "\"不唯一，无法导出", "确定");
                            return false;
                        }
                        else
                        {
                            path_name_dic.Add(sPathName);
                        }
                    }
                    else
                    {
                        name_dic.Add(cur_go.name);
                        path_name_dic.Add(sPathName);
                    }
                }
                UIElement ui_element = cur_go.AddComponent<UIElement>();
                ui_element.Hide = !cur_go.activeSelf;
                UIWidget ui_widget = ui_element.GetWidget();
                ui_element.FullPathName = sPathName;
                SetWidgetSelfPivot(ui_widget, UIWidget.Pivot.BottomLeft);
            }

            return true;
        }
        //在导入layout之前检测一下，待导入的prefab中是否有重名物体（同级别下），并给与提示
        public static bool ProcessBeforeImport(GameObject go)
        {
            if (go == null)
            {
                return false;
            }

            HashSet<string> path_name_dic = new HashSet<string>();
            Stack<GameObject> stack = new Stack<GameObject>();
            stack.Push(go);
            while (stack.Count > 0)
            {
                GameObject cur_go = stack.Pop();
                GameObject[] children = EditorTool.GetSubChildren(cur_go);
                for (int i = 0; i < children.Length; ++i)
                {
                    stack.Push(children[i]);
                }

                if (HasUI(cur_go, false))
                {
                    string sPathName = EditorTool.GetGameObjectFullPathName(cur_go, go);
                    if (path_name_dic.Contains(sPathName))
                    {
                        EditorUtility.DisplayDialog("", "选中的Prefab中有重名ＵＩ节点\"" + cur_go.name + "\"请先处理", "确定");
                        return false;
                    }
                    else
                    {
                        path_name_dic.Add(sPathName);
                    }
                }
            }
            return true;
        }

        public static string GetTmpPrefabPath(string prefab)
        {
            string folder = EditorTool.GetFolder(prefab);
            string filename = EditorTool.GetFileName(prefab, false);
            int index = 1;
            string tmp_prefab = folder + filename + "_" + (index++).ToString() + s_prefab_postfix;

            while (File.Exists(tmp_prefab))
            {
                tmp_prefab = folder + filename + "_" + (index++).ToString() + s_prefab_postfix;
            }
            return tmp_prefab;
        }
        public static string GetLayoutFullPath(string prefab)
        {
            string fullpath = prefab.ToLower();

            if (!fullpath.StartsWith(ConfigTool.Instance.prefab_prefix.ToLower()))
            {
                Debug.LogError("Prefab路径\"" + prefab + "\"不规范");
                return "";
            }
            fullpath = fullpath.Substring(ConfigTool.Instance.prefab_prefix.Length);
            fullpath = ConfigTool.Instance.layout_prefix + fullpath;
            fullpath = EditorTool.GetProjectFullpath() + fullpath;
            fullpath = EditorTool.MakesureFolderExit(fullpath) + EditorTool.GetFileName(prefab, false) + s_layout_postfix;

            return fullpath;
        }
        public static string GetPrefabPath(string layout_fullpath)
        {
            string layout_fullpath_prefix = EditorTool.GetSystemFullpath(ConfigTool.Instance.prefab_prefix).ToLower();

            if (!layout_fullpath_prefix.EndsWith(ConfigTool.Instance.prefab_prefix.ToLower()))
            {
                Debug.LogError("Prefab路径前缀\"" + layout_fullpath_prefix + "\"有误");
                return "";
            }
            layout_fullpath_prefix = layout_fullpath_prefix.Substring(0, layout_fullpath_prefix.Length - ConfigTool.Instance.prefab_prefix.Length);
            layout_fullpath_prefix = EditorTool.MakesureFolderExit(layout_fullpath_prefix + ConfigTool.Instance.layout_prefix).ToLower();
            layout_fullpath = layout_fullpath.ToLower();
            if (!layout_fullpath.StartsWith(layout_fullpath_prefix))
            {
                Debug.LogError("Layout路径\"" + layout_fullpath + "\"有误");
                return "";
            }

            return ConfigTool.Instance.prefab_prefix + layout_fullpath.Substring(layout_fullpath_prefix.Length).Replace(s_layout_postfix, s_prefab_postfix);
        }

        public static string OpenBackImage()
        {
            string back_texture = EditorUtility.OpenFilePanel("请选择背景图", EditorPrefs.GetString("BackTextureFolder"), "png");

            if (File.Exists(back_texture))
            {
                EditorPrefs.SetString("BackTextureFolder", back_texture);
            }

            return back_texture;
        }
        public static string OpenLayoutDialog()
        {
            string layout = EditorUtility.OpenFilePanel("请选择Layout文件", EditorPrefs.GetString("LayoutFolder"), "layout");

            if (File.Exists(layout))
            {
                EditorPrefs.SetString("LayoutFolder", layout);
            }

            return layout;
        }
        public static GameObject LoadLayout(string layout_fullpath)
        {
            return LoadLayout(layout_fullpath, false);
        }
        public static GameObject LoadLayout(string layout_fullpath, bool for_edit)
        {
            string name = EditorTool.GetFileName(layout_fullpath, false);
            string prefab = LayoutTool.GetPrefabPath(layout_fullpath);

            if (string.IsNullOrEmpty(prefab))
            {
                return null;
            }

            string tmp_prefab = LayoutTool.GetTmpPrefabPath(prefab);
            string tmp_prefab_fullname = EditorTool.GetSystemFullpath(tmp_prefab);

            EditorTool.FileWriteable(tmp_prefab_fullname);
            File.Copy(layout_fullpath, tmp_prefab_fullname, true);
            AssetDatabase.Refresh();
            GameObject tmp_prefab_obj = Resources.LoadAssetAtPath(tmp_prefab, typeof(GameObject)) as GameObject;

            if (tmp_prefab_obj == null)
            {
                Debug.LogError("无法打开Layout文件\"" + layout_fullpath + "\"");
                FileUtil.DeleteFileOrDirectory(tmp_prefab);
                AssetDatabase.Refresh();
                return null;
            }
            GameObject prefab_obj = Object.Instantiate(tmp_prefab_obj) as GameObject;
            if (for_edit)
            {
                prefab_obj.transform.parent = Root.transform;
            }
            prefab_obj.transform.localPosition = tmp_prefab_obj.transform.position;
            prefab_obj.layer = s_editor_layer;
            prefab_obj.name = name;
            RemoveInvalidNode(prefab_obj);
            FileUtil.DeleteFileOrDirectory(tmp_prefab);
            AssetDatabase.Refresh();

            return prefab_obj;
        }
        public static bool SaveLayout(string layout_fullpath, GameObject inst)
        {
            string prefab = LayoutTool.GetPrefabPath(layout_fullpath);
            string tmp_prefab = LayoutTool.GetTmpPrefabPath(prefab);

            PrefabUtility.CreatePrefab(tmp_prefab, inst);
            AssetDatabase.SaveAssets();
            EditorTool.FileWriteable(layout_fullpath);
            File.Copy(EditorTool.GetSystemFullpath(tmp_prefab), layout_fullpath, true);
            FileUtil.DeleteFileOrDirectory(tmp_prefab);
            AssetDatabase.Refresh();

            return true;
        }

        public static void SetWidgetSelfPivot(UIWidget widget, UIWidget.Pivot pivot)
        {
            if (widget != null)
            {
                GameObject[] children = EditorTool.GetSubChildren(widget.gameObject);
                Vector3 org_pos = widget.transform.position;
                widget.pivot = pivot;
                Vector3 inv_pos_delta = (org_pos - widget.transform.position);
                for (int i = 0; i < children.Length; ++i)
                {
                    children[i].transform.Translate(inv_pos_delta);
                }
            }
        }
        public static void SetWidgetSelfPos(UIWidget widget, Vector3 pos)
        {
            if (widget != null)
            {
                GameObject[] children = EditorTool.GetSubChildren(widget.gameObject);
                Vector3 inv_pos_delta = (widget.transform.position - pos);

                widget.transform.position = pos;
                for (int i = 0; i < children.Length; ++i)
                {
                    children[i].transform.Translate(inv_pos_delta);
                }
            }
        }

        public static bool LoadWidgetInfo(GameObject go, UIWidget ui)
        {
            UIWidget go_ui = go.GetComponent<UIWidget>();

            if (go_ui == null || go_ui.GetType() != ui.GetType())
            {
                return false;
            }

            UIWidget.Pivot pivot = go_ui.pivot;

            SetWidgetSelfPivot(go_ui, ui.pivot);
            SetWidgetSelfPos(go_ui, ui.transform.position);
            go_ui.depth = ui.depth;
            go_ui.width = ui.width;
            go_ui.height = ui.height;
            if (go_ui is UISprite)
            {
                LoadSpecialInfo((UISprite)go_ui, (UISprite)ui);
            }
            else if (go_ui is UILabel)
            {
                LoadSpecialInfo((UILabel)go_ui, (UILabel)ui);
            }
            else if (go_ui is UITexture)
            {
                LoadSpecialInfo((UITexture)go_ui, (UITexture)ui);
            }
            SetWidgetSelfPivot(go_ui, pivot);

            return true;
        }
        private static void LoadSpecialInfo(UISprite go_ui, UISprite ui)
        {
            go_ui.atlas = ui.atlas;
            go_ui.spriteName = ui.spriteName;
            go_ui.type = ui.type;
        }
        private static void LoadSpecialInfo(UILabel go_ui, UILabel ui)
        {
            go_ui.text = ui.text;
            go_ui.effectStyle = ui.effectStyle;
            go_ui.effectDistance = ui.effectDistance;
            go_ui.maxLineCount = ui.maxLineCount;
        }
        private static void LoadSpecialInfo(UITexture go_ui, UITexture ui)
        {
            // 目前无编辑信息需导入
        }
    }

    public class ConfigTool
    {
        public string version = "";
        public int target_width = 1920, target_height = 1280;
        public string prefab_prefix = "Assets/";
        public string layout_prefix = "Resource/";
        public string help_url = "";

        static ConfigTool s_Instance = null;
        static string s_config_file = "Assets/Editor/LayoutEditor/Config/global_config.xml";
        static string back_texture = "Assets/Editor/LayoutEditor/Config/back_texture.png";

        public static ConfigTool Instance
        {
            get
            {
                if (null == s_Instance)
                {
                    s_Instance = new ConfigTool();
                    s_Instance.LoadConfig();
                }
                return s_Instance;
            }
        }

        public Texture2D GetBackTexture(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }

            string back_texture_fullname = EditorTool.GetSystemFullpath(back_texture);

            EditorTool.FileWriteable(back_texture_fullname);
            File.Copy(filename, back_texture_fullname, true);
            AssetDatabase.Refresh();

            return (Resources.LoadAssetAtPath(back_texture, typeof(Texture2D)) as Texture2D);
        }

        private void LoadConfig()
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(s_config_file);
            XmlNode root = doc.SelectSingleNode("GlobalConfig");
            if (root != null)
            {
                XmlNode version_node = root.SelectSingleNode("Version");
                if (version_node != null)
                {
                    version = version_node.InnerText;
                }
                XmlNode target_size_node = root.SelectSingleNode("TargetSize");
                if (target_size_node != null)
                {
                    int v = 0;
                    XmlNode width_node = target_size_node.SelectSingleNode("Width");
                    if (width_node != null)
                    {
                        int.TryParse(width_node.InnerText, out v);
                        if (v > 0)
                        {
                            target_width = v;
                        }
                    }
                    XmlNode height_node = target_size_node.SelectSingleNode("Height");
                    if (height_node != null)
                    {
                        int.TryParse(height_node.InnerText, out v);
                        if (v > 0)
                        {
                            target_height = v;
                        }
                    }
                }
                XmlNode layout_path_node = root.SelectSingleNode("LayoutPath");
                if (layout_path_node != null)
                {
                    XmlNode prefab_node = layout_path_node.SelectSingleNode("PrefabFolder");
                    if (prefab_node != null)
                    {
                        prefab_prefix = prefab_node.InnerText;
                    }
                    XmlNode layout_node = layout_path_node.SelectSingleNode("LayoutFolder");
                    if (layout_node != null)
                    {
                        layout_prefix = layout_node.InnerText;
                    }
                }
                XmlNode help_node = root.SelectSingleNode("Help");
                if (help_node != null)
                {
                    help_url = help_node.InnerText;
                }
            }
        }
    }
}
