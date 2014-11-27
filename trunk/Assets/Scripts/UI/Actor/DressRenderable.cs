using UnityEngine;
using System.Collections;

public class DressRenderable : MonoBehaviour 
{
    void Awake()
    {
        trans = transform;
    }

    public void SetUsed( bool used )
    {
        isUsed = used;
        if( !isUsed )
        {
            trans.localPosition = new Vector3(3000.0f, 3000.0f, 3000.0f);
        }
        else
        {
            trans.localPosition = Vector3.zero;
            trans.localRotation = Quaternion.identity;
            trans.localScale = Vector3.one;
        }
    }

    public bool IsUsed()
    {
        return isUsed;
    }

    public void SetPos( Vector2 pos )
    {
        this.gameObject.transform.localPosition = pos;
    }

    public void SetScale( Vector2 scale )
    {
        this.gameObject.transform.localScale = scale;
    }

    public void SetTexture( Texture2D tex )
    {
        uiTexture.mainTexture = tex;
        uiTexture.SetDimensions(tex.width, tex.height);
    }

    public void SetDepth( int depth )
    {
        uiTexture.depth = depth; 
    }
 


    public UITexture uiTexture;
    private Transform trans;
    private bool isUsed = false;
}
