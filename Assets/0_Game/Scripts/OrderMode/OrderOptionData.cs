using NFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    [Serializable]
    public class OrderOptionData
    {
        public List<string> WallOptions;
        public List<string> FloorOptions;
        public List<FurnitureOptionData> FurnitureOptions = new List<FurnitureOptionData>();
        public List<RoomFurnitureData> DecorOptions = new List<RoomFurnitureData>();

        [HideInInspector] public int StartUnboxOptionIndex = 0;
        [HideInInspector] public List<FurnitureOptionData> RepaintingOptions = new List<FurnitureOptionData>();
        [HideInInspector] public List<FurnitureOptionData> UnboxOptions = new List<FurnitureOptionData>();

        public OrderOptionData(List<string> wallOptions, List<string> floorOptions, List<FurnitureOptionData> furnitureOptions, List<RoomFurnitureData> decorOptions, int startUnboxOptionIndex)
        {
            WallOptions = wallOptions;
            FloorOptions = floorOptions;
            FurnitureOptions = furnitureOptions;
            DecorOptions = decorOptions;
            StartUnboxOptionIndex = startUnboxOptionIndex;

            for (var i = 0; i < StartUnboxOptionIndex; i++)
            {
                RepaintingOptions.Add(FurnitureOptions[i]);
            }

            for (var i = StartUnboxOptionIndex; i < furnitureOptions.Count; i++)
            {
                UnboxOptions.Add(furnitureOptions[i]);
            }
        }

        public int OptionCount() => FurnitureOptions.Count + 2; // 2 are wall and floor options

        public FurnitureOptionData GetRepaintingOptionAtIndex(int index)
        {
            if (RepaintingOptions.IsIndexOutOfList(index))
                return null;

            return RepaintingOptions[index];
        }

        public FurnitureOptionData GetUnboxOptionAtIndex(int index)
        {
            if (UnboxOptions.IsIndexOutOfList(index))
                return null;

            return UnboxOptions[index];
        }
    }

    [Serializable]
    public class FurnitureOptionData
    {
        public List<RoomFurnitureData> FurnitureOption = new List<RoomFurnitureData>();

        public FurnitureOptionData(List<RoomFurnitureData> furnitureOption)
        {
            FurnitureOption = furnitureOption;
        }
    }
}
