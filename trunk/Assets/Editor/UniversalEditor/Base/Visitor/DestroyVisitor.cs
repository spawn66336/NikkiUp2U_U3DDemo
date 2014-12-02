using UnityEngine;
using System.Collections;

public class DestroyVisitor : EditorCtrlVisitor 
{
    EditorDestroyStrategy mainViewStrategy = new MainViewDestroyStrategy();

    private EditorDestroyStrategy _GetStrategy( EditorControl c )
    {
        System.Type ctrlType = c.GetType();

        if( ctrlType == typeof(MainViewCtrl))
        {
            return mainViewStrategy;
        }
        return null;
    }

    public override void Visit(EditorControl c)
    {
        EditorDestroyStrategy strategy = _GetStrategy(c);
        if( strategy != null )
        {
            strategy.Destroy(c);
        }
    }
}
