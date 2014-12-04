using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Serialization;

public enum DialogNpcImgShowPos
{
	[XmlEnum("1")]
    Left = 1,
	[XmlEnum("2")]
    Middle,
	[XmlEnum("3")]
    Right
}
 

//对话Npc图片信息
public class DialogNpcImgInfo
{
    //Npc图片路径
    public string imgPath;
    //Npc在对话窗口中的显示位置
    public DialogNpcImgShowPos showPos;
}

//对话内容信息
public class DialogContentInfo
{
    //当前对话内容是否是关键内容
    public bool isKey;
    //Npc名称
    public string npcName;
    //当前对话内容
    public string content;
    //当前Npc图片显示信息
    public List<DialogNpcImgInfo> npcImgInfos = new List<DialogNpcImgInfo>();
}

//剧情对话信息
public class DialogInfo
{
    //剧情对话背景图片
    public string bkImgPath;
    //当前关卡对话内容
    public List<DialogContentInfo> contents
        = new List<DialogContentInfo>();
}

public class LevelInfo
{
    //关卡id
    public int id;
    //关卡所属地图
    public int areaMapId;
    //关卡名
    public string name; 
    //是否有时限
    public bool isTimeLimit;
    //当前关卡时限
    public float timeLimit;
    //关卡的剧情对话信息
    public DialogInfo dialogInfo = new DialogInfo(); 
}

public delegate void GetLevelInfoCallback( LevelInfo info );

public interface ILevel 
{
    //获取关卡信息
    void GetLevelInfo(int id ,GetLevelInfoCallback callback);
}
