using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;

public class UIInspector
{
    private static UISprite mSprite = null;
    private static UIWidget mWidget = null;
    private static UILabel mLabel = null;
    private static UITexture mTex = null;
    private static UIElement element = null;

    private static HOEditorUndoManager undoManager;

    static UIInspector()
    {
        undoManager = new HOEditorUndoManager(LayoutEditorWindow.Instance, "Inspector change");
    }

    public static void DrawUIProp(List<UIElement> elementList, Rect rect)
    {
        if (elementList.Count > 1 || elementList.Count == 0)
        {
            if (element != null)
            {
                LayoutEditorWindow.RequestRepaint();
                element = null;
            }
            return;
        }

        if (Event.current.type == EventType.MouseDown && !rect.Contains(Event.current.mousePosition))
            GUI.FocusControl("");

        if (element != elementList[0])
        {
            element = elementList[0];
            LayoutEditorWindow.RequestRepaint();
        }

        DrawPositionProp(element);


        NGUIEditorTools.SetLabelWidth(80f);
        if (element.GetWidget() != null)
        {
            mWidget = element.GetWidget();

            if (mWidget is UISprite)
            {
                mSprite = mWidget as UISprite;
                DrawUISpriteProp();
            }
            else if (mWidget is UILabel)
            {
                mLabel = mWidget as UILabel;
                DrawUILabelProp();
            }
            else if (mWidget is UITexture)
            {
                mTex = mWidget as UITexture;
                DrawUITextureProp();
            }
        }

        //EditorGUILayout.EndVertical();
    }

    private static void DrawUITextureProp()
    {
        EditorGUILayout.BeginVertical();
        /*
        if (mTex.material != null || mTex.mainTexture == null)
        {
            Material mat = EditorGUILayout.ObjectField("Material", mTex.material, typeof(Material), false) as Material;

            if (mTex.material != mat)
            {
                NGUIEditorTools.RegisterUndo("Material Selection", mTex);
                mTex.material = mat;
            }
        }

        if (mTex.material == null || mTex.hasDynamicMaterial)
        {
            Shader shader = EditorGUILayout.ObjectField("Shader", mTex.shader, typeof(Shader), false) as Shader;

            if (mTex.shader != shader)
            {
                NGUIEditorTools.RegisterUndo("Shader Selection", mTex);
                mTex.shader = shader;
            }

            Texture tex = EditorGUILayout.ObjectField("Texture", mTex.mainTexture, typeof(Texture), false) as Texture;

            if (mTex.mainTexture != tex)
            {
                NGUIEditorTools.RegisterUndo("Texture Selection", mTex);
                mTex.mainTexture = tex;
            }
        }

        if (mTex.mainTexture != null)
        {
            Rect rect = EditorGUILayout.RectField("UV Rectangle", mTex.uvRect);

            if (rect != mTex.uvRect)
            {
                NGUIEditorTools.RegisterUndo("UV Rectangle Change", mTex);
                mTex.uvRect = rect;
            }
        }
        if (mWidget.material != null)*/
        {

            DrawCommonProperties();
        }

        EditorGUILayout.EndVertical();
    }

