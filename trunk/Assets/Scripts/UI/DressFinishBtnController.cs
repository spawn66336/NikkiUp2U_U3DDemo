using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DressFinishBtnController : MonoBehaviour 
{
    public List<Transform> anchorObjs = new List<Transform>();

    float offset = 0.0f;

	// Use this for initialization
	void Start () 
    {
	    if( anchorObjs.Count > 0 )
        {
            offset = transform.localPosition.x - anchorObjs[0].localPosition.x;
        }
	}
	
	// Update is called once per frame
	void Update () 
    {
        float minX = 0.0f;

        if( anchorObjs.Count > 0 )
        {
            minX = anchorObjs[0].localPosition.x;
        }

        foreach( var o in anchorObjs )
        {
            if( o.localPosition.x < minX )
            {
                minX = o.localPosition.x;
            }
        }

        Vector3 newPos = transform.localPosition;
        newPos.x = minX + offset;
        transform.localPosition = newPos;
	}
}
