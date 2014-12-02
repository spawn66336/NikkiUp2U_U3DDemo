using UnityEngine;
using System.Collections;

[ExecuteInEditMode]   
public class SpecialEffectAnimation : SpecialEffectElement{

    [HideInInspector]
    [System.NonSerialized]
    private Animation anim;

    [HideInInspector]
    [System.NonSerialized]
    private string animClipName;

    [HideInInspector]
    [System.NonSerialized]
    //是否已经开始播放
    private bool isPlayed = false;

    protected override void _Init()
    {
        anim = GetComponent<Animation>();
        animClipName = anim.clip.name;
        if (anim == null)
        {
            Debug.Log("未成功获得动画脚本！");
        }
    }

    protected override void _PlayImpl()
    {
        if (anim == null)
            return;

        anim[animClipName].speed = 1;

        if (!isPlayed)
        {
            anim.Play(animClipName);
            isPlayed = true;
        }
    }

    protected override void _PauseImpl()
    {
        if (anim == null)
            return;

        if (isPlayed)
        {
            anim[animClipName].speed = 0;
        }
    }

    protected override void _ResetImpl()
    {
        if (anim == null)
            return; 

        anim.Rewind(animClipName);
        anim.Stop(animClipName);
        isPlayed = false;
    }

    private bool _IsAnimClipFinish()
    {
        AnimationState clipState = anim[animClipName];
        return clipState.enabled == false && (clipState.speed > 0.0f);
    }

    protected override void _SetCurrPlayTime(float t)
    {
        if (anim == null)
            return;
         
        float ltime = _CalcLocalTime(t); 
        if( ltime < 0 )
            return;
        if( ltime > anim[animClipName].length )
            ltime = anim[animClipName].length;

        anim.Play(animClipName);
        anim[animClipName].time = ltime; 

#if UNITY_EDITOR
        //在Unity Edit 模式下需调用
        if( !Application.isPlaying )
            anim.Sample();
#endif
    }
}