    private static Rect previewRect = new Rect();
    private static void DrawUISpriteProp()
    {
        EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(true));
        EditorComSelector.Draw<UIAtlas>(mSprite.atlas, OnSelectAtlas);
        if (mSprite.atlas != null)
        {      
            AdvancedSpriteField(mSprite.atlas, mSprite.spriteName, SelectSprite, false); 
            UISpriteDrawExtraProperties();
            DrawCommonProperties();

            if (Selection.activeObject == mSprite.atlas.gameObject)
            {
                AtlasInspector.Instance.mAtlas = mSprite.atlas;

                AtlasInspector.Instance.OnInspectorGUI();

                // Render Preview
                GUILayout.Box("", GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));

                if (Event.current.type == EventType.repaint)
                    previewRect = GUILayoutUtility.GetLastRect();

                GUILayout.BeginArea(previewRect);
                AtlasInspector.Instance.OnPreviewGUI(new Rect(0, 0, previewRect.width, previewRect.height));
                GUILayout.EndArea();
            }
        }
        EditorGUILayout.EndVertical();
    }

    static string mEditedName = null;
    static string mLastSprite = null;
    static public void AdvancedSpriteField(UIAtlas atlas, string spriteName, SpriteSelector.Callback callback, bool editable,
        params GUILayoutOption[] options)
    {

#if NGUI_3_5_8
        if (atlas == null) return;

        // Give the user a warning if there are no sprites in the atlas
        if (atlas.spriteList.Count == 0)
        {
            EditorGUILayout.HelpBox("No sprites found", MessageType.Warning);
            return;
        }

        // Sprite selection drop-down list
        GUILayout.BeginHorizontal();
        {
            if (NGUIEditorTools.DrawPrefixButton("Sprite"))
            {
                NGUISettings.atlas = atlas;
                NGUISettings.selectedSprite = spriteName;
                SpriteSelector.Show(callback);
            }

            if (editable)
            {
                if (!string.Equals(spriteName, mLastSprite))
                {
                    mLastSprite = spriteName;
                    mEditedName = null;
                }

                string newName = GUILayout.TextField(string.IsNullOrEmpty(mEditedName) ? spriteName : mEditedName);

                if (newName != spriteName)
                {
                    mEditedName = newName;

                    if (GUILayout.Button("Rename", GUILayout.Width(60f)))
                    {
                        UISpriteData sprite = atlas.GetSprite(spriteName);

                        if (sprite != null)
                        {
                            NGUIEditorTools.RegisterUndo("Edit Sprite Name", atlas);
                            sprite.name = newName;

                            List<UISprite> sprites = FindAll<UISprite>();

                            for (int i = 0; i < sprites.Count; ++i)
                            {
                                UISprite sp = sprites[i];

                                if (sp.atlas == atlas && sp.spriteName == spriteName)
                                {
                                    NGUIEditorTools.RegisterUndo("Edit Sprite Name", sp);
                                    sp.spriteName = newName;
                                }
                            }

                            mLastSprite = newName;
                            spriteName = newName;
                            mEditedName = null;

                            NGUISettings.atlas = atlas;
                            NGUISettings.selectedSprite = spriteName;
                        }
                    }
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
                GUILayout.Space(18f);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("编辑", GUILayout.Width(40f)))
                {
                    NGUISettings.atlas = atlas;
                    NGUISettings.selectedSprite = spriteName;
                    Select(atlas.gameObject);
                }
            }
        }
        GUILayout.EndHorizontal();
#else
        // Give the user a warning if there are no sprites in the atlas
        if (atlas.spriteList.Count == 0)
        {
            EditorGUILayout.HelpBox("No sprites found", MessageType.Warning);
            return;
        }

        // Sprite selection drop-down list
        GUILayout.BeginHorizontal();
        {
            if (GUILayout.Button("Sprite", "DropDownButton", GUILayout.Width(76f)))
            {
                SpriteSelector.Show(atlas, spriteName, callback);
            }

            if (editable)
            {
                if (!string.Equals(spriteName, mLastSprite))
                {
                    mLastSprite = spriteName;
                    mEditedName = null;
                }

                string newName = GUILayout.TextField(string.IsNullOrEmpty(mEditedName) ? spriteName : mEditedName);

                if (newName != spriteName)
                {
                    mEditedName = newName;

                    if (GUILayout.Button("Rename", GUILayout.Width(60f)))
                    {
                        UIAtlas.Sprite sprite = atlas.GetSprite(spriteName);

                        if (sprite != null)
                        {
                            CmdManager.Instance.RegisterUndo(atlas, "Edit Sprite Name");
                            //NGUIEditorTools.RegisterUndo("Edit Sprite Name", atlas);
                            sprite.name = newName;

                            List<UISprite> sprites = FindAll<UISprite>();

                            for (int i = 0; i < sprites.Count; ++i)
                            {
                                UISprite sp = sprites[i];

                                if (sp.atlas == atlas && sp.spriteName == spriteName)
                                {
                                    CmdManager.Instance.RegisterUndo(sp, "Edit Sprite Name");
                                    //NGUIEditorTools.RegisterUndo("Edit Sprite Name", sp);
                                    sp.spriteName = newName;
                                }
                            }

                            mLastSprite = newName;
                            spriteName = newName;
                            mEditedName = null;

                            NGUISettings.selectedSprite = spriteName;
                        }
                    }
                }
            }
            else
            {
                GUILayout.BeginHorizontal();
                GUILayout.Label(spriteName, "HelpBox", GUILayout.Height(18f));
                GUILayout.Space(18f);
                GUILayout.EndHorizontal();

                if (GUILayout.Button("编辑", GUILayout.Width(40f)))
                {
                    NGUISettings.selectedSprite = spriteName;
                    Select(atlas.gameObject);
                }
            }
        }
        GUILayout.EndHorizontal();
#endif
    }

    static public List<T> FindAll<T>() where T : Component
    {
        T[] comps = Resources.FindObjectsOfTypeAll(typeof(T)) as T[];

        List<T> list = new List<T>();

        foreach (T comp in comps)
        {
            if (comp.gameObject.hideFlags == 0)
            {
                string path = AssetDatabase.GetAssetPath(comp.gameObject);
                if (string.IsNullOrEmpty(path)) list.Add(comp);
            }
        }
        return list;
    }

    static GameObject mPrevious;
    static public void Select(GameObject go)
    {
        mPrevious = Selection.activeGameObject;
        Selection.activeGameObject = go;
    }


    private static void DrawUILabelProp()
    {
        EditorGUILayout.BeginVertical();

        //ComponentSelector.Draw<UIFont>(mLabel.font, OnSelectFont);

        //if (mLabel.font != null)
        {
            GUI.skin.textArea.wordWrap = true;
            string text = string.IsNullOrEmpty(mLabel.text) ? "" : mLabel.text;


            undoManager.CheckUndo(new Object[] { mLabel.transform, mLabel }, "change label text");

            text = EditorGUILayout.TextArea(mLabel.text, GUI.skin.textArea, GUILayout.Height(100f));
            if (!text.Equals(mLabel.text)) { /*RegisterUndo();*/ mLabel.text = text; mLabel.transform.localPosition = mLabel.transform.localPosition; mLabel.MarkAsChanged(); }

            undoManager.CheckDirty(new Object[] { mLabel.transform, mLabel }, "change label text");

            /*
            UILabel.Overflow ov = (UILabel.Overflow)EditorGUILayout.EnumPopup("Overflow", mLabel.overflowMethod);
            if (ov != mLabel.overflowMethod) { RegisterUndo(); mLabel.overflowMethod = ov; }

            // Only input fields need this setting exposed, and they have their own "is password" setting, so hiding it here.
            //GUILayout.BeginHorizontal();
            //bool password = EditorGUILayout.Toggle("Password", mLabel.password, GUILayout.Width(100f));
            //GUILayout.Label("- hide characters");
            //GUILayout.EndHorizontal();
            //if (password != mLabel.password) { RegisterUndo(); mLabel.password = password; }

            GUILayout.BeginHorizontal();
            bool encoding = EditorGUILayout.Toggle("Encoding", mLabel.supportEncoding, GUILayout.Width(100f));
            GUILayout.Label("use emoticons and colors");
            GUILayout.EndHorizontal();
            if (encoding != mLabel.supportEncoding) { RegisterUndo(); mLabel.supportEncoding = encoding; }

            //GUILayout.EndHorizontal();

            if (encoding && mLabel.font.hasSymbols)
            {
                UIFont.SymbolStyle sym = (UIFont.SymbolStyle)EditorGUILayout.EnumPopup("Symbols", mLabel.symbolStyle, GUILayout.Width(170f));
                if (sym != mLabel.symbolStyle) { RegisterUndo(); mLabel.symbolStyle = sym; }
            }
            */
            GUILayout.BeginHorizontal();
            {
                UILabel.Effect effect = (UILabel.Effect)EditorGUILayout.EnumPopup("效果", mLabel.effectStyle, GUILayout.Width(170f));
                if (effect != mLabel.effectStyle) { RegisterUndo(); mLabel.effectStyle = effect; }

                if (effect != UILabel.Effect.None)
                {
                    Color c = EditorGUILayout.ColorField(mLabel.effectColor);
                    if (mLabel.effectColor != c) { RegisterUndo(); mLabel.effectColor = c; }
                }
            }
            GUILayout.EndHorizontal();

            if (mLabel.effectStyle != UILabel.Effect.None)
            {
#if UNITY_3_5 || UNITY_4_0 || UNITY_4_1 || UNITY_4_2
                GUILayout.Label("距离", GUILayout.Width(70f));
                GUILayout.Space(-34f);
                GUILayout.BeginHorizontal();
                GUILayout.Space(70f);
                Vector2 offset = EditorGUILayout.Vector2Field("", mLabel.effectDistance);
                GUILayout.Space(20f);
                GUILayout.EndHorizontal();
#else
				Vector2 offset = mLabel.effectDistance;

				GUILayout.BeginHorizontal();
				GUILayout.Label("Distance", GUILayout.Width(76f));
				offset.x = EditorGUILayout.FloatField(offset.x);
				offset.y = EditorGUILayout.FloatField(offset.y);
				GUILayout.Space(18f);
				GUILayout.EndHorizontal();
#endif
                if (offset != mLabel.effectDistance)
                {
                    RegisterUndo();
                    mLabel.effectDistance = offset;
                }
            }

            int count = EditorGUILayout.IntField("最大行数", mLabel.maxLineCount, GUILayout.Width(100f));
            if (count != mLabel.maxLineCount) { RegisterUndo(); mLabel.maxLineCount = count; }

            DrawCommonProperties();


        }
        //else
        //    EditorGUILayout.Space();

        EditorGUILayout.EndVertical();
    }

    static void OnSelectFont(MonoBehaviour obj)
    {
        if (mLabel != null)
        {
            CmdManager.Instance.RegisterUndo(mLabel, "Font Selection");
            //NGUIEditorTools.RegisterUndo("Font Selection", mLabel);
            bool resize = (mLabel.font == null);
            mLabel.font = obj as UIFont;
            if (resize) mLabel.MakePixelPerfect();
        }
    }

    static void RegisterUndo()
    {
        CmdManager.Instance.RegisterUndo(new Object[] { mLabel, mLabel.transform }, "Label Change");
        mLabel.transform.localPosition = mLabel.transform.localPosition;
        //CmdManager.Instance.RegisterUndo(mLabel, "Label Change");
        //NGUIEditorTools.RegisterUndo("Label Change", mLabel); 
    }

    static bool changed = false;
    private static void DrawPositionProp(UIElement element)
    {
        EditorGUILayout.BeginVertical(LayoutEditorGUIStyle.panelBox);

        GUI.changed = false;

        undoManager.CheckUndo(element.gameObject.transform, "change ui pos");
        if (H3DEditor.LayoutTool.IsRoot(element))
        {
            int x = EditorGUILayout.IntField("x", System.Convert.ToInt32(element.LocalPos.x + H3DEditor.ConfigTool.Instance.target_width / 2));
            int y = EditorGUILayout.IntField("y", System.Convert.ToInt32(element.LocalPos.y + H3DEditor.ConfigTool.Instance.target_height / 2));

            if (GUI.changed)
            {
                element.LocalPos = new Vector3(x - H3DEditor.ConfigTool.Instance.target_width / 2, y - H3DEditor.ConfigTool.Instance.target_height / 2, element.LocalPos.z);
            }
        }
        else
        {
            int x = EditorGUILayout.IntField("x", System.Convert.ToInt32(element.LocalPos.x));
            int y = EditorGUILayout.IntField("y", System.Convert.ToInt32(element.LocalPos.y));

            if (GUI.changed)
            {
                element.LocalPos = new Vector3(x, y, element.LocalPos.z);
            }
        }
        undoManager.CheckDirty(element.gameObject.transform, "change ui pos");

        EditorGUILayout.EndVertical();
    }

    static void OnSelectAtlas(MonoBehaviour obj)
    {
        if (mSprite != null)
        {
            CmdManager.Instance.RegisterUndo(mSprite, "Atlas Selection");
            //NGUIEditorTools.RegisterUndo("Atlas Selection", mSprite);
            mSprite.atlas = obj as UIAtlas;
            EditorUtility.SetDirty(mSprite.gameObject);
        }
    }

    static void SelectSprite(string spriteName)
    {
        if (mSprite != null && mSprite.spriteName != spriteName)
        {
            CmdManager.Instance.RegisterUndo(mSprite, "Sprite Change");
            //NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
            mSprite.spriteName = spriteName;
            EditorUtility.SetDirty(mSprite.gameObject);
            LayoutEditorWindow.RequestRepaint();
        }
    }

    static void UISpriteDrawExtraProperties()
    {
        GUILayout.Space(6f);


        //GUILayout.BeginHorizontal();
        UISprite.Type type = (UISprite.Type)EditorGUILayout.EnumPopup("Sprite类型", mSprite.type);
        //GUILayout.Label("sprite", GUILayout.Width(58f));
        //GUILayout.EndHorizontal();

        if (mSprite.type != type)
        {
            CmdManager.Instance.RegisterUndo(mSprite, "Sprite Change");
            //NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
            mSprite.type = type;
            EditorUtility.SetDirty(mSprite.gameObject);
        }

        if (mSprite.type == UISprite.Type.Sliced)
        {
            bool fill = EditorGUILayout.Toggle("Fill Center", mSprite.fillCenter);

            if (mSprite.fillCenter != fill)
            {
                CmdManager.Instance.RegisterUndo(mSprite, "Sprite Change");
                //NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
                mSprite.fillCenter = fill;
                EditorUtility.SetDirty(mSprite.gameObject);
            }
        }
        else if (mSprite.type == UISprite.Type.Filled)
        {
            if ((int)mSprite.fillDirection > (int)UISprite.FillDirection.Radial360)
            {
                mSprite.fillDirection = UISprite.FillDirection.Horizontal;
                EditorUtility.SetDirty(mSprite);
            }

            UISprite.FillDirection fillDirection = (UISprite.FillDirection)EditorGUILayout.EnumPopup("Fill Dir", mSprite.fillDirection);
            float fillAmount = EditorGUILayout.Slider("Fill Amount", mSprite.fillAmount, 0f, 1f);
            bool invert = EditorGUILayout.Toggle("Invert Fill", mSprite.invert);

            if (mSprite.fillDirection != fillDirection || mSprite.fillAmount != fillAmount || mSprite.invert != invert)
            {
                CmdManager.Instance.RegisterUndo(mSprite, "Sprite Change");
                //NGUIEditorTools.RegisterUndo("Sprite Change", mSprite);
                mSprite.fillDirection = fillDirection;
                mSprite.fillAmount = fillAmount;
                mSprite.invert = invert;
                EditorUtility.SetDirty(mSprite);
            }
        }
        GUILayout.Space(4f);
    }


    private static void DrawCommonProperties()
    {
        PrefabType type = PrefabUtility.GetPrefabType(mWidget.gameObject);

        if (NGUIEditorTools.DrawHeader("Widget"))
        {
            NGUIEditorTools.BeginContents();
            /*
            // Color tint
            GUILayout.BeginHorizontal();
            Color color = EditorGUILayout.ColorField("色调", mWidget.color);
            if (GUILayout.Button("复制", GUILayout.Width(50f)))
                NGUISettings.color = color;
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            NGUISettings.color = EditorGUILayout.ColorField("剪贴板", NGUISettings.color);
            if (GUILayout.Button("粘贴", GUILayout.Width(50f)))
                color = NGUISettings.color;
            GUILayout.EndHorizontal();

            if (mWidget.color != color)
            {
                NGUIEditorTools.RegisterUndo("Color Change", mWidget);
                mWidget.color = color;
            }

            GUILayout.Space(6f);

#if UNITY_3_5
			// Pivot point -- old school drop-down style
			UIWidget.Pivot pivot = (UIWidget.Pivot)EditorGUILayout.EnumPopup("Pivot", mWidget.pivot);

			if (mWidget.pivot != pivot)
			{
				NGUIEditorTools.RegisterUndo("Pivot Change", mWidget);
				mWidget.pivot = pivot;
			}
#else
            // Pivot point -- the new, more visual style
            GUILayout.BeginHorizontal();
            GUILayout.Label("中心点", GUILayout.Width(76f));
            Toggle("\u25C4", "ButtonLeft", UIWidget.Pivot.Left, true);
            Toggle("\u25AC", "ButtonMid", UIWidget.Pivot.Center, true);
            Toggle("\u25BA", "ButtonRight", UIWidget.Pivot.Right, true);
            Toggle("\u25B2", "ButtonLeft", UIWidget.Pivot.Top, false);
            Toggle("\u258C", "ButtonMid", UIWidget.Pivot.Center, false);
            Toggle("\u25BC", "ButtonRight", UIWidget.Pivot.Bottom, false);
            GUILayout.EndHorizontal();
#endif
            */
            // Depth navigation

            if (type != PrefabType.Prefab)
            {
                GUILayout.Space(2f);
                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.PrefixLabel("深度");

                    int depth = mWidget.depth;
                    if (GUILayout.Button("后移", GUILayout.Width(60f))) --depth;
                    depth = EditorGUILayout.IntField(depth, GUILayout.MinWidth(20f));
                    if (GUILayout.Button("前移", GUILayout.Width(60f))) ++depth;

                    if (mWidget.depth != depth)
                    {
                        CmdManager.Instance.RegisterUndo(new Object[] { mWidget, mWidget.transform }, "Depth Change");
                        //CmdManager.Instance.RegisterUndo(mWidget, "Depth Change");
                        //NGUIEditorTools.RegisterUndo("Depth Change", mWidget);
                        mWidget.depth = depth;
                        mWidget.transform.localPosition = mWidget.transform.localPosition;
                        //mWidget.MarkAsChanged();
                        //UIPanel.SetDirty();
                        //CmdManager.Instance.RegisterUndoEnd();
                    }
                }
                GUILayout.EndHorizontal();

                int matchingDepths = 0;

#if NGUI_3_5_8
                foreach( var p in UIPanel.list )
                {
                    foreach( var w in p.widgets )
                    {
                        if (w != null && w.depth == mWidget.depth)
                            ++matchingDepths;
                    }
                }
#else
                for (int i = 0; i < UIWidget.list.size; ++i)
                {
                    UIWidget w = UIWidget.list[i];
                    if (w != null && w.depth == mWidget.depth)
                        ++matchingDepths;
                }
#endif

                if (matchingDepths > 1)
                {
                    EditorGUILayout.HelpBox(matchingDepths + " 个 widget 正在使用同一个深度值 " + mWidget.depth +
                        ". 这将导致这些 widget 的前后关系不明确.", MessageType.Warning);
                }
            }

            GUI.changed = false;
            GUILayout.BeginHorizontal();

            undoManager.CheckUndo(new Object[] { mWidget.transform, mWidget }, "change uiwidget size");

            int width = EditorGUILayout.IntField("尺寸", mWidget.width, GUILayout.Width(128f));
            NGUIEditorTools.SetLabelWidth(12f);
            int height = EditorGUILayout.IntField("x", mWidget.height, GUILayout.MinWidth(30f));
            NGUIEditorTools.SetLabelWidth(80f);

            if (GUI.changed)
            {
                //CmdManager.Instance.RegisterUndo(mWidget, "Widget Change");
                //NGUIEditorTools.RegisterUndo("Widget Change", mWidget);
                mWidget.width = width;
                mWidget.height = height;
                mWidget.transform.localPosition = mWidget.transform.localPosition;
                //mWidget.transform.position = mWidget.transform.position;
            }

            undoManager.CheckDirty(new Object[] { mWidget.transform, mWidget }, "change uiwidget size");

            if (type != PrefabType.Prefab)
            {
                if (GUILayout.Button("贴图大小", GUILayout.Width(68f)))
                {
                    Object[] objs = new Object[2] { mWidget, mWidget.transform };
                    CmdManager.Instance.RegisterUndo(objs, "Widget Change");
                    //CmdManager.Instance.RegisterUndo(mWidget.transform, "Make Pixel-Perfect");
                    //NGUIEditorTools.RegisterUndo("Widget Change", mWidget);
                    //NGUIEditorTools.RegisterUndo("Make Pixel-Perfect", mWidget.transform);
                    mWidget.MakePixelPerfect();
                }
            }
            else
            {
                GUILayout.Space(70f);
            }
            GUILayout.EndHorizontal();
            NGUIEditorTools.EndContents();
        }
    }

    static void Toggle(string text, string style, UIWidget.Pivot pivot, bool isHorizontal)
    {
        bool isActive = false;

        switch (pivot)
        {
            case UIWidget.Pivot.Left:
                isActive = IsLeft(mWidget.pivot);
                break;

            case UIWidget.Pivot.Right:
                isActive = IsRight(mWidget.pivot);
                break;

            case UIWidget.Pivot.Top:
                isActive = IsTop(mWidget.pivot);
                break;

            case UIWidget.Pivot.Bottom:
                isActive = IsBottom(mWidget.pivot);
                break;

            case UIWidget.Pivot.Center:
                isActive = isHorizontal ? pivot == GetHorizontal(mWidget.pivot) : pivot == GetVertical(mWidget.pivot);
                break;
        }

        if (GUILayout.Toggle(isActive, text, style) != isActive)
            SetPivot(pivot, isHorizontal);
    }

    static bool IsLeft(UIWidget.Pivot pivot)
    {
        return pivot == UIWidget.Pivot.Left ||
            pivot == UIWidget.Pivot.TopLeft ||
            pivot == UIWidget.Pivot.BottomLeft;
    }

    static bool IsRight(UIWidget.Pivot pivot)
    {
        return pivot == UIWidget.Pivot.Right ||
            pivot == UIWidget.Pivot.TopRight ||
            pivot == UIWidget.Pivot.BottomRight;
    }

    static bool IsTop(UIWidget.Pivot pivot)
    {
        return pivot == UIWidget.Pivot.Top ||
            pivot == UIWidget.Pivot.TopLeft ||
            pivot == UIWidget.Pivot.TopRight;
    }

    static bool IsBottom(UIWidget.Pivot pivot)
    {
        return pivot == UIWidget.Pivot.Bottom ||
            pivot == UIWidget.Pivot.BottomLeft ||
            pivot == UIWidget.Pivot.BottomRight;
    }

    static UIWidget.Pivot GetHorizontal(UIWidget.Pivot pivot)
    {
        if (IsLeft(pivot)) return UIWidget.Pivot.Left;
        if (IsRight(pivot)) return UIWidget.Pivot.Right;
        return UIWidget.Pivot.Center;
    }

    static UIWidget.Pivot GetVertical(UIWidget.Pivot pivot)
    {
        if (IsTop(pivot)) return UIWidget.Pivot.Top;
        if (IsBottom(pivot)) return UIWidget.Pivot.Bottom;
        return UIWidget.Pivot.Center;
    }

    static void SetPivot(UIWidget.Pivot pivot, bool isHorizontal)
    {
        UIWidget.Pivot horizontal = GetHorizontal(mWidget.pivot);
        UIWidget.Pivot vertical = GetVertical(mWidget.pivot);

        pivot = isHorizontal ? Combine(pivot, vertical) : Combine(horizontal, pivot);

        if (mWidget.pivot != pivot)
        {
            CmdManager.Instance.RegisterUndo(mWidget, "Pivot change");
            //NGUIEditorTools.RegisterUndo("Pivot change", mWidget);
            mWidget.pivot = pivot;
        }
    }

    static UIWidget.Pivot Combine(UIWidget.Pivot horizontal, UIWidget.Pivot vertical)
    {
        if (horizontal == UIWidget.Pivot.Left)
        {
            if (vertical == UIWidget.Pivot.Top) return UIWidget.Pivot.TopLeft;
            if (vertical == UIWidget.Pivot.Bottom) return UIWidget.Pivot.BottomLeft;
            return UIWidget.Pivot.Left;
        }

        if (horizontal == UIWidget.Pivot.Right)
        {
            if (vertical == UIWidget.Pivot.Top) return UIWidget.Pivot.TopRight;
            if (vertical == UIWidget.Pivot.Bottom) return UIWidget.Pivot.BottomRight;
            return UIWidget.Pivot.Right;
        }
        return vertical;
    }
}

