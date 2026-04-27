using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_HouseVisitModeToolController : MonoBehaviour
    {
        public GD_ToolWorld World;

        [Title("House Visit Mode Config")]
        public string HouseId;
        public string BotId;
        public string Like;
        public string DefaultFloorId;
        public string DefaultWallId;

        [TextArea(5, 25), ReadOnly]
        public string DefaultRoomFurniture;
        [TextArea(5, 25)]
        public string HouseVisitRoomConfig;

        [Button]
        public void GenDefaultRoomFurniture()
        {
            var tempStringList = new List<string>();

            foreach (var fur in World.FurnitureList)
            {
                var furString = "";

                var id = fur.Config.Id;
                var pos = fur.Position;
                var direction = fur.FlipController.CurDirection;

                furString += $"{id}|{pos.x},{pos.y},{pos.z}|{direction}|";
                if (fur.NestedController.HasParent())
                {
                    var parent = fur.NestedController.Parent;
                    var indexInFurList = World.FurnitureList.IndexOf(parent);
                    furString += indexInFurList;
                }
                else
                {
                    furString += "-1";
                }
                tempStringList.Add(furString);
            }

            DefaultRoomFurniture = "";
            for (var i = 0; i < tempStringList.Count; i++)
            {
                var str = tempStringList[i];
                var isLastIndex = i == tempStringList.Count - 1;
                DefaultRoomFurniture += str + $"{(isLastIndex ? "" : "\n")}";
            }
        }

        [Button]
        public void GetFullConfigHouseVisit()
        {
            HouseVisitRoomConfig =
                $"{HouseId}\t" +
                $"{BotId}\t" +
                $"{Like}\t" +
                $"{DefaultFloorId}\t" +
                $"{DefaultWallId}\t" +
                $"\"{DefaultRoomFurniture}\"";
        }
    }
}
