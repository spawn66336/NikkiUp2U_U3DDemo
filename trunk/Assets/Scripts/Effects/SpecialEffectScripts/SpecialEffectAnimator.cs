using UnityEngine;
using System.Collections;

public class SpecialEffectAnimator : SpecialEffectElement 
{ 
    [HideInInspector]
    [System.NonSerialized]
    protected Animator animator; 
     

    [HideInInspector]
    [System.NonSerialized]
    protected float animLength;

    //当前动画片段是否为循环
    [HideInInspector]
    [System.NonSerialized]
    protected bool loop;

    protected override void _Init()
    {
        animator = GetComponent<Animator>(); 
        if (animator == null)
        {
            Debug.Log("未成功获得Animator，检查是否插件绑定错误！");
            return;
        }

        animator.speed = 0f;
        animator.Play("");
        animator.Update(0);
         

        AnimatorStateInfo state = animator.GetCurrentAnimatorStateInfo(0);
        animLength = state.length;
        loop = state.loop;
    }

    protected override void _PlayImpl()
    {
        if (animator == null)
            return;

        animator.speed = 1f;
    }

    protected override void _PauseImpl()
    {
        if (animator == null)
            return;

        animator.speed = 0f;
    }

    protected override void _ResetImpl()
    {
        if (animator == null)
            return;

        animator.Play("",0,0f);
        animator.Update(0);
    }
    protected override void _SetCurrPlayTime(float t)
    {
        if (animator == null )
            return;

        float ltime = _CalcLocalTime(t);

        float normTime = ltime / animLength;
        if( !loop && normTime > 1f )
            normTime = 1f;
         
        animator.Play("",0,normTime);
        animator.Update(0f);
    }
     
}
