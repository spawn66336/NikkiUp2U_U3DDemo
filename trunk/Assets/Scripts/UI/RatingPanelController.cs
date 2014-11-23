using UnityEngine;
using System.Collections;

public class RatingPanelController : UIController 
{
    public void OnClickBackground()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }
    
}
