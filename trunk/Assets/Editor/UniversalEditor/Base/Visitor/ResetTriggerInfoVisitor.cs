using UnityEngine;
using System.Collections;

public class ResetTriggerInfoVisitor : EditorCtrlVisitor
{
    public override void Visit(EditorControl c)
    {
        c.frameTriggerInfo.Reset();
    }

}
