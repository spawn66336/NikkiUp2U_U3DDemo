using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[ExecuteInEditMode]   
public class SpecialEffect : MonoBehaviour 
{

    public enum PlayStyle
    { 
        Once,
        Loop
    }
     
    //特效的子元素
    [HideInInspector] 
    public List<SpecialEffectElement> elems = new List<SpecialEffectElement>();
     
    //特效总结束时间
    [HideInInspector] 
    public float totalTime;

    //播放方式
    [HideInInspector] 
    public PlayStyle style = PlayStyle.Once;

    //是否在Awake时播放，默认关闭
    [HideInInspector]
    public bool playOnAwake = false;

    //绑定目标路径
    [HideInInspector]
    public string bindingTargetPath = "";

    [HideInInspector]
    public string BindingTargetPath
    {
        get
        {
            return bindingTargetPath;
        }
        set
        {
            bindingTargetPath = value;
        }
    }

    [HideInInspector] 
    public int TotalFrames
    {
        get
        {
            return Mathf.FloorToInt(totalTime / frameInterval);
        }
         
    }

    [HideInInspector]
    public float CurrPlayTime
    {
        get { return lastElapseTime; }
        set
        {
            lastElapseTime = value;
            if (lastElapseTime > totalTime)
                lastElapseTime = totalTime;
            else if (lastElapseTime < 0)
                lastElapseTime = 0;

            //根据设置帧时间，重新推算起始时间戳
            _RecalcStartTimeStamp();
            isPlaying = true;
            Update();
            _SetCurrPlayTime(lastElapseTime);
            Pause();
        }
    }

    
    //获取与设置当前播放帧，设置完后会暂停 
    [HideInInspector] 
    public int CurrFrame
    {
        get
        {
            return frame;
        }

        set
        {  
            frame = value;

            if (frame >= TotalFrames)
                frame = TotalFrames - 1;
            else if (frame < 0)
                frame = 0;

            lastElapseTime = ((float)frame) * frameInterval;
            //根据设置帧时间，重新推算起始时间戳
            _RecalcStartTimeStamp();
            isPlaying = true;
            Update();
            _SetCurrPlayTime(lastElapseTime);
            Pause();
        }
    }

    [HideInInspector]
    [System.NonSerialized]
    //播放状态，目前只支持播放与停止 
    private bool isPlaying = false;
    //开始时间戳
    [HideInInspector]
    [System.NonSerialized]
    private float startTimeStamp = 0;

    //最近一次播放时长
    [HideInInspector]
    [System.NonSerialized]
    private float lastElapseTime = 0;

    [HideInInspector]
    [System.NonSerialized]
    private int frame = 0;
    
    //是否在启动时已经播放过了（用于PlayOnAwake选项）
    [HideInInspector]
    [System.NonSerialized]
    private bool isAwakePlayed = false;

    private const float frameInterval = 1f / 30f;


    //将特效绑定到BindingTargetPath路径指定的物体上。
    //若路径指定物体不存在，则返回false
    public bool BindTarget( GameObject go )
    {
        if (go == null)
            return false;

        //若绑定路径为空，则直接绑定到此GameObject上
        if (BindingTargetPath.Equals(""))
        {
            _ApplyBindTarget(go.transform);
            return true;
        }

        //略过根节点名
        string path = BindingTargetPath.Substring(BindingTargetPath.IndexOf('/') + 1);

        Transform findTrans = go.transform.Find(path);

        //若没有找到路径所指定对象，不绑定，并返回false
        if( findTrans == null )
        {
            
#if !UNITY_EDITOR
            //在客户端环境下，在没有找到绑定物体的情况下
            //需要将特效绑定到物体根节点上 
            _ApplyBindTarget(go.transform);
#endif
            return false;
        } 
        _ApplyBindTarget(findTrans);
        return true;
    }

    //将当前特效根节点绑定到指定Transform上
    private void _ApplyBindTarget( Transform trans )
    {
        Vector3 localPos = this.transform.localPosition;
        Quaternion localRotate = this.transform.localRotation;
        Vector3 localScale = this.transform.localScale;

        this.transform.parent = trans;

        this.transform.localPosition = localPos;
        this.transform.localRotation = localRotate;
        this.transform.localScale = localScale;

        //保留Prefab姿态
        //this.transform.localPosition = Vector3.zero;
        //this.transform.localRotation = Quaternion.identity;
    }

    //只在编辑器中调用
    public void PlayInEditModeInit()
    {
#if UNITY_EDITOR
        foreach (var e in elems)
        {
            //先启用当前elem所属的GameObject
            //这样避免在起始状态禁用的情况下
            //调用初始化函数失败
            e.gameObject.SetActive(true);
            e.SendMessage("_Init");
        }
        Stop(); 
#endif
    }

    void Awake()
    {   
    }
     

