using System;
using System.Collections.Generic;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class BotHouseConfigSO : GoogleSheetConfigSO<BotHouseConfigData>
    {
        private Dictionary<string, BotHouseConfigData> _botHouseConfigDic;
        public Dictionary<string, BotHouseConfigData> BotHouseConfigDic => _botHouseConfigDic;

        public void Init()
        {
            _botHouseConfigDic = new Dictionary<string, BotHouseConfigData>();

            foreach (var data in _datas)
            {
                _botHouseConfigDic[data.Id] = data;
            }
        }

#if UNITY_EDITOR
        protected override void OnSynced(List<BotHouseConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.Parse();
            }
        }
#endif
    }

    [Serializable]
    public class BotHouseConfigData
    {
        public string Id;
        public string BotId;
        public int Like;
        public string DefaultFloorId;
        public string DefaultWallId;

        public List<RoomFurnitureData> DefaultRoomFurnitures;

#if UNITY_EDITOR
        [HideInInspector] public string DefaultRoomFurnitureString;

        public void Parse()
        {
            DefaultRoomFurnitures = ParseDefaultRoomFurnitures();
        }

        private List<RoomFurnitureData> ParseDefaultRoomFurnitures()
        {
            var result = new List<RoomFurnitureData>();

            if (string.IsNullOrEmpty(DefaultRoomFurnitureString))
                return result;

            var furDelimeter = " ";
            var splitFurData = DefaultRoomFurnitureString.Split(furDelimeter);

            foreach (var furDataString in splitFurData)
            {
                var roomFurData = Utilities.ParseStringToRoomFurnitureData(furDataString);
                result.Add(roomFurData);
            }

            return result;
        }
#endif
    }
}
