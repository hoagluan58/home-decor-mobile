using NFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class LikeRewardConfigSO : GoogleSheetConfigSO<LikeRewardConfigData>
    {
        private Dictionary<string, LikeRewardConfigData> _likeRewardConfigDic;
        public Dictionary<string, LikeRewardConfigData> LikeRewardConfigDic => _likeRewardConfigDic;

        public void Init()
        {
            _likeRewardConfigDic = new Dictionary<string, LikeRewardConfigData>();

            foreach (var data in _datas)
            {
                _likeRewardConfigDic[data.Id] = data;
            }
        }

#if UNITY_EDITOR
        protected override void OnSynced(List<LikeRewardConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.Reward = Utilities.ParseStringToRewardDatas(data.RewardString).FirstOrDefault();
            }
        }
#endif
    }

    [Serializable]
    public class LikeRewardConfigData
    {
        public string Id;
        public int Like;
        public float PositionOnBar;
        public RewardData Reward;

        [HideInInspector] public string RewardString;
    }
}
