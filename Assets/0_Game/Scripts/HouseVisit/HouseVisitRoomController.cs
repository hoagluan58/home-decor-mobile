using IsoTools;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HouseVisitRoomController : MonoBehaviour
    {
        [SerializeField] private IsoWorld _isoWorld;
        [SerializeField] private WallController _wallController;
        [SerializeField] private FloorController _floorController;

        private BotHouseConfigData _roomConfig;
        private List<FurnitureController> _furnitureList;

        private void Start() => Init();

        private void Init() => LoadRoom();

        private void LoadRoom()
        {
            _roomConfig = HouseVisitManager.Instance.GetCurBotRoomConfigData();
            _furnitureList = new List<FurnitureController>();
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

        private void SetWall(string wallId)
        {
            var wallConfig = AllConfig.Instance.WallConfigDic[wallId];
            _wallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
        }

        private void SetFloor(string floorId)
        {
            var floorConfig = AllConfig.Instance.FloorConfigDic[floorId];
            _floorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
        }

        private FurnitureController SpawnFurniture(RoomFurnitureData data)
        {
            var furInstance = FurnitureManager.Instance.Get(data.ConfigId);
            furInstance.gameObject.SetActive(true);
            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.FlipController.FlipTo(data.Direction);
            furInstance.MoveController.SetPosition(data.Position);
            furInstance.VisualController.Normalize(false);
            _furnitureList.Add(furInstance);
            return furInstance;
        }
    }
}
