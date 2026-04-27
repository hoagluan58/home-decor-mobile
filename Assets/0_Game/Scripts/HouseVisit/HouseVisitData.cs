using NFramework;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HouseVisitData : SingletonMono<HouseVisitData>, ISaveable
    {
        public static event Action OnCurrentLikeChanged;
        public static event Action OnLikeRewardsChanged;

        [SerializeField] private SaveData _saveData;

        public int CurrentLike
        {
            get => _saveData.CurrentLike;
            set
            {
                _saveData.CurrentLike = value;
                OnCurrentLikeChanged?.Invoke();
                DataChanged = true;
            }
        }

        public bool CanClaimBonusLike
        {
            get => _saveData.CanClaimBonusLike;
            set
            {
                _saveData.CanClaimBonusLike = value;
                DataChanged = true;
            }
        }

        public void SaveLikeRewardsClaimed(string id)
        {
            if (IsClaimedLikeReward(id))
                return;

            _saveData.LikeRewardsClaimed.Add(id);
            OnLikeRewardsChanged?.Invoke();
            DataChanged = true;
        }

        public void ClaimBonusLike()
        {
            if (CanClaimBonusLike)
            {
                CurrentLike += 20;
                CanClaimBonusLike = false;
            }
        }

        public void StartAutoUpdateLike()
        {
            StartCoroutine(UpdateLikesEveryFiveMinutes());
        }

        private IEnumerator UpdateLikesEveryFiveMinutes()
        {
            while (true)
            {
                yield return new WaitForSeconds(300); // Wait for 5 minutes (300 seconds)
                CurrentLike += 2;
                CanClaimBonusLike = true;
            }
        }

        public bool IsClaimedLikeReward(string id) => _saveData.LikeRewardsClaimed.Contains(id);

        #region ISaveable

        [System.Serializable]
        public class SaveData
        {
            public int CurrentLike = 10;
            public bool CanClaimBonusLike;
            public List<string> LikeRewardsClaimed = new List<string>();
        }

        public string SaveKey => "HouseVisitData";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void OnAllDataLoaded()
        {
        }

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new SaveData();
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<SaveData>(data);
            }
        }

        #endregion
    }
}