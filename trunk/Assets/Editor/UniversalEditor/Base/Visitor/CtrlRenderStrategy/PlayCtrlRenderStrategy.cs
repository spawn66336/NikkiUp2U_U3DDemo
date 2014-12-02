using UnityEngine;
using System.Collections;
using UnityEditor;
using System;

public class PlayCtrlRenderStrategy : EditorRenderStrategy 
{
    public override void Visit(EditorControl c)
    {
        currCtrl = c as PlayCtrl;

        if (currCtrl == null)
            return;
        float newPlayTime = 0.0f;
        try
        {
            newPlayTime =
            EditorGUILayout.Slider(currCtrl.PlayTime, 0.0f, currCtrl.TotalTime,
                new GUILayoutOption[] { GUILayout.ExpandWidth(true), GUILayout.Height(20f), GUILayout.MinWidth(300f) });
        }catch(Exception e )
        {
            //Debug.Log(e.Message);
        }


        c.UpdateLastRect();
        
        //若鼠标在播放条发生点按事件,暂停播放
        if( c.LastRect.Contains( FrameInputInfo.GetInstance().currPos ) ) 
        {
            if( 
                FrameInputInfo.GetInstance().leftButtonDown && 
                FrameInputInfo.GetInstance().leftBtnPress 
                )
            {
                currCtrl.Pause();
            }
        }

        if( !currCtrl.IsPlaying )
        {
            if( Mathf.Abs( currCtrl.PlayTime - newPlayTime ) > Mathf.Epsilon )
            {
                currCtrl.frameTriggerInfo.isValueChanged = true;
            }
            currCtrl.PlayTime = newPlayTime;  
        }

        GUILayoutOption[] btnOptions = new GUILayoutOption[] { 
            GUILayout.Width(40),GUILayout.Height(20)
        }; 

        if( GUILayout.Button("播放", btnOptions) )
        {
            currCtrl.Play();
            currCtrl.frameTriggerInfo.isValueChanged = true;
        }
        if( GUILayout.Button("暂停", btnOptions) )
        {
            currCtrl.Pause();
            currCtrl.frameTriggerInfo.isValueChanged = true;
        }
        if( GUILayout.Button("停止", btnOptions) )
        {
            currCtrl.Stop();
            currCtrl.frameTriggerInfo.isValueChanged = true;
        }
    }

    PlayCtrl currCtrl = null;
}
