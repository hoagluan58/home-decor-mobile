using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using IsoTools;
using NFramework;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class DecorRoomController : MonoBehaviour
    {
        [Title("CONFIG")]
        [SerializeField] private int _roomIndex;

        [Title("ISO WORLD")]
        [SerializeField] private IsoWorld _isoWorld;

        [Title("WALL AND FLOOR")]
        [SerializeField] private FloorController _floorController;
        [SerializeField] private WallController _wallController;

        [Title("SUB CONTROLLERS")]
        [SerializeField] private DecorDataController _dataController;
        [SerializeField] private DecorHistoryController _decorHistoryController;
        [SerializeField] private DecorFurnitureController _furnitureController;

        public int RoomIndex => _roomIndex;
        public DecorHistoryController HistoryController => _decorHistoryController;
        public DecorDataController DataController => _dataController;
        public DecorFurnitureController FurnitureController => _furnitureController;
        public FloorController FloorController => _floorController;
        public WallController WallController => _wallController;

        public void Init()
        {
            var decorData = DecorModeData.Instance.GetRoomData(_roomIndex);
            LoadData(decorData);
            _dataController.SaveTemp();
            transform.position = Vector3.up * (_roomIndex * 20);
            _furnitureController.enabled = false;

            if (_roomIndex == 1)
            {
                this.gameObject.SetActive(UserData.Instance.CurLevel >= 5);
            }
        }

        public void Enter(Action moveDone)
        {
            if (transform.position != Vector3.zero)
            {
                transform.DOMove(Vector3.zero, 0.5f)
                    .OnComplete(() =>
                    {
                        _furnitureController.ReleaseAll();
                        var decorData = _dataController.TempData;
                        LoadData(decorData);
                        _decorHistoryController.InitHistory();
                        _furnitureController.enabled = true;
                        moveDone?.Invoke();
                    });
            }
            else
            {
                _decorHistoryController.InitHistory();
                _furnitureController.enabled = true;
                moveDone?.Invoke();
            }
        }

        public void Exit()
        {
            _furnitureController.CurFurniture = null;
            _dataController.SaveTemp();
            transform.DOMoveY(_roomIndex == 0 ? -20 : 20, 0.5f);
            _furnitureController.enabled = false;
            _decorHistoryController.ClearHistory();
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

        public void SpawnFurniture(List<RoomFurnitureData> furnitureData)
        {
            var tempData = new Dictionary<string, FurnitureController>();
            foreach (var furData in furnitureData)
            {
                var furInstance = _furnitureController.SpawnFurnitureByData(furData);
                tempData.Add(furData.Id, furInstance);
            }
            foreach (var furData in furnitureData)
            {
                if (furData.ParentId == "") continue;

                var furChild = tempData[furData.Id];
                var furParent = tempData[furData.ParentId];
                RoomHelper.NestFurniture(furParent, furChild, false);
            }
        }

        public void LoadData(RoomData data)
        {
            SetFloor(data.FloorId);
            SetWall(data.WallId);
            SpawnFurniture(data.FurnitureData);
        }
    }
}