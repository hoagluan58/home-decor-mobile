using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandData : SingletonMono<HomeLandData>, ISaveable
    {
        [SerializeField] private SaveData _saveData;

        public int CurrentLandId
        {
            get
            {
                var result = -1;
                var landCompleted = 0;
                foreach (var landId in _saveData.LandProgressDic.Keys)
                {
                    if (!IsLandCompleted(landId))
                    {
                        result = landId;
                        break;
                    }
                    landCompleted++;
                }

                if (landCompleted == _saveData.LandProgressDic.Keys.Count)
                {
                    result = _saveData.LandProgressDic.Keys.LastOrDefault();
                }

                return result;
            }
        }

        public void InitLandSaveData(int landId)
        {
            if (_saveData.LandProgressDic.ContainsKey(landId))
                return;

            _saveData.LandProgressDic.Add(landId, new LandProgress());
            if (Define.DEFAULT_LAND_UNLOCKED_LIST.Contains(landId))
            {
                _saveData.LandProgressDic[landId].IsUnlock = true;
            }
            DataChanged = true;
        }

        public void UnlockNewLandFur(int landId, int indexFur, int optionUnlocked, string furId)
            => _saveData.LandProgressDic[landId].UnlockNewFur(indexFur, optionUnlocked, furId);

        public SerializableDictionaryBase<int, UnlockFurData> GetLandUnlockFurData(int landId)
            => _saveData.LandProgressDic.TryGetValue(landId, out var landProgress) ? landProgress.UnlockedFurnitureDic : null;

        public int GetLandCurUnlockIndex(int landId)
            => _saveData.LandProgressDic.TryGetValue(landId, out var landProgress) ? landProgress.GetCurUnlockIndex() : 0;

        public int GetLandNextUnlockIndex(int landId)
            => _saveData.LandProgressDic.TryGetValue(landId, out var landProgress) ? landProgress.GetNextUnlockIndex() : -1;

        public int GetLandUnlockFurOption(int landId, int indexFur)
            => _saveData.LandProgressDic.TryGetValue(landId, out var landProgress) ? landProgress.GetUnlockedFurOption(indexFur) : -1;

        public LandProgress GetLandProgress(int landId)
            => _saveData.LandProgressDic.TryGetValue(landId, out var landProgress) ? landProgress : null;

        public RoomFurnitureData UnlockNewDecorFurniture(int landId)
        {
            if (_saveData.LandProgressDic.TryGetValue(landId, out var landProgress))
            {
                var decorDataConfig = AllConfig.Instance.LandConfigDic[landId].DecorFurnitureData;

                if (IsLandCompleted(landId) || landProgress.UnlockedDecorFurniture.Count >= decorDataConfig.Count)
                    return null;

                // Get the next item in sequence based on how many have been unlocked
                var nextIndex = landProgress.UnlockedDecorFurniture.Count;
                var newDecorFur = decorDataConfig[nextIndex];

                // Add the new decor furniture to unlocked list and mark data as changed
                landProgress.UnlockedDecorFurniture.Add(newDecorFur);
                DataChanged = true;

                return newDecorFur;
            }
            return null;
        }

        public bool IsLandCompleted(int landId)
        {
            if (_saveData.LandProgressDic.TryGetValue(landId, out var landProgress))
            {
                var decorDataConfig = AllConfig.Instance.LandConfigDic[landId].DecorFurnitureData;
                if (landProgress.UnlockedDecorFurniture.Count == decorDataConfig.Count)
                    return true;
            }

            return false;
        }

        public bool IsLastLandId(int landId) => landId == _saveData.LandProgressDic.Keys.LastOrDefault();

        public bool CanUnlockLand(int landId)
        {
            var prevLandId = landId - 1;
            if (_saveData.LandProgressDic.ContainsKey(prevLandId))
            {
                if (IsLandCompleted(prevLandId))
                {
                    return true;
                }
            }
            return false;
        }

        public bool IsContainLand(int landId) => _saveData.LandProgressDic.ContainsKey(landId);

        #region ISaveable

        [Serializable]
        public class SaveData
        {
            public SerializableDictionaryBase<int, LandProgress> LandProgressDic = new SerializableDictionaryBase<int, LandProgress>();
        }

        public string SaveKey => "HomeLandSaveData";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

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

        public void OnAllDataLoaded() { }

        #endregion
    }
}
