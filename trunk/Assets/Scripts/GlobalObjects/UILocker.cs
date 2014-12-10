using UnityEngine;
using System.Collections;

public class UILocker : MonoBehaviour 
{
    void Awake()
    {
        s_instance = this;
        gameObject.SetActive(false);
    }

	// Use this for initialization
	void Start () 
    {
	    
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public bool IsLocked()
    {
        return lockCount > 0;
    }

    public void Lock(GameObject obj )
    {
        lockCount++;
        if( lockCount > 0 )
        {
            gameObject.SetActive(true);
        }
    }

    public void UnLock(GameObject obj)
    {
        lockCount--;
        if( lockCount <= 0 )
        {
            gameObject.SetActive(false);
        }
    }

    public void ForceUnlock()
    {
        lockCount = 0;
        gameObject.SetActive(false);
    }


    int lockCount = 0;

    public static UILocker GetInstance()
    {
        return s_instance;
    }

    private static UILocker s_instance;
}
