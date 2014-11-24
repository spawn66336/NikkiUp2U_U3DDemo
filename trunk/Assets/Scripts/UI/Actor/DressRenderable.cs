using UnityEngine;
using System.Collections;

public class DressRenderable : MonoBehaviour 
{
    void Awake()
    {
        uiTexture = this.gameObject.GetComponent<UITexture>();
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
    }

    public void SetDepth( int depth )
    {
        uiTexture.depth = depth;
    }

 
    UITexture uiTexture;
}
