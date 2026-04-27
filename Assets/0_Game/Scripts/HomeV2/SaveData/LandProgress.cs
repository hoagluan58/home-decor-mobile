using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    [Serializable]
    public class LandProgress
    {
        public List<RoomFurnitureData> FurnitureData = new List<RoomFurnitureData>(); // Room Furniture

        public SerializableDictionaryBase<int, UnlockFurData> UnlockedFurnitureDic = new SerializableDictionaryBase<int, UnlockFurData>(); // key: indexUnlockConfig
        public List<RoomFurnitureData> UnlockedDecorFurniture = new List<RoomFurnitureData>();
        public List<string> ValidDecorFurList = new List<string>();
        public bool IsUnlock = false;

        public bool DataChanged
        {
            get
            {
                return HomeLandData.Instance.DataChanged;
            }
            set
            {
                HomeLandData.Instance.DataChanged = value;
            }
        }

        public List<RoomFurnitureData> GetRoomFurDataValidDecorList()
        {
            var result = new List<RoomFurnitureData>();
            foreach (var furData in UnlockedDecorFurniture)
            {
                if (!ValidDecorFurList.Contains(furData.Id))
                    continue;
                result.Add(furData);
            }
            return result;
        }

        public List<RoomFurnitureData> GetDecorBoxFurList()
        {
            var result = new List<RoomFurnitureData>();
            foreach (var furData in UnlockedDecorFurniture)
            {
                if (ValidDecorFurList.Contains(furData.Id))
                    continue;
                result.Add(furData);
            }
            return result;
        }

        public int GetCurUnlockIndex()
        {
            if (UnlockedFurnitureDic.Count == 0)
                return -1;

            return UnlockedFurnitureDic.Keys.Last();
        }

        public int GetNextUnlockIndex()
        {
            if (UnlockedFurnitureDic.Count == 0)
                return 0;

            return UnlockedFurnitureDic.Keys.Last() + 1;
        }

        public void UnlockNewFur(int keyFurUnlock, int optionUnlocked, string furId)
        {
            if (UnlockedFurnitureDic.ContainsKey(keyFurUnlock))
            {
                UnlockedFurnitureDic[keyFurUnlock].ChangeOption(optionUnlocked, furId);
            }
            else
            {
                var unlockFurData = new UnlockFurData(optionUnlocked, furId);
                UnlockedFurnitureDic.Add(keyFurUnlock, unlockFurData);
                unlockFurData.AddBoughtOption(optionUnlocked);
            }
            DataChanged = true;
        }

        public int GetUnlockedFurOption(int keyFurUnlock)
        {
            if (UnlockedFurnitureDic.ContainsKey(keyFurUnlock))
            {
                return UnlockedFurnitureDic[keyFurUnlock].OptionUsing;
            }
            return -1;
        }

        public List<int> GetBoughtOptions(int keyFurUnlock)
        {
            if (UnlockedFurnitureDic.ContainsKey(keyFurUnlock))
            {
                return UnlockedFurnitureDic[keyFurUnlock].BoughtOptions;
            }
            return new List<int>();
        }

        public void SaveValidDecorFur(string id, Vector3 pos, FurnitureDirection dir)
        {
            var furData = UnlockedDecorFurniture.FirstOrDefault(x => x.Id == id);
            if (furData != null)
            {
                furData.Position = pos;
                furData.Direction = dir;

                if (!ValidDecorFurList.Contains(furData.Id))
                {
                    ValidDecorFurList.Add(furData.Id);
                }
                DataChanged = true;
            }
        }

        public void UnlockLand()
        {
            IsUnlock = true;
            DataChanged = true;
        }
    }

    [Serializable]
    public class UnlockFurData
    {
        public int OptionUsing;
        public string FurId;
        public List<int> BoughtOptions = new List<int>();

        public UnlockFurData(int optionUnlocked, string furId)
        {
            OptionUsing = optionUnlocked;
            FurId = furId;
        }

        public void AddBoughtOption(int option)
        {
            if (BoughtOptions.Contains(option) || option < 0)
                return;
            BoughtOptions.Add(option);
        }

        public void ChangeOption(int optionUsing, string furId)
        {
            OptionUsing = optionUsing;
            FurId = furId;
            AddBoughtOption(optionUsing);
        }
    }
}
