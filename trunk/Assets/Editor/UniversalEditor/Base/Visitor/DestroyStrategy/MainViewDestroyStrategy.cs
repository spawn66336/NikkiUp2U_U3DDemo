using UnityEngine;
using System.Collections;

public class MainViewDestroyStrategy : EditorDestroyStrategy 
{
    public override void Destroy(EditorControl c) 
    {
        currCtrl = c as MainViewCtrl;

        if( null == currCtrl )
        {
            return;
        }

        if( currCtrl.mainViewRoot != null )
        {
            GameObject.DestroyImmediate(currCtrl.mainViewRoot);
            currCtrl.mainViewRoot = null;
        }
        
    }

    MainViewCtrl currCtrl = null;
}
