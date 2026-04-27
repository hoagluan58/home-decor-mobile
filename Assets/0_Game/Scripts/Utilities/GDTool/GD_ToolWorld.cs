using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_ToolWorld : MonoBehaviour
    {
        public FloorConfigSO FloorConfigSO;
        public WallConfigSO WallConfigSO;
        public WallController WallController;
        public FloorController FloorController;

        [ReadOnly] public List<FurnitureController> FurnitureList;

        [Title("CONFIG")]
        public string WallId;
        public string FloorId;

        [Button]
        public void SetDataWorld()
        {
            WallConfigSO.Init();
            FloorConfigSO.Init();

            if (!string.IsNullOrEmpty(WallId))
            {
                if (WallConfigSO.WallConfigDic.TryGetValue(WallId, out var wallConfig))
                {
                    WallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
                }
            }

            if (!string.IsNullOrEmpty(FloorId))
            {
                if (FloorConfigSO.FloorConfigDic.TryGetValue(FloorId, out var floorConfig))
                {
                    FloorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
                }
            }

            FurnitureList = new List<FurnitureController>();

            var allFurInScene = FindObjectsOfType<FurnitureController>();

            for (var i = allFurInScene.Length - 1; i >= 0; i--)
            {
                var fur = allFurInScene[i];
                if (fur.gameObject.activeSelf)
                {
                    FurnitureList.Add(fur);
                }
            }

            foreach (var fur in FurnitureList)
            {
                if (fur.Config.CanPlaceOnOthers)
                {
                    var parentFur = RoomHelper.GetFurnitureOnSurface(fur.Position, fur.Size, fur, FurnitureList);
                    if (parentFur != null)
                    {
                        RoomHelper.NestFurniture(parentFur, fur, true);
                    }
                }
            }
        }

        [Button]
        public void RemoveAllFurniture()
        {
            if (FurnitureList != null || FurnitureList.Count > 0)
            {
                foreach (var fur in FurnitureList)
                {
                    DestroyImmediate(fur.gameObject);
                }
                FurnitureList.Clear();
            }
        }
    }
}