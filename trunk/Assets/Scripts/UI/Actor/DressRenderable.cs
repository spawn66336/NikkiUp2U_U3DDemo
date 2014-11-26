using UnityEngine;
using System.Collections;

public class DressRenderable : MonoBehaviour 
{

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
}
