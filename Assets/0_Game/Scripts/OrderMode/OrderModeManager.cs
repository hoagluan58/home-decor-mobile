using NFramework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace YoyoDesign
{
    public class OrderModeManager : Singleton<OrderModeManager>
    {
        public class OrderRewardData
        {
            public int DiamondReward;
            public int StarReward;
            public string FurnitureReward;

            public OrderRewardData(int diamondReward, int starReward, string furnitureReward)
            {
                DiamondReward = diamondReward;
                StarReward = starReward;
                FurnitureReward = furnitureReward;
            }
        }

        public OrderRewardData LastestRewardData;
        public int PrevLandId => _prevLandId;
        public bool NeedShowRewardPopup { get; set; }
        public bool IsLastFurnitureInLand { get; set; }

        private const string FLAG = "";
        private const int MAX_ORDER_RATING = 5;
        private static readonly int BOT_NODE_PATTERN_COUNT = 3;

        private int _prevLandId = -1;
        private BotOrderData _curBotOrderData;
        private OrderModeRoomConfigData _curRoomConfig;
        private string _historyOrderId = FLAG;

        private Dictionary<int, int> _orderOptionAdsItem = new Dictionary<int, int>();
        private List<int> _watchedOrderOptionItemKey = new List<int>();

        public OrderMapData TryLoadMap()
        {
            if (OrderModeData.Instance.IsMapClear())
            {
                GenerateMap();
            }
            return OrderModeData.Instance.OrderMapData;
        }

        public void TakeOrder(BotOrderData botOrderData, int prevLandId = -1)
        {
            NeedShowRewardPopup = false;
            _prevLandId = prevLandId;
            _curBotOrderData = botOrderData;
            _curRoomConfig = GetOrderModeRoomConfigData(_curBotOrderData.RoomId);
            InitAdsOrderOption();
        }

        public void QuitOrder()
        {
            NeedShowRewardPopup = false;
            IsLastFurnitureInLand = false;
            _curBotOrderData = null;
            _curRoomConfig = null;
        }

        public void RetakeOrder(string historyOrderId)
        {
            _historyOrderId = historyOrderId;
            var historyOrderData = OrderModeData.Instance.OrderHistoryDatas.Find(x => x.Id == historyOrderId);
            TakeOrder(historyOrderData.OrderData);
        }

        public (int, int, string) DoneOrder(bool isRewardAds = false)
        {
            var isRetakeOrder = _historyOrderId != FLAG;

            if (isRetakeOrder)
            {
                OrderModeData.Instance.SaveRetakeOrder(_historyOrderId, _curBotOrderData);
            }
            else
            {
                OrderModeData.Instance.SaveOrder(_curBotOrderData);
            }

            var multiply = isRewardAds ? 3 : 1;
            var diamondReward = GetDiamondRewardData(multiply);
            var starReward = _curBotOrderData.StarRating;
            var curLandId = HomeLandData.Instance.CurrentLandId;

            UserData.Instance.ModifyCurrencyDic(diamondReward.currencyType, diamondReward.amount);
            UserData.Instance.ModifyCurrencyDic(ECurrencyType.Star, starReward);
            var furReward = HomeLandData.Instance.UnlockNewDecorFurniture(curLandId);

            if (UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_INTRO)
            {
                UserData.Instance.CurTutorialIndex = Define.TutorialIndex.DONE_ORDER;
            }

            _historyOrderId = FLAG;
            _curBotOrderData = null;
            _curRoomConfig = null;

            IsLastFurnitureInLand = HomeLandData.Instance.IsLandCompleted(curLandId) && !HomeLandData.Instance.IsLastLandId(curLandId);
            LastestRewardData = new OrderRewardData(diamondReward.amount, starReward, furReward != null ? furReward.ConfigId : null);
            NeedShowRewardPopup = true;
            return (diamondReward.amount, starReward, furReward != null ? furReward.ConfigId : null);
        }

        public string GetScreenshotFileName()
        {
            var result = "";

            if (_curBotOrderData == null)
                return result;

            var isRetakeOrder = _historyOrderId != FLAG;
            var id = isRetakeOrder ? _historyOrderId : FriendlyGUID.NewId_FromRandomInt();

            result = $"OrderRoomCompleted_{id}";
            return result;
        }

        public RewardData GetDiamondRewardData(int multiply = 1)
        {
            var amount = _curBotOrderData.StarRating == MAX_ORDER_RATING ? Define.ConstValue.ORDER_REWARD_5_STAR : Define.ConstValue.ORDER_REWARD_4_STAR;
            return new RewardData(ERewardType.Currency, ECurrencyType.Diamond, amount * multiply);
        }

        public RewardData GetExperienceRewardData() => new RewardData(ERewardType.Experience, _curBotOrderData.StarRating);

        public BotOrderData GetCurBotOrderData() => _curBotOrderData;

        public OrderModeRoomConfigData GetCurOrderRoomConfigData() => _curRoomConfig;

        public OrderModeRoomConfigData GetOrderModeRoomConfigData(string id) => AllConfig.Instance.OrderModeRoomConfigDic[id];

        public int GetAdsItemOptionAtIndex(int index)
        {
            var isTutorial = UserData.Instance.CurTutorialIndex == 0;
            if (isTutorial)
                return -1;

            if (_orderOptionAdsItem.TryGetValue(index, out var value))
            {
                var result = _watchedOrderOptionItemKey.Contains(index) ? -1 : value;
                return result;
            }

            var rndValue = UnityEngine.Random.Range(0, 3);
            _orderOptionAdsItem.Add(index, rndValue);
            return rndValue;
        }

        public void AddToWatchedItemKey(int key)
        {
            if (_watchedOrderOptionItemKey.Contains(key))
                return;

            _watchedOrderOptionItemKey.Add(key);
        }

        public void InitAdsOrderOption()
        {
            _orderOptionAdsItem = new Dictionary<int, int>();
            _watchedOrderOptionItemKey = new List<int>();
        }

        private void GenerateMap()
        {
            var mapData = new OrderMapData()
            {
                MapId = RandomMapId(),
                BotOrderDatas = GenerateBotOrderDatas(),
            };
            OrderModeData.Instance.SaveMap(mapData);
        }

        private List<BotOrderData> GenerateBotOrderDatas()
        {
            var result = new List<BotOrderData>();
            var nodeCount = RandomBotNodeNumber();

            var botIds = new string[nodeCount];
            var roomTypes = new EBotRoomType[nodeCount];
            var randomizedRoomType = new List<EBotRoomType>();

            for (int i = 0; i < nodeCount; i++)
            {
                var orderId = i;
                var botId = BotManager.Instance.GetRandomBotId();
                var roomType = RandomBotRoomType();

                while (randomizedRoomType.Contains(roomType))
                {
                    roomType = RandomBotRoomType();
                    if (randomizedRoomType.Count >= 3)
                        break;
                }

                while (botIds.Contains(botId))
                {
                    botId = BotManager.Instance.GetRandomBotId();
                }

                var roomId = RandomRoomIdWithType(roomType);

                botIds[i] = botId;
                roomTypes[i] = roomType;
                randomizedRoomType.Add(roomType);
                result.Add(new BotOrderData(orderId, botId, roomType, roomId));
            }

            return result;
        }

        private int RandomBotNodeNumber() => UnityEngine.Random.Range(2, 4);

        private int RandomMapId() => UnityEngine.Random.Range(0, BOT_NODE_PATTERN_COUNT);

        private string RandomRoomIdWithType(EBotRoomType roomType)
            => AllConfig.Instance.OrderModeRoomConfigDic.Values.Where(x => x.RoomType == roomType).ToList().RandomItem().RoomId;

        private EBotRoomType RandomBotRoomType() => Utilities.RandomEnumValue<EBotRoomType>();
    }

    public enum EBotRoomType
    {
        Bedroom,
        Kitchen,
        LivingRoom,
        Bathroom,
    }

    public enum EOrderState
    {
        None,
        CleanTrash,
        RemoveOldFurniture,
        Repainting,
        UnboxFurniture,
        Decorate
    }

    public enum EOrderDecorTier
    {
        Tier1,
        Tier2,
        Tier3,
    }
}
