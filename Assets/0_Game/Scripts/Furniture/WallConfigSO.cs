using NFramework;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class WallConfigSO : GoogleSheetConfigSO<WallConfigData>
    {
        private Dictionary<string, WallConfigData> _wallConfigDic;
        public Dictionary<string, WallConfigData> WallConfigDic => _wallConfigDic;

        public void Init()
        {
            _wallConfigDic = new Dictionary<string, WallConfigData>();
            foreach (var data in _datas)
            {
                _wallConfigDic[data.Id] = data;
            }
        }

#if UNITY_EDITOR

        [Button]
        protected override void Sync()
        {
            base.Sync();
        }

        protected override void OnSynced(List<WallConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.Sprite = FileUtils.LoadFirstAssetWithName<Sprite>(data.SpriteString);
            }
        }
#endif
    }

    [Serializable]
    public class WallConfigData
    {
        public string Id;
        public ERoomConceptName Concept;
        public Sprite Sprite;
        public FurnitureUnlockType UnlockType;
        public float Price;
        public bool IsMessy;
        public bool IsDecor;

        [HideInInspector] public string SpriteString;
    }
}
