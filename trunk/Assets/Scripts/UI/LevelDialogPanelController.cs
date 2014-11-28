using UnityEngine;
using System.Collections;

public class LevelDialogPanelController : UIController 
{
    public TypewriterEffect mTypewriterEffect;
    [System.NonSerialized]
    public string[] dialogStrs;
    DialogContentUIInfo[] mDialogContentInfo;
    public UILabel textName;
    int dialogLength;
    public override void OnEnterUI()
    {
        base.OnEnterUI();
        textName.text = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrentMapLevelIndex].levelInfo.name;
        mDialogContentInfo = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrentMapLevelIndex].levelInfo.dialogInfo.contents.ToArray();
        dialogStrs = new string[mDialogContentInfo.Length];
        for (int i = 0; i < mDialogContentInfo.Length; ++i)
        {
            dialogStrs[i] = mDialogContentInfo[i].content;
        }
        dialogLength = dialogStrs.Length;
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
