using UnityEngine;
using System.Collections;

public class UpdateVisitor : EditorCtrlVisitor
{
    private EditorUpdateStrategy playCtrlUpdateStrategy = new PlayCtrlUpdateStrategy();


    private EditorUpdateStrategy _GetStrategy(EditorControl c)
    {
        System.Type ctrlType = c.GetType();

        if( ctrlType == typeof(PlayCtrl))
        {
            return playCtrlUpdateStrategy;
        }
        return null;
    }

    public override void Visit(EditorControl c)
    {
        EditorUpdateStrategy strategy = _GetStrategy(c);
        if( null != strategy )
        {
            strategy.Update(c,deltaTime);
        }
    }


    public void _InternalUpdate( float dt)
    {
        deltaTime = dt;
    }

    float deltaTime = 0.0f;
}
