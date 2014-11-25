using UnityEngine;
using System.Collections;

public class CommonUIResource : UIResourceManager 
{ 

	public static CommonUIResource GetInstance()
    {
        if( s_instance == null )
        {
            s_instance = new CommonUIResource();
        }
        return s_instance;
    }

    static CommonUIResource s_instance;
}
