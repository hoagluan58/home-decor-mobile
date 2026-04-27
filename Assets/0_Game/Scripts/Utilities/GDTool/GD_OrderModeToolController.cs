using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class GD_OrderModeToolController : MonoBehaviour
    {
        public GD_ToolWorld World;

        [Title("Order Mode Config")]
        public string RoomId;
        public EBotRoomType RoomType;
        public string DefaultFloorId;
        public string DefaultWallId;
        public string DefaultRoomSprite;
        public string WallDataOption;
        public string FloorDataOption;
        public string UnboxStartIndex;

        [TextArea(5, 25), ReadOnly]
        public string DefaultRoomFurniture;
        [TextArea(2, 10), ReadOnly]
        public string FurnitureDataOption;
        [TextArea(2, 10), ReadOnly]
        public string DecorDataOption;
        [TextArea(2, 10), ReadOnly]
        public string DecorTierDic;
        [TextArea(5, 25)]
        public string OrderModeRoomConfig;

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
        public void GenFullConfigOrderRoom()
        {
            OrderModeRoomConfig =
                $"{RoomId}\t" +
                $"{RoomType}\t" +
                $"{DefaultFloorId}\t" +
                $"{DefaultWallId}\t" +
                $"\"{DefaultRoomFurniture}\"\t" +
                $"{DefaultRoomSprite}\t" +
                $"\"{WallDataOption}\"\t" +
                $"\"{FloorDataOption}\"\t" +
                $"\"{FurnitureDataOption}\"\t" +
                $"\"{DecorDataOption}\"\t" +
                $"\"{DecorTierDic}\"\t" +
                $"\"{UnboxStartIndex}\"";
        }
    }
}
