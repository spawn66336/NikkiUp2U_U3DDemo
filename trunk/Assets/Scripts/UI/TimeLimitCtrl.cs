using UnityEngine;
using System.Collections;

public class TimeLimitCtrl : MonoBehaviour 
{
    public delegate void VoidDelegate();

    public UILabel remainingTimeLabel;

    public float remainingSeconds = 0f;

    public VoidDelegate onTimeUpCallback;

    int startTicks = 0;

	void Start () 
    {
        startTicks = System.Environment.TickCount;
	}
	 
	void Update () 
    {
        int tickElapse = System.Environment.TickCount - startTicks;
        float secs = ((float)tickElapse) * 0.001f;

        if( secs > remainingSeconds )
        {
            if( onTimeUpCallback != null )
            {
                onTimeUpCallback();
                onTimeUpCallback = null;
            }
            return;
        }

        remainingTimeLabel.text = (remainingSeconds - secs).ToString("f1");

	}
}
