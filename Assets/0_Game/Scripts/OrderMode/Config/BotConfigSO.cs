using NFramework;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class BotConfigSO : GoogleSheetConfigSO<BotConfigData>
    {
        private Dictionary<string, BotConfigData> _botConfigDic;
        public Dictionary<string, BotConfigData> BotConfigDic => _botConfigDic;

        public void Init()
        {
            _botConfigDic = new Dictionary<string, BotConfigData>();

            foreach (var data in _datas)
            {
                _botConfigDic[data.Id] = data;
            }
        }

#if UNITY_EDITOR
        [SerializeField] private OutfitConfigSO _outfitConfigSO;

        protected override void OnSynced(List<BotConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);

            _outfitConfigSO.Init();

            foreach (var data in _datas)
            {
                data.AvatarSprite = FileUtils.LoadFirstAssetWithName<Sprite>(data.AvatarSpriteString);
                data.OutfitDatas = Utilities.ParseStringToOutfitDatas(data.OutfitDataString);
            }
        }
#endif
    }

    [System.Serializable]
    public class BotConfigData
    {
        public string Id;
        public string Name;
        public Sprite AvatarSprite;
        public List<CharacterOutfitData> OutfitDatas;

#if UNITY_EDITOR
        [HideInInspector] public string AvatarSpriteString;
        [HideInInspector] public string OutfitDataString;
#endif
    }
}
