using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace YoyoDesign
{
    public class SpriteManager : SingletonMono<SpriteManager>
    {
        [SerializeField] private SerializableDictionaryBase<ESpriteType, Sprite> _spriteDictionary = new SerializableDictionaryBase<ESpriteType, Sprite>();

        public Sprite GetSprite(ESpriteType spriteType)
        {
            if (_spriteDictionary.TryGetValue(spriteType, out var sprite))
            {
                return sprite;
            }
            Logger.Log(this, $"Can't get sprite: {spriteType}");
            return null;
        }

        public Sprite GetCurrencySprite(ECurrencyType currencyType)
        {
            switch (currencyType)
            {
                case ECurrencyType.Diamond:
                    return GetSprite(ESpriteType.CurrencyDiamond);
                case ECurrencyType.Star:
                    return GetSprite(ESpriteType.CurrencyStar);
                default:
                    return GetSprite(ESpriteType.CurrencyStar);
            }
        }
    }

    public enum ESpriteType
    {
        CurrencyStar,
        CurrencyDiamond,
    }
}
