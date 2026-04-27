using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderRepaintingRoomState : OrderModeState
    {
        public override EOrderState StateName => EOrderState.Repainting;
        public static event Action<int, int> OnRepaintedFurniture;

        private bool _confirmSetWall, _confirmSetFloor;
        private int _curIndexFurRepainting;
        private int _curTotalFurNeedRepainted, _curFurRepainted;
        private int _lastRoomFurCount;
        private FurnitureController _curFurRepainting;
        private Dictionary<int, FurnitureController> _repaintingFurSpawnDic = new Dictionary<int, FurnitureController>();
        private Dictionary<int, int> _confirmedFurOptionDic = new Dictionary<int, int>();

        public override void Enter()
        {
            var config = OrderModeManager.Instance.GetCurOrderRoomConfigData();
            _curFurRepainted = 0;
            _curTotalFurNeedRepainted = config.OrderOptions.OptionCount();
            OnRepaintedFurniture?.Invoke(_curFurRepainted, _curTotalFurNeedRepainted);
        }

        public void SelectFurniture(int index, string furId, Vector3 isoPosition, FurnitureDirection direction)
        {
            _lastRoomFurCount = _repaintingFurSpawnDic.Keys.Count;
            _curIndexFurRepainting = index;
            TryRemoveFurnitureAtIndex(_curIndexFurRepainting);

            var roomFurList = _orderRoomController.FurnitureList;
            var isoWorld = _orderRoomController.IsoWorld;

            _curFurRepainting = FurnitureManager.Instance.Get(furId);
            _curFurRepainting.transform.SetParent(isoWorld.transform);
            _curFurRepainting.gameObject.SetActive(true);
            _curFurRepainting.FlipController.FlipTo(direction);
            _curFurRepainting.MoveController.SetPosition(isoPosition);
            _curFurRepainting.MoveController.ToggleMoveable(false);
            roomFurList.Add(_curFurRepainting);
            AddFurnitureToDicAtIndex(_curIndexFurRepainting);
        }

        public void ConfirmSetWall()
        {
            if (!_confirmSetWall)
            {
                _curFurRepainted++;
                OnRepaintedFurniture?.Invoke(_curFurRepainted, _curTotalFurNeedRepainted);
            }
            _confirmSetWall = true;
            TryFurnitureRepainting();
        }

        public void ConfirmSetFloor()
        {
            if (!_confirmSetFloor)
            {
                _curFurRepainted++;
                OnRepaintedFurniture?.Invoke(_curFurRepainted, _curTotalFurNeedRepainted);
            }
            _confirmSetFloor = true;
            TryFurnitureRepainting();
        }

        public void ConfirmSetFurniture(int index, int optionIndex)
        {
            var dataCount = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.RepaintingOptions.Count;
            var adsItemIndex = OrderModeManager.Instance.GetAdsItemOptionAtIndex(index);
            var isAdsItem = adsItemIndex == optionIndex;
            if (isAdsItem)
            {
                OrderModeManager.Instance.AddToWatchedItemKey(index);
            }
            AddFurnitureOptionConfirmedToDicAtIndex(index, optionIndex);
            if (_lastRoomFurCount != _repaintingFurSpawnDic.Count)
            {
                OnRepaintedFurniture?.Invoke(_curFurRepainted, _curTotalFurNeedRepainted);
            }

            if (_repaintingFurSpawnDic.Keys.Count == dataCount && _orderRoomController.CurOrderState != EOrderState.UnboxFurniture)
            {
                _orderRoomController.ChangeState(EOrderState.UnboxFurniture);
            }
        }

        public void TryFurnitureRepainting()
        {
            if (_confirmSetWall && _confirmSetFloor)
            {
                _orderRoomController.RoomWorldUI.ToggleGroup2Repainting(true);
            }
        }

        public int GetOptionConfirmedAtIndex(int index)
        {
            if (_confirmedFurOptionDic.TryGetValue(index, out var confirmedOption))
            {
                return confirmedOption;
            }

            return -1;
        }

        private void TryRemoveFurnitureAtIndex(int index)
        {
            if (_repaintingFurSpawnDic.TryGetValue(index, out var fur))
            {
                _orderRoomController.ReleaseFurniture(fur);
            }
        }

        private void AddFurnitureToDicAtIndex(int index)
        {
            if (_repaintingFurSpawnDic.ContainsKey(index))
            {
                _repaintingFurSpawnDic[index] = _curFurRepainting;
            }
            else
            {
                _repaintingFurSpawnDic.Add(index, _curFurRepainting);
                _curFurRepainted++;
            }
        }

        private void AddFurnitureOptionConfirmedToDicAtIndex(int index, int optionConfirmed)
        {
            if (_confirmedFurOptionDic.ContainsKey(index))
            {
                _confirmedFurOptionDic[index] = optionConfirmed;
            }
            else
            {
                _confirmedFurOptionDic.Add(index, optionConfirmed);
            }
        }
    }
}
