using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.Xml.Serialization;

//序列化类定义
//区域地图
public class AreaMaps
{
	List<Map> maps;
	[XmlElementAttribute("Map")]
	public List<Map> Maps
	{
		get { return maps; }
		set { maps = value; }
	}
}
[Serializable]
public class Map
{
	int id;
	string name;
	string iconid;
	List<Level> levs = new List<Level> ();
	List<string> imageList;
	List<Unlock> unlockCond = new List<Unlock>();
	
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value;}
	}
	[XmlAttribute]
	public string Name
	{
		get { return name; }
		set { name = value;}
	}
	[XmlAttribute]
	public string IconId
	{
		get { return iconid; }
		set { iconid = value; }
	}
	[XmlElementAttribute("MapUnlockCondition")]
	public List<Unlock> UnlockCond
	{
		get { return unlockCond; }
		set { unlockCond = value; }
	}
	[XmlArray("ImageList"), XmlArrayItem("Image")]
	public List<string> ImgList
	{
		set { imageList = value; }
		get { return imageList; }
	}
	[XmlElementAttribute("Level")]
	public List<Level> Levs
	{
		get { return levs; }
		set { levs = value; }
	}
}
//关卡
public class Level
{
	int id;
	string name;
	LevelRank finishGrade;
	int limiteTime;
	int rewardid;
	int dialogid;
	RatingRule ratingRule;
	RatingFactor theme;
	
	List<Unlock> cond;
	
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value; }
	}
	[XmlAttribute]
	public string Name
	{
		get { return name; }
		set { name = value; }
	}
	[XmlAttribute]
	public LevelRank FinishGrade
	{
		get { return finishGrade; }
		set { finishGrade = value; }
	}
	[XmlAttribute]
	public int LimiteTime
	{
		get { return limiteTime; }
		set { limiteTime = value; }
	}
	[XmlAttribute]
	public int RewardId
	{
		get { return rewardid; }
		set { rewardid = value; }
	}
	[XmlAttribute]
	public int DialogId
	{
		get { return dialogid; }
		set { dialogid = value; }
	}
	[XmlElementAttribute("RatingRule")]
	public RatingRule RRule
	{
		get { return ratingRule; }
		set { ratingRule = value; }
	}
	[XmlElementAttribute("UnLockCondition")]
	public List<Unlock> Cond
	{
		get { return cond; }
		set { cond = value; }
	}
	[XmlElementAttribute("RatingFactor")]
	public RatingFactor Thm
	{
		get { return theme; }
		set { theme = value;}
	}
}
public class RatingRule
{
	List<GradeInfo> grdsc = new List<GradeInfo>();
	[XmlElementAttribute("GradeInfo")]
	public List<GradeInfo> GradeSc
	{
		get { return grdsc; }
		set { grdsc = value; }
	}
}

public class RatingFactor
{
	List<Style> styles = new List<Style>();
	List<Atribute> attrs = new List<Atribute>();
	List<SpecialItem> speItems = new List<SpecialItem>();
	List<FixItem> fixItems = new List<FixItem>();
    List<Atribute> attStyleList = new List<Atribute>();
    List<Atribute> attMatrialList = new List<Atribute>();

    public List<Atribute> AttStyleList
    {
        get { return attStyleList; }
        set{attStyleList = value;}
    }
    public List<Atribute> AttMatrialList
    {
        get { return attMatrialList; }
        set { attMatrialList = value; }
    }
	[XmlElementAttribute("Style")]
	public List<Style> Stl
	{
		get {return styles;}
		set {styles = value;}
	}
	[XmlElementAttribute("Attribute")]
	public List<Atribute> Attrs
	{
		get{return attrs;}
		set{attrs = value;}
	}
	[XmlElementAttribute("SpecialItem")]
	public List<SpecialItem> SpeItms
	{
		get {return speItems;}
		set {speItems = value;}
	}
	[XmlArray("FixItems"), XmlArrayItem("Item")]
	public List<FixItem> FixItems
	{
		get{return fixItems;}
		set {fixItems = value;}
	}
}

