using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;



public interface IEditorCommand
{
     string Name { get; }

    //指明此命令只执行不记录，没有UndoRedo功能
     bool DontSaved { get; }

     void Execute();
     void UnExecute();
}

//用于与Untiy自带的Undo机制连结
public class EditorCmdCounter : MonoBehaviour
{
    //执行Command的前一个索引
    [HideInInspector]
    public int count = 0;

    public void Reset()
    {
        count = 0;
    }
}

public class EditorCommandManager  
{
     
    List<IEditorCommand> cmdList = new List<IEditorCommand>();

    bool undoCombineKeyDown = false;
    bool redoCombineKeyDown = false;

    //当前cmdList状态，指向执行Command后的下一个索引
    int currCount = 0;

    public delegate void CmdMgrCallback();
    public delegate void CmdExecuteCallback(IEditorCommand cmd);

    public CmdMgrCallback callback;
    public CmdExecuteCallback onBeforeCmdExecute;
    public CmdExecuteCallback onAfterCmdExecute;

    //是否完全使用UnityUndo功能
    public bool useUnityUndo = false;
    
    private EditorCommandManager()
    {
        
    }

    public void Add( IEditorCommand cmd )
    {
        if (cmd == null)
            return;

        //移除当前命令之后的所有已执行命令
        int removeNum = cmdList.Count - currCount;
        if( removeNum > 0 )
        {
            cmdList.RemoveRange(currCount, removeNum); 
        }

        NotifyBeforeCmdExecute(cmd);

        cmd.Execute();

        NotifyAfterCmdExecute(cmd); 


        if (cmd.DontSaved || useUnityUndo)
        {//命令本身为只执行不保存的或只使用Unity Undo
            cmdList.Add(cmd);
            currCount++;
        }

        NotifyChange(); 
    }

    public void PerformUndo()
    {
        if (currCount == 0)
            return;

        cmdList[--currCount].UnExecute();

        NotifyChange();
    }

    public void PerformRedo()
    {
        if (currCount >= cmdList.Count)
            return;
        cmdList[currCount++].Execute();

        NotifyChange();
    }
     
    public void Clear()
    {        
        cmdList.Clear();
        currCount = 0; 
        NotifyChange();
    }

    public string GetNextUndoCmdName()
    {
        if (currCount == 0)
            return "";
        return cmdList[currCount - 1].Name;
    }

    public string GetNextRedoCmdName()
    {
        if (currCount >= cmdList.Count)
            return "";
        return cmdList[currCount].Name;
    }
  

    public void OnGUI()
    {
        Event e = Event.current;
        if( e.control && e.shift && e.keyCode == KeyCode.Z )
        {
            if (!undoCombineKeyDown)
            {
                undoCombineKeyDown = true;
                PerformUndo();
            }
        }
        else
        {
            undoCombineKeyDown = false;
        }

        if (e.control && e.shift && e.keyCode == KeyCode.Y )
        {
            if (!redoCombineKeyDown)
            {
                redoCombineKeyDown = true;
                PerformRedo();
            }
        }
        else
        {
            redoCombineKeyDown = false;
        }
    }
     
    public void NotifyChange()
    {
        if (callback != null)
        {
            callback();
        }
    }

    public void NotifyBeforeCmdExecute( IEditorCommand cmd )
    {
        if(onBeforeCmdExecute != null )
        {
            onBeforeCmdExecute(cmd);
        }
    }

    public void NotifyAfterCmdExecute( IEditorCommand cmd )
    {
        if(onAfterCmdExecute != null )
        {
            onAfterCmdExecute(cmd);
        }
    }

    public void Destroy()
    {
        callback = null;
        Clear(); 
    }

    ////快捷键 Ctrl + Shift + Z
    //[MenuItem("H3D/UNDO/_Undo %#z")]
    //public static void HotKeyUndo()
    //{
    //    EditorCommandManager.GetInstance().PerformUndo();
    //}

    ////快捷键 Ctrl + Shift + Y
    //[MenuItem("H3D/UNDO/_Redo %#y")]
    //public static void HotKeyRedo()
    //{
    //    EditorCommandManager.GetInstance().PerformRedo();
    //}

    private static EditorCommandManager s_cmdManager = null;

    public static EditorCommandManager GetInstance()
    {
        if(  s_cmdManager == null )
        {
            s_cmdManager = new EditorCommandManager();
        }
        return s_cmdManager;
    }
 
}

 