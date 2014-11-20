using UnityEngine;
using System.Collections;

public class Root : MonoBehaviour 
{
    void Awake()
    {
        if(s_instance == null )
        {
            s_instance = this;
        }
    }

    public static Root GetInstance()
    {
        return s_instance;
    }


    private static Root s_instance;
 
}
