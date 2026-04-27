using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_OrderModeDecorTierController : MonoBehaviour
    {
        [SerializeField] private GD_OrderModeToolController _orderModeToolController;

        [SerializeField] private SerializableDictionaryBase<FurnitureController, EOrderDecorTier> _furnitureTiers;

        private GD_ToolWorld _world => _orderModeToolController.World;


        [Button]
        public void SetData()
        {
            _furnitureTiers = new();
            foreach (var fur in _world.FurnitureList)
            {
                _furnitureTiers.Add(fur, EOrderDecorTier.Tier3);
            }
        }

        [Button]
        public void AddToDecorTierDicData()
        {
            var result = "";
            var tempDic = new Dictionary<EOrderDecorTier, List<string>>();
            var index = 0;

            foreach (var kvp in _furnitureTiers)
            {
                var tier = kvp.Value;
                if (!tempDic.ContainsKey(tier))
                {
                    tempDic.Add(tier, new List<string>());
                }

                tempDic[tier].Add(index.ToString());
                index++;
            }

            foreach (var kvp in tempDic)
            {
                var data = $"{kvp.Key}-";

                for (var i = 0; i < kvp.Value.Count; i++)
                {
                    var isLastIndex = i == kvp.Value.Count - 1;
                    data += isLastIndex ? kvp.Value[i] + "\n" : kvp.Value[i] + ";";
                }

                result += data;
            }

            result = result.TrimEnd();
            _orderModeToolController.DecorTierDic = result;
        }
    }
}
