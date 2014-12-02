using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class FindControlByTypeVisitor<T> : EditorCtrlVisitor 
{ 
   public List<EditorControl> results = new List<EditorControl>();
   public override void Visit(EditorControl c)
   { 
       if (c.GetType() == typeof(T))
       {
           results.Add(c);
       } 
   }
	 
}
