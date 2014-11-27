using UnityEngine;
using System.Collections;

public class LevelDialogPanelController : UIController 
{
    public TypewriterEffect mTypewriterEffect;
    public string[] dialogStrs;
    int dialogLength;
    public override void OnEnterUI()
    {
        base.OnEnterUI();
        dialogLength= dialogStrs.Length;
        mTypewriterEffect.SetInit(dialogStrs[0]);
    }
    public override void OnLeaveUI()
    {
        base.OnLeaveUI();
    }
    public void OnClickBackground()
    {
        if (mTypewriterEffect.rightNowIndex < dialogLength)
        {
            mTypewriterEffect.ChangeNextTex(dialogStrs[mTypewriterEffect.rightNowIndex]);
            
        }
        else
        {

            if (mTypewriterEffect.rightNowIndex == dialogLength)
            {
                mTypewriterEffect.ShowLastTex();
            }
            else
            {
                GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDressUI);
            }
        }
    }
}
