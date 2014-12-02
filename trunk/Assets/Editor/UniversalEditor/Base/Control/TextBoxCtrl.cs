using UnityEngine;
using System.Collections;

public class TextBoxCtrl : EditorControl 
{
    public string Text
    {
        get
        {
            return text;
        }
        set
        {
            text = value;
        }
    }

    public Texture Icon
    {
        get { return image; }
        set { image = value; }
    }

    private string text = "";
    private Texture image = null;
}
