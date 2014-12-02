using UnityEngine;
using System.Collections;
using System;

//特效元素
[Serializable]
[ExecuteInEditMode]   
public class SpecialEffectElement : MonoBehaviour
{
 

    //起始播放时间（单位：秒） 
    [HideInInspector]
    [SerializeField]
    public float startTime;

    //指示此特效元素是否采用播放时间，若不采用playTime，
    //此特效将在特效总结束时间时技术播放。 
    [HideInInspector]
    [SerializeField]
    public bool isLoop = true;

    //在播放开始后还要播放多长时间（单位：秒）。 
    [HideInInspector]
    [SerializeField]
    public float playTime;
      
    //附属动画是否播放
    [HideInInspector] 
    [System.NonSerialized]
    private bool isPlaying = false;

    public override bool Equals(object o)
    {
        if (o == null)
            return false;

        if (o == this)
            return true;

        if (GetType() != o.GetType())
            return false;

        SpecialEffectElement oe = o as SpecialEffectElement;

        if (startTime != oe.startTime)
            return false;

        if (playTime != oe.playTime)
            return false;

        if (isLoop != oe.isLoop)
            return false;

        return true;
    }


    //此函数只供SpecialEffect._CopyValues调用外界不许调用
    public bool _CopyValues( SpecialEffectElement o)
    {
 #if UNITY_EDITOR
        if (o == null)
            return false;

        if (o == this)
            return true;

        if (GetType() != o.GetType())
            return false;

        startTime = o.startTime;
        playTime = o.playTime;
        isLoop = o.isLoop; 
        return true;
 #endif
        //客户端版，此函数永远运行失败
        return false;
    }
 
    void OnEnable()
    { 
    }

    void OnDisable()
    { 
    }

    void Start()
    {
#if UNITY_EDITOR 
        //在编辑器下，只有在播放状态下才在
        //Start中自动初始化
      if( Application.isPlaying )  
#endif
        _Init();
    }

    //查看当前目标object是否启用
    public bool IsPlaying()
    {
        return isPlaying;
    }

    public bool IsEnable()
    {
        return gameObject.activeSelf;
    } 

    public void SetEnable( bool b )
    {
        gameObject.SetActive(b);
    }

    //开始播放
    public void Play()
    {
        _PlayImpl();
        isPlaying = true;
    }

    //暂停播放
    public void Pause()
    {
        _PauseImpl();
        isPlaying = false;
    }

    //停止播放
    public void Stop()
    {
        _ResetImpl();
        Pause();
    }

    //不改变物体的可见性，重置播放
    public void Reset()
    {
        _ResetImpl();
        if (isPlaying)
        {
            Play();
        }
        else
        {
            Pause();
        }
    }

    public void SetCurrPlayTime( float t )
    {
        if ( IsEnable() )
        {
            _SetCurrPlayTime(t);
        }
    }
     

    public void UpdateState( float elapseTime )
    {
        bool isInPlayInterval = IsInPlayTimeInterval(elapseTime);

        //若当前时间在特效元素播放区间内，则显示
       

        if (isInPlayInterval)
        { 
            if (!IsEnable())
            { 
                //先启用在播放
                SetEnable(true); 
            }
        }
        else
        {
            if (IsEnable())
            { 
                Stop();
                //先停止再禁用
                SetEnable(false);
            }
            
        } 
    }

    public void UpdatePlayingState( float elapseTime )
    {
        if( IsInPlayTimeInterval(elapseTime) )
        {
            if(!IsPlaying())
            {
                Play();
            }
        }
    }


    //查看当前时间是否在播放区间内
    protected bool IsInPlayTimeInterval(float elapseTime)
    {
        //若为循环播放，则大于起始时间即可
        if (isLoop)
        {
            if (elapseTime >= startTime)
                return true;
            return false;
        }

        if (   (elapseTime - (startTime + playTime) ) < SpecialEffectUtility.timeEpsilon
                && elapseTime >= startTime
            )
            return true;
        return false;
    }

    protected float _CalcLocalTime( float elapseTime )
    {
        return elapseTime - startTime;
    }
 


    protected virtual void _Init()
    {

    } 

    //子类实现特定的播放操作
    protected virtual  void _PlayImpl()
    {

    }

    
    //子类实现特定的暂停操作
    protected virtual void _PauseImpl()
    {

    }

    //子类实现特定的重置操作
    protected virtual void _ResetImpl()
    {

    }

    protected virtual void _SetCurrPlayTime( float t )
    {

    }
}