public class AtlasInspector
{
    private static AtlasInspector _Instance = null;
    static AtlasInspector()
    {
        _Instance = new AtlasInspector();
    }

    public static AtlasInspector Instance { get { return _Instance; } }

    enum AtlasType
    {
        Normal,
        Reference,
    }

    public UIAtlas mAtlas;
    AtlasType mType = AtlasType.Normal;
    UIAtlas mReplacement = null;


    void MarkSpriteAsDirty()
    {
#if NGUI_3_5_8
        UISpriteData sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
        if (sprite == null) return;

        UISprite[] sprites = NGUITools.FindActive<UISprite>();

        foreach (UISprite sp in sprites)
        {
            if (UIAtlas.CheckIfRelated(sp.atlas, mAtlas) && sp.spriteName == sprite.name)
            {
                UIAtlas atl = sp.atlas;
                sp.atlas = null;
                sp.atlas = atl;
                EditorUtility.SetDirty(sp);
            }
        }

        UILabel[] labels = NGUITools.FindActive<UILabel>();

        foreach (UILabel lbl in labels)
        {
            if (lbl.bitmapFont != null && UIAtlas.CheckIfRelated(lbl.bitmapFont.atlas, mAtlas) && lbl.bitmapFont.UsesSprite(sprite.name))
            {
                UIFont font = lbl.bitmapFont;
                lbl.bitmapFont = null;
                lbl.bitmapFont = font;
                EditorUtility.SetDirty(lbl);
            }
        }
#else
        UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
        if (sprite == null) return;

        UISprite[] sprites = NGUITools.FindActive<UISprite>();

        foreach (UISprite sp in sprites)
        {
            if (sp.spriteName == sprite.name)
            {
                sp.atlas = null;
                sp.atlas = mAtlas;
                EditorUtility.SetDirty(sp);
            }
        }

        UILabel[] labels = NGUITools.FindActive<UILabel>();

        foreach (UILabel lbl in labels)
        {
            if (lbl.font != null && UIAtlas.CheckIfRelated(lbl.font.atlas, mAtlas) && lbl.font.UsesSprite(sprite.name))
            {
                UIFont font = lbl.font;
                lbl.font = null;
                lbl.font = font;
                EditorUtility.SetDirty(lbl);
            }
        }
#endif
    }

