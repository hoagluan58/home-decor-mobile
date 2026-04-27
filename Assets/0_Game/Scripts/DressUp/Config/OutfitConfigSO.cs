using NFramework;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OutfitConfigSO : GoogleSheetConfigSO<OutfitConfigData>
    {
        private Dictionary<string, OutfitConfigData> _outfitConfigDatas;

        public Dictionary<string, OutfitConfigData> OutfitConfigDataDic => _outfitConfigDatas;

        public void Init()
        {
            _outfitConfigDatas = new Dictionary<string, OutfitConfigData>();

            foreach(var data in _datas)
            {
                _outfitConfigDatas[data.Id] = data;
            }
        }

#if UNITY_EDITOR
        protected override void OnSynced(List<OutfitConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach(var data in _datas)
            {
                data.IconSprite = FileUtils.LoadFirstAssetWithName<Sprite>(data.IconSpriteString);
            }
        }
#endif
    }

    [Serializable]
    public class OutfitConfigData
    {
        public string Id;
        public string NameSkin;
        [ShowIf("UnlockType", UnlockType.Diamond)]
        public float Price;
        public OutfitType OutfitType;
        public UnlockType UnlockType;
        public Sprite IconSprite;

        [HideInInspector] public string IconSpriteString;
    }

    public enum OutfitType
    {
        Hair,
        Face,
        Shirt,
        Skirt,
        Dress,
        Shoe,
        Hat,
        Accessory,
        Skin
    }

    public enum UnlockType
    {
        Free,
        Ads,
        Diamond,
    }
}
