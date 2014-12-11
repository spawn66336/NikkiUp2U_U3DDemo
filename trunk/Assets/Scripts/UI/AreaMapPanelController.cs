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
        if (!enterButtoneffect.IsPlaying())
        {
            GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.CommonButtonClick2);
            enterButtoneffect.Play();
            playing = true;
        }
    }
    void ChangeToLevelDialog()
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDialogUI);        

    }
    void OnEnable()
    {
        playing = false;
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
