using UnityEngine;
using System.Collections;

public class NewBehaviourScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void OnGUI()
    {
        Rect rect = new Rect(100, 100, 100, 100);
        foreach( var p in XMLTools.paths)
        {
            GUILayout.Button(p);
        }
    }
}
