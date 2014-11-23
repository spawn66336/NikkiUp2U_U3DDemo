using UnityEngine;
using System.Collections;

public class LoginPanelController : UIController 
{
    public Transform loginBackground;
	 
    public void OnBackgroundClick()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }
}
