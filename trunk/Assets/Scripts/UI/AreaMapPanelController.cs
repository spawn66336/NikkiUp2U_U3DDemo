using UnityEngine;
using System.Collections;

public class AreaMapPanelController : UIController 
{

    public void OnClickEnterLevelBtn()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDialogUI);
    }
	 

    public void OnClickBackBtn()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LoginUI);
    }
}
