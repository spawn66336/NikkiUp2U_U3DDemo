using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RewardPanelManager : MonoBehaviour 
{
    public GameObject rewardBk;

    public UILabel rewardName;

    public UILabel rewardDesc;

    public UITexture rewardIcon;

    class RewardNotifyInfo
    {
        public RewardNotifyInfo(string name , string desc , Texture2D icon )
        {
            rewardName = name;
            rewardDesc = desc;
            rewardIcon = icon;
        }

        public string rewardName;
        public string rewardDesc;
        public Texture2D rewardIcon;
    }

    void Awake()
    {
        s_instance = this;
    }
     
	void Start () {
	
	}
	 
	void Update () 
    {
	    if( currRewardInfo == null )
        {
            if( infos.Count > 0 )
            {
                currRewardInfo = infos[0];
                infos.RemoveAt(0);

                rewardBk.SetActive(true);

                rewardName.text = currRewardInfo.rewardName;
                rewardDesc.text = currRewardInfo.rewardDesc;
                rewardIcon.mainTexture = currRewardInfo.rewardIcon;
            }

            if( currRewardInfo == null )
            {
                rewardBk.SetActive(false);
            }

        }
	}

    public void NewReward( string rewardName , string rewardDesc , Texture2D icon )
    {
        RewardNotifyInfo info = new RewardNotifyInfo(rewardName, rewardDesc, icon);
        infos.Add(info);
    }

    public void OnClickRewardPanel()
    {
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.RewardSound);
        currRewardInfo = null;
    }

    
    
    List<RewardNotifyInfo> infos = new List<RewardNotifyInfo>();

    RewardNotifyInfo currRewardInfo = null;

    public static RewardPanelManager GetInstance()
    {
        return s_instance;
    }

    static RewardPanelManager s_instance;
}
