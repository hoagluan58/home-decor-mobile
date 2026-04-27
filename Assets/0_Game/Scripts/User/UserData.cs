using System;
using System.Collections;
using System.Collections.Generic;
using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using YoyoDesign;

public class UserData : SingletonMono<UserData>, ISaveable
{
    public static event Action OnUserCurrencyChanged;
    public static event Action OnUserExpChanged;
    public static event Action<int> OnUserLevelUp;

    [SerializeField] private SaveData _saveData;

    public bool IsUnlockSecondFloor => _saveData.TutorialIndex >= 4 && _saveData.Level >= 5;

    public int CurLevel
    {
        get => _saveData.Level;
        set
        {
            _saveData.Level = value;
            DataChanged = true;
        }
    }

    public bool IsVibrateOn
    {
        get => _saveData.IsVibrateOn;
        set
        {
            _saveData.IsVibrateOn = value;
            DataChanged = true;
        }
    }

    public bool IsMusicOn
    {
        get => _saveData.IsMusicOn;
        set
        {
            _saveData.IsMusicOn = value;
            DataChanged = true;
        }
    }

    public bool IsSoundOn
    {
        get => _saveData.IsSoundOn;
        set
        {
            _saveData.IsSoundOn = value;
            DataChanged = true;
        }
    }

    public float CurLevelExp => _saveData.CurLevelExp;
    public int CurTutorialIndex
    {
        get => _saveData.TutorialIndex;
        set
        {
            _saveData.TutorialIndex = value;
            DataChanged = true;
        }
    }

    public bool CanClaimNPCReward
    {
        get => _saveData.CanClaimNPCReward;
        set
        {
            _saveData.CanClaimNPCReward = value;
            if (!_saveData.CanClaimNPCReward)
            {
                StopCoroutine(EnableClaimNpcReward());
                StartCoroutine(EnableClaimNpcReward());
            }
            DataChanged = true;
        }
    }

    public float TotalExp
    {
        get => _saveData.TotalExp;
        set
        {
            _saveData.TotalExp = value;
            UserManager.Instance.CalculateLevelAndCurLevelExp(out int level, out float exp);
            _saveData.CurLevelExp = exp;

            if (!UserManager.Instance.IsMaxLevel(CurLevel))
            {
                if (level > CurLevel)
                {
                    OnUserLevelUp?.Invoke(level);
                }
                CurLevel = level;
                OnUserExpChanged?.Invoke();
            }

            DataChanged = true;
        }
    }

    public void ChangeUserId(string userId)
    {
        _saveData.PlayerId = userId;
        DataChanged = true;
    }

    public void AddRewardToUserData(RewardData rewardData, float multiply = 1)
    {
        switch (rewardData.type)
        {
            case ERewardType.Currency:
                ModifyCurrencyDic(rewardData.currencyType, rewardData.amount * multiply);
                break;
            default:
                break;
        }
    }

    public void ModifyCurrencyDic(ECurrencyType type, float amount)
    {
        if (_saveData.UserCurrencyDic.ContainsKey(type))
        {
            _saveData.UserCurrencyDic[type] += amount;
        }
        else
        {
            _saveData.UserCurrencyDic.Add(type, amount);
        }
        OnUserCurrencyChanged?.Invoke();
        DataChanged = true;
    }

    public float GetCurrencyAmount(ECurrencyType type) => _saveData.UserCurrencyDic.ContainsKey(type) ? _saveData.UserCurrencyDic[type] : 0;

    public bool IsFurnitureUnlocked(string configId)
    {
        return _saveData.FurnitureUnlocked.Contains(configId);
    }

    public void UnlockFurniture(string furUnlock)
    {
        _saveData.FurnitureUnlocked.Add(furUnlock);
        DataChanged = true;
    }

    private IEnumerator EnableClaimNpcReward()
    {
        yield return new WaitForSeconds(300); // Wait for 5 minutes (300 seconds)
        CanClaimNPCReward = true;
    }

    #region ISaveable

    [Serializable]
    public class SaveData
    {
        public string PlayerId;
        public int Level = 1;
        public float CurLevelExp = 0;
        public float TotalExp = 0;
        public SerializableDictionaryBase<ECurrencyType, float> UserCurrencyDic = new SerializableDictionaryBase<ECurrencyType, float>();
        public int TutorialIndex = 0;
        public List<string> FurnitureUnlocked = new();
        public bool CanClaimNPCReward;

        #region SETTINGS

        public bool IsMusicOn = true;
        public bool IsSoundOn = true;
        public bool IsVibrateOn = true;

        #endregion
    }

    public string SaveKey => "UserData";

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

    public void OnAllDataLoaded()
    {
    }

    #endregion
}