    void OnEnable()
    { 
    }

    void OnDisable()
    {
        isAwakePlayed = false;
    }

	void Start () 
    {  
#if UNITY_EDITOR
        if (Application.isPlaying)
#endif
        {
            Stop();
            if( playOnAwake && !isAwakePlayed )
            {
                Play();
                isAwakePlayed = true;
            }
        }
	}

    public bool IsPlaying()
    {
        return isPlaying;
    }

    //播放特效
    public void Play()
    {
        isPlaying = true;
        _RecalcStartTimeStamp();
    }

    //暂停特效
    public void Pause()
    {
        isPlaying = false;
        _PauseAllElems();
    }

    //停止特效
    public void Stop()
    {
        isPlaying = false;

        _ResetStartTimeStamp();
        _ShutDownAllElems();
    }
	 
	void Update () 
    { 
        if (!IsPlaying())
            return;

        float elapseTime = _GetLocalTime();
        lastElapseTime = elapseTime;
        
        //若从开始播放的时间流失时间已经超过播放总时间
        if (elapseTime - totalTime > SpecialEffectUtility.timeEpsilon)
        {
            _ResetStartTimeStamp();
            elapseTime = _GetLocalTime();

            if (style == PlayStyle.Once)
            {
                Stop();
                return;
            }

            _ResetAllElems(); 
        }

        foreach( var e in elems )
        {
            e.UpdateState(elapseTime);
        } 

        foreach( var e in elems )
        {
            e.UpdatePlayingState(elapseTime);
        }
	}


    public override bool Equals(object o)
    {
        if (o == null)
            return false;

        //自反
        if (o == this)
            return true;

        if (GetType() != o.GetType())
            return false;

        SpecialEffect otherSpe = o as SpecialEffect;

        if( totalTime != otherSpe.totalTime )
            return false;

        if( style != otherSpe.style )
            return false;

        if( playOnAwake != otherSpe.playOnAwake )
            return false;

       

        if( !bindingTargetPath.Equals(otherSpe.bindingTargetPath) )
            return false;


         //元素数不想等
        if( elems.Count != otherSpe.elems.Count )
            return false;

        //元素已经通过SpecialEffectEditorUtility按广度
        //有先遍历添加，所以如果层级关系相同必然顺序一致
        for (int i = 0; i < elems.Count; i++ )
        {
            SpecialEffectElement e0 = elems[i];
            SpecialEffectElement e1 = otherSpe.elems[i];

            if (!e0.Equals(e1))
                return false;
        }


        return true;
    }


    //此函数将一个特效属性值与子元素属性值拷贝给自身。
    //此函数只允许编辑器调用，外界不许调用
    //前置条件：脚本所挂接的gameObject除特效之外的其他属性必需相等
    public bool _CopyValues( SpecialEffect o )
    {
#if UNITY_EDITOR
        if (o == null)
            return false;

        if (o == this)
            return true;

        if (GetType() != o.GetType())
            return false;

        if (elems.Count != o.elems.Count)
            return false;

        //检查底下的元素类型是否完全一样
        for (int i = 0; i < elems.Count; i++ )
        {
            if (elems[i].GetType() != o.elems[i].GetType())
                return false;
        }

        totalTime = o.totalTime;
        style = o.style;
        playOnAwake = o.playOnAwake;
        bindingTargetPath = o.bindingTargetPath;

        for (int i = 0; i < elems.Count; i++)
        {
            if (!elems[i]._CopyValues(o.elems[i]))
            {//此种情况禁止出现
                Debug.LogError("注意!"+"子元素拷贝失败！出现不同步的特效！拷贝操作从\""+o.gameObject.name+"\"到\""+gameObject.name+"\"");
                return false;
            }
        }

        return true;
#endif
        //客户端版，此函数永远运行失败
        return false;
    }


    float _GetLocalTime()
    {
        return Time.realtimeSinceStartup - startTimeStamp;
    }


    //重置起始时间戳，用于重放特效
    void _ResetStartTimeStamp()
    {
        lastElapseTime = 0;
        _RecalcStartTimeStamp();
    }

    //根据上次的播放时间，重新计算起始时间戳，用于暂停后重放
    void _RecalcStartTimeStamp()
    {

        startTimeStamp = Time.realtimeSinceStartup - lastElapseTime;
    }

    void _PauseAllElems()
    {
        foreach( var e in elems )
        {
            e.Pause();
        }
    }

    void _ResetAllElems()
    {
        foreach( var e in elems )
        {
            e.Reset();
        }
    }

    void _ShutDownAllElems()
    {
        foreach( var e in elems )
        { 
            e.Stop();
            e.SetEnable(false);
        }
    }

    void _SetCurrPlayTime( float t )
    {
        foreach( var e in elems )
        {
            e.SetCurrPlayTime(t);
        }
    }
}
