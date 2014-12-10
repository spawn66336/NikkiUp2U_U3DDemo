﻿using UnityEngine;
using System.Collections;

public class AreaMapController : MonoBehaviour 
{
    public Transform followTarget;

    Transform trans;
    UIRect contentRect;
    UIPanel mPanel;
    Bounds mBounds;
    Vector3 offset = Vector3.zero; 

    void FindPanel()
    {
        if( mPanel == null )
        mPanel = UIPanel.Find(gameObject.transform.parent);
    }

     
	// Use this for initialization
	void Start () 
    {
        trans = gameObject.transform; 
        contentRect = GetComponent<UIRect>();
	}
	
    void Update()
    {
        if( followTarget != null )
        {
            FindPanel();
            Vector3 targetWorldPos = followTarget.position;
            Transform t = mPanel.cachedTransform;
            Matrix4x4 toLocal = t.worldToLocalMatrix;
            Vector3 targetLocalPos = toLocal.MultiplyPoint3x4(targetWorldPos);
            trans.localPosition -= targetLocalPos;
        }
    }

    void UpdateBounds()
    {
        FindPanel();
        Transform t = mPanel.cachedTransform;
        Matrix4x4 toLocal = t.worldToLocalMatrix;
        Vector3[] corners = contentRect.worldCorners;
        for (int i = 0; i < 4; ++i) corners[i] = toLocal.MultiplyPoint3x4(corners[i]);
        mBounds = new Bounds(corners[0], Vector3.zero);
        for (int i = 1; i < 4; ++i) mBounds.Encapsulate(corners[i]); 
    }

    void OnDrag( Vector2 delta )
    { 
        offset.Set(delta.x, delta.y, 0.0f); 
        trans.localPosition += offset;
    }

    void LateUpdate()
    { 
        UpdateBounds(); 
        mPanel.ConstrainTargetToBounds(trans, ref mBounds, true);

    }
}
