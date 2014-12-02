using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class GameObjectTool
{
    public static List<GameObject> ListAllChild(GameObject go)
    {
        List<GameObject> list = new List<GameObject>();

        ListAllChild(go, list);
        return list;
    }

    static void ListAllChild(GameObject go, List<GameObject> list)
    {
        if (go == null)
        {
            return;
        }

        list.Add(go);
        foreach (Transform t in go.transform)
        {
            ListAllChild(t.gameObject, list);
        }
    }
}



public class DressSet : MonoBehaviour
{
    void Start()
    {
        Init();
        SetDress(DressType.Hair, null);
    }

    public void ClearDress()
    {
    }

    public DressRenderable AllocDressRenderable()
    {
        foreach( var r in renderables )
        {
            //找到一个已存在的未启用Renderable
            if( !r.IsUsed() )
            {
                r.SetUsed(true);
                return r;
            }
        }

        //池中已没有可用Renderable
        GameObject go = new GameObject("DressRenderable", 
            new System.Type[]{ 
                typeof(DressRenderable),
                typeof(UITexture)});

        DressRenderable renderable = go.GetComponent<DressRenderable>();
        renderable.uiTexture = go.GetComponent<UITexture>(); 
        go.transform.parent = this.transform;
        renderable.SetUsed(true);
         
        renderables.Add(renderable);
        return renderable;
    }

    public void ClearRenderables()
    {
        foreach (var r in renderables)
        {
            r.SetUsed(false);
        }
    }

    //获取用于评分的衣服信息
    public DressSetInfo GetDressSetInfo()
    {
        DressSetInfo newInfo = new DressSetInfo();

        foreach (var part in dressParts)
        {
            if (part.dress != null)
            {
                newInfo.dressList.Add(part.dress.Id);
            }
        }

        return newInfo;
    }

    //当前服装是否已经穿着
    public bool IsUsed( Dress dress )
    {
        foreach( var dpt in dressParts )
        {
            if( dpt.dress != null &&
                dpt.dress.Id == dress.Id 
                )
            {
                return true;
            }
        }
        return false;
    }

    class DressPart
    {
        public Dress dress = null;
        public int[] depth = null;
        public Texture2D[] initTex = null;
        public int[] initDepth = null;
        public Vector2 initPos = Vector2.one;
    }

    // 遮挡关系表
    DressPart[] dressParts = new DressPart[NumDressType] {
        new DressPart {depth = new int[]{18,  3 }, initDepth = new int[] {18, 3}, initPos = new Vector2(620.5f, 1531.5f)},      //  Hair = 0,  
        new DressPart {depth = new int[]{15,  8 }, initDepth = new int[] { 7, -1}, initPos = new Vector2(689.5f, 955.5f)},	    //  Tops, 
        new DressPart {depth = new int[]{16,  4 }},			//    Coat,               
        new DressPart {depth = new int[]{15,  5 }, initDepth = new int[] {15, -1}, initPos = new Vector2(669.5f, 1181f)},		//  Dress,              
        new DressPart {depth = new int[]{12,  5 }, initDepth = new int[] { 7, -1}, initPos = new Vector2(689.5f, 955.5f)},		//  Bottoms,            
        new DressPart {depth = new int[]{ 9, -1 }},			//    Socks,              
        new DressPart {depth = new int[]{11, -1 }},			//    Shoes,              
        new DressPart {depth = new int[]{20,  2 }},			//    AccHead,            
        new DressPart {depth = new int[]{19, -1 }},			//    AccEar,             
        new DressPart {depth = new int[]{17, -1 }},			//    AccNeck,            
        new DressPart {depth = new int[]{14, -1 }},			//    AccHand,            
        new DressPart {depth = new int[]{20,  2 }},			//    AccWaist,           
        new DressPart {depth = new int[]{10, -1 }},			//    AccLeg,             
        new DressPart {depth = new int[]{21,  1 }},			//    AccSpecial,         
        new DressPart {depth = new int[]{ 7, -1 }},			//    AccFace,        //14  
        new DressPart {depth = new int[]{13, -1 }},			//    AccBag, 
    };



    DressPart GetDressPart(DressType type)
    {
        return dressParts[(int)type];
    }

