using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;


[Serializable]
[ExecuteInEditMode]   
public class SpecialEffectParticleSys : SpecialEffectElement 
{
    [HideInInspector]
    [System.NonSerialized]
    protected ParticleSystem particleSys;

    protected override void _Init()
    {
        particleSys = GetComponent<ParticleSystem>();
         
        if (particleSys == null)
        {
            Debug.Log("未成功获取粒子系统，检查是否插件绑定错误！");
        }
    }

    protected override void _PlayImpl()
    {
        if (particleSys == null)
            return;

        particleSys.Play(true);
         
    }

    protected override void _PauseImpl()
    {
        if (particleSys == null)
            return; 

        particleSys.Pause(true); 
    }
 
    protected override void _ResetImpl()
    {
        if (particleSys == null)
            return; 
         
        particleSys.Clear(true);
        particleSys.Simulate(0, true, true);
    }

    protected override void _SetCurrPlayTime(float t)
    {
        if (particleSys == null)
            return;

        float ltime = _CalcLocalTime(t);

        particleSys.Clear(true);
        particleSys.Simulate(ltime, true, true);
    }
}
