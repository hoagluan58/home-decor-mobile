using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

namespace YoyoDesign
{
    public class DressUpData : SingletonMono<DressUpData>, ISaveable
    {
        #region PARAM

        public static UnityEvent<string> EventCharacterID = new();

        [SerializeField] private SaveData _saveData;
        public string CurCharacterId => _saveData.CurrentCharacterId;

        #endregion

        #region FEATURE METHODS
        public void UnlockOutfit(string itemClickedId)
        {
            if (_saveData.OutfitUnlocked.Contains(itemClickedId)) return;
            _saveData.OutfitUnlocked.Add(itemClickedId);
            DataChanged = true;
        }
        public bool CheckIsUnlock(string itemId)
        {
            return _saveData.OutfitUnlocked.Contains(itemId);
        }
        public bool CheckOutfitIsEquip(OutfitType outfitType, string itemId)
        {
            return _saveData.CharacterCurOutfitData[CurCharacterId].OutfitSaveDatas.Find(x => x.Id == itemId) != null;
        }
        public List<string> GetOutfitUnlocked()
        {
            return _saveData.OutfitUnlocked;
        }
        public List<CharacterOutfitSaveData> GetCharacterOutfitSaveDatas()
        {
            return _saveData.CharacterCurOutfitData[_saveData.CurrentCharacterId].OutfitSaveDatas;
        }
        public void SaveOutfit(List<CharacterOutfitData> characterOutfitDatas)
        {
            foreach (var data in characterOutfitDatas)
            {
                var saveData = _saveData.CharacterCurOutfitData[CurCharacterId].OutfitSaveDatas.FirstOrDefault(d => d.OutfitType == data.OutfitType);
                if (saveData != null) saveData.Id = data.OutfitId;
            }
            DataChanged = true;
        }
        public void SaveCharacter(string id)
        {
            _saveData.CurrentCharacterId = id;

            DataChanged = true;
            EventCharacterID?.Invoke(id);
        }
        #endregion

        #region ISAVEABLE METHODS

        public string SaveKey => "DressUpData";
        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new SaveData();

                var defaultChar = CharacterManager.Instance.GetCharacterConfigDatas().First();

                _saveData.CurrentCharacterId = defaultChar.Id;
                _saveData.CharacterCurOutfitData = new SerializableDictionaryBase<string, CharacterOutfit>();

                foreach (var VARIABLE in CharacterManager.Instance.GetCharacterConfigDatas())
                {
                    CharacterOutfit characterOutfit = new CharacterOutfit();
                    List<CharacterOutfitSaveData> characterOutfitSaveDatas = new List<CharacterOutfitSaveData>();
                    characterOutfit.OutfitSaveDatas = characterOutfitSaveDatas;

                    foreach (OutfitType outfitType in Enum.GetValues(typeof(OutfitType)))
                    {
                        characterOutfitSaveDatas.Add(new CharacterOutfitSaveData
                        {
                            Id = "dressUp_" + outfitType.ToString().ToLower() + "_00",
                            OutfitType = outfitType,
                        });
                    }
                    _saveData.CharacterCurOutfitData[VARIABLE.Id] = characterOutfit;
                }

                defaultChar.OutfitDefault.ForEach(outfit =>
                {
                    var saveData = _saveData.CharacterCurOutfitData[_saveData.CurrentCharacterId].OutfitSaveDatas.FirstOrDefault(d => d.OutfitType == outfit.OutfitType);
                    if (saveData != null) saveData.Id = outfit.OutfitId;
                });

                DressUpManager.Instance.GetDresUpConfig().ForEach(x =>
                {
                    if (!_saveData.OutfitUnlocked.Contains(x.Id))
                    {
                        if (x.UnlockType == UnlockType.Free)
                        {
                            _saveData.OutfitUnlocked.Add(x.Id);
                        }
                    }
                });
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<SaveData>(data);

                #region Load OutfitConfig Data isUnlocked 

                DressUpManager.Instance.GetDresUpConfig().ForEach(x =>
                {
                    if (!_saveData.OutfitUnlocked.Contains(x.Id))
                    {
                        if (x.UnlockType == UnlockType.Free)
                        {
                            _saveData.OutfitUnlocked.Add(x.Id);
                        }
                    }
                });

                DataChanged = true;
                #endregion
            }

        }

        public void OnAllDataLoaded()
        {
            Debug.Log("Data Loaded");
        }

        [Serializable]
        public class SaveData
        {
            public string CurrentCharacterId;
            public SerializableDictionaryBase<string, CharacterOutfit> CharacterCurOutfitData;
            public List<string> OutfitUnlocked = new();
        }

        [Serializable]
        public class CharacterOutfitSaveData
        {
            public OutfitType OutfitType;
            public string Id;
        }

        [Serializable]
        public class CharacterOutfit
        {
            public List<CharacterOutfitSaveData> OutfitSaveDatas = new List<CharacterOutfitSaveData>();
        }

        #endregion
    }
}