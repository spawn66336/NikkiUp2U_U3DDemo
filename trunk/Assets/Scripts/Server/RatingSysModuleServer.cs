using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RatingSysModuleServer : ModuleServer 
{
    public override void HandleRequest(ServerRequestMessage request)
    {
        if (request.msg.Message == (int)RequestMessageDef.Request_Rating)
        {
            RatingRequestMsg msg = (RatingRequestMsg)request.msg;
            int levelId = msg.levelId;
            DressSetInfo dressSet = msg.dressSet;
            Level levelInfo = DataManager.GetInstance().getLevelInfo(levelId);
            // todo 计算得分。返回数据

            RatingInfo info = new RatingInfo();
            info.levelRank = LevelRank.A;
            info.score = 1300;
            List<LevelRewardInfo> rewardList=new List<LevelRewardInfo>();
            LevelRewardInfo reward = new LevelRewardInfo();
            reward.itemId=10001;
            reward.itemCount=1;
            rewardList.Add(reward);
            info.rewards.AddRange(rewardList);

            PlayerInfoManager.getInstance().finishLevel(levelId, dressSet.dressList, info);
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.message = RequestMessageDef.Request_Rating;
            replyMsg.resultObject = info;
            ReplyToClient(replyMsg);
        }
    }
	 
}
