using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DressSet : MonoBehaviour 
{

    public void SetDress(DressType dressType , Dress dress )
    {
        if( dress == null )
        {
            foreach( var d in dressList )
            {
                if( d.ClothType == dressType )
                {
                    dressList.Remove(d);
                    IsDirty = true;
                    break;
                }
            } 
            return;
        }

        dressList.Add(dress);
        IsDirty = true; 
    }

    public void ClearDress()
    {
        dressList.Clear();
        IsDirty = true;
    }
    
    public List<Dress> GetDressList() { return dressList; }

    public DressRenderable AllocDressRenderable()
    {
        foreach( var r in renderables )
        {
            //找到一个已存在的未启用Renderable
            if( !r.gameObject.activeInHierarchy )
            {
                r.gameObject.SetActive(true);
                return r;
            }
        }

        //池中已没有可用Renderable
        GameObject go = new GameObject("DressRenderable", 
            new System.Type[]{ 
                typeof(DressRenderable),
                typeof(UITexture)});

        go.transform.parent = this.transform;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one;

        DressRenderable newRenderable = go.GetComponent<DressRenderable>();
        renderables.Add(newRenderable);
        return newRenderable;
    }

    public void ClearRenderables()
    {
        foreach( var r in renderables )
        {
            r.gameObject.SetActive(false);
        }
    }

    void Rebuild()
    {
        if (IsDirty)
        {
            checkStrategy.Update(this);
            renderStrategy.Update(this);
            IsDirty = false;
        }
    }        

    void Start()
    {

    }

    void Update()
    {
        Rebuild();
    }
    

    bool IsDirty
    {
        get { return isDirty; }
        set { isDirty = value; }
    }

    
    DefaultDressSetCheckStrategy checkStrategy = new DefaultDressSetCheckStrategy();
    DefaultDressSetRenderStrategy renderStrategy = new DefaultDressSetRenderStrategy();
    List<Dress> dressList = new List<Dress>(); 
    List<DressRenderable> renderables = new List<DressRenderable>();
    bool isDirty = false;
}
