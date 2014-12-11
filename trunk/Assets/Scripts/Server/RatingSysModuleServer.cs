using System;
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
            int score = (int)RankingSystemManager.getInstance().getFinalScore(levelInfo, dressSet.dressList);
            List<GradeInfo> gradeList = levelInfo.RRule.GradeSc;
            GradeInfo finalGrade = null;
            for (int i = 0; i < gradeList.Count; i++)
            {
                GradeInfo grade = gradeList[i];
                if (score < grade.MaxScore)
                {
                    if (i == 0)
                    {
                        finalGrade = grade;
                        break;
                    }
                    finalGrade = gradeList[i-1];
                    break;
                }
            }
            if (finalGrade == null)
            {
                finalGrade = gradeList[gradeList.Count - 1];
            }



            RatingInfo info = new RatingInfo();
            info.levelRank = (LevelRank)finalGrade.Grade;
            info.score = score;
            List<LevelRewardInfo> rewardList=new List<LevelRewardInfo>();
            foreach (int rewardId in DataManager.GetInstance().getRewardList(levelInfo.RewardId))
            {
                LevelRewardInfo reward = new LevelRewardInfo();
                reward.itemId = rewardId;
                reward.itemCount = 1;
                rewardList.Add(reward);
            }
            info.rewards.AddRange(rewardList);
            info.comment = DataManager.GetInstance().getGradeComment(finalGrade.Grade);
            PlayerInfoManager.getInstance().finishLevel(levelInfo, dressSet.dressList, info,finalGrade.UsePower);
            foreach (LevelRewardInfo rewardInfo in rewardList)
            {
                BagInfoManager.getInstance().addItemToBag(rewardInfo.itemId,rewardInfo.itemCount);
            }
            
            ServerReplyMessage replyMsg = new ServerReplyMessage();
            replyMsg.message = RequestMessageDef.Request_Rating;
            replyMsg.serial = request.serial;
            replyMsg.resultObject = info;
            ReplyToClient(replyMsg);
        }
    }
	 
}
