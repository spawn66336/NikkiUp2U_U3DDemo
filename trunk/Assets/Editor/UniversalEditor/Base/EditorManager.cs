using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Reflection;
using System;

public class EditorManager 
{ 
    private EditorManager()
    {
        Application.RegisterLogCallback(_EditorLogDispatcher);
    }

    public delegate void VoidDelegate(EditorRoot root);


    public EditorRoot CreateEditor( string name , bool utility , VoidDelegate initCallback )
    {
        if( roots.ContainsKey(name) )
        {//有重名编辑器
            Debug.LogError("出现重名编辑器"+"\"name\"!");
            return null;
        }

        //EditorRoot newEditor =  EditorWindow.GetWindow<EditorRoot>(name, true);
        //EditorRoot newEditor = EditorWindow.GetWindow<EditorRoot>(false,name,true);
        //EditorRoot newEditor = EditorWindow.GetWindowWithRect<EditorRoot>(new Rect(0, 0, 512, 512), false, name, true);
         
        EditorRoot newEditor = EditorWindow.CreateInstance<EditorRoot>();
        //初始化编辑器最新的组件
        newEditor.Init();

        //记录初始化回调，用于反射重生
        newEditor.initCallbackRefType = initCallback.Method.ReflectedType.FullName;
        newEditor.initCallback = initCallback.Method.Name;
        newEditor.isUtility = utility;

        newEditor.name = name;
        newEditor.title = name;
         
        if (utility)
        {
            newEditor.ShowUtility();
        }
        else
        {
            newEditor.Show();
        }
        newEditor.Focus();

        //初始化控件
        initCallback(newEditor);
        

        if (newEditor.onAwake != null)
        {
            newEditor.onAwake(newEditor);
        }

        if( newEditor.onEnable != null )
        {
            newEditor.onEnable(newEditor);
        }

        roots.Add(name, newEditor);
        return newEditor;
    }

    public EditorRoot FindEditor( string name )
    {
        EditorRoot editor = null;
        roots.TryGetValue(name,out editor);
        return editor;
    }

    public bool RemoveEditor( string name )
    { 
       return roots.Remove(name); 
    }

    //重新创建编辑器
    public void RespawnEditor( EditorRoot e )
    {
        string editorName = e.name;
        string initCallbackRefTypeName = e.initCallbackRefType;
        string initCallbackName = e.initCallback;
        bool isUtility = e.isUtility;

        //Debug.Log("Editor Respawn " + e.initCallbackRefType + "." + e.initCallback + "  utility=" + isUtility);
        e.Close(); 

        EditorRoot findEditor = FindEditor(editorName);
        if( findEditor != null )
        {
            RemoveEditor(editorName);
        }

        Type refType = Assembly.GetExecutingAssembly().GetType(initCallbackRefTypeName); 
        MethodInfo initCallbackInfo = refType.GetMethod(initCallbackName,BindingFlags.Public|BindingFlags.Static); 
        if (refType == null || initCallbackInfo == null)
        {
            Debug.Log("编辑器\""+editorName+"\"恢复失败！");
            return;
        }
         
        VoidDelegate initDelegate = Delegate.CreateDelegate(typeof(VoidDelegate), null, initCallbackInfo, false) as VoidDelegate;
        CreateEditor(editorName, isUtility, initDelegate );
    }

    public int GetCount()
    {
        return roots.Count;
    }

    public void Clear()
    {
        roots.Clear();
    }

    //用来分发Unity的Log
    private void _EditorLogDispatcher(string condition, string stackTrace, LogType type)
    {
        if( logCallback != null )
        {
            logCallback(condition, stackTrace, type);
        }
    }


    public Application.LogCallback logCallback;

    private Dictionary<string, EditorRoot> roots = 
        new Dictionary<string,EditorRoot>();

    private static EditorManager s_instance = null;
    public static EditorManager GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new EditorManager();
        }
        return s_instance;
    }
}
