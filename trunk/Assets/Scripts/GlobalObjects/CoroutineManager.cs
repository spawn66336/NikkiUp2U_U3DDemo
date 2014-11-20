using UnityEngine;
using System.Collections;

public class CoroutineManager : MonoBehaviour {

    void Awake()
    {
        if( s_instance == null )
        {
            s_instance = this;
        }
    }
     

    static public CoroutineManager GetInstance()
    {
        return s_instance;
    }

    private static CoroutineManager s_instance;
}
