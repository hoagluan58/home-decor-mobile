using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    [System.Serializable]
    public class RewardData
    {
        public ERewardType type;

        [ConditionalField(nameof(type), compareValues: ERewardType.Currency)]
        public ECurrencyType currencyType;
        [ConditionalField(nameof(type), compareValues: ERewardType.Furniture)]
        public string furnitureId;

        public Sprite icon;
        public int amount;

        public RewardData(ERewardType type, ECurrencyType currencyType, int amount)
        {
            this.type = type;
            this.currencyType = currencyType;
            this.icon = GetIconSprite(currencyType);
            this.amount = amount;
        }

        public RewardData(ERewardType type, int amount)
        {
            this.type = type;
            this.icon = GetIconSprite();
            this.amount = amount;
        }

        public RewardData(ERewardType type, string furnitureId, int amount)
        {
            this.type = type;
            this.furnitureId = furnitureId;
            this.icon = GetIconSprite();
            this.amount = amount;
        }

        private Sprite GetIconSprite(ECurrencyType currencyType = ECurrencyType.Diamond)
        {
            switch (type)
            {
                case ERewardType.Currency:
#if UNITY_EDITOR
                    return FileUtils.LoadFirstAssetWithName<Sprite>($"currency_{currencyType}".ToLower());
#else
                    return SpriteManager.Instance.GetCurrencySprite(currencyType);
#endif
                case ERewardType.Experience:
#if UNITY_EDITOR
                    return FileUtils.LoadFirstAssetWithName<Sprite>($"experience_star".ToLower());
#else
                    return SpriteManager.Instance.GetSprite(ESpriteType.CurrencyStar);
#endif
                case ERewardType.Furniture:
#if UNITY_EDITOR
                    return FileUtils.GetAssetByName<Sprite>(furnitureId); ;
#else
                    return FurnitureManager.Instance.GetFurnitureConfig(furnitureId).Config.Sprite;
#endif
                default:
                    return null;
            }
        }
    }

    public enum ERewardType
    {
        Experience,
        Currency,
        Outfit,
        Furniture,
    }

    public enum ECurrencyType
    {
        Diamond,
        Star,
        OrderCleanTrash,
    }
}