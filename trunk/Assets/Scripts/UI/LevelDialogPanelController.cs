﻿using UnityEngine;
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
    public float[] yValue;//0 nikki 1 yoyo 2 neko
    public string[] containsName;
    public GameObject backMapButton;

    public GameObject tiaoGuoButton;
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
        mTypewriterEffect.rightNowIndex = 0;
        bkImg.mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.bkImg;
        ChangeTexes();
        mTypewriterEffect.rightNowIndex = 1;
        UIEventListener.Get(sureObj).onClick += EnsureButton;
        UIEventListener.Get(cancelObj).onClick += CancelButton;
        kaishiRenwuObj.SetActive(false);
        UIEventListener.Get(backMapButton).onClick += BackToMapEvent;
        UIEventListener.Get(tiaoGuoButton).onClick += JumpToChange;
//        Debug.Log("***************************currlevel index  " + PlayerUIResource.GetInstance().CurrLevelIndex + "   " + PlayerUIResource.GetInstance().CurrAreaMapFinishedLevelCount);
        if (PlayerUIResource.GetInstance().CurrLevelIndex < (PlayerUIResource.GetInstance().CurrAreaMapFinishedLevelCount))
        {
            tiaoGuoButton.SetActive(true);
        }
        else
        {
            tiaoGuoButton.SetActive(false);
        }
    }

    private void JumpToChange(GameObject go)
    {        
        kaishiRenwuObj.SetActive(true);
    }
    
    public override void OnLeaveUI()
    {
        base.OnLeaveUI();
        UIEventListener.Get(sureObj).onClick -= EnsureButton;
        UIEventListener.Get(cancelObj).onClick -= CancelButton;
        UIEventListener.Get(backMapButton).onClick -= BackToMapEvent;
        UIEventListener.Get(tiaoGuoButton).onClick -= JumpToChange;
    }
    public void OnClickBackground()
    {
        if (mTypewriterEffect.rightNowIndex < dialogLength)
        {
            ChangeTexes();
            mTypewriterEffect.ChangeNextTex(dialogStrs[mTypewriterEffect.rightNowIndex]);
//            Debug.Log(" 00000  mTypewriterEffect.rightNowIndex  " + mTypewriterEffect.rightNowIndex + " dialog length  " + dialogLength);
            
        }
        else
        {

            if (mTypewriterEffect.rightNowIndex == dialogLength)
            {
                //Debug.Log("index is last   " + dialogLength + "  reset  " + mTypewriterEffect.mReset + "  rightnow index  " + mTypewriterEffect.rightNowIndex);
                if (mTypewriterEffect.mReset)
                {
                    mTypewriterEffect.ShowLastTex();
                }
                else
                {

                    kaishiRenwuObj.SetActive(true);
                }
            }
            else
            {
                kaishiRenwuObj.SetActive(true);
            }
        }
    }

    void ChangeTexes()
    {
        //Debug.Log("1111111111111   "+mTypewriterEffect.rightNowIndex);
       
        showall = new bool[3] { false, false, false };
        for (int i = 0; i < PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs.Count; ++i)
        {

            switch (PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].pos)
            {
                case DialogNpcImgShowPos.Left:
                    {
                        texes[0].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;
                        texes[0].SetDimensions(texes[0].mainTexture.width, texes[0].mainTexture.height);  
                        for(int j=0;j<containsName.Length;++j)
                        {
                          if(texes[0].mainTexture.name.Contains(containsName[j]))
                          {
                              texes[0].transform.position = new Vector3(-0.3f, yValue[j], 0);
                          }
                        }
                        texes[0].depth = 1;
                        showall[0] = true;
                        texes[0].gameObject.SetActive(true);
                     

                    }
                    break;
                case DialogNpcImgShowPos.Right:
                    {
                        texes[1].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;
                        texes[1].SetDimensions(texes[1].mainTexture.width, texes[1].mainTexture.height);
                        for (int j = 0; j < containsName.Length; ++j)
                        {
                            if (texes[1].mainTexture.name.Contains(containsName[j]))
                            {
                                texes[1].transform.position = new Vector3(0.3f, yValue[j], 0);
                            }
                        }
                        texes[1].depth = 1;
                        //texes[1].transform.position = new Vector3(0.3f, texes[1].mainTexture.height * 0.5f, 0);
                        showall[1] = true;
                        texes[1].gameObject.SetActive(true);
                       
                    }
                    break;
                case DialogNpcImgShowPos.Middle:
                    {
                        texes[2].mainTexture = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcImgs[i].img;
                        texes[2].SetDimensions(texes[2].mainTexture.width, texes[2].mainTexture.height);
                        for (int j = 0; j < containsName.Length; ++j)
                        {
                            if (texes[2].mainTexture.name.Contains(containsName[j]))
                            {
                                texes[2].transform.position = new Vector3(0, yValue[j], 0);
                            }
                        }
                        texes[2].depth = 1;
                        //texes[2].transform.position = new Vector3(0, texes[2].mainTexture.height * 0.5f, 0);
                        showall[2] = true;
                        texes[2].gameObject.SetActive(true);
                       
                    }
                    break;
                default:
                    Debug.LogError("wrong npcImgs the index is " + i);
                    break;
            }
            switch (PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcNameShowPos)
            {
                case DialogNpcImgShowPos.Left:
                       showNames[0].text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcName;
                       showNames[0].gameObject.SetActive(true);
                       showNames[1].gameObject.SetActive(false);
                       showNames[2].gameObject.SetActive(false);
                    break;
                case DialogNpcImgShowPos.Right:
                     showNames[1].text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcName;
                     showNames[1].gameObject.SetActive(true);
                        showNames[0].gameObject.SetActive(false);
                        showNames[2].gameObject.SetActive(false);
                    break;
                case DialogNpcImgShowPos.Middle:
                     showNames[2].text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.dialogInfo.contents[mTypewriterEffect.rightNowIndex].npcName;
                     showNames[2].gameObject.SetActive(true);
                     showNames[0].gameObject.SetActive(false);
                     showNames[1].gameObject.SetActive(false);
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
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.CommonButtonClick);
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.LevelDressUI);
    }
    void CancelButton(GameObject button)
    {
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.CommonButtonClick);
        kaishiRenwuObj.SetActive(false);
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
    }

    void BackToMapEvent(GameObject button)
    {        
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.CommonButtonClick);
        GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);

    }
}

