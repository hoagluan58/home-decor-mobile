using System.Collections.Generic;
using Redcode.Extensions;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeRoomController : MonoBehaviour
    {
        public const float FLOOR_HEIGHT = 11.5f;

        [SerializeField] private int _roomIndex;
        [SerializeField] private Transform _furnitureContainer;
        [SerializeField] private WallController _wallController;
        [SerializeField] private FloorController _floorController;
        [SerializeField] private List<FurnitureController> _furList = new();

        public int AdditionRenderValue => ((int)FLOOR_HEIGHT + 20) * _roomIndex;
        public float AdditionZValue =>  FLOOR_HEIGHT * _roomIndex;

        public void LoadRoomData()
        {
            ClearRoom();
            if (_roomIndex == 1 && UserData.Instance.CurTutorialIndex != 5)
            {
                LockRoom();
            }
            else
            {
                var roomData = DecorModeData.Instance.GetRoomData(_roomIndex);
                SetWall(roomData.WallId);
                _floorController.SetActiveFloor(true);
                SetFloor(roomData.FloorId);
                foreach (var furData in roomData.FurnitureData)
                {
                    SpawnFurnitureByData(furData.ConfigId, furData.Position, furData.Direction, _roomIndex);
                }
            }
        }

        public void SetWall(string wallId)
        {
            var wallConfig = AllConfig.Instance.WallConfigDic[wallId];
            _wallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
            _wallController.SetSortingOrder(Define.SortingOrder.WALL +AdditionRenderValue);
        }

        public void SetFloor(string floorId)
        {
            var floorConfig = AllConfig.Instance.FloorConfigDic[floorId];
            _floorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
            _floorController.SetSortingOrder(Define.SortingOrder.FLOOR + AdditionRenderValue);
        }

        public FurnitureController SpawnFurnitureByData(string furId, Vector3 position, FurnitureDirection direction, int roomIndex)
        {
            var furInstance = FurnitureManager.Instance.Get(furId);
            furInstance.transform.SetParent(_furnitureContainer);
            furInstance.gameObject.SetActive(true);
            furInstance.FlipController.FlipTo(direction);
            var rightPosition = position.WithZ(position.z + AdditionZValue);
            furInstance.MoveController.SetPosition(rightPosition);
            furInstance.VisualController.Normalize(false);
            furInstance.SpriteRenderer.sortingOrder += AdditionRenderValue;
            _furList.Add(furInstance);
            return furInstance;
        }

        public void ClearRoom()
        {
            foreach (var fur in _furList)
            {
                FurnitureManager.Instance.Release(fur);
            }
        }

        public void LockRoom()
        {
            SetWall("lock_room");
            _floorController.SetActiveFloor(false);
        }

        public void UnlockRoom()
        {
            _floorController.SetActiveFloor(true);
            SetWall(Define.DefaultId.WALL);
            SetFloor(Define.DefaultId.FLOOR);
        }

        public bool PutCharacterInside(HomeCharController character)
        {
            var randomPosition = new Vector3(Random.Range(0, 12), Random.Range(0, 12), FLOOR_HEIGHT * _roomIndex);
            var validPosition = RoomHelper.GetValidPosition(randomPosition, character.FurnitureController.Size, FurnitureDirection.Right,
                character.FurnitureController, new Vector3(13, 13, 20), _furList, AdditionZValue);
            if (validPosition == Vector3.back) return false;

            validPosition = validPosition.WithZ(AdditionZValue);
            character.transform.SetParent(_furnitureContainer);
            character.FurnitureController.MoveController.SetPosition(validPosition);
            character.SetRenderOrder(Mathf.RoundToInt(Define.SortingOrder.NORMAL + AdditionRenderValue));
            character.SetSize(Vector3.one * 0.25f);
            return true;
        }
    }
}