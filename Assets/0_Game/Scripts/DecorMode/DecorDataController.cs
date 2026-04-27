using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    /// <summary>
    ///  Read - write room decor data.
    /// </summary>
    public class DecorDataController : MonoBehaviour
    {
        [SerializeField] private DecorRoomController _controller;

        private RoomData _tempData;
        public RoomData TempData => _tempData;

        public void SaveTemp()
        {
            _tempData = GetCurrentRoomData();
        }
        
        public void Save()
        {
            DecorModeData.Instance.SaveDecor(_tempData);
        }
        
        public RoomData GetCurrentRoomData()
        {
            var tempData = new Dictionary<FurnitureController, string>();
            var result = new RoomData
            {
                Index =  _controller.RoomIndex,
                FloorId = _controller.FloorController.CurFloorId,
                WallId = _controller.WallController.CurWallId
            };

            foreach (var fur in _controller.FurnitureController.FurList)
            {
                var newFurData = new RoomFurnitureData(fur.Config.Id, fur.Position, fur.CurDirection);
                result.FurnitureData.Add(newFurData);
                tempData.Add(fur, newFurData.Id);
            }

            foreach (var fur in _controller.FurnitureController.FurList)
            {
                if (fur.NestedController.Parent != null)
                {
                    var furData = result.FurnitureData.FirstOrDefault(f => f.Id == tempData[fur]);
                    furData.ParentId = tempData[fur.NestedController.Parent];
                }
            }

            return result;
        }
    }
    
}