    /// <summary>
    /// Replacement atlas selection callback.
    /// </summary>
    /// 

#if NGUI_3_5_8
    void OnSelectAtlas(Object obj)
    {
        if (mReplacement != obj)
        {
            // Undo doesn't work correctly in this case... so I won't bother.
            //NGUIEditorTools.RegisterUndo("Atlas Change");
            //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

            mAtlas.replacement = obj as UIAtlas;
            mReplacement = mAtlas.replacement;
            NGUITools.SetDirty(mAtlas);
            if (mReplacement == null) mType = AtlasType.Normal;
        }
    }
#else 
    void OnSelectAtlas(MonoBehaviour obj)
    {
        if (mReplacement != obj)
        {
            // Undo doesn't work correctly in this case... so I won't bother.
            //NGUIEditorTools.RegisterUndo("Atlas Change");
            //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

            mAtlas.replacement = obj as UIAtlas;
            mReplacement = mAtlas.replacement;
            UnityEditor.EditorUtility.SetDirty(mAtlas);
            if (mReplacement == null) mType = AtlasType.Normal;
        }
    }
#endif

    /// <summary>
    /// Draw the inspector widget.
    /// </summary>

    public void OnInspectorGUI()
    {
        NGUIEditorTools.SetLabelWidth(80f);

#if NGUI_3_5_8 
        UISpriteData sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
#else
        UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
#endif

        GUILayout.Space(6f);

        if (mAtlas.replacement != null)
        {
            mType = AtlasType.Reference;
            mReplacement = mAtlas.replacement;
        }

        GUILayout.BeginHorizontal();
        AtlasType after = (AtlasType)EditorGUILayout.EnumPopup("Atlas类型", mType);
        GUILayout.Space(18f);
        GUILayout.EndHorizontal();

        if (mType != after)
        {
            if (after == AtlasType.Normal)
            {
                mType = AtlasType.Normal;
                OnSelectAtlas(null);
            }
            else
            {
                mType = AtlasType.Reference;
            }
        }

        if (mType == AtlasType.Reference)
        {
#if NGUI_3_5_8
            ComponentSelector.Draw<UIAtlas>(mAtlas.replacement, OnSelectAtlas, true);
#else
            ComponentSelector.Draw<UIAtlas>(mAtlas.replacement, OnSelectAtlas);
#endif

            GUILayout.Space(6f);
            EditorGUILayout.HelpBox("你可以使当前的Atlas引用一个已经存在的Atlas，这样便于统一管理。例如，将低分辨率贴图替换为高分辨率贴图，或将英文贴图替换为中文贴图。所有引用的Atlas都会在对应被引用的Atlas发生改变时自动更新。"/* +
                "You can have one atlas simply point to " +
                "another one. This is useful if you want to be " +
                "able to quickly replace the contents of one " +
                "atlas with another one, for example for " +
                "swapping an SD atlas with an HD one, or " +
                "replacing an English atlas with a Chinese " +
                "one. All the sprites referencing this atlas " +
                "will update their references to the new one."*/, MessageType.Info);

            if (mReplacement != mAtlas && mAtlas.replacement != mReplacement)
            {
                CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
                //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                mAtlas.replacement = mReplacement;
                UnityEditor.EditorUtility.SetDirty(mAtlas);
            }
            return;
        }

        //GUILayout.Space(6f);
        Material mat = EditorGUILayout.ObjectField("材质", mAtlas.spriteMaterial, typeof(Material), false) as Material;

        if (mAtlas.spriteMaterial != mat)
        {
            CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
            //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
            mAtlas.spriteMaterial = mat;

            // Ensure that this atlas has valid import settings
            if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false, !mAtlas.premultipliedAlpha);
#if NGUI_3_5_8
            mAtlas.MarkAsChanged();      
#else
            mAtlas.MarkAsDirty();
#endif
        }

