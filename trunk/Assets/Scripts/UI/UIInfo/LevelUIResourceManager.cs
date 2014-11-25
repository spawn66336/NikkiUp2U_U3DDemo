using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class NpcImgUIInfo
{
    public Texture2D img;
    public DialogNpcImgShowPos pos;
}

public class DialogContentUIInfo
{
    //当前对话内容是否是关键内容
    public bool isKey;
    //当前对话内容
    public string content;
    //当前Npc图片显示信息
    public List<NpcImgUIInfo> npcImgs = new List<NpcImgUIInfo>();
}

public class DialogUIInfo
{
    //剧情对话背景图片
    public Texture2D bkImg;
    //当前关卡对话内容
    public List<DialogContentUIInfo> contents
        = new List<DialogContentUIInfo>();
}

public class LevelUIInfo
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
    public DialogUIInfo dialogInfo = new DialogUIInfo();
    //关卡锁定原因
    public string lockReason;
}

public class LevelUIResourceManager : UIResourceManager
{  
    public LevelUIInfo TryGetLevelInfo( int id )
    {
        if( !_IsCached(id) )
        {
            PostRequest(new UIResourceSyncRequest(id, ""));
            return null;
        }
        LevelUIInfo uiInfo = null;
        levelUIInfoCache.TryGetValue(id,out uiInfo);
        return uiInfo;
    }
    
    public override IEnumerator Sync()
    {
        LevelModule levelModule = GlobalObjects.GetInstance().GetLogicMain().GetModule<LevelModule>(); 
        syncCount = requestList.Count;
        foreach( var req in requestList )
        {
            if (_IsCached(req.ResID))
            {
                syncCount--;
            }
            else
            {
                levelModule.GetLevelInfo(req.ResID, _OnGetLevelInfoFinished);
                yield return 0;
            }
        } 

        while( syncCount != 0 )
        {
            yield return 0;
        }
    }

    bool _IsCached( int id )
    {
        return levelUIInfoCache.ContainsKey(id);
    }

    void _OnGetLevelInfoFinished(LevelInfo info)
    {
        LevelUIInfo newUIInfo = new LevelUIInfo();
        newUIInfo.id = info.id;
        newUIInfo.name = info.name;
        newUIInfo.isAvaliable = info.isAvaliable;
        newUIInfo.isTimeLimit = info.isTimeLimit;
        newUIInfo.lockReason = info.lockReason;
        newUIInfo.dialogInfo.bkImg = ResourceManager.GetInstance().Load(ResourceType.LevelDialogBackground, info.dialogInfo.bkImgPath) as Texture2D;
        foreach( var content in info.dialogInfo.contents )
        {
            DialogContentUIInfo contentUIInfo = new DialogContentUIInfo();
            contentUIInfo.isKey = content.isKey;
            contentUIInfo.content = content.content;

            foreach( var npcImgInfo in content.npcImgInfos )
            {
                NpcImgUIInfo npcImgUIInfo = new NpcImgUIInfo();
                npcImgUIInfo.img = ResourceManager.GetInstance().Load(ResourceType.Npc, npcImgInfo.imgPath) as Texture2D;
                npcImgUIInfo.pos = npcImgInfo.showPos;
                contentUIInfo.npcImgs.Add(npcImgUIInfo);
            }

            newUIInfo.dialogInfo.contents.Add(contentUIInfo);
        }
        syncCount--;
    }

    int syncCount = 0;
    Dictionary<int, LevelUIInfo> levelUIInfoCache = new Dictionary<int, LevelUIInfo>();

    public static LevelUIResourceManager GetInstance()
    {
        if (s_instance == null)
        {
            s_instance = new LevelUIResourceManager();
        }
        return s_instance;
    }

    static LevelUIResourceManager s_instance;
}
