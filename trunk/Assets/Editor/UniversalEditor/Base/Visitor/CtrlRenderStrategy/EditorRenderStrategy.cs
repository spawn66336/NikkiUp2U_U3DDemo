using UnityEngine;
using System.Collections;

public class EditorRenderStrategy  
{
    public virtual bool PreVisit(EditorControl c) { return true; }
    public virtual void Visit(EditorControl c) { }
    public virtual void AfterVisit(EditorControl c) { }
    public virtual bool PreVisitChildren(EditorControl c) { return true; }
    public virtual void AfterVisitChildren(EditorControl c) { }
    public virtual bool PreVisitChild(EditorControl c, int ichild) { return true; }
    public virtual void AfterVisitChild(EditorControl c, int ichild) { }
 
}
