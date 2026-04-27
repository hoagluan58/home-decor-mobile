using DG.Tweening;
using JSAM;
using NFramework;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderRemoveOldFurState : OrderModeState
    {
        public override EOrderState StateName => EOrderState.RemoveOldFurniture;

        public static event Action<int, int> OnItemCleaned;
        public static event Action OnDoneCleanRoom;

        [SerializeField] private PooledObject _goFXSmoke;

        private int _furClearCount;
        private int _totalMessyCount;
        private List<FurnitureController> _roomOldFurList;
        private int _allMessCleanCount => _maxTrashCount + _furClearCount;

        private int _maxTrashCount;
        private bool IsRoomEmpty => _orderRoomController.FurnitureList.Count == 0;

        public override void Enter()
        {
            var apdaptSize = CameraController.Instance.GetAdaptSize(Define.Size.ORDER_CAMERA_SIZE);
            CameraController.Instance.MoveAndZoomAnimation(new Vector3(0, 6, 0), apdaptSize, 0.5f);
            var worldUI = _orderRoomController.RoomWorldUI;

            _roomOldFurList = new List<FurnitureController>();
            foreach (var fur in _orderRoomController.FurnitureList)
            {
                if (fur.NestedController.HasParent())
                    continue;

                _roomOldFurList.Add(fur);
            }

            foreach (var fur in _roomOldFurList)
            {
                worldUI.SpawnOldRemoveFurButton(fur.Position, fur.Size, () => RemoveOldFur(fur));
            }

            var orderId = OrderModeManager.Instance.GetCurBotOrderData().OrderId;

            _maxTrashCount = orderId == Define.DefaultId.TUTORIAL_ORDER_ID ? Define.ConstValue.ORDER_TRASH_AMOUNT_TUTORIAL
                                                                           : Define.ConstValue.ORDER_TRASH_AMOUNT;
            _totalMessyCount = _maxTrashCount + _roomOldFurList.Count;
            OnItemCleaned?.Invoke(_allMessCleanCount, _totalMessyCount);
        }

        private void RemoveOldFur(FurnitureController fur)
        {
            var furDirection = fur.CurDirection;
            var fromPos = fur.CurDirection == FurnitureDirection.Left ? fur.Position.x : fur.Position.y;

            var fx = PoolManager.Instance.GetPool(_goFXSmoke).GetPooledObject();
            fx.transform.position = _isoWorld.IsoToScreen(fur.Position + fur.Size / 2);
            AudioManager.PlaySound(ESound.Remove);
            DOVirtual.Float(fromPos, fromPos - 20f, 0.3f, x =>
                {
                    var position = fur.Position;
                    position = furDirection == FurnitureDirection.Left
                        ? new Vector3(x, position.y, position.z)
                        : new Vector3(position.x, x, position.z);
                    fur.MoveController.SetPosition(position);
                })
                .SetEase(Ease.InBack)
                .OnComplete(() =>
                {
                    _furClearCount++;
                    _orderRoomController.ReleaseFurniture(fur);
                    OnItemCleaned?.Invoke(_allMessCleanCount, _totalMessyCount);

                    if (IsRoomEmpty)
                    {
                        OnDoneCleanRoom?.Invoke();
                    }
                });
        }
    }
}