public class FixItem
{
	int id;
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value; }
	}
}
public class Style
{
	int id;
	float ratio;
	[XmlAttribute]
	public int Id
	{
		get {return id;}
		set{id = value;}
	}
	[XmlAttribute]
	public float Ratio
	{
		get {return ratio;}
		set {ratio = value;}
	}
}
public class Atribute
{
	int type;
	int id;
	float ratio;
	[XmlAttribute]
	public int Type
	{
		get { return type;}
		set {type = value;}
	}
	[XmlAttribute]
	public int Id
	{
		get {return id;}
		set {id = value;}
	}
	[XmlAttribute]
	public float Ratio
	{
		get {return ratio;}
		set {ratio = value;}
	}
}

public class SpecialItem
{
	int id;
	int score;
	[XmlAttribute]
	public int Id
	{
		get {return id;}
		set {id = value;}
	}
	[XmlAttribute]
	public int Score
	{
		get {return score;}
		set {score = value;}
	}
}

public class GradeInfo
{
	int grade;
	int maxScore;
	int usePower;
	
	[XmlAttribute]
	public int Grade
	{
		get { return grade; }
		set { grade = value; }
	}
	[XmlAttribute]
	public int MaxScore
	{
		get { return maxScore; }
		set { maxScore = value; }
	}
	[XmlAttribute]
	public int UsePower
	{
		get { return usePower; }
		set { usePower = value; }
	}
}

public class Unlock
{
	UnLockCondition type;
	string val;
	[XmlAttribute]
	public UnLockCondition Type
	{
		get { return type; }
		set { type = value; }
	}
	[XmlAttribute]
	public string Value
	{
		get { return val; }
		set { val = value; }
	}
}
//剧情对话
public class LevelDialogList
{
	List<LevelDialog> levd;
	[XmlElementAttribute("LevelDialog")]
	public List<LevelDialog> LevD
	{
		get { return levd; }
		set { levd = value;}
	}
}
public class LevelDialog
{
	int id;
	string backgroundImageId;
	List<Content> content;
	
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value; }
	}
	[XmlAttribute]
	public string BackgroundImageId
	{
		get { return backgroundImageId;}
		set { backgroundImageId = value; }
	}
	[XmlElementAttribute("Content")]
	public List<Content> Cntent
	{
		get { return content; }
		set { content = value; }
	}
}
public class Content
{
	List<NPC> npc;
	string content;
	int id;
	bool key;
	
	public List<NPC> NPCS
	{
		get{return npc;}
		set{npc = value;}
	}
	public string Value
	{
		get{return content;}
		set{content = value;}
	}
	public int Item
	{
		get {return id;}
		set {id = value;}
	}
	public string Key
	{
		get
		{
			if (key)
				return "True";
			else
				return "False";
		}
		set
		{
			if (value == "True")
				key = true;
			else
				key = false;
		}
	}
}
public class NPC
{
	string name;
	string imageId;
	DialogNpcImgShowPos position;
	[XmlAttribute]
	public string Name
	{
		set {name = value;}
		get {return name;}
	}
	[XmlAttribute]
	public string ImageId
	{
		set {imageId = value;}
		get {return imageId;}
	}
	[XmlAttribute]
	public DialogNpcImgShowPos Position
	{
		set {position = value;}
		get {return position;}
	}
}

//评语
public class GradeList
{
	List<Grade> grads;
	[XmlElementAttribute("Grade")]
	public List<Grade> Grades
	{
		get { return grads; }
		set { grads = value; }
	}
}
public class Grade
{
	int id;
	List<string> descs;
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value;}
	}
	[XmlArray("Descs"),XmlArrayItem("Desc")]
	public List<string> Descs
	{
		get{ return descs;}
		set{ descs = value;}
	}
}
//奖励列表
public class RewardList
{
	List<Reward> rl = new List<Reward>();
	[XmlElementAttribute("Reward")]
	public List<Reward> RewardL
	{
		get { return rl; }
		set { rl = value; }
	}
}

public class Reward
{
	int id;
	int num;
	List<RewardItem> itms = new List<RewardItem>();

