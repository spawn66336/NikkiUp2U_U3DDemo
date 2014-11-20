using UnityEngine;
using System.Collections;
using System.Collections.Generic;

enum DialogNpcImgShowPos
{
    Left = 1,
    Middle,
    Right
}

//对话Npc图片信息
public class DialogNpcImgInfo
{
    //Npc图片路径
    string imgPath;
    //Npc在对话窗口中的显示位置
    DialogNpcImgShowPos showPos;
}

//对话内容信息
public class DialogContentInfo
{
    //当前对话内容是否是关键内容
    public bool isKey;
    //当前对话内容
    public string content;
    //当前Npc图片显示信息
    List<DialogNpcImgInfo> npcImgInfos = new List<DialogNpcImgInfo>();
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
    //关卡名
    public string name;
    //关卡是否解锁
    public bool isAvaliable;
    //是否有时限
    public bool isTimeLimit;
    //当前关卡时限
    public float timeLimit;
    //关卡的剧情对话信息
    public DialogInfo dialogInfo = new DialogInfo();
    //关卡锁定原因
    public string lockReason;
}

public delegate void GetLevelInfoCallback( LevelInfo info );

public interface ILevel 
{
    //获取关卡信息
    void GetLevelInfo(int id ,GetLevelInfoCallback callback);
}
