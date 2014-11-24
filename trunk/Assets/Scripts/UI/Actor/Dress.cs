using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Dress : GameItem 
{

    public Vector2 Pos
    {
        get { return pos; }
        set { pos = value; }
    }

    public Vector2 Scale
    {
        get { return scale; }
        set { scale = value; }
    }

    public DressType ClothType
    {
        get { return dressType; }
        set { dressType = value; }
    }

    public List<Texture2D> DressImgs
    {
        get { return dressImgs; }
        set { dressImgs = value; }
    }

    Vector2 pos = Vector2.zero;
    Vector2 scale = Vector2.one;
    DressType dressType = DressType.Hair;
    List<Texture2D> dressImgs = new List<Texture2D>();
}
