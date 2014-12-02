using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System;

public class EditorRoot : EditorWindow
{
    public EditorControl RootCtrl
    {
        get { return rootCtrl; }
        set
        {
            rootCtrl = value;
            if (rootCtrl != null)
            {
                rootCtrl.Root = this;
            }
        }
    }

    //编辑器名称
    public string name;


    private EditorControl rootCtrl = null;
    public EditorRenderer renderer = null;

    //窗口协程
    public EditorCoroutine coroutine = new EditorCoroutine();

    public delegate void VoidDelegate(EditorRoot root);
    public delegate void MessageDelegate(ControlMessage msg);
    public delegate void CoroutineMessageDelegate(EditorCoroutineMessage msg);
    public delegate void CoroutineTaskFinishedDelegate(Guid taskID, object resultObj);

    public VoidDelegate onAwake;
    public VoidDelegate onEnable;
    public VoidDelegate onDisable;
    public VoidDelegate onUpdate;
    public VoidDelegate onGUI;
    public VoidDelegate onDestroy;
    public MessageDelegate onMessage;
    public CoroutineMessageDelegate onCoroutineMessage;
    public CoroutineTaskFinishedDelegate onCoroutineTaskFinished;

    //控件消息队列
    private Queue<ControlMessage> messageQueue = new Queue<ControlMessage>();


    /**
     * 用于反射重生
     */
    //初始化回调类类型名
    public string initCallbackRefType;
    //初始化编辑器所用回调名，用于反射重建编辑器
    public string initCallback;
    //是否为Utility类型窗口
    public bool isUtility;
    //窗体是否关闭
    public bool isClosed = false; 
    //当前窗口的输入信息
    private FrameInputInfo inputInfo = new FrameInputInfo();

    public FrameInputInfo InputInfo
    { get { return inputInfo; } }

    public EditorRoot()
    {
    }

    //编辑器初始化，实例化后必需调用
    public void Init()
    {
        renderer = new EditorRenderer();
    }

    public EditorCoroutine GetCoroutine()
    {
        return coroutine;
    }

    public EditorControl FindControl(string ctrlName)
    {
        FindControlByNameVisitor finder = new FindControlByNameVisitor();
        finder.name = ctrlName;
        if (rootCtrl != null)
        {
            rootCtrl.Traverse(finder);
        }
        if (finder.results.Count > 0)
        {
            return finder.results[0];
        }
        return null;
    }

    public T FindControl<T>() where T : EditorControl
    {
        FindControlByTypeVisitor<T> finder = new FindControlByTypeVisitor<T>();
        if (rootCtrl != null)
        {
            rootCtrl.Traverse(finder);
        }
        if (finder.results.Count > 0)
        {
            return finder.results[0] as T;
        }
        return null;
    }

    public List<EditorControl> FindControls(string ctrlName)
    {
        FindControlByNameVisitor finder = new FindControlByNameVisitor();
        finder.name = ctrlName;
        if (rootCtrl != null)
        {
            rootCtrl.Traverse(finder);
        }
        return finder.results;
    }

    public List<EditorControl> FindControls<T>()
    {
        FindControlByTypeVisitor<T> finder = new FindControlByTypeVisitor<T>();
        if (rootCtrl != null)
        {
            rootCtrl.Traverse(finder);
        }
        return finder.results;
    }

    public void EnqueueMessage(ControlMessage msg)
    {
        messageQueue.Enqueue(msg);
    }

    public ControlMessage DequeueMessage()
    {
        ControlMessage msg = null;
        if (messageQueue.Count > 0)
        {
            msg = messageQueue.Dequeue();
        }
        return msg;
    }

    public bool HasMessage()
    {
        return messageQueue.Count > 0;
    }

    public void NotifyMessages()
    {
        while (HasMessage())
        {
            ControlMessage msg = DequeueMessage();
            if (onMessage != null)
            {
                onMessage(msg);
            }
        }
    }

    void Awake()
    {
        if (onAwake != null)
        {
            onAwake(this);
        }
    }

    void OnEnable()
    {
        if (onEnable != null)
        {
            onEnable(this);
        }
    }

    void OnDisable()
    {
        if (onDisable != null)
        {
            onDisable(this);
        }
    }
    public static Rect windowRect = new Rect(20, 20, 120, 50);

    void OnGUI()
    {
        if (isClosed)
        {
            Close();
            return;
        }

        if (rootCtrl == null)
        { 
            return;
        }

        //若当前窗口不是当前窗口，则不更新此
        //窗口的输入信息
        if (this == EditorWindow.focusedWindow)
        { 
            InputInfo.Update();
        }

        FrameInputInfo.SetCurrInputInfo(InputInfo);

        _PrepareDrawGUI();

        renderer.Render(rootCtrl, position);

        //广播本帧产生的所有消息
        NotifyMessages();

        if (onGUI != null)
        {
            onGUI(this);
        }
        
        if (renderer.IsRepaintRequested())
        {
            Repaint();
        }
        
    }

    void Update()
    {
        if(isClosed)
        {
            Close();
            return;
        }

        if (rootCtrl == null)
        {
            _HandleRespawn();
            return;
        }


        if (GetCoroutine() != null)
        {
            //更新协程
            GetCoroutine().Update();

            //处理协程发来的UI消息
            ProcessCoroutineUIMessage();

            //处理协程已完成的任务
            ProcessCoroutineFinishedTasks();
        }

        if (onUpdate != null)
        {
            onUpdate(this);
        }

        renderer.Update(rootCtrl);
        if (renderer.IsRepaintRequested())
        {
            Repaint();
        }
    }

    void ProcessCoroutineUIMessage()
    {
        EditorCoroutineMessage msg = null;
        while ((msg = GetCoroutine().GetUIMessage()) != null)
        {
            if (onCoroutineMessage != null)
            {
                onCoroutineMessage(msg);
            }
        }
    }

    void ProcessCoroutineFinishedTasks()
    {
        IEditorCoroutineTask task = null;
        while ((task = GetCoroutine().GetAFinishedTask()) != null)
        {
            if (onCoroutineTaskFinished != null)
            {
                onCoroutineTaskFinished(task.TaskID, task.GetFinishedObject());
            }
        }
    }

    void OnDestroy()
    {
        coroutine = null;

        if (rootCtrl == null)
        {
            return;
        }

        if (onDestroy != null)
        {
            onDestroy(this);
        }

        renderer.Destroy(rootCtrl);
        rootCtrl = null;
        EditorManager.GetInstance().RemoveEditor(name);


        onAwake = null;
        onEnable = null;
        onDisable = null;
        onUpdate = null;
        onGUI = null;
        onDestroy = null;
        onMessage = null;
        onCoroutineMessage = null;
        onCoroutineTaskFinished = null;

        renderer = null; 
        GC.Collect();
    }

    public void RequestRepaint()
    {
        if (renderer != null)
        {
            renderer.RequestRepaint();
        }
    }

    public void ShutDown()
    {
        isClosed = true;
        RequestRepaint();
    }

    void _HandleRespawn()
    {
        EditorManager.GetInstance().RespawnEditor(this);
    }

    static void _PrepareDrawGUI()
    {
        SpecialEffectEditorStyle.Init();
        SpecialEffectEditorOption.Init();
    }



}
