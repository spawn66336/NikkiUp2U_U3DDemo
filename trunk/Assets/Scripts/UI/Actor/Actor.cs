using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Actor : MonoBehaviour 
{
    public DressSet dressSet;

    public void Awake()
    {
        GameObject obj = new GameObject("DressSet", new System.Type[] { typeof(DressSet) });
        dressSet = obj.GetComponent<DressSet>();

        obj.transform.parent = transform;
        obj.transform.localPosition = Vector3.zero;
        obj.transform.localRotation = Quaternion.identity;
        obj.transform.localScale = Vector3.one;
    }

    public void SetDress(DressType type, Dress dress)
    {
        dressSet.SetDress( type, dress );
    }
    
    public void ClearAllDress()
    {
        dressSet.ClearDress();
    }


    public void OnGUI()
    {

        Rect r = new Rect(300, 100, 100, 100);
        if( GUI.Button(r, "添加Renderable") )
        {
            dressSet.AllocDressRenderable();
        }
    }
}
