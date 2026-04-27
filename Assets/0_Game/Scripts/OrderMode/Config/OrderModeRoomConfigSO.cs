using DG.Tweening.Plugins.Options;
using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderModeRoomConfigSO : GoogleSheetConfigSO<OrderModeRoomConfigData>
    {
        private Dictionary<string, OrderModeRoomConfigData> _orderModeRoomConfigDic;
        public Dictionary<string, OrderModeRoomConfigData> OrderModeRoomConfigDic => _orderModeRoomConfigDic;

        public void Init()
        {
            _orderModeRoomConfigDic = new Dictionary<string, OrderModeRoomConfigData>();
            foreach (var data in _datas)
            {
                _orderModeRoomConfigDic[data.RoomId] = data;
            }
        }

#if UNITY_EDITOR
        protected override void OnSynced(List<OrderModeRoomConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.Parse();
                data.DefaultRoomSprite = FileUtils.LoadFirstAssetWithName<Sprite>(data.DefaultRoomSpriteString);
            }
        }
#endif
    }

    [System.Serializable]
    public class OrderModeRoomConfigData
    {
        public string RoomId;
        public EBotRoomType RoomType;
        public int UnboxStartIndex;
        public string DefaultFloorId;
        public string DefaultWallId;
        public Sprite DefaultRoomSprite;
        public List<RoomFurnitureData> DefaultRoomFurnitures;
        public OrderOptionData OrderOptions;
        public SerializableDictionaryBase<EOrderDecorTier, OrderModeDecorTierData> DecorTierDic;

        public EOrderDecorTier GetRoomFurnitureDecorTier(string id)
        {
            var result = EOrderDecorTier.Tier3;
            foreach (var tier in DecorTierDic.Keys)
            {
                foreach (var decorId in DecorTierDic[tier].DecorIdList)
                {
                    if (decorId == id)
                    {
                        result = tier;
                        break;
                    }
                }
            }
            return result;
        }

#if UNITY_EDITOR
        [HideInInspector] public string DefaultRoomSpriteString;
        [HideInInspector] public string DefaultRoomFurnitureString;
        [HideInInspector] public string WallDataOptionString;
        [HideInInspector] public string FloorDataOptionString;
        [HideInInspector] public string FurnitureDataOptionString;
        [HideInInspector] public string DecorDataOptionString;
        [HideInInspector] public string DecorTierDicString;

        public void Parse()
        {
            DefaultRoomFurnitures = ParseDefaultRoomFurnitures();
            OrderOptions = new OrderOptionData(ParseWallOptions(),
                                               ParseFloorOptions(),
                                               ParseStringToFurnitureOptions(),
                                               ParseStringToRoomFurnitureDatas(DecorDataOptionString, " "),
                                               UnboxStartIndex);
            DecorTierDic = ParseStringToDecorTierDic();
        }

        // mr_bed_2|5,8,0|Left|-1
        // mr_sidebed_1|10,2,0|Right|-1

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

        private List<string> ParseWallOptions()
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(WallDataOptionString))
                return result;

            var delimeter = ";";
            var splitData = WallDataOptionString.Split(delimeter);

            foreach (var data in splitData)
            {
                result.Add(data);
            }
            return result;
        }

        private List<string> ParseFloorOptions()
        {
            var result = new List<string>();

            if (string.IsNullOrEmpty(FloorDataOptionString))
                return result;

            var delimeter = ";";
            var splitData = FloorDataOptionString.Split(delimeter);

            foreach (var data in splitData)
            {
                result.Add(data);
            }
            return result;
        }

        private List<FurnitureOptionData> ParseStringToFurnitureOptions()
        {
            var result = new List<FurnitureOptionData>();

            if (string.IsNullOrEmpty(FurnitureDataOptionString))
                return result;
            var furOptionDelimeter = " ";
            var furOptionsString = FurnitureDataOptionString.Split(furOptionDelimeter);

            foreach (var data in furOptionsString)
            {
                var furnitureOptions = ParseStringToRoomFurnitureDatas(data, ";");
                var orderFurOption = new FurnitureOptionData(furnitureOptions);
                result.Add(orderFurOption);
            }

            return result;
        }

        private List<RoomFurnitureData> ParseStringToRoomFurnitureDatas(string dataString, string delimeter)
        {
            var result = new List<RoomFurnitureData>();

            if (string.IsNullOrEmpty(dataString))
                return result;

            var splitData = dataString.Split(delimeter);
            foreach (var furOption in splitData)
            {
                var data = Utilities.ParseStringToRoomFurnitureData(furOption);
                result.Add(data);
            }
            return result;
        }

        private SerializableDictionaryBase<EOrderDecorTier, OrderModeDecorTierData> ParseStringToDecorTierDic()
        {
            var result = new SerializableDictionaryBase<EOrderDecorTier, OrderModeDecorTierData>();
            var orderDecorOption = OrderOptions.DecorOptions;
            if (string.IsNullOrEmpty(DecorTierDicString))
                return result;

            var decorTierString = DecorTierDicString.Split(" ");
            foreach (var str in decorTierString)
            {
                var splitData = str.Split("-");
                var decorIdString = splitData[1].Split(";");

                if (Enum.TryParse<EOrderDecorTier>(splitData[0], out var tier))
                {
                    if (!result.ContainsKey(tier))
                    {
                        result.Add(tier, new OrderModeDecorTierData(new List<string>()));
                    }

                    foreach (var data in decorIdString)
                    {
                        var index = int.Parse(data);
                        var decor = orderDecorOption[index];
                        result[tier].DecorIdList.Add(decor.Id);
                    }
                }
            }
            return result;
        }
#endif
    }

    [System.Serializable]
    public class OrderModeDecorTierData
    {
        public List<string> DecorIdList;

        public OrderModeDecorTierData(List<string> decorIdList)
        {
            DecorIdList = decorIdList;
        }
    }
}
