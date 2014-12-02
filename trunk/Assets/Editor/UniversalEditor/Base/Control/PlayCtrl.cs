using UnityEngine;
using System.Collections;

public class PlayCtrl : EditorControl 
{
    public float PlayTime
    {
        get { return playTime; }
        set { 
            playTime = value;
            if (playTime > totalTime)
                playTime = totalTime;
        }
    }

    public float TotalTime
    {
        get { return totalTime; }
        set { 
            totalTime = value;
            if (totalTime < 0f)
                totalTime = 0f;
            if (playTime > totalTime)
                playTime = totalTime;
        }
    }

    public bool IsPlaying
    {
        get { return playing; }
    }

    public void Play()
    {
        playing = true;
    }

    public void Pause()
    {
        playing = false;
    }

    public void Stop()
    {
        playTime = 0.0f;
        playing = false;
    }

    

    private float playTime = 0.0f;
    private float totalTime = 0.0f;
    private bool  playing = false;
}
