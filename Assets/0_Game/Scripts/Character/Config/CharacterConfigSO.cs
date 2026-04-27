using NFramework;
using Spine.Unity;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class CharacterConfigSO : GoogleSheetConfigSO<CharacterConfigData>
    {
        private Dictionary<string, CharacterConfigData> _characterConfigDic = new();

        public Dictionary<string, CharacterConfigData> CharacterConfigDic => _characterConfigDic;

        public void Init()
        {
            _characterConfigDic = new Dictionary<string, CharacterConfigData>();

            foreach (var data in _datas)
            {
                _characterConfigDic[data.Id] = data;
            }
        }

#if  UNITY_EDITOR
        protected override void OnSynced(List<CharacterConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.OutfitDefault = Utilities.ParseStringToOutfitDatas(data.StringOutfitDefault);
                data.Icon = FileUtils.LoadFirstAssetWithName<Sprite>(data.StringIcon);
                data.OutfitConfigSo = FileUtils.LoadFirstAssetWithName<OutfitConfigSO>(data.StringOutfitSO);
                data.SkeletonDataAsset = FileUtils.LoadFirstAssetWithName<SkeletonDataAsset>(data.StringSkeletonAsset);
            }
        }
#endif
    }

    [Serializable]
    public class CharacterConfigData
    {
        public string Id;
        public string Name;
        public Sprite Icon;
        public SkeletonDataAsset SkeletonDataAsset;
        public OutfitConfigSO OutfitConfigSo;
        public List<CharacterOutfitData> OutfitDefault;

        [HideInInspector] public string StringOutfitDefault;
        [HideInInspector] public string StringIcon;
        [HideInInspector] public string StringOutfitSO;
        [HideInInspector] public string StringSkeletonAsset;
    }
}
