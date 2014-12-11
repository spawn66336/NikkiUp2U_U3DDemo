using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RewardPanelManager : MonoBehaviour 
{
    public GameObject rewardBk;

    public UILabel rewardName;

    public UILabel rewardDesc;

    public UITexture rewardIcon;  

    //点击后特效
    public SpecialEffect rewardSpe;

    Animation rewardAnim;
     

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
     
	void Start () 
    {
        rewardAnim = rewardBk.GetComponent<Animation>();
	}
	 
	void Update () 
    {
	    if( currRewardInfo == null )
        {
            if( !_ExitAnimFinished() )
            {
                return;
            }
             
            if (UILocker.GetInstance().IsLocked())
            {
                rewardSpe.Stop();
                UILocker.GetInstance().ForceUnlock();
            }

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

    //退出动画播放完毕
    bool _ExitAnimFinished()
    {
        return !rewardAnim.isPlaying;
    }


    public void NewReward( string rewardName , string rewardDesc , Texture2D icon )
    {
        RewardNotifyInfo info = new RewardNotifyInfo(rewardName, rewardDesc, icon);
        infos.Add(info);
    }

    public void OnClickRewardPanel()
    {
        GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.RewardSound);

        UILocker.GetInstance().Lock(this.gameObject);

        rewardAnim[rewardAnim.clip.name].speed = 1f;
        rewardAnim[rewardAnim.clip.name].normalizedTime = 0;
        rewardAnim.Play();

        rewardSpe.Play();
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
