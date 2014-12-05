using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour 
{
    
    public enum SoundType
    {
        //普通按钮按键音
        CommonButtonClick,
        //大按钮按键音
        CommonButtonClick2,
        //换装完毕按键音
        DressFinished,
        //奖品音效
        RewardSound,
        //计分板音效
        ScoreBoardSound
    }


	void Start () 
    {
	
	}
	 
	void Update () 
    {
	
	}

    public void Play( string name , bool loop )
    {
        AudioClip clip = ResourceManager.GetInstance().Load(ResourceType.UISound, name) as AudioClip;
        if( clip == null )
        {
            return;
        }

        AudioSource newAudioSource = AllocAudioSource();
        newAudioSource.clip = clip;
        newAudioSource.loop = loop;
        newAudioSource.Play();
    }

    public void Stop( string name )
    {
        foreach( var s in audioSourceList )
        {
            if( s.clip.name.Equals(name) )
            {
                s.Stop();
                return;
            }
        }
    }

    public AudioSource AllocAudioSource()
    {
        foreach( var s in audioSourceList )
        {
            if( !s.isPlaying )
            {
                return s;
            }
        }

        //需要创建AudioSource
        GameObject obj = new GameObject("AudioSource",new System.Type[]{typeof(AudioSource)});

        obj.transform.parent = this.transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;

        AudioSource audioSource = obj.GetComponent<AudioSource>();
        audioSourceList.Add(audioSource);
        return audioSource;
    }

    public void Play( SoundType type )
    {
        switch( type )
        {
           case SoundType.CommonButtonClick:
                Play("tongyongyin", false);
                break;
           case SoundType.CommonButtonClick2:
                Play("daanniu", false);
                break;
           case SoundType.DressFinished:
                Play("Ftishi", false);
                break;
            case SoundType.RewardSound:
                Play("tanpai", false);
                break;
            case SoundType.ScoreBoardSound:
                Play("fanye", true);
                break;
           default:
                break;
        }
    }

    public void Stop( SoundType type )
    {
        switch (type)
        {
            case SoundType.CommonButtonClick:
                Stop("tongyongyin");
                break;
            case SoundType.CommonButtonClick2:
                Stop("daanniu");
                break;
            case SoundType.DressFinished:
                Stop("Ftishi");
                break;
            case SoundType.RewardSound:
                Stop("tanpai");
                break;
            case SoundType.ScoreBoardSound:
                Stop("fanye");
                break;
            default:
                break;
        }
    }

    List<AudioSource> audioSourceList = new List<AudioSource>();
}
