using UnityEngine;
using System.Collections;

public class GameItem 
{
    public int Id
    {
        get{ return id;}
        set{ id = value;}
    }

    public string Name
    {
        get { return name; }
        set { name = value; }
    }

    public Texture2D Icon
    {
        get { return icon; }
        set { icon = value; }
    }

    public int Rareness
    {
        get { return rareness; }
        set { rareness = value; }
    }

    public string Desc
    {
        get { return desc; }
        set { desc = value; }
    }

    int id;
    string name;
    int rareness;
    Texture2D icon;
    string desc;
}
