using Cysharp.Threading.Tasks;
using NFramework;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderModeData : SingletonMono<OrderModeData>, ISaveable
    {
        private readonly string SCREENSHOT_FOLDER_PATH = "Screenshot/OrderRoom/";

        [SerializeField] private SaveData _saveData;

        public OrderMapData OrderMapData => _saveData.MapData;
        public int NextHistoryOrderId => _saveData.HistoryData.Count;

        public List<OrderHistoryData> OrderHistoryDatas => _saveData.HistoryData;

        public void SaveMap(OrderMapData data)
        {
            _saveData.MapData = data;
            DataChanged = true;
        }

        public void SaveOrder(BotOrderData botOrderData)
        {
            var isTutorialOrder = botOrderData.OrderId == Define.DefaultId.TUTORIAL_ORDER_ID;
            if (isTutorialOrder)
            {
                UserData.Instance.CurTutorialIndex = Define.TutorialIndex.DONE_INTRO;
                return;
            }

            botOrderData.IsDone = true;
            SaveToHistoryOrder(FriendlyGUID.NewId_FromRandomInt(), botOrderData);
            DataChanged = true;
        }

        public void SaveRetakeOrder(string retakeOrderId, BotOrderData data)
        {
            var historyOrderData = _saveData.HistoryData.Find(x => x.Id == retakeOrderId);
            historyOrderData.OrderData = data;
            SaveToHistoryOrder(retakeOrderId, data);
            DataChanged = true;
        }

        public bool IsMapClear()
        {
            if (_saveData.MapData == null) return true;
            if (_saveData.MapData.BotOrderDatas.Count == 0) return true;
            return _saveData.MapData.BotOrderDatas.All(x => x.IsDone);
        }

        public BotOrderData GetFirstNotDoneOrder()
        {
            foreach (var data in _saveData.MapData.BotOrderDatas)
            {
                if (data.IsDone)
                    continue;

                return data;
            }

            return null;
        }

        public void SaveScreenshotToOrderData(BotOrderData botOrderData, string fileName)
        {
            //var tempTexture = new Texture2D(1024, 1024);
            var savePath = Path.Combine(Application.persistentDataPath, fileName);
            //var data = File.ReadAllBytes(savePath);
            //tempTexture.LoadImage(data);
            //var sprite = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, tempTexture.height), new Vector2(1024 / 2f, 1024 / 2f));
            //botOrderData.RoomSprite = sprite;
            botOrderData.RoomSpritePath = savePath;
            DataChanged = true;
        }

        public async UniTask<Sprite> LoadRoomScreenshot(string filePath)
        {
            try
            {
                var data = await File.ReadAllBytesAsync(filePath);
                var tempTexture = new Texture2D(1024, 1024);
                tempTexture.LoadImage(data);
                tempTexture.Apply();
                var sprite = Sprite.Create(tempTexture, new Rect(0, 0, tempTexture.width, tempTexture.height), new Vector2(1024 / 2f, 1024 / 2f), 100f, 0, SpriteMeshType.FullRect);
                return sprite;
            }
            catch
            {
                Logger.Log(this, $"Can't load room screenshot at {filePath}");
                return null;
            }
        }

        private void SaveToHistoryOrder(string id, BotOrderData data)
        {
            var orderHistoryData = _saveData.HistoryData.Find(x => x.Id == id);
            if (orderHistoryData != null)
            {
                orderHistoryData.OrderData = data;
                orderHistoryData.OrderData.NeedReloadRoomSprite = true;
                _saveData.HistoryData.Remove(orderHistoryData);
                _saveData.HistoryData.Insert(0, orderHistoryData);
            }
            else
            {
                _saveData.HistoryData.Insert(0, new OrderHistoryData(id, data));
            }
            if (_saveData.HistoryData.Count > 10)
            {
                _saveData.HistoryData.RemoveAt(_saveData.HistoryData.Count - 1);
            }
            DataChanged = true;
        }

        #region ISaveable

        [System.Serializable]
        public class SaveData
        {
            public OrderMapData MapData;
            public List<OrderHistoryData> HistoryData = new List<OrderHistoryData>();
        }

        public string SaveKey => "OrderData";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new SaveData();
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<SaveData>(data);
            }
        }

        public void OnAllDataLoaded()
        {
        }

        #endregion
    }

    [System.Serializable]
    public class OrderMapData
    {
        public int MapId;
        public List<BotOrderData> BotOrderDatas = new List<BotOrderData>();
    }

    [System.Serializable]
    public class BotOrderData
    {
        public int OrderId;
        public string BotId;
        public EBotRoomType RoomType;
        public string RoomId;
        public bool IsDone;
        public int StarRating = 4;
        public BotConfigData BotData;
        public string RoomSpritePath;

        [HideInInspector] public bool NeedReloadRoomSprite = false;
        private Sprite _roomSprite; // Save with data path

        public BotOrderData(int orderId, string botId, EBotRoomType roomType, string roomId)
        {
            OrderId = orderId;
            BotId = botId;
            RoomType = roomType;
            RoomId = roomId;
            IsDone = false;
            BotData = BotManager.Instance.GetBotConfigData(BotId);
        }

        public BotOrderData()
        {

        }

        public async UniTask<Sprite> GetRoomScreenshotSprite()
        {
            if (_roomSprite == null || NeedReloadRoomSprite)
            {
                _roomSprite = await OrderModeData.Instance.LoadRoomScreenshot(RoomSpritePath);
                NeedReloadRoomSprite = false;
            }
            return _roomSprite;
        }

        public string GetOrderName()
        {
            var botName = BotManager.Instance.GetBotConfigData(BotId).Name;
            return $"{botName}'s {RoomType}";
        }


        public Sprite GetDefaultRoomSprite() => AllConfig.Instance.OrderModeRoomConfigDic[RoomId].DefaultRoomSprite;
    }

    [System.Serializable]
    public class OrderHistoryData
    {
        public string Id;
        public BotOrderData OrderData;

        public OrderHistoryData(string id, BotOrderData orderData)
        {
            Id = id;
            OrderData = orderData;
        }
    }
}