using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

[CustomEditor(typeof(SpecialEffect),true)] 
public class SpecialEffectInspector : Editor 
{
     
    private static GUIContent elemContent = GUIContent.none;
    private static GUIContent noneTextLable = GUIContent.none;

    private static GUILayoutOption ctrlWidth = GUILayout.MaxWidth(140f);
    private static GUILayoutOption floatCtrlWidth = GUILayout.MaxWidth(50f);
    private static GUILayoutOption elemLableWidth = GUILayout.MaxWidth(70f);
    private static GUILayoutOption toggleWidth = GUILayout.MaxWidth(20f);
    
     
    private SerializedObject spe;
    private SerializedProperty
            totalTime,
            style 
            ;
    private List<SerializedObject> elems = new List<SerializedObject>();
    
    //在每次选中GameObject时被调用
    void OnEnable()
    {
        SpecialEffect spe = target as SpecialEffect;

        //刷新子元素，并自动挂接脚本
        SpecialEffectEditorUtility.RefreshSpecialEffect(spe.gameObject);

        _UpdateSerializedObjects();

        SpecialEffectEditorUtility.MarkSpecialEffectDirty(spe);
    }

   
    public override void OnInspectorGUI()
    {
  
        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("总时长(秒)", GUILayout.MaxWidth(100f));
            if ( SpecialEffectEditorConfig.editableSpeInspector )
            { 
                EditorGUILayout.PropertyField(totalTime, noneTextLable);
            }
            else
            {
                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f), GUILayout.MaxWidth(30f));
                    EditorGUILayout.LabelField(totalTime.floatValue.ToString(), elemLableWidth);
                EditorGUILayout.EndHorizontal();
            }

        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("播放类型", GUILayout.MaxWidth(100f));
            if (SpecialEffectEditorConfig.editableSpeInspector)
            {
                EditorGUILayout.PropertyField(style, noneTextLable);
            }
            else
            {
                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f), GUILayout.MaxWidth(30f));
                EditorGUILayout.LabelField(style.enumNames[style.enumValueIndex], elemLableWidth);
                EditorGUILayout.EndHorizontal();
            }
        EditorGUILayout.EndHorizontal();

        ShowSpecialEffectElems();

        if (!SpecialEffectEditorConfig.editableSpeInspector)
        {
            //ShowUnEditableInspectorInfo();
        }


        if (!Application.isPlaying)
        {
            if (SpecialEffectEditorConfig.editableSpeInspector)
            {
                //刷新
                ShowRefreshAllElemsButton();
                ShowRemoveAllElemsButton();
            }
            else
            {
                ShowEditSpecialEffectButton();
            } 
                
        }
        spe.ApplyModifiedProperties();
        _MarkSpecialEffectDirty();
    }

    public void ShowUnEditableInspectorInfo()
    {
        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f), GUILayout.MaxWidth(30f));
        EditorGUILayout.LabelField("请使用特效编辑器编辑此特效，目前Inspector禁用编辑");
        EditorGUILayout.EndHorizontal();
    }

    public void ShowSpecialEffectElems()
    { 

       GUILayout.Space(2f);
       GUI.color = Color.yellow;
      
       GUILayout.Label("动画元素列表", "GUIEditor.BreadcrumbLeft");
       GUI.color = Color.white;
       GUILayout.Space(4f); 
       int count = 0;

       foreach( var e in elems )
       {
           count++;
           GUILayout.Space(4f);
           EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(10f));
           GUILayout.Space(2f);
           
           ShowSingleElem(e , count);

           EditorGUILayout.EndHorizontal(); 
           GUILayout.Space(4f);

           e.ApplyModifiedProperties();
       }

       
    }

    public void ShowSingleElem( SerializedObject e , int count )
    {
        SpecialEffectElement speElem = e.targetObject as SpecialEffectElement;

        EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f),GUILayout.MaxWidth(30f));
            EditorGUILayout.LabelField(speElem.gameObject.name, elemLableWidth);
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.LabelField("起始时间(秒)", elemLableWidth);
        if (SpecialEffectEditorConfig.editableSpeInspector)
        {
            EditorGUILayout.PropertyField(e.FindProperty("startTime"), noneTextLable, floatCtrlWidth);
        }
        else 
        {
            EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f), GUILayout.MaxWidth(30f));
            EditorGUILayout.LabelField(e.FindProperty("startTime").floatValue.ToString(), elemLableWidth);
            EditorGUILayout.EndHorizontal();
        }

        EditorGUILayout.LabelField("循环", GUILayout.MaxWidth(30f));

        if (SpecialEffectEditorConfig.editableSpeInspector)
        {
            EditorGUILayout.PropertyField(e.FindProperty("isLoop"), noneTextLable, toggleWidth);
        }
        else
        {
            EditorGUILayout.Toggle(e.FindProperty("isLoop").boolValue, toggleWidth);
        }

        if (!e.FindProperty("isLoop").boolValue)
        {
            EditorGUILayout.LabelField("播放时长(秒)", elemLableWidth);

            if (SpecialEffectEditorConfig.editableSpeInspector)
            {
                EditorGUILayout.PropertyField(e.FindProperty("playTime"), noneTextLable, floatCtrlWidth);
            }
            else
            {
                EditorGUILayout.BeginHorizontal("AS TextArea", GUILayout.MinHeight(8f), GUILayout.MaxWidth(30f));
                EditorGUILayout.LabelField(e.FindProperty("playTime").floatValue.ToString(), elemLableWidth);
                EditorGUILayout.EndHorizontal();
            }
        }
    }

    public void ShowEditSpecialEffectButton()
    {
        GUILayout.Space(10f);
        GUI.color = Color.green;
        if (GUILayout.Button("编辑特效"))
        {
            _StartUpEditor();
            _TakeEditTargetToSpeEditor();
        }
        GUI.color = Color.white;
    }

    public void ShowRefreshAllElemsButton()
    {
        GUILayout.Space(10f);
        GUI.color = Color.green;
        if( GUILayout.Button("刷新所有动画元素") )
        {
            _RefreshAllElems();
        }
        GUI.color = Color.white;
    }

    private void ShowRemoveAllElemsButton()
    {
        GUILayout.Space(10f);
        GUI.color = Color.yellow;
        if (GUILayout.Button("清除所有动画元素"))
        {
            _RemoveAllElems();
        }
        GUI.color = Color.white;
    }

    void _StartUpEditor()
    {
        EditorRoot root = EditorManager.GetInstance().FindEditor("特效编辑器");
        if (root == null)
        {
            root = EditorManager.GetInstance().CreateEditor("特效编辑器", false, SpecialEffectEditor.InitControls);
        }
    }


    //将编辑内容直接递交给特效编辑器
    void _TakeEditTargetToSpeEditor()
    {
        SpecialEffect spe = target as SpecialEffect; 
        SpecialEffectEditorModel.GetInstance().SetEditTarget(spe.gameObject);
    }

    private void _RemoveAllElems()
    {
        SpecialEffect currSpe = target as SpecialEffect; 
        currSpe.elems.Clear(); 

        Transform[] trans = currSpe.transform.GetComponentsInChildren<Transform>();
        foreach (var t in trans)
        {
            if (currSpe.gameObject.transform == t)
            {
                continue;
            }
            SpecialEffectElement speElem = t.gameObject.GetComponent<SpecialEffectElement>();

            if (speElem != null)
            {
                //移除所有已绑定的插件
                DestroyImmediate(speElem);
            }
        }

        _MarkSpecialEffectDirty();
        _UpdateSerializedObjects(); 
        spe.ApplyModifiedProperties();
    }

    private void _RefreshAllElems()
    {
        SpecialEffect currSpe = target as SpecialEffect;

        SpecialEffectEditorUtility.RefreshSpecialEffect(currSpe.gameObject);
         
        _MarkSpecialEffectDirty();
        _UpdateSerializedObjects();

        spe.ApplyModifiedProperties();
        
    }

    private void _MarkSpecialEffectDirty()
    { 
        SpecialEffect currSpe = target as SpecialEffect; 
        SpecialEffectEditorUtility.MarkSpecialEffectDirty(currSpe);
    }


    //用来更新Inspector所用的所有可持久化物体
    void _UpdateSerializedObjects()
    {
        SpecialEffect tmp = target as SpecialEffect;

        spe = new SerializedObject(target);
        totalTime = spe.FindProperty("totalTime");
        style = spe.FindProperty("style");

        elems.Clear();
        foreach (var e in tmp.elems)
        {
            if (e == null)
                continue;

            elems.Add(new SerializedObject(e));
        }
    }
    
     
}
