using UnityEngine;
using System.Collections;

public class DressDescBox : MonoBehaviour 
{
    //衣服图标
    public UITexture dressIcon;
    //衣服名称
    public UILabel dressName;
    //衣服描述
    public UILabel dressDesc;

	// Use this for initialization
	void Start () 
    {
	
	}
	
	// Update is called once per frame
	void Update () 
    {
	
	}

    public void SetDesc( string name , string desc , Texture2D icon )
    {
        dressName.text = name;
        dressDesc.text = desc;
        dressIcon.mainTexture = icon;
    }
}
