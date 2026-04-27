using System;
using System.Collections.Generic;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class LevelUpRewardData : SingletonMono<LevelUpRewardData>, ISaveable
    {
        public bool IsLevelRewarded(int itemLevel)
        {
            return _saveData.Rewarded.Contains(itemLevel);
        }

        public void ClaimReward(int itemLevel)
        {
            _saveData.Rewarded.Add(itemLevel);
            DataChanged = true;
        }

        #region ISaveable

        [SerializeField] private SaveData _saveData;

        public string SaveKey => "DecorData";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new();
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<SaveData>(data);
            }
        }

        public void OnAllDataLoaded()
        {
        }

        [Serializable]
        public class SaveData
        {
            public List<int> Rewarded = new();
        }

        #endregion
    }
}