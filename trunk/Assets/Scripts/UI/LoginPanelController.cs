using UnityEngine;
using System.Collections;

public class LoginPanelController : UIController 
{
    public Transform loginBackground;
	 
    public void OnBackgroundClick()
    {
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.CommonButtonClick);
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }
}
