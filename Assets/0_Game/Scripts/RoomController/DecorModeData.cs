using System;
using System.Collections.Generic;
using System.Linq;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class DecorModeData : SingletonMono<DecorModeData>, ISaveable
    {
        public void SaveDecor(RoomData decorModeData)
        {
            var roomData = GetRoomData(decorModeData.Index);
            if (roomData != null)
            {
                _saveData.RoomDatas.Remove(roomData);
            }
            _saveData.RoomDatas.Add(decorModeData);
            DataChanged = true;
        }

        public RoomData GetRoomData(int roomIndex)
        {
             var result = _saveData.RoomDatas.FirstOrDefault(r => r.Index == roomIndex);
             if (result != null) return result;
             
             result = new() { Index = roomIndex };
             _saveData.RoomDatas.Add(result);
             return result;
        }

        #region ISaveable

        [SerializeField] private DecorModeSaveData _saveData;

        public string SaveKey => "DecorData";

        public bool DataChanged { get; set; }

        public object GetData() => _saveData;

        public void SetData(string data)
        {
            if (string.IsNullOrEmpty(data))
            {
                _saveData = new();
                DataChanged = true;
            }
            else
            {
                _saveData = JsonUtility.FromJson<DecorModeSaveData>(data);
            }
        }

        public void OnAllDataLoaded()
        {
        }

        #endregion
    }

    [Serializable]
    public class DecorModeSaveData
    {
        public List<RoomData> RoomDatas = new();
    }

    [Serializable]
    public class RoomData
    {
        public int Index;
        public string FloorId = Define.DefaultId.FLOOR;
        public string WallId = Define.DefaultId.WALL;
        public List<RoomFurnitureData> FurnitureData = new();
    }

    [Serializable]
    public class RoomFurnitureData
    {
        public RoomFurnitureData(string configId, Vector3 position, FurnitureDirection direction, int parentIndexInFurList = -1)
        {
            Id = FriendlyGUID.NewId_FromRandomInt();
            ConfigId = configId;
            Position = position;
            Direction = direction;
            ParentIndexInFurnitureList = parentIndexInFurList;
        }

        public string Id;
        public string ConfigId;
        public Vector3 Position;
        public FurnitureDirection Direction;
        public string ParentId = "";
        public int ParentIndexInFurnitureList = -1; // -1 means no parent

        public bool NeedSetParent() => ParentIndexInFurnitureList != -1;
    }
}