        if (mat != null)
        {
            TextAsset ta = EditorGUILayout.ObjectField("纹理包导入", null, typeof(TextAsset), false) as TextAsset;

            if (ta != null)
            {
                // Ensure that this atlas has valid import settings
                if (mAtlas.texture != null) NGUIEditorTools.ImportTexture(mAtlas.texture, false, false, !mAtlas.premultipliedAlpha);

                CmdManager.Instance.RegisterUndo(mAtlas, "Import Sprites");
                //NGUIEditorTools.RegisterUndo("Import Sprites", mAtlas);
                NGUIJson.LoadSpriteData(mAtlas, ta);
                if (sprite != null) sprite = mAtlas.GetSprite(sprite.name);
#if NGUI_3_5_8
                mAtlas.MarkAsChanged();
#else
                mAtlas.MarkAsDirty();
#endif
            }

#if !NGUI_3_5_8
            GUILayout.BeginHorizontal();
            UIAtlas.Coordinates coords = (UIAtlas.Coordinates)EditorGUILayout.EnumPopup("坐标", mAtlas.coordinates);
            GUILayout.Space(18f);
            GUILayout.EndHorizontal();

            if (coords != mAtlas.coordinates)
            {
                CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
                //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                mAtlas.coordinates = coords;
            }
#endif

            float pixelSize = EditorGUILayout.FloatField("像素大小", mAtlas.pixelSize, GUILayout.Width(120f));

            if (pixelSize != mAtlas.pixelSize)
            {
                CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
                //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                mAtlas.pixelSize = pixelSize;
            }
        }

