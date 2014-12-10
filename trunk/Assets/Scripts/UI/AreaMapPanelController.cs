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
    public GameObject enterButton;
    public SpecialEffect enterButtoneffect;
    bool playing;
    public void OnClickEnterLevelBtn(GameObject button)
    {
        enterButtoneffect.gameObject.SetActive(true);
        if (!enterButtoneffect.IsPlaying())
        {
            enterButtoneffect.Play();
            playing = true;
            //Invoke("ChangeToLevelDialog", 1);
        }
        //bigMapName.text=PlayerUIResource.GetInstance()
    }
    void ChangeToLevelDialog()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDialogUI);        

    }
    void OnEnable()
    {
        playing = false;
        enterButtoneffect.gameObject.SetActive(false);
        mapName.text = PlayerUIResource.GetInstance().CurrAreaMapUIInfo.name;
        sNum.text = PlayerUIResource.GetInstance().CurrAreaMapRankSLevelCount.ToString() + "/" + PlayerUIResource.GetInstance().CurrAreaMapFinishedLevelCount.ToString();
        UIEventListener.Get(enterButton).onClick += OnClickEnterLevelBtn;
    }
    void OnDisable()
    {
        UIEventListener.Get(enterButton).onClick -= OnClickEnterLevelBtn;

    }
    void Update()
    {
        if (playing)
        {
            if (!enterButtoneffect.IsPlaying())
            {
                playing = false;
                ChangeToLevelDialog();
            }
        }
        diamondNum.text = PlayerUIResource.GetInstance().Diamond.ToString();
        goldNum.text = PlayerUIResource.GetInstance().Gold.ToString();
        energyNum.text = PlayerUIResource.GetInstance().Energy.ToString() + "/30";
    }

    public void OnClickBackBtn()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LoginUI);
    }
    
}
