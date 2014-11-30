using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DebugLogger : MonoBehaviour {

    void Awake()
    {
        s_instance = this;
    }

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        foreach( var l in logs )
        {
            GUILayout.Button(l);
        }
    }

    public void Error(string err )
    {
        logs.Add(err);
    }

    public static DebugLogger GetInstance()
    {
        return s_instance;
    }

    static DebugLogger s_instance = null;

    private List<string> logs = new List<string>();
}
