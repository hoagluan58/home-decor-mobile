using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = System.Random;

namespace YoyoDesign
{
    public class HouseVisitManager : Singleton<HouseVisitManager>
    {
        private string _curHouseId;

        public BotHouseConfigData GetCurBotRoomConfigData() => GetBotHouseConfigData(_curHouseId);

        public BotHouseConfigData GetBotHouseConfigData(string id) => AllConfig.Instance.BotHouseConfigDic[id];

        public LikeRewardConfigData GetLikeRewardConfigData(string id) => AllConfig.Instance.LikeRewardConfigDic[id];

        public List<BotHouseConfigData> GetRandomBotConfigDatas(int amount)
        {
            var rnd = new Random();
            var result = new List<BotHouseConfigData>();
            var selectedKeys = AllConfig.Instance.BotHouseConfigDic.Keys.OrderBy(x => rnd.Next()).Take(amount).ToList();
            foreach (var key in selectedKeys)
            {
                var data = GetBotHouseConfigData(key);
                result.Add(data);
            }
            return result;
        }

        public void ClaimLikeReward(string id, float multiply = 1, Action onClaimCallback = null)
        {
            if (!CanClaimLikeReward(id))
                return;

            var config = GetLikeRewardConfigData(id);
            HouseVisitData.Instance.SaveLikeRewardsClaimed(id);
            UserData.Instance.AddRewardToUserData(config.Reward, multiply);
            onClaimCallback?.Invoke();
        }

        public bool CanClaimLikeReward(string id)
        {
            var config = GetLikeRewardConfigData(id);
            var isClaimed = HouseVisitData.Instance.IsClaimedLikeReward(id);
            return HouseVisitData.Instance.CurrentLike >= config.Like && !isClaimed;
        }

        public bool IsLikeRewardClaimed(string id)
        {
            return HouseVisitData.Instance.IsClaimedLikeReward(id);
    }

        public void VisitBotHouse(string id, Action onVisitHouse = null)
        {
            _curHouseId = id;
        }

        public void LeaveBotHouse()
        {
            _curHouseId = "";
        }
    }
}