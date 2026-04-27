using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_OrderModeOptionController : MonoBehaviour
    {
        [SerializeField] private GD_OrderModeToolController _orderModeToolController;

        private GD_ToolWorld _world => _orderModeToolController.World;

        [TextArea(2, 10), ReadOnly]
        public string TempFurnitureDataOption;

        [TextArea(2, 10), ReadOnly]
        public string TempDecorDataOption;

        [Button]
        public void AddToTempFurnitureData()
        {
            var result = "";
            if (!string.IsNullOrEmpty(TempFurnitureDataOption))
            {
                TempFurnitureDataOption += "\n";
            }

            for (int i = 0; i < _world.FurnitureList.Count; i++)
            {
                var fur = _world.FurnitureList[i];

                var id = fur.Config.Id;
                var pos = fur.Position;
                var direction = fur.FlipController.CurDirection;

                result = $"{id}|{pos.x},{pos.y},{pos.z}|{direction}";

                var isLastIndex = i == _world.FurnitureList.Count - 1;
                result += $"{(isLastIndex ? "" : ";")}";
                TempFurnitureDataOption += result;
            }
        }

        [Button]
        public void AddToTempDecorData()
        {
            TempDecorDataOption = "";

            for (var i = 0; i < _world.FurnitureList.Count; i++)
            {
                var result = "";
                var fur = _world.FurnitureList[i];
                var id = fur.Config.Id;
                var direction = fur.FlipController.CurDirection;

                result = $"{id}||{direction}";
                var isLastIndex = i == _world.FurnitureList.Count - 1;
                result += $"{(isLastIndex ? "" : "\n")}";
                TempDecorDataOption += result;
            }
        }

        [Button]
        public void AddToFunitureDataOption() => _orderModeToolController.FurnitureDataOption = TempFurnitureDataOption;

        [Button]
        public void AddToDecorDataOption() => _orderModeToolController.DecorDataOption = TempDecorDataOption;

        [Button]
        public void ClearTempFurnitureData() => TempFurnitureDataOption = string.Empty;
    }
}
