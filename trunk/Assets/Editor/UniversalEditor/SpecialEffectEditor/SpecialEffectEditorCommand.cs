using UnityEngine;
using System.Collections;
using System.Collections.Generic;

  
public class SpeItemChangeCmd : IEditorCommand
{ 
    public static List<TimeLineItem> tmpItems = new List<TimeLineItem>();
    public static void ClearTmpItems()
    {
        tmpItems = new List<TimeLineItem>();
    }

    public List<int> indices = null;
    public List<TimeLineItem> oldTimeLineItems = new List<TimeLineItem>();
    public List<TimeLineItem> newTimeLineItems = new List<TimeLineItem>();

    public string Name { get { return "SpecialEffect Element Time Changed"; } }

    public bool DontSaved { get { return false; } }

    public void Execute()
    { 

        if( !SpecialEffectEditorModel.GetInstance().HasEditTarget() )
        {
            return;
        }
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();

        int i = 0;
        foreach( var item in newTimeLineItems )
        {
            int itemIndx = indices[i];
            spe.SetItemTimeInfo(itemIndx, item.startTime, item.length);
            i++;
        }
         
          
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        //值变动时刷新Inspector
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged(); 
    }

    public void UnExecute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }
        SpecialEffectEditProxy spe = SpecialEffectEditorModel.GetInstance().GetEditTarget();

        int i = 0;
        foreach (var item in oldTimeLineItems)
        {
            int itemIndx = indices[i];
            spe.SetItemTimeInfo(itemIndx, item.startTime, item.length);
            i++;
        }
         
          
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        //值变动时刷新Inspector
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged();
    }
}

public class SpeItemSelectChangeCmd : IEditorCommand
{
    public int oldSelection = -1;
    public int newSelection = -1;

    public string Name { get { return "SpecialEffect Select Item Change"; } }
    public bool DontSaved { get { return false; } }

    public void Execute()
    {  
        SpecialEffectEditorModel.GetInstance().SetItemSelectIndx(newSelection);
    }

    public void UnExecute()
    { 
        SpecialEffectEditorModel.GetInstance().SetItemSelectIndx(oldSelection);
    }
}

public class SpeInspectorValueChangeCmd : IEditorCommand
{
    public SpeInspectorTarget oldValue;
    public SpeInspectorTarget newValue;

    public string Name { get { return "SpecialEffect Inspector Change"; } }
    public bool DontSaved { get { return false; } }

    public void Execute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        SpecialEffectEditorModel.GetInstance().GetEditTarget().Style = newValue.playStyle;
        SpecialEffectEditorModel.GetInstance().GetEditTarget().PlayOnAwake = newValue.playOnAwake;

        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged();
    }

    public void UnExecute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        SpecialEffectEditorModel.GetInstance().GetEditTarget().Style = oldValue.playStyle;
        SpecialEffectEditorModel.GetInstance().GetEditTarget().PlayOnAwake = oldValue.playOnAwake;

        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged();
    }
}

public class SpeElemInspectorValueChangeCmd : IEditorCommand
{
    public SpeElemInspectorTarget oldValue;
    public SpeElemInspectorTarget newValue;

    public string Name { get { return "SpecialEffectElement Inspector Change"; } }
    public bool DontSaved { get { return false; } }

    public void Execute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        int ielem = newValue.selectItem; 
        SpecialEffectEditorModel.GetInstance().GetEditTarget().SetItemStateInfo(ielem, newValue.isLoop);

        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged();
    }

    public void UnExecute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        int ielem = oldValue.selectItem; 
        SpecialEffectEditorModel.GetInstance().GetEditTarget().SetItemStateInfo(ielem, oldValue.isLoop);
  
        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange();
        SpecialEffectEditorModel.GetInstance().NotifySelectItemChanged();
    }
}

public class SpeBindingTargetChangeCmd : IEditorCommand
{ 
    public string oldPath;
    public string newPath;

    public string Name { get { return "SpecialEffect Bind Target Change"; } }

    public bool DontSaved { get { return false; } }

    public void Execute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        SpecialEffectEditorModel.GetInstance().GetEditTarget().BindingTargetPath = newPath; 

        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange(); 
    }

    public void UnExecute()
    {
        if (!SpecialEffectEditorModel.GetInstance().HasEditTarget())
        {
            return;
        }

        SpecialEffectEditorModel.GetInstance().GetEditTarget().BindingTargetPath = oldPath; 

        SpecialEffectEditorModel.GetInstance().SetDirty(); 
        SpecialEffectEditorModel.GetInstance().NotifyEditTargetValueChange(); 
    }

 
}