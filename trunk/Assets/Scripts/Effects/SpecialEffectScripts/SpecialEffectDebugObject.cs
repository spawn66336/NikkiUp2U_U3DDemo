using UnityEngine;
using System.Collections;
using UnityEditor;

public class SpecialEffectDebugObject : MonoBehaviour {


    public SpecialEffect speTarget;

    private int currFrame = 0;


 

    void OnGUI()
    {
#if UNITY_EDITOR
        if (speTarget == null)
            return;
         
        if( GUILayout.Button("播放特效") )
        {
            speTarget.Play();
        }

        if( GUILayout.Button("暂停特效") )
        {
            speTarget.Pause();
        }

        if( GUILayout.Button("停止特效") )
        {
            speTarget.Stop();
        }

        GUILayout.BeginHorizontal();
        currFrame = (int)GUILayout.HorizontalSlider(currFrame, 0, speTarget.TotalFrames - 1, GUILayout.MaxWidth(200));
        GUILayout.TextField(currFrame.ToString());
        GUILayout.EndHorizontal();
        if (GUILayout.Button("应用当前帧"))
        {
            speTarget.CurrFrame = currFrame;
        } 
#endif
    }
}
