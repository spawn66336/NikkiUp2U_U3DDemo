using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.IO;
using System.Threading;


public class EditorCoroutineMessage
{
    public enum Message
    {
        NONE = 0,
        CANCEL_TASK = 1,
        PROGRESS_UI = 2
    }

    public EditorCoroutineMessage(Guid id, Message m, object p0, object p1)
    {
        taskID = id;
        msg = m;
        param0 = p0;
        param1 = p1;
    }

    public Guid taskID;
    public Message msg;
    public object param0;
    public object param1;
}

public class EditorUIProgressInfo
{
    public string msg = "";
    public int curr;
    public int total;
}

public interface IEditorCoroutineTask
{
    EditorCoroutine Co { get; set; }

    Guid TaskID { get; }

    bool DoOneStep();

    void Cancel();

    bool IsFinished();

    bool IsCanceled();

    object GetFinishedObject();
}

public class EditorCoroutine
{
    public EditorCoroutine()
    {
    }

    public void Quit()
    {
        quit = true;
    }

    public void AddTask(IEditorCoroutineTask task)
    {
        taskList.Add(task);
        task.Co = this;
    }

    public EditorCoroutineMessage GetUIMessage()
    {
        if (uiMsgQueue.Count > 0)
        {
            return uiMsgQueue.Dequeue();
        }
        return null;
    }

    public bool IsTaskFinished(Guid taskID)
    {
        foreach (var t in finishTaskList)
        {
            if (t.TaskID.Equals(taskID))
            {
                return true;
            }
        }
        return false;
    }

    public bool GetTaskResult(Guid taskID, out object res)
    {
        bool ret = false;
        res = null;
        if (IsTaskFinished(taskID))
        {
            IEditorCoroutineTask delTask = null;
            foreach (var t in finishTaskList)
            {
                if (t.TaskID.Equals(taskID))
                {
                    delTask = t;
                    res = delTask.GetFinishedObject();
                    ret = true;
                    break;
                }
            }

            if (delTask != null)
            {
                finishTaskList.Remove(delTask);
            }
        }
        return ret;
    }

    public IEditorCoroutineTask GetAFinishedTask()
    {
        if( finishTaskList.Count > 0 )
        {
            IEditorCoroutineTask t = finishTaskList[0];
            finishTaskList.Remove(t);
            return t;
        }
        return null;
    }

    public void PostTaskMessage(EditorCoroutineMessage msg)
    {
        msgQueue.Enqueue(msg);
    }

    public void PostUIMessage(EditorCoroutineMessage msg)
    {
        uiMsgQueue.Enqueue(msg);
    }

    public void Update()
    {
        _ProcessCurrMessages();

        _UpdateTaskState();

        _UpdateTaskOneStepForward();
    }


    private bool _IsQuit()
    {
        return quit;
    }

    private void _ProcessCurrMessages()
    {
        while (msgQueue.Count > 0)
        {
            EditorCoroutineMessage msg = msgQueue.Dequeue();

            foreach (var t in taskList)
            {
                if (msg.taskID != t.TaskID)
                {
                    continue;
                }
                switch (msg.msg)
                {
                    case EditorCoroutineMessage.Message.CANCEL_TASK:
                        t.Cancel();
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void _UpdateTaskState()
    {
        List<IEditorCoroutineTask> finishList = new List<IEditorCoroutineTask>();

        foreach (var t in taskList)
        {
            if (t.IsCanceled() || t.IsFinished())
            {
                finishList.Add(t);
            }
        }

        foreach (var t in finishList)
        {
            finishTaskList.Add(t);
        }

        foreach (var t in finishList)
        {
            taskList.Remove(t);
        }
    }

    private void _UpdateTaskOneStepForward()
    {
        foreach (var t in taskList)
        {
            t.DoOneStep();
        }
    }


    private bool quit = false;

    private List<IEditorCoroutineTask> taskList = new List<IEditorCoroutineTask>();

    private List<IEditorCoroutineTask> finishTaskList = new List<IEditorCoroutineTask>();

    private Queue<EditorCoroutineMessage> msgQueue = new Queue<EditorCoroutineMessage>();

    private Queue<EditorCoroutineMessage> uiMsgQueue = new Queue<EditorCoroutineMessage>();

}
