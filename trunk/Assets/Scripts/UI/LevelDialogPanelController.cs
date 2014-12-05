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
    public float[] yValue;//0 nikki 1 yoyo 2 neko
    public string[] containsName;
    public GameObject backMapButton;
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
        UIEventListener.Get(backMapButton).onClick += BackToMapEvent;
    }
    
    public override void OnLeaveUI()
    {
        base.OnLeaveUI();
        UIEventListener.Get(sureObj).onClick -= EnsureButton;
        UIEventListener.Get(cancelObj).onClick -= CancelButton;
        UIEventListener.Get(backMapButton).onClick -= BackToMapEvent;
    }
    public void OnClickBackground()
    {
        if (mTypewriterEffect.rightNowIndex < dialogLength)
        {
            ChangeTexes();
            mTypewriterEffect.ChangeNextTex(dialogStrs[mTypewriterEffect.rightNowIndex]);

            
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

