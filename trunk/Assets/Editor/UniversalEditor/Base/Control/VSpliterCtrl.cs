using UnityEngine;
using System.Collections;

public class VSpliterCtrl : SpliterCtrl
{
    public override GUIStyle GetStyle()
    {
        return SpecialEffectEditorStyle.VDivider;
    }
    public override GUILayoutOption[] GetOptions()
    {
        return SpecialEffectEditorOption.vDividerOptions;
    }
    
 
}
