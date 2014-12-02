using UnityEngine;
using System.Collections;
using UnityEditor;

public class SliderRenderStrategy : EditorRenderStrategy 
{
    public override void Visit(EditorControl c)
    {
        float newValue = EditorGUILayout.Slider(c.CurrValue, c.ValueRange.x, c.ValueRange.y, c.GetOptions());
        
        if( Mathf.Abs(c.CurrValue - newValue) > c.ValueEpsilon )
        { 
            c.frameTriggerInfo.isValueChanged = true;
        }
        c.CurrValue = newValue;
    }
}
