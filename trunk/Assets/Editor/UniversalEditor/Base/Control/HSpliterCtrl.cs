using UnityEngine;
using System.Collections;

public class HSpliterCtrl : SpliterCtrl
{
    public override GUIStyle GetStyle() 
    {
        return SpecialEffectEditorStyle.HDivider; 
    }
    public override GUILayoutOption[] GetOptions() 
    {
        return SpecialEffectEditorOption.hDividerOptions; 
    }

   

}
