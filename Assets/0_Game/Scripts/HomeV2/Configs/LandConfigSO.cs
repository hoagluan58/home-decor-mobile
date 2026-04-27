using NFramework;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class LandConfigSO : GoogleSheetConfigSO<LandConfigData>
    {
        private Dictionary<int, LandConfigData> _landConfigDic;
        public Dictionary<int, LandConfigData> LandConfigDic => _landConfigDic;

        public void Init()
        {
            _landConfigDic = new Dictionary<int, LandConfigData>();
            foreach (var item in _datas)
            {
                _landConfigDic.Add(item.Id, item);
            }
        }

#if UNITY_EDITOR
        protected override void OnSynced(List<LandConfigData> googleSheetData)
        {
            base.OnSynced(googleSheetData);
            foreach (var data in _datas)
            {
                data.Parse();
            }
        }
#endif
    }

    [System.Serializable]
    public class LandConfigData
    {
        public int Id;
        public Vector3 Position;
        public string DefaultWallId;
        public string DefaultFloorId;
        public List<int> NearbyLandList;
        public SerializableDictionaryBase<ELandNavigation, int> LandNavigation;
        public List<FurnitureOptionData> UnlockFurnitureOptions;
        public List<FurniturePrice> UnlockFurniturePrices;
        public List<RoomFurnitureData> DecorFurnitureData;
        public List<int> UnlockFurnitureStarPrices;
        public int AdditionSortingValue;
        public int SubtractionSortingValue;

        [HideInInspector] public string PositionString;
        [HideInInspector] public string NearbyLandListString;
        [HideInInspector] public string LandNavigationString;
        [HideInInspector] public string DefaultRoomFurnitureString;
        [HideInInspector] public string UnlockFurnitureOptionString;
        [HideInInspector] public string UnlockFurnitureOptionPriceString;
        [HideInInspector] public string UnlockFurnitureStarPriceString;
        [HideInInspector] public string DecorDataOptionString;

        public void Parse()
        {
            Position = Utilities.ParseStringToVector3(PositionString);
            NearbyLandList = Utilities.ParseStringToList<int>(NearbyLandListString, ",");
            LandNavigation = Utilities.ParseStringToDictionary(LandNavigationString, "-");
            UnlockFurnitureOptions = Utilities.ParseStringToFurnitureOptions(UnlockFurnitureOptionString, " ");
            UnlockFurniturePrices = ParseStringToFurniturePrices(UnlockFurnitureOptionPriceString, " ");
            UnlockFurnitureStarPrices = Utilities.ParseStringToList<int>(UnlockFurnitureStarPriceString, ",");
            DecorFurnitureData = Utilities.ParseStringToRoomFurnitureDatas(DecorDataOptionString, " ");
        }

        private List<FurniturePrice> ParseStringToFurniturePrices(string dataString, string delimeter)
        {
            var result = new List<FurniturePrice>();

            if (string.IsNullOrEmpty(dataString))
            {
                return result;
            }

            var splitDatas = dataString.Split(delimeter);
            foreach (var data in splitDatas)
            {
                var furniturePrice = new FurniturePrice();
                furniturePrice.Prices = Utilities.ParseStringToList<int>(data, ";");
                result.Add(furniturePrice);
            }
            return result;
        }
    }

    [System.Serializable]
    public class FurniturePrice
    {
        public List<int> Prices = new List<int>();
    }

    public enum ELandNavigation
    {
        Up,
        Down,
        Left,
        Right,
        UpLeft,
        UpRight,
        DownLeft,
        DownRight
    }
}