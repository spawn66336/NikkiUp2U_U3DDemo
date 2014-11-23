using UnityEngine;
using System.Collections;

public class LevelDialogPanelController : UIController 
{
    

    public void OnClickBackground()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDressUI);
    }
}
