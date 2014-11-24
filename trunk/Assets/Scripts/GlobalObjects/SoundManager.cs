using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SoundManager : MonoBehaviour 
{
    
 
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

    List<AudioSource> audioSourceList = new List<AudioSource>();
}
