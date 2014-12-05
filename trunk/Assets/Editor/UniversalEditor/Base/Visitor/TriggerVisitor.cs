using UnityEngine;
using System.Collections;

public class TriggerVisitor : EditorCtrlVisitor 
{
    public override void Visit(EditorControl c)
    {
        if( c.frameTriggerInfo.isClick )
        {
            if( null != c.onClick )
            {
                c.onClick(c);
            }
        }

        if( c.frameTriggerInfo.isHover )
        {
            if( null != c.onHover )
            {
                c.onHover(c);
            }
        }

        if( c.frameTriggerInfo.isValueChanged )
        {
            if( null != c.onValueChange )
            {
                c.onValueChange(c, c.CurrValue);
            }
        }

        if( c.frameTriggerInfo.lastSelectItem != -1 )
        {
            if( null != c.onItemSelected )
            {
                c.onItemSelected(c, c.frameTriggerInfo.lastSelectItem);
            }
        }

        //add by liteng for atlas start
        if (c.frameTriggerInfo.lastSelectItemR != -1)
        {
            if (null != c.onItemSelectedR)
            {
                c.onItemSelectedR(c, c.frameTriggerInfo.lastSelectItemR);
            }
        }

        if (c.frameTriggerInfo.lastSelectItemRU != -1)
        {
            if (null != c.onItemSelectedRU)
            {
                c.onItemSelectedRU(c, c.frameTriggerInfo.lastSelectItemRU);
            }
        }
        //add by liteng end
        if( c.frameTriggerInfo.isScroll )
        {
            if( null != c.onScroll )
            {
                c.onScroll(c, c.frameTriggerInfo.scrollPos);
            }
        }


        if( c.frameTriggerInfo.isDraggingObjs )
        {
            if( null != c.onDragingObjs )
            {
                c.onDragingObjs(c, FrameInputInfo.GetInstance().dragObjs, FrameInputInfo.GetInstance().dragObjsPaths);
            }
        }

        if( c.frameTriggerInfo.isDropObjs )
        {
            if( null != c.onDropObjs )
            {
                c.onDropObjs(c, FrameInputInfo.GetInstance().dragObjs, FrameInputInfo.GetInstance().dragObjsPaths);
            }
        }

    }
	 
}
