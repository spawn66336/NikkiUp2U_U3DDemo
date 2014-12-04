using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatingPanelController : UIController 
{
	public UILabel LevelName;
	//评级
	public UITexture Rank;
	//评语
	public UILabel Remark;
	public UITexture Miao;
	//评分板
	public GameObject ScorePanel; 
	//得分
	public UITexture Defen;

	public Actor Nikki;

	public UINumPos S_1;
	public UINumPos S_2;
	public UINumPos S_3;
	public UINumPos S_4;
	public UINumPos S_5;

	enum RatingPanelState
	{
		Enter,
		Rank,
		Remark,
		Reward,
		Leave
	}

	private PlayerLevelRewardUIInfo info;
	private RatingPanelState curState = RatingPanelState.Enter;
	private List<Dress> dresses;
	private string rank = "F";

	public override void OnEnterUI ()
	{
		base.OnEnterUI ();
		Rank.gameObject.SetActive(false);
		info = PlayerUIResource.GetInstance().CurrRaingUIInfo;
		
		if (PlayerUIResource.GetInstance().CurrLevelUIInfo != null
		    && PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo != null)
		{	
			LevelName.text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.name;
		}
		else
		{
			LevelName.text = "关卡名获取失败！";
		}

		dresses = PlayerUIResource.GetInstance().CurrLevelDressList;
		foreach(Dress drs in dresses)
		{
			Nikki.SetDress(drs.ClothType, drs);
		}

		UILocker.GetInstance().Lock(gameObject);
	    
		_PlayScorePanel();

		UILocker.GetInstance().UnLock(gameObject);
	}

	private void _PlayScorePanel()
	{
		if(info.score < 0)
			info.score = 0;
		
		StartCoroutine(_WaitPlayNum(0.2f,S_1));
		StartCoroutine(_WaitSet(1.2f, S_1, info.score/10000%10));
		StartCoroutine(_WaitPlayNum(0.4f,S_2));
		StartCoroutine(_WaitSet(1.4f, S_2, info.score/1000%10));
		StartCoroutine(_WaitPlayNum(0.8f,S_3));
		StartCoroutine(_WaitSet(1.6f, S_3, info.score/100%10));
		StartCoroutine(_WaitPlayNum(1.0f,S_4));
		StartCoroutine(_WaitSet(1.8f, S_4, info.score/10%10));
		StartCoroutine(_WaitPlayNum(1.2f,S_5));
		StartCoroutine(_WaitSet(2.0f, S_5, info.score/10%10));
		
		Defen.gameObject.SetActive(false);
		
		StartCoroutine(_WaitPlayAnimation(2.0f));
	}

	IEnumerator _WaitSet(float waitTime, UINumPos num, int i)
	{
		yield return new WaitForSeconds(waitTime);

		num.SetPos(i);
	}

	IEnumerator _WaitPlayAnimation(float waitTime)
	{
		yield return new WaitForSeconds(waitTime);
		_playAnimation(ScorePanel);
	}

	IEnumerator _WaitPlayNum(float waitTime, UINumPos num)
	{
		yield return new WaitForSeconds(waitTime);
		num.PlayAnimation();
	}

	public void AnimFinish()
	{
		UILocker.GetInstance().UnLock(gameObject);
		StopAllCoroutines();
	}

	void _playAnimation(GameObject go)
	{
		Animation anim = go.GetComponent<Animation>();
		if (anim == null)
		{
			return;
		}
		
		AnimationClip defaultClip = anim.clip;
		
		if (defaultClip == null)
			return;
		
		float speed = 1f;

		anim[defaultClip.name].wrapMode = WrapMode.Once;
		anim[defaultClip.name].speed = speed;
		
		ActiveAnimation.Play(anim, AnimationOrTween.Direction.Forward);
	}

	void _SetNextState(RatingPanelState state)
	{
		curState = state;
		_KickStateTransition();
	}

	void _KickStateTransition()
	{
		switch(curState)
		{
			case RatingPanelState.Rank:
				_OnKickRankShowTransition();
				break;
			case RatingPanelState.Remark:
				_OnKickRemarkShowTransition();
				break;
			case RatingPanelState.Reward:
				_OnKickRewardShowTransition();
				break;
		}
	}

	void _OnKickRankShowTransition()
	{
		switch(info.levelRank)
		{
			case LevelRank.S:
				rank = "S";
				break;
			case LevelRank.A:
				rank = "A";
				break;
			case LevelRank.B:
				rank = "B";
				break;
			case LevelRank.C:
				rank = "C";
				break;
			case LevelRank.D:
				rank = "D";
				break;
			case LevelRank.F:
				rank = "F";
				break;
			default:
				rank = "F";
				break;
		}
		_ActiveRank();
	}

	void _ActiveRank()
	{
		string icon = rank + "_0";
		Rank.mainTexture = ResourceManager.GetInstance().Load(ResourceType.CommentNpc, icon) as Texture;

		Rank.gameObject.SetActive(true);

		Animation anim = Rank.animation;
		if (anim == null)
		{
			return;
		}
		
		AnimationClip defaultClip = anim.clip;
		
		if (defaultClip == null)
			return;
		
		float speed = 1f;
		
		anim[defaultClip.name].wrapMode= WrapMode.Once;
		anim[defaultClip.name].speed = speed;
		anim.Play();
	}

	void _OnKickRemarkShowTransition()
	{
		string icon;

		if(rank != "A" && rank != "B" && rank != "C" && rank != "S")
			icon = "p_dm_C"; 
		else
			icon = "p_dm_" + rank;
		
		Miao.mainTexture = ResourceManager.GetInstance().Load(ResourceType.CommentNpc, icon) as Texture;
		//Miao.gameObject.SetActive(true);

		if(info.comment != null)
			Remark.text = info.comment;
		else
			Remark.text = "VeryGood";
	}
	void _OnKickRewardShowTransition()
	{
		foreach(BagItemUIInfo item in info.rewards)
		{
			RewardPanelManager.GetInstance().NewReward(item.item.Name, item.item.Desc, item.item.Icon);
		}
	}

    public void OnClickBackground()
    {
		switch(curState)
		{
			case RatingPanelState.Enter:
				_SetNextState(RatingPanelState.Rank);
				break;
			case RatingPanelState.Rank:
				_SetNextState(RatingPanelState.Remark);
				break;
			case RatingPanelState.Remark:
				_SetNextState(RatingPanelState.Reward);
				break;
			case RatingPanelState.Reward:
				GlobalObjects.GetInstance().GetUISwitchManager().SetNextState(UIState.AreaMapUI);
				break;
		}
    }
    
}
