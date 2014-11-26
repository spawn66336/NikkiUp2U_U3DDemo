using UnityEngine;
using System.Collections;

public class BagItemBtnController : MonoBehaviour 
{
    
    public UILabel ItemIDLabel;
    public UILabel ItemName;
    public UITexture ItemIcon;
    public UISprite ItemUsed;

    private Dress dress;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () { 
	}

    public void SetUsed( bool b )
    {
        if( ItemUsed.gameObject.activeInHierarchy != b )
        {
            ItemUsed.gameObject.SetActive(b);
        }
    }

    public void SetItemIcon( Texture2D icon )
    {
        ItemIcon.mainTexture = icon;
    }

    public void SetItemName( string name )
    {
        ItemName.text = name;
    }

    public void SetItemID( int id )
    {
        ItemIDLabel.text = id.ToString();
    }

    public void SetDress( Dress d )
    {
        dress = d;
    }

    public Dress GetDress()
    {
        return dress;
    }

}
