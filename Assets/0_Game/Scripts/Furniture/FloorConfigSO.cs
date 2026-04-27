using NFramework;
using System;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class FloorConfigSO : GoogleSheetConfigSO<FloorConfigData>
    {
        private Dictionary<string, FloorConfigData> _floorConfigDic;
        public Dictionary<string, FloorConfigData> FloorConfigDic => _floorConfigDic;

        public void Init()
        {
            _floorConfigDic = new Dictionary<string, FloorConfigData>();
            foreach (var data in _datas)
            {
                _floorConfigDic[data.Id] = data;
            }
        }

#if UNITY_EDITOR
        [Button]
        protected override void Sync()
        {
            base.Sync();
        }

        protected override void OnSynced(List<FloorConfigData> googleSheetData)
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
    public class FloorConfigData
    {
        public string Id;
        public Sprite Sprite;
        public FurnitureUnlockType UnlockType;
        public float Price;
        public bool IsMessy;
        public ERoomConceptName Concept;
        public bool IsDecor;

        [HideInInspector] public string SpriteString;
    }
}