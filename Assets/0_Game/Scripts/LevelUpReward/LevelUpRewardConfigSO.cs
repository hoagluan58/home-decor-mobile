using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class LevelUpRewardConfigSO : GoogleSheetConfigSO<LevelUpRewardsConfig>
    {
        private Dictionary<int, LevelUpRewardsConfig> _levelUpConfigDic;
        public Dictionary<int, LevelUpRewardsConfig> LevelUpConfigDic => _levelUpConfigDic;

        public void Init()
        {
            _levelUpConfigDic = new Dictionary<int, LevelUpRewardsConfig>();
            
            foreach (var data in _datas)
            {
                _levelUpConfigDic[data.Level] = data;
            }
        }
    }

    [Serializable]
    public class LevelUpRewardsConfig
    {
        public int Level;
        public LevelUpRewardConfig SpecialReward;
       public List<LevelUpRewardConfig> NormalRewards;
    }

    [Serializable]
    public class LevelUpRewardConfig
    {
        public ERewardType RewardType;
        public string RewardId;
        public int RewardAmount;
    }
    
   
}