        if (mAtlas.spriteMaterial != null)
        {
            Color blue = new Color(0f, 0.7f, 1f, 1f);
            Color green = new Color(0.4f, 1f, 0f, 1f);

            if (sprite == null && mAtlas.spriteList.Count > 0)
            {
                string spriteName = NGUISettings.selectedSprite;
                if (!string.IsNullOrEmpty(spriteName)) sprite = mAtlas.GetSprite(spriteName);
                if (sprite == null) sprite = mAtlas.spriteList[0];
            }

            if (sprite != null)
            {
                if (sprite == null) return;

                Texture2D tex = mAtlas.spriteMaterial.mainTexture as Texture2D;

                if (tex != null)
                {
                    if (!NGUIEditorTools.DrawHeader("Sprite Details")) return;

                    NGUIEditorTools.BeginContents();

                    GUILayout.Space(3f);
#if NGUI_3_5_8
                    NGUIEditorTools.DrawAdvancedSpriteField(mAtlas, sprite.name, SelectSprite, true);              
#else
                    NGUIEditorTools.AdvancedSpriteField(mAtlas, sprite.name, SelectSprite, true);
#endif
                    GUILayout.Space(6f);

#if NGUI_3_5_8
                    GUI.changed = false;

                    GUI.backgroundColor = green;
                    NGUIEditorTools.IntVector sizeA = NGUIEditorTools.IntPair("尺寸", "X", "Y", sprite.x, sprite.y);
                    NGUIEditorTools.IntVector sizeB = NGUIEditorTools.IntPair(null, "Width", "Height", sprite.width, sprite.height);

                    EditorGUILayout.Separator();
                    GUI.backgroundColor = blue;
                    NGUIEditorTools.IntVector borderA = NGUIEditorTools.IntPair("边框", "Left", "Right", sprite.borderLeft, sprite.borderRight);
                    NGUIEditorTools.IntVector borderB = NGUIEditorTools.IntPair(null, "Bottom", "Top", sprite.borderBottom, sprite.borderTop);

                    EditorGUILayout.Separator();
                    GUI.backgroundColor = Color.white;
                    NGUIEditorTools.IntVector padA = NGUIEditorTools.IntPair("边距", "Left", "Right", sprite.paddingLeft, sprite.paddingRight);
                    NGUIEditorTools.IntVector padB = NGUIEditorTools.IntPair(null, "Bottom", "Top", sprite.paddingBottom, sprite.paddingTop);

                    if (GUI.changed)
                    {
                        NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);

                        sprite.x = sizeA.x;
                        sprite.y = sizeA.y;
                        sprite.width = sizeB.x;
                        sprite.height = sizeB.y;

                        sprite.paddingLeft = padA.x;
                        sprite.paddingRight = padA.y;
                        sprite.paddingBottom = padB.x;
                        sprite.paddingTop = padB.y;

                        sprite.borderLeft = borderA.x;
                        sprite.borderRight = borderA.y;
                        sprite.borderBottom = borderB.x;
                        sprite.borderTop = borderB.y;

                        MarkSpriteAsDirty();
                    }

                    GUILayout.Space(3f);

                    GUILayout.BeginHorizontal();

   
                    GUILayout.EndHorizontal();
                    NGUIEditorTools.EndContents();           
#else

                    Rect inner = sprite.inner;
                    Rect outer = sprite.outer;

                    if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                    {
                        GUI.backgroundColor = green;
                        outer = NGUIEditorTools.IntRect("尺寸", sprite.outer);

                        Vector4 border = new Vector4(
                            sprite.inner.xMin - sprite.outer.xMin,
                            sprite.inner.yMin - sprite.outer.yMin,
                            sprite.outer.xMax - sprite.inner.xMax,
                            sprite.outer.yMax - sprite.inner.yMax);

                        GUI.backgroundColor = blue;
                        border = NGUIEditorTools.IntPadding("边框", border);
                        GUI.backgroundColor = Color.white;

                        inner.xMin = sprite.outer.xMin + border.x;
                        inner.yMin = sprite.outer.yMin + border.y;
                        inner.xMax = sprite.outer.xMax - border.z;
                        inner.yMax = sprite.outer.yMax - border.w;
                    }
                    else
                    {
                        // Draw the inner and outer rectangle dimensions
                        GUI.backgroundColor = green;
                        outer = EditorGUILayout.RectField("Outer Rect", sprite.outer);
                        GUI.backgroundColor = blue;
                        inner = EditorGUILayout.RectField("Inner Rect", sprite.inner);
                        GUI.backgroundColor = Color.white;
                    }

                    if (outer.xMax < outer.xMin) outer.xMax = outer.xMin;
                    if (outer.yMax < outer.yMin) outer.yMax = outer.yMin;

                    if (outer != sprite.outer)
                    {
                        float x = outer.xMin - sprite.outer.xMin;
                        float y = outer.yMin - sprite.outer.yMin;

                        inner.x += x;
                        inner.y += y;
                    }

                    // Sanity checks to ensure that the inner rect is always inside the outer
                    inner.xMin = Mathf.Clamp(inner.xMin, outer.xMin, outer.xMax);
                    inner.xMax = Mathf.Clamp(inner.xMax, outer.xMin, outer.xMax);
                    inner.yMin = Mathf.Clamp(inner.yMin, outer.yMin, outer.yMax);
                    inner.yMax = Mathf.Clamp(inner.yMax, outer.yMin, outer.yMax);

                    bool changed = false;

                    if (sprite.inner != inner || sprite.outer != outer)
                    {
                        CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
                        //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                        sprite.inner = inner;
                        sprite.outer = outer;
                        MarkSpriteAsDirty();
                        changed = true;
                    }

                    EditorGUILayout.Separator();

                    if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                    {
                        int left = Mathf.RoundToInt(sprite.paddingLeft * sprite.outer.width);
                        int right = Mathf.RoundToInt(sprite.paddingRight * sprite.outer.width);
                        int top = Mathf.RoundToInt(sprite.paddingTop * sprite.outer.height);
                        int bottom = Mathf.RoundToInt(sprite.paddingBottom * sprite.outer.height);

                        NGUIEditorTools.IntVector a = NGUIEditorTools.IntPair("边距", "Left", "Top", left, top);
                        NGUIEditorTools.IntVector b = NGUIEditorTools.IntPair(null, "Right", "Bottom", right, bottom);

                        if (changed || a.x != left || a.y != top || b.x != right || b.y != bottom)
                        {
                            CmdManager.Instance.RegisterUndo(mAtlas, "Atlas Change");
                            //NGUIEditorTools.RegisterUndo("Atlas Change", mAtlas);
                            sprite.paddingLeft = a.x / sprite.outer.width;
                            sprite.paddingTop = a.y / sprite.outer.height;
                            sprite.paddingRight = b.x / sprite.outer.width;
                            sprite.paddingBottom = b.y / sprite.outer.height;
                            MarkSpriteAsDirty();
                        }
                    }
                    else
                    {
                        // Create a button that can make the coordinates pixel-perfect on click
                        GUILayout.BeginHorizontal();
                        {
                            GUILayout.Label("Correction", GUILayout.Width(75f));

                            Rect corrected0 = outer;
                            Rect corrected1 = inner;

                            if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
                            {
                                corrected0 = NGUIMath.MakePixelPerfect(corrected0);
                                corrected1 = NGUIMath.MakePixelPerfect(corrected1);
                            }
                            else
                            {
                                corrected0 = NGUIMath.MakePixelPerfect(corrected0, tex.width, tex.height);
                                corrected1 = NGUIMath.MakePixelPerfect(corrected1, tex.width, tex.height);
                            }

                            if (corrected0 == sprite.outer && corrected1 == sprite.inner)
                            {
                                GUI.color = Color.grey;
                                GUILayout.Button("Make Pixel-Perfect");
                                GUI.color = Color.white;
                            }
                            else if (GUILayout.Button("Make Pixel-Perfect"))
                            {
                                outer = corrected0;
                                inner = corrected1;
                                GUI.changed = true;
                            }
                        }
                        GUILayout.EndHorizontal();
                    }
                    NGUIEditorTools.EndContents();
#endif
                }

                if (NGUIEditorTools.previousSelection != null)
                {
                    GUILayout.Space(3f);
                    GUI.backgroundColor = Color.green;

                    if (GUILayout.Button("<< Return to " + NGUIEditorTools.previousSelection.name))
                    {
                        NGUIEditorTools.SelectPrevious();
                    }
                    GUI.backgroundColor = Color.white;
                }
            }
        }
    }

    /// <summary>
    /// Sprite selection callback.
    /// </summary>

    void SelectSprite(string spriteName)
    {
        NGUISettings.selectedSprite = spriteName;
        LayoutEditorWindow.RequestRepaint();
    }

    /// <summary>
    /// Draw the sprite preview.
    /// </summary>

    public void OnPreviewGUI(Rect rect)
    {
#if NGUI_3_5_8   
        Texture2D tex = mAtlas.texture as Texture2D;
        if (tex == null) return;

        UISpriteData sd = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
        NGUIEditorTools.DrawSprite(tex, rect, sd, Color.white);
#else
        UIAtlas.Sprite sprite = (mAtlas != null) ? mAtlas.GetSprite(NGUISettings.selectedSprite) : null;
        if (sprite == null) return;

        Texture2D tex = mAtlas.texture as Texture2D;
        if (tex == null) return;

        Rect outer = new Rect(sprite.outer);
        Rect inner = new Rect(sprite.inner);
        Rect uv = outer;

        if (mAtlas.coordinates == UIAtlas.Coordinates.Pixels)
        {
            uv = NGUIMath.ConvertToTexCoords(outer, tex.width, tex.height);
        }
        else
        {
            outer = NGUIMath.ConvertToPixels(outer, tex.width, tex.height, true);
            inner = NGUIMath.ConvertToPixels(inner, tex.width, tex.height, true);
        }
        NGUIEditorTools.DrawSprite(tex, rect, outer, inner, uv, Color.white);
#endif
    }
}

