using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatingPanelController : UIController 
{
	//关卡名
	public UILabel LevelName;
	//评级
	public UITexture Rank;
	//评语
	public UILabel Remark;
	//大喵头像
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
	private RatingPanelState curState;
	private List<Dress> dresses;
	private string rank = "F";

	private float fromStarttime  = 0f;
	private float startTime = 0f;
	private bool playflag = false;
	private bool playrank = false;
	private bool locked = false;
	private SoundManager.SoundType stype;

	public void Update()
	{
		if(startTime == 0f)
			return;
		if(curState == RatingPanelState.Enter)
		{
			fromStarttime = Time.time - startTime;
			//计分板滚动
			if(fromStarttime >= 0 && fromStarttime < 1.0)
			{
				S_5.PlayAnimation();
			}

			if(fromStarttime >= 0.2 && fromStarttime < 1.2)
			{
				S_4.PlayAnimation();
			}

			if(fromStarttime >= 0.4 && fromStarttime < 1.4)
			{
				S_3.PlayAnimation();
			}

			if(fromStarttime >= 0.6 && fromStarttime < 1.6)
			{
				S_2.PlayAnimation();
			}
			if(fromStarttime >= 0.8 && fromStarttime < 1.8)
			{
				S_1.PlayAnimation();
			}
			//计分板移动缩放
			if(fromStarttime >= 2.0 && !playflag)
			{
				_playAnimation(ScorePanel);
				playflag = true;
			}
			//计分板停止
			if(fromStarttime >= 1.0)
			{
				S_5.SetPos(info.score%10);
			}
			if(fromStarttime >= 1.2)
			{
				S_4.SetPos(info.score/10%10);
			}
			if(fromStarttime >= 1.4)
			{
				S_3.SetPos(info.score/100%10);
			}
			if(fromStarttime >= 1.6)
			{
				S_2.SetPos(info.score/1000%10);
			}
			if(fromStarttime >= 1.8)
			{
				S_1.SetPos(info.score/10000%10);
				GlobalObjects.GetInstance().GetSoundManager().Stop(SoundManager.SoundType.ScoreBoardSound);
				Defen.gameObject.SetActive(false);
			}

			if(locked && playflag && fromStarttime >= 3.0)
			{
				locked = false;
				fromStarttime = 0;
				startTime = 0f;
			}
		}else if(curState == RatingPanelState.Rank)
		{
			fromStarttime = Time.time - startTime;
			if(locked && playrank && fromStarttime > 1.0f)
			{
				locked = false;
			}
		}
	}

	public override void OnEnterUI ()
	{
		base.OnEnterUI ();

		curState = RatingPanelState.Enter;
		playflag = false;
		playrank = false;
		fromStarttime = 0f;

		info = PlayerUIResource.GetInstance().CurrRaingUIInfo;
		_RankToString();

		if(info.score < 0)
			info.score = 0;
		//关卡名
		if (PlayerUIResource.GetInstance().CurrLevelUIInfo != null
		    && PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo != null)
		{	
			LevelName.text = PlayerUIResource.GetInstance().CurrLevelUIInfo.levelInfo.name;
		}
		else
		{
			LevelName.text = "关卡名获取失败！";
		}
		//服饰搭配
		dresses = PlayerUIResource.GetInstance().CurrLevelDressList;
		foreach(Dress drs in dresses)
		{
			Nikki.SetDress(drs.ClothType, drs);
		}   
		//大喵表情
		string icon;
		if(rank == "F")
			icon = "p_dm_F2";
		else
			icon = "p_dm_" + rank;
		Miao.mainTexture = ResourceManager.GetInstance().Load(ResourceType.Npc, icon) as Texture;

		startTime = Time.time;
		locked = true;
		GlobalObjects.GetInstance().GetSoundManager().Play(SoundManager.SoundType.ScoreBoardSound);
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
		
		float speed = 1.2f;
		float time = 0f;

		anim[defaultClip.name].normalizedTime = time;
		anim[defaultClip.name].speed = speed;
		ActiveAnimation.Play(anim,AnimationOrTween.Direction.Forward);
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

	void _RankToString()
	{
		switch(info.levelRank)
		{
		case LevelRank.SSS:
			rank = "SSS";
			stype = SoundManager.SoundType.LevelRankS;
			break;
		case LevelRank.SS:
			rank = "SS";
			stype = SoundManager.SoundType.LevelRankS;
			break;
		case LevelRank.S:
			rank = "S";
			stype = SoundManager.SoundType.LevelRankS;
			break;
		case LevelRank.A:
			rank = "A";
			stype = SoundManager.SoundType.LevelRankABC;
			break;
		case LevelRank.B:
			rank = "B";
			stype = SoundManager.SoundType.LevelRankABC;
			break;
        case LevelRank.C:
			rank = "C";
			stype = SoundManager.SoundType.LevelRankABC;
			break;
		case LevelRank.F:
			rank = "F";
			stype = SoundManager.SoundType.LevelRankF;
			break;
		default:
			rank = "F";
			break;
		}
	}

	void _OnKickRankShowTransition()
	{
		_ActiveRank();
	}

	void _ActiveRank()
	{
		locked = true;

		string icon = rank + "_0";
		Rank.mainTexture = ResourceManager.GetInstance().Load(ResourceType.CommentNpc, icon) as Texture;

		Rank.gameObject.SetActive(true);

		startTime = Time.time;
		_playAnimation(Rank.gameObject);
		GlobalObjects.GetInstance().GetSoundManager().Play(stype);

		playrank = true;
	}

	void _OnKickRemarkShowTransition()
	{
		if(info.comment != null)
			Remark.text = info.comment;
		else
			Remark.text = "NULL Comment!";
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
		if(!locked)
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
    
}