    public Dictionary<int, int> dicItemRatio = new Dictionary<int, int>();
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value; }
	}
	[XmlAttribute]
	public int Num
	{
		get { return num; }
		set { num = value; }
	}
	[XmlElementAttribute("Item")]
	public List<RewardItem> RwItems
	{
		get { return itms; }
		set { itms = value; }
	}
}
public class RewardItem
{
	int id;
	int ratio;
	[XmlAttribute]
	public int Id
	{
		get { return id; }
		set { id = value; }
	}
	[XmlAttribute]
	public int Ratio
	{
		get { return ratio; }
		set { ratio = value; }
	}
}

//解锁条件枚举
public enum UnLockCondition
{
	[XmlEnum("1")]
	UnLockCondition_BagDressNum = 1,
	[XmlEnum("2")]
	UnLockCondition_SpecTypeDressNum,
	[XmlEnum("3")]
	UnLockCondition_LevelRatingResult
}

public class DataManager
{
	public static string areamapPath = "areamap.xml";
	public static string leveldilogsPath = "leveldialog.xml";
	public static string remarkPath = "remarktable.xml";
	public static string rewardsPath = "rewards.xml";

	private AreaMaps areaMaps = new AreaMaps();
	private LevelDialogList diaList = new LevelDialogList();
	private GradeList remarks = new GradeList();
	private RewardList rewards = new RewardList();

	Dictionary<int, AreaMapInfo> mapDic = new Dictionary<int, AreaMapInfo>();
	Dictionary<int, LevelInfo> levelDic = new Dictionary<int, LevelInfo>();

	public AreaMaps AreaMapsData
	{
		get { return areaMaps;}
	}

	public LevelDialogList LevelDialogData
	{
		get { return diaList; }
	}

	public Dictionary<int, AreaMapInfo> MapDic
	{
		get { return mapDic; }
	}

	public Dictionary<int, LevelInfo> LevelDic
	{
		get { return levelDic; }
	}
	//初始化
	public void Init()
	{
		areaMaps = (AreaMaps)XMLTools.readXml(areamapPath, typeof(AreaMaps));
		diaList = (LevelDialogList)XMLTools.readXml(leveldilogsPath, typeof(LevelDialogList));
		remarks = (GradeList)XMLTools.readXml(remarkPath, typeof(GradeList));
		rewards = (RewardList)XMLTools.readXml(rewardsPath, typeof(RewardList));
		convert();
	}

	private void convert()
	{
        if (rewards != null)
        {
            foreach (Reward rewardInfo in rewards.RewardL)
            {
                int ratio = 0;
                foreach (RewardItem item in rewardInfo.RwItems)
                {
                    ratio += item.Ratio * 100;
                    rewardInfo.dicItemRatio.Add(ratio, item.Id);
                }
            }
        }
		if(areaMaps != null)
		{
			foreach(Map m in areaMaps.Maps)
			{
				AreaMapInfo areaMapInfo = new AreaMapInfo();
				areaMapInfo.id = m.Id;
				areaMapInfo.mapIconImgPath = m.IconId;
				areaMapInfo.name = m.Name;
			/*
			foreach(Unlock u in m.UnlockCond)
			{
				Debug.Log(u.Type);
			}
			*/
				List<string> imgs = new List<string>();
				foreach(string s in m.ImgList)
				{
					imgs.Add(s);
				}
				areaMapInfo.mapImgPaths = imgs.ToArray();
			
				List<int> ids = new List<int>();
				foreach(Level l in m.Levs)
				{
					LevelInfo levInfo = new LevelInfo();
					levInfo.areaMapId = areaMapInfo.id;
					levInfo.id = l.Id;
					levInfo.name = l.Name;
					levInfo.isTimeLimit = (l.LimiteTime == 0)?false:true;
					levInfo.timeLimit = l.LimiteTime;
					DialogInfo dialog = new DialogInfo();
					if(diaList != null)
					{
						foreach(LevelDialog ld in diaList.LevD)
						{
							if(ld.Id == l.DialogId)
							{
								dialog.bkImgPath = ld.BackgroundImageId;
								List<DialogContentInfo> contents = new List<DialogContentInfo>();
								foreach(Content c in ld.Cntent)
								{
									DialogContentInfo ctInfo = new DialogContentInfo();
									ctInfo.isKey = (c.Key == "True")?true:false;
									ctInfo.content = c.Value;
									if(c.NPCS.Count != 0)
										ctInfo.npcName = c.NPCS[0].Name;
									List<DialogNpcImgInfo> npcImgInfos  = new List<DialogNpcImgInfo>();
									foreach(NPC npc in c.NPCS)
									{
										DialogNpcImgInfo info = new DialogNpcImgInfo();
										info.imgPath = npc.ImageId;
										info.showPos = npc.Position;
										npcImgInfos.Add(info);
									}
									ctInfo.npcImgInfos = npcImgInfos; 
									contents.Add(ctInfo);
								}
								dialog.contents = contents;
								break;
							}
						}
					}
                    foreach (Atribute att in l.Thm.Attrs)
                    {
                        if (att.Type == 0)
                        {
                            l.Thm.AttStyleList.Add(att);
                        }
                        else if (att.Type == 1)
                        {
                            l.Thm.AttMatrialList.Add(att);
                        }
                    }
					levInfo.dialogInfo = dialog;
					levelDic.Add(l.Id,levInfo);
					ids.Add(l.Id);
				}
				areaMapInfo.levels = ids.ToArray();
				mapDic.Add(m.Id, areaMapInfo);
			}
		}
	}