public class HOEditorUndoManager
{

    // VARS ///////////////////////////////////////////////////

    private Object defTarget;
    private string defName;
    private bool autoSetDirty;
    private bool listeningForGuiChanges;
    private bool isMouseDown;
    private Object waitingToRecordPrefab; // If different than NULL indicates the prefab instance that will need to record its state as soon as the mouse is released. 

    // ***********************************************************************************
    // CONSTRUCTOR
    // ***********************************************************************************

    /// <summary>
    /// Creates a new HOEditorUndoManager,
    /// setting it so that the target is marked as dirty each time a new undo is stored. 
    /// </summary>
    /// <param name="p_target">
    /// The default <see cref="Object"/> you want to save undo info for.
    /// </param>
    /// <param name="p_name">
    /// The default name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// </param>
    public HOEditorUndoManager(Object p_target, string p_name) : this(p_target, p_name, true) { }
    /// <summary>
    /// Creates a new HOEditorUndoManager. 
    /// </summary>
    /// <param name="p_target">
    /// The default <see cref="Object"/> you want to save undo info for.
    /// </param>
    /// <param name="p_name">
    /// The default name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// </param>
    /// <param name="p_autoSetDirty">
    /// If TRUE, marks the target as dirty each time a new undo is stored.
    /// </param>
    public HOEditorUndoManager(Object p_target, string p_name, bool p_autoSetDirty)
    {
        defTarget = p_target;
        defName = p_name;
        autoSetDirty = p_autoSetDirty;
    }

