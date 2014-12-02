using UnityEngine;
using System.Collections;
using UnityEditor;

public class InspectorRenderStrategy : EditorRenderStrategy
{ 

    public override bool PreVisit(EditorControl c) 
    {
        EditorGUILayout.BeginVertical(c.GetStyle(), c.GetOptions()); 
        return true;
    }

    public override void AfterVisit(EditorControl c) 
    {
        EditorGUILayout.EndVertical(); 
    }

    public override void Visit(EditorControl c) 
    { 
        currCtrl = c as InspectorViewCtrl;

        if (currCtrl == null)
        {
            return;
        }

        if (currCtrl.onInspector != null)
        {
            currCtrl.onInspector(c, currCtrl.editTarget);
        }

       
    }
     

    private InspectorViewCtrl currCtrl = null;
}

public class SpeInspectorTarget
{
    public void Set( SpecialEffectEditProxy spe )
    {
        if( spe == null )
        {
            Reset();
            return;
        }
        name = spe.Name;
        bindTargetPath = spe.BindingTargetPath;
        totalTime = spe.TotalTime;
        elemNum = spe.GetItemCount();
        playStyle = spe.Style;
        playOnAwake = spe.PlayOnAwake;
    }

    public void Reset()
    {
        name = "";
        bindTargetPath = "";
        totalTime = 0.0f;
        elemNum = 0;
        playStyle = SpecialEffect.PlayStyle.Once;
        playOnAwake = false;
    }

    public SpeInspectorTarget Copy()
    {
        return this.MemberwiseClone() as SpeInspectorTarget;
    }

    public string name;
    public string bindTargetPath;
    public float totalTime;
    public int elemNum;
    public SpecialEffect.PlayStyle playStyle;
    public bool playOnAwake;
}

public class SpeElemInspectorTarget
{
   public void Set( SpecialEffectEditProxy e , int sel)
   {
        if( e == null )
        {
            selectItem = -1;
            return;
        }

        selectItem = sel;
        e.GetItemName(sel, ref name);
        e.GetItemTimeInfo(sel, ref startTime, ref length);
        e.GetItemStateInfo(sel, ref isLoop); 
   }

   public SpeElemInspectorTarget Copy()
   {
       return this.MemberwiseClone() as SpeElemInspectorTarget;
   }


   public int selectItem = -1;
   public string name;
   public float startTime;
   public float length;
   public bool isLoop = false;
}

public class SpecialEffectEditorInspectorRenderDelegate
{
    public static void OnSpeInspector( EditorControl c , object target )
    {
        if (c == null || target == null)
            return;

        SpeInspectorTarget spe = target as SpeInspectorTarget;

        if (spe == null)
            return;


        bool isValueChange = false;

        GUILayout.Space(10f); 
        EditorGUILayout.LabelField("特效名", spe.name);
        EditorGUILayout.LabelField("绑定路径",spe.bindTargetPath);
        EditorGUILayout.LabelField("总时长", spe.totalTime.ToString("f2"));
        EditorGUILayout.LabelField("元素数", spe.elemNum.ToString());
        SpecialEffect.PlayStyle newPlayStyle = 
        (SpecialEffect.PlayStyle)EditorGUILayout.EnumPopup("播放方式", (System.Enum)spe.playStyle ); 
        

        if (newPlayStyle != spe.playStyle)
        {
            spe.playStyle = newPlayStyle;
            isValueChange = true;
        }
        
        bool newPlayOnAwake = EditorGUILayout.Toggle("在唤醒时播放", spe.playOnAwake );
        if( newPlayOnAwake != spe.playOnAwake )
        {
            spe.playOnAwake = newPlayOnAwake;
            isValueChange = true;
        }

        GUILayout.Space(10f);

        if( isValueChange )
        {
            c.frameTriggerInfo.isValueChanged = true;
        }
    }

    public static void OnSpeElemInspector( EditorControl c , object target )
    {
        if (c == null || target == null)
            return;

        SpeElemInspectorTarget elem = target as SpeElemInspectorTarget;
        
        if (elem == null)
            return;

        if (elem.selectItem == -1)
            return;
        

        GUILayout.Space(10f); 
        bool isValueChange = false;

        EditorGUILayout.LabelField("特效元素",elem.name );  
        bool isLoop = EditorGUILayout.Toggle("循环", elem.isLoop);

        if( isLoop != elem.isLoop )
        {
            elem.isLoop = isLoop;
            isValueChange = true;
        }

        EditorGUILayout.LabelField("起始时间", elem.startTime.ToString("f2"));
        EditorGUILayout.LabelField("播放时长", elem.length.ToString("f2"));
        EditorGUILayout.LabelField("结束时间", (elem.startTime + elem.length).ToString("f2"));

        GUILayout.Space(10f);

        if( isValueChange )
        {
            c.frameTriggerInfo.isValueChanged = true;
        }
          
    }
}