    void LoadDefaultDress(DressType type, string path)
    {
        DressPart part = dressParts[(int)type];
        part.initTex = new Texture2D[2];
        part.initTex[0] = Resources.Load<Texture2D>(path);
        if (part.initTex[0] == null)
        {
            Debug.LogError("LoadDefaultDress : can not load texture " + path);
        }
    }


    public void Init()
    {
        // initial hair
        int id = 11026;
        string path = null;

        // 初始头发
        DressPart part = dressParts[(int)DressType.Hair];
        part.initTex = new Texture2D[2];
        path = "Art/Dress/0_" + id.ToString() + "_1";
        part.initTex[0] = Resources.Load<Texture2D>(path);
        if (part.initTex[0] == null)
        {
            Debug.LogError("Dressing : can not load texture " + path);
        }

        // 这里可能为null，说明这件衣服只有一层
        path = "Art/Dress/0_" + id.ToString() + "_2";
        part.initTex[1] = Resources.Load<Texture2D>(path);

        // 初始服装
        LoadDefaultDress(DressType.Dress, "Art/Dress/defaultclothes");
        LoadDefaultDress(DressType.Tops, "Art/Dress/nikki_1");
        LoadDefaultDress(DressType.Bottoms, "Art/Dress/nikki_2");
    }

    void AddRenderable(Vector2 pos, Texture2D tex, int depth)
    {
        if (tex == null)
        {
            return;
        }

        DressRenderable r = AllocDressRenderable();

        // 从暖暖的坐标转换成我们的坐标
        Vector2 p = new Vector2((pos.x - 645.5f) / 2.0f,
                                (pos.y - 913.5f) / 2.0f);
        r.SetPos(p);
        r.SetTexture(tex);
        r.SetDepth(depth);
    }

    // 换装实现
    public void SetDress(DressType dressType, Dress dress)
    {
        ClearRenderables();

        GetDressPart(dressType).dress = dress;

        // 连衣裙与上下装互斥
        if (dressType == DressType.Dress)
        {
            GetDressPart(DressType.Tops).dress = null;
            GetDressPart(DressType.Bottoms).dress = null;
        }
        if (dressType == DressType.Tops ||
            dressType == DressType.Bottoms)
        {
            GetDressPart(DressType.Dress).dress = null;
        }

        // 初始头发
        if (GetDressPart(DressType.Hair).dress == null)
        {
            DressPart part = GetDressPart(DressType.Hair);
            AddRenderable(part.initPos, part.initTex[0], part.initDepth[0]);
            AddRenderable(part.initPos, part.initTex[1], part.initDepth[1]);
        }

        // 初始背心
        if (GetDressPart(DressType.Dress).dress == null &&
            GetDressPart(DressType.Tops).dress == null &&
            GetDressPart(DressType.Bottoms).dress == null &&
            GetDressPart(DressType.Coat).dress == null)
        {
            DressPart part = GetDressPart(DressType.Dress);
            AddRenderable(part.initPos, part.initTex[0], part.initDepth[0]);
        }

        // 如果有上装和下装，先用素体遮住内衣
        if (GetDressPart(DressType.Tops).dress != null)
        {
            DressPart part = GetDressPart(DressType.Tops);
            AddRenderable(part.initPos, part.initTex[0], part.initDepth[0]);
        }
        if (GetDressPart(DressType.Bottoms).dress != null)
        {
            DressPart part = GetDressPart(DressType.Bottoms);
            AddRenderable(part.initPos, part.initTex[0], part.initDepth[0]);
        }

        for (int i = 0; i < dressParts.Length; i++)
        {
            DressPart part = dressParts[i];
            Dress d = part.dress;
            if (d == null)
            {
                continue;
            }

            List<Texture2D> images = d.DressImgs;
            if (images.Count < 1 || images[0] == null)
            {
                // error
            }

            AddRenderable(d.Pos, images[0], part.depth[0]);

            if (images.Count > 1 && images[1] != null)
            {
                AddRenderable(d.Pos, images[1], part.depth[1]);
            }
        }
    }


    const int NumDressType = (int)DressType.DressTypeLength;
    List<DressRenderable> renderables = new List<DressRenderable>();
}
