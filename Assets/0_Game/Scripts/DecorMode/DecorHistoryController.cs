using System;
using System.Collections.Generic;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class DecorHistoryController : MonoBehaviour
    {
        public static event Action HistoryChangeEvent;

        [SerializeField] private DecorRoomController _decorController;

        private List<RoomData> _historyDatas = new();

        public bool CanUndo => _historyDatas.Count > 1;

        public void InitHistory()
        {
            _historyDatas.Clear();
            var roomData = _decorController.DataController.GetCurrentRoomData();
            _historyDatas.Add(roomData);
            HistoryChangeEvent?.Invoke();
        }

        public void Undo()
        {
            if (!CanUndo) return;

            // Remove last
            _historyDatas.RemoveAt(_historyDatas.Count - 1);

            var previousRoomData = _historyDatas.Last();
            _decorController.FurnitureController.ReleaseAll();
            _decorController.LoadData(previousRoomData);
            HistoryChangeEvent?.Invoke();
        }

        public void OnModify()
        {
            var roomData = _decorController.DataController.GetCurrentRoomData();
            _historyDatas.Add(roomData);
            
            // When history 
            if (_historyDatas.Count >= Define.ConstValue.HISTORY_LIST_LENGTH)
            {
                _historyDatas.RemoveAt(0);
            }
            
            HistoryChangeEvent?.Invoke();
        }

        public void ClearHistory()
        {
            _historyDatas.Clear();
            HistoryChangeEvent?.Invoke();
        }
    }
}