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
       
    public UITexture[] texes;
    public UILabel[] showNames;
    bool[] showall;
    public UITexture bkImg;

    public GameObject kaishiRenwuObj;
    public GameObject sureObj;
    public GameObject cancelObj;
    public override void OnEnterUI()
    {
        base.OnEnterUI();
        textName.text = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrLevelIndex].levelInfo.name;
        mDialogContentInfo = PlayerUIResource.GetInstance().CurrAreaMapLevelUIInfos[PlayerUIResource.GetInstance().CurrLevelIndex].levelInfo.dialogInfo.contents.ToArray();
        dialogStrs = new string[mDialogContentInfo.Length];
        for (int i = 0; i < mDialogContentInfo.Length; ++i)
        {
            dialogStrs[i] = mDialogContentInfo[i].content;

        }

        dialogLength = dialogStrs.Length;
        mTypewriterEffect.SetInit(dialogStrs[0]);
        bkImg.mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.bkImg;
        ChangeTexes();
        UIEventListener.Get(sureObj).onClick += EnsureButton;
        UIEventListener.Get(cancelObj).onClick += CancelButton;
        kaishiRenwuObj.SetActive(false);
    }
    
    public override void OnLeaveUI()
    {
        base.OnLeaveUI();
        UIEventListener.Get(sureObj).onClick -= EnsureButton;
        UIEventListener.Get(cancelObj).onClick -= CancelButton;
    }
    public void OnClickBackground()
    {
        if (mTypewriterEffect.rightNowIndex < dialogLength)
        {
            ChangeTexes();
            mTypewriterEffect.ChangeNextTex(dialogStrs[mTypewriterEffect.rightNowIndex]);
            //Debug.Log("2222222222222222222222222222  " + mTypewriterEffect.rightNowIndex + "  " + dialogStrs.Length + "  " + PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents.Count);

            
        }
        else
        {

            if (mTypewriterEffect.rightNowIndex == dialogLength)
            {
                mTypewriterEffect.ShowLastTex();
                //ChangeTexes();
            }
            else
            {
                //TweenScale.Begin(kaishiRenwuObj,0.5f,)
                kaishiRenwuObj.SetActive(true);
            }
        }
    }

    void ChangeTexes()
    {
        showNames[0].text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcName;

        showNames[1].gameObject.SetActive(false);
        showNames[2].gameObject.SetActive(false);
        showall = new bool[3] { false, false, false };
        for (int i = 0; i < PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs.Count; ++i)
        {

            switch (PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].pos)
            {
                case DialogNpcImgShowPos.Left:
                    {
                        texes[0].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;

                        showall[0] = true;
                        texes[0].gameObject.SetActive(true);

                    }
                    break;
                case DialogNpcImgShowPos.Right:
                    {
                        texes[1].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;
                        showall[1] = true;
                        texes[1].gameObject.SetActive(true);
                    }
                    break;
                case DialogNpcImgShowPos.Middle:
                    {
                        texes[2].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;
                        showall[2] = true;
                        texes[2].gameObject.SetActive(true);
                    }
                    break;
                default:
                    Debug.LogError("wrong npcImgs the index is " + i);
                    break;
            }
        }
        for (int i = 0; i < showall.Length; ++i)
        {
            if (!showall[i])
            {
                texes[i].gameObject.SetActive(false);
                showNames[i].gameObject.SetActive(false);
            }
        }
    }

    void EnsureButton(GameObject button)
    {
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDressUI);
    }
    void CancelButton(GameObject button)
    {
        kaishiRenwuObj.SetActive(false);
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }
}

