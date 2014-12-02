using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindControlByNameVisitor : EditorCtrlVisitor 
{
    public string name;
    public List<EditorControl> results = new List<EditorControl>();

    public override void Visit(EditorControl c)
    {
        if (c.Name == name )
        {
            results.Add(c);
        }
    }
}