	//由关卡Id返回所在地图信息
	public Dictionary<int, List<Level>> getMapInfo(int levelId)
	{
		Dictionary<int, List<Level>> mp = new Dictionary<int, List<Level>>();
		bool find = false;
		if(areaMaps != null)
		{
			foreach(Map m in areaMaps.Maps)
			{
				//查找Levelid所在地图
				foreach(Level lev in m.Levs)
				{
					if(lev.Id == levelId)
					{
						find = true;
						break;
					}
				}
				//添加关卡所属地图的所有关卡信息
				if(find)
				{
					mp.Add(m.Id, m.Levs);
					return mp;
				}
			}
		}
		return null;
	}

	//由关卡Id返回关卡信息
	public Level getLevelInfo(int levelId)
	{
		if(areaMaps != null)
		{
			foreach(Map m in areaMaps.Maps)
			{
				//查找Levelid所在地图
				foreach(Level lev in m.Levs)
				{
					if(lev.Id == levelId)
					{
						return lev;
					}
				}
			}
		}
		return null;
	}

	//下一关卡信息
	public Level getNextLevel(int levelId)
	{
		if(areaMaps != null)
		{
			foreach(Map m in areaMaps.Maps)
			{
				//查找Levelid所在地图
				foreach(Level lev in m.Levs)
				{
					if(lev.Id == (levelId+1))
					{
						return lev;
					}
				}
			}
		}
		return null;
	}

	public List<Map> getMapList()
	{
		if(areaMaps != null)
			return areaMaps.Maps;
		return null;
	}
    public List<Level> getLevelListForMapId(int mapId)
    {
        if (areaMaps != null)
        {
            foreach (Map map in areaMaps.Maps)
            {
                if (map.Id == mapId)
                {
                    return map.Levs;
                }
            }
        }
        return null;
    }
    public string getGradeComment(int gradeId)
    {
        if (remarks.Grades.Count <= gradeId)
        {
            return "数据错误";
        }
        System.Random ran = new System.Random();
        return remarks.Grades[gradeId].Descs[ran.Next(remarks.Grades[gradeId].Descs.Count-1)];
    }

    public List<int> getRewardList(int rewardId)
    {
        List<int> finalId = new List<int>();
        foreach (Reward reward in rewards.RewardL)
        {
            if (reward.Id == rewardId)
            {
                for (int i = 0; i < reward.Num; i++)
                {
                    System.Random ran = new System.Random();
                    int flag=ran.Next(0, 100);
                    foreach (int ratio in reward.dicItemRatio.Keys)
                    {
                        if (flag <= ratio)
                        {
                            if (finalId.Contains(reward.dicItemRatio[ratio]))
                            {
                                i--;
                                break;
                            }
                            finalId.Add(reward.dicItemRatio[ratio]);
                        }
                    }
                }
            }
        }
        return finalId;
    }
	public static DataManager GetInstance()
	{
		if( data_instance == null )
		{
			data_instance = new DataManager();
		}
		return data_instance;
	}
	
	static DataManager data_instance; 
}
