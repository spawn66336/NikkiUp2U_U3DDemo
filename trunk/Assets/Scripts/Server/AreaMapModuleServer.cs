using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.Xml;
using System.Xml.Serialization;
using System.IO;


public class SerialiazeManager
{
	//序列化数据类

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
		List<Level> levs;
		List<string> imageList;
		List<Unlock> unlockCond;
		
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
	public class Level
	{
		int id;
		int finishGrade;
		float limiteTime;
		int rewardid;
		int dialogid;
		RatingRule ratingRule;
		
		Unlock[] cond;
		
		[XmlAttribute]
		public int Id
		{
			get { return id; }
			set { id = value; }
		}
		[XmlAttribute]
		public int FinishGrade
		{
			get { return finishGrade; }
			set { finishGrade = value; }
		}
		[XmlAttribute]
		public float LimiteTime
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
		public Unlock[] Cond
		{
			get { return cond; }
			set { cond = value; }
		}
	}
	public class RatingRule
	{
		GradeInfo[] grdsc;
		[XmlElementAttribute("GradeInfo")]
		public GradeInfo[] GradeSc
		{
			get { return grdsc; }
			set { grdsc = value; }
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
		int type;
		string val;
		[XmlAttribute]
		public int Type
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
		int position;
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
		public int Position
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
	public static SerialiazeManager GetInstance()
	{
		if( s_instance == null )
		{
			s_instance = new SerialiazeManager();
		}
		return s_instance;
	}
	
	static SerialiazeManager s_instance; 

	AreaMaps areamaps = new AreaMaps();
	LevelDialogList diaList = new LevelDialogList();
	GradeList gradeList = new GradeList();
	RewardList rewardList = new RewardList(); 

	public AreaMaps Areas
	{
		get {return areamaps;}
	}

	public LevelDialogList LevDlg
	{
		get {return diaList;}
	}

	public GradeList GrdList
	{
		get { return gradeList; }
	}
	public RewardList RewdList
	{
		get { return rewardList; }
	}

	//反序列化
	public void Deserilaize()
	{
		using(FileStream fs = new FileStream(Application.dataPath + "", FileMode.Open))
		{
			XmlSerializer serializer = new XmlSerializer(typeof(AreaMaps));
			areamaps = (AreaMaps)serializer.Deserialize(fs);
		}
	}
}

public class AreaMapModuleServer : ModuleServer 
{
	Dictionary<int, AreaMapInfo> map_dic = new Dictionary<int, AreaMapInfo>();

	public override void HandleRequest( ServerRequestMessage request )
	{
        if (request.msg.Message == (int)RequestMessageDef.Request_AreaMapIdList)
        {
            ServerReplyMessage rpl = new ServerReplyMessage();
            List<int> mapIdList = new List<int>();
            mapIdList.Add(1);
            mapIdList.Add(12);
            mapIdList.Add(123);
            rpl.serial = request.serial;
            rpl.resultObject = mapIdList;

            ReplyToClient(rpl);
        }
        else if (request.msg.Message == (int)RequestMessageDef.Request_AreaMapInfo)
        {
            ServerReplyMessage rpl = new ServerReplyMessage();
        }
	}
}