    // ===================================================================================
    // METHODS ---------------------------------------------------------------------------

    /// <summary>
    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the default target, with the default name.
    /// </summary>
    public void CheckUndo() { CheckUndo(defTarget, defName); }
    /// <summary>
    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the given target, with the default name.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    public void CheckUndo(Object p_target) { CheckUndo(p_target, defName); }
    /// <summary>
    /// Call this method BEFORE any undoable UnityGUI call.
    /// Manages undo for the given target, with the given name.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    /// <param name="p_name">
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// </param>
    public void CheckUndo(Object p_target, string p_name)
    {
        Event e = Event.current;

        if (waitingToRecordPrefab != null)
        {
            // Record eventual prefab instance modification.
            // TODO Avoid recording if nothing changed (no harm in doing so, but it would be nicer).
            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.KeyDown:
                case EventType.KeyUp:
                    PrefabUtility.RecordPrefabInstancePropertyModifications(waitingToRecordPrefab);
                    break;
            }
        }

        if ((e.type == EventType.MouseDown && e.button == 0) || (e.type == EventType.KeyUp && e.keyCode == KeyCode.Tab))
        {
            // When the LMB is pressed or the TAB key is released,
            // store a snapshot, but don't register it as an undo
            // (so that if nothing changes we avoid storing a useless undo).
            CmdManager.Instance.SetSnapshotTarget(p_target, p_name);
            CmdManager.Instance.CreateSnapshot();
            CmdManager.Instance.RegisterSnapshot();
            listeningForGuiChanges = true;
        }
    }

    public void CheckUndo(Object[] p_targets, string p_name)
    {
        Event e = Event.current;

        if (waitingToRecordPrefab != null)
        {
            // Record eventual prefab instance modification.
            // TODO Avoid recording if nothing changed (no harm in doing so, but it would be nicer).
            switch (e.type)
            {
                case EventType.MouseDown:
                case EventType.MouseUp:
                case EventType.KeyDown:
                case EventType.KeyUp:
                    PrefabUtility.RecordPrefabInstancePropertyModifications(waitingToRecordPrefab);
                    break;
            }
        }

        if ((e.type == EventType.MouseDown && e.button == 0) || (e.type == EventType.KeyUp && e.keyCode == KeyCode.Tab))
        {
            // When the LMB is pressed or the TAB key is released,
            // store a snapshot, but don't register it as an undo
            // (so that if nothing changes we avoid storing a useless undo).
            CmdManager.Instance.SetSnapshotTarget(p_targets, p_name);
            CmdManager.Instance.CreateSnapshot();
            CmdManager.Instance.RegisterSnapshot();
            listeningForGuiChanges = true;
        }
    }

    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the default target, with the default name,
    /// and returns a value of TRUE if the target is marked as dirty.
    /// </summary>
    public bool CheckDirty() { return CheckDirty(defTarget, defName); }
    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the given target, with the default name,
    /// and returns a value of TRUE if the target is marked as dirty.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    public bool CheckDirty(Object p_target) { return CheckDirty(p_target, defName); }
    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Manages undo for the given target, with the given name,
    /// and returns a value of TRUE if the target is marked as dirty.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    /// <param name="p_name">
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// </param>
    public bool CheckDirty(Object p_target, string p_name)
    {
        if (listeningForGuiChanges && GUI.changed)
        {
            // Some GUI value changed after pressing the mouse
            // or releasing the TAB key.
            // Register the previous snapshot as a valid undo.
            SetDirty(p_target, p_name);
            return true;
        }
        return false;
    }

    public bool CheckDirty(Object[] p_targets, string p_name)
    {
        if (listeningForGuiChanges && GUI.changed)
        {
            // Some GUI value changed after pressing the mouse
            // or releasing the TAB key.
            // Register the previous snapshot as a valid undo.
            SetDirty(p_targets, p_name);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the default target, with the default name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    /// </summary>
    public void ForceDirty() { ForceDirty(defTarget, defName); }
    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the given target, with the default name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    public void ForceDirty(Object p_target) { ForceDirty(p_target, defName); }
    /// <summary>
    /// Call this method AFTER any undoable UnityGUI call.
    /// Forces undo for the given target, with the given name.
    /// Used to undo operations that are performed by pressing a button,
    /// which doesn't set the GUI to a changed state.
    /// </summary>
    /// <param name="p_target">
    /// The <see cref="Object"/> you want to save undo info for.
    /// </param>
    /// <param name="p_name">
    /// The name of the thing to undo (displayed as "Undo [name]" in the main menu).
    /// </param>
    public void ForceDirty(Object p_target, string p_name)
    {
        if (!listeningForGuiChanges)
        {
            // Create a new snapshot.
            CmdManager.Instance.SetSnapshotTarget(p_target, p_name);
            CmdManager.Instance.CreateSnapshot();
        }
        SetDirty(p_target, p_name);
    }

    // ===================================================================================
    // PRIVATE METHODS -------------------------------------------------------------------

    private void SetDirty(Object p_target, string p_name)
    {
        CmdManager.Instance.SetSnapshotTarget(p_target, p_name);
        CmdManager.Instance.RegisterSnapshot();
        if (autoSetDirty) EditorUtility.SetDirty(p_target);
        listeningForGuiChanges = false;

        if (CheckTargetIsPrefabInstance(p_target))
        {
            // Prefab instance: record immediately and also wait for value to be changed and than re-record it
            // (otherwise prefab instances are not updated correctly when using Custom Inspectors).
            PrefabUtility.RecordPrefabInstancePropertyModifications(p_target);
            waitingToRecordPrefab = p_target;
        }
        else
        {
            waitingToRecordPrefab = null;
        }
    }

    private void SetDirty(Object[] p_targets, string p_name)
    {
        CmdManager.Instance.SetSnapshotTarget(p_targets, p_name);
        CmdManager.Instance.RegisterSnapshot();
        if (autoSetDirty) foreach (Object obj in p_targets) EditorUtility.SetDirty(obj);
        listeningForGuiChanges = false;

        waitingToRecordPrefab = null;
    }

    private bool CheckTargetIsPrefabInstance(Object p_target)
    {
        return (PrefabUtility.GetPrefabType(p_target) == PrefabType.PrefabInstance);
    }
}