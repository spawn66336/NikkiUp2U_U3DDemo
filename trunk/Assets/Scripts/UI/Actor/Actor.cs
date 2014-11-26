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

    public DressSet GetDressSet()
    {
        return dressSet;
    }
    
    public void ClearAllDress()
    {
        dressSet.ClearDress();
    }


 
}
