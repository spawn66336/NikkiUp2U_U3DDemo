using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class LayoutUpdateVisitor : EditorCtrlVisitor 
{ 
    //栈顶元素为当前控件可用区域
    public Stack<Rect> areaStack = new Stack<Rect>();
    public override void Visit(EditorControl c) 
    {
        Rect currArea = areaStack.Peek();
        if (c.GetType() == typeof(HBoxCtrl))
        {
            c.Size = currArea;
        }
        else if (c.GetType() == typeof(VBoxCtrl))
        {
            c.Size = currArea;
        }
    }

    public override bool PreVisitChild(EditorControl c, int i) 
    {
        Rect currArea = areaStack.Peek();
        Rect newArea = new Rect();
        LayoutConstraint constraint = c.layoutConstraint;
        float part0Width, part0Height, part1Width, part1Height;

        if (c.GetType() == typeof(HSpliterCtrl))
        {
           if( !constraint.spliterOffsetInv )
           {
               part0Height = constraint.spliterOffset;
               part1Height = currArea.height - constraint.spliterOffset;
               if (part1Height < 0.0f)
                   part1Height = 1.0f;
           }
           else
           {
               part1Height = constraint.spliterOffset;
               part0Height = currArea.height - constraint.spliterOffset;
               if (part0Height < 0.0f)
                   part0Height = 1.0f;
           }
            
            if( i == 0 )
            {
                newArea.Set(0, 0, currArea.width, part0Height);
            }
            else if (i == 1)
            { 
                newArea.Set(0, 0, currArea.width, part1Height);
            }
            areaStack.Push(newArea);
        }
        else if (c.GetType() == typeof(VSpliterCtrl))
        {
            if (!constraint.spliterOffsetInv)
            {
                part0Width = constraint.spliterOffset;
                part1Width = currArea.width - constraint.spliterOffset;
                if (part1Width < 0.0f)
                    part1Width = 1.0f;
            }
            else
            {
                part1Width = constraint.spliterOffset;
                part0Width = currArea.width - constraint.spliterOffset;
                if (part0Width < 0.0f)
                    part0Width = 1.0f;

            }

            if (i == 0)
            {
                newArea.Set(0, 0, part0Width, currArea.height);
            }
            else if (i == 1)
            { 
                newArea.Set(0, 0, part1Width, currArea.height);
            }
            areaStack.Push(newArea);
        }else if( c.GetType() == typeof(HBoxCtrl) )
        {

        }else if( c.GetType() == typeof(VBoxCtrl) )
        {

        }

        return true;
    }
    public override void AfterVisitChild(EditorControl c, int i) 
    {
        Rect currArea = areaStack.Peek();

        if (c.GetType() == typeof(HSpliterCtrl))
        {
            areaStack.Pop();
        }
        else if (c.GetType() == typeof(VSpliterCtrl))
        {
            areaStack.Pop();
        }
        else if (c.GetType() == typeof(HBoxCtrl))
        { 
        }
        else if (c.GetType() == typeof(VBoxCtrl))
        {  
        }
    }
}
