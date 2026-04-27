using IsoTools;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderRoomController : SingletonMono<OrderRoomController>
    {
        public static event Action<EOrderState> OnOrderStateChanged;

        [Header("CONTROLLERS")]
        [SerializeField] private IsoWorld _isoWorld;
        [SerializeField] private WallController _wallController;
        [SerializeField] private FloorController _floorController;
        [SerializeField] private OrderRoomWorldUI _roomWorldUI;
        [SerializeField] private ScreenshotHandler _screenshotHandler;

        [Header("STATES")]
        [SerializeField] private List<OrderModeState> _statesList;

        [Header("CONFIGS")]
        [SerializeField] private Vector3 _roomBounds;

        private List<FurnitureController> _furnitureList = new();
        private OrderModeState _curState;
        private OrderModeRoomConfigData _roomConfig;

        public ScreenshotHandler ScreenshotHandler => _screenshotHandler;
        public OrderCleanRoomState CleanRoomState => (OrderCleanRoomState)_statesList.FirstOrDefault(s => s.StateName == EOrderState.CleanTrash);
        public OrderUnboxFurnitureState UnboxFurnitureState => (OrderUnboxFurnitureState)_statesList.FirstOrDefault(s => s.StateName == EOrderState.UnboxFurniture);
        public OrderRepaintingRoomState RepaintingRoomState => (OrderRepaintingRoomState)_statesList.FirstOrDefault(s => s.StateName == EOrderState.Repainting);
        public OrderDecorState DecorRoomState => (OrderDecorState)_statesList.FirstOrDefault(s => s.StateName == EOrderState.Decorate);
        public OrderRoomWorldUI RoomWorldUI => _roomWorldUI;
        public IsoWorld IsoWorld => _isoWorld;
        public Vector3 RoomBounds => _roomBounds;
        public List<FurnitureController> FurnitureList
        {
            get => _furnitureList;
            set => _furnitureList = value;
        }
        public EOrderState CurOrderState => _curState == null ? EOrderState.None : _curState.StateName;

        private void Start() => Init();

        private void Update() => _curState?.OnUpdate();

        private void Init()
        {
            foreach (var s in _statesList)
            {
                s.Init(_isoWorld, this);
            }

            LoadDefaultRoom();
            ChangeState(EOrderState.CleanTrash);
        }

        private void LoadDefaultRoom()
        {
            _roomConfig = OrderModeManager.Instance.GetCurOrderRoomConfigData();
            SetWall(_roomConfig.DefaultWallId);
            SetFloor(_roomConfig.DefaultFloorId);
            SpawnDefaultFurnitures(_roomConfig.DefaultRoomFurnitures);

            return;

            void SpawnDefaultFurnitures(List<RoomFurnitureData> dataList)
            {
                foreach (var data in dataList)
                {
                    var fur = SpawnFurniture(data);
                    fur.MoveController.ToggleMoveable(false);
                }

                for (var i = 0; i < dataList.Count; i++)
                {
                    var data = dataList[i];
                    if (!data.NeedSetParent())
                        continue;

                    var parent = _furnitureList[data.ParentIndexInFurnitureList];
                    var child = _furnitureList[i];
                    child.NestedController.SetParent(parent);
                    parent.NestedController.AddChild(child);
                }
            }
        }

        public void SetWall(string wallId)
        {
            var wallConfig = AllConfig.Instance.WallConfigDic[wallId];
            _wallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
        }

        public void SetFloor(string floorId)
        {
            var floorConfig = AllConfig.Instance.FloorConfigDic[floorId];
            _floorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
        }

        public FurnitureController SpawnFurniture(RoomFurnitureData data)
        {
            var furInstance = FurnitureManager.Instance.Get(data.ConfigId);
            furInstance.gameObject.SetActive(true);
            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.FlipController.FlipTo(data.Direction);
            furInstance.MoveController.SetPosition(data.Position);
            furInstance.SpriteRenderer.sortingOrder = Define.SortingOrder.NORMAL;
            _furnitureList.Add(furInstance);
            return furInstance;
        }

        public void ChangeState(EOrderState stateName)
        {
            _curState?.Exit();
            _curState = _statesList.FirstOrDefault(s => s.StateName == stateName);
            if (_curState == null) return;
            _curState.Enter();
            OnOrderStateChanged?.Invoke(CurOrderState);
        }

        public void ReleaseFurniture(FurnitureController furRemove)
        {
            // Remove parent
            RoomHelper.RemoveNested(furRemove.NestedController.Parent, furRemove);

            // Remove all childs
            var childs = furRemove.NestedController.Childs;
            foreach (var child in childs)
            {
                _furnitureList.Remove(child);
                FurnitureManager.Instance.Release(child);
            }

            _furnitureList.Remove(furRemove);
            FurnitureManager.Instance.Release(furRemove);
        }

        public void TakeScreenshot()
        {
            var curBotOrderData = OrderModeManager.Instance.GetCurBotOrderData();
            var fileName = OrderModeManager.Instance.GetScreenshotFileName();
            _screenshotHandler.TakeScreenshot(fileName, 1024, 1024);
            OrderModeData.Instance.SaveScreenshotToOrderData(curBotOrderData, fileName);
        }
    }
}