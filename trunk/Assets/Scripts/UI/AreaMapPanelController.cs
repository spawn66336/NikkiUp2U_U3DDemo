using UnityEngine;
using System.Collections;

public class AreaMapPanelController : UIController 
{
    public UILabel diamondNum;
    public UILabel goldNum;
    public UILabel energyNum;
    public UILabel bigMapName;
    public UILabel mapName;
    public UILabel sNum;
    public void OnClickEnterLevelBtn()
    { 
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDialogUI);        
        //bigMapName.text=PlayerUIResource.GetInstance()
    }
    void OnEnable()
    {
        mapName.text = PlayerUIResource.GetInstance().CurrAreaMapUIInfo.name;
        sNum.text = PlayerUIResource.GetInstance().CurrAreaMapRankSLevelCount.ToString() + "/" + PlayerUIResource.GetInstance().CurrAreaMapFinishedLevelCount.ToString();
    }
    void OnDisable()
    {
 
    }
    void Update()
    {
        diamondNum.text = PlayerUIResource.GetInstance().Diamond.ToString();
        goldNum.text = PlayerUIResource.GetInstance().Gold.ToString();
        energyNum.text = PlayerUIResource.GetInstance().Energy.ToString() + "/30";
    }

    public void OnClickBackBtn()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LoginUI);
    }
    
}
