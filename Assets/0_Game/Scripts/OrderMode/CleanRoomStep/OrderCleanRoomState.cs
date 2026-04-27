using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

namespace YoyoDesign
{
    public class OrderCleanRoomState : OrderModeState
    {
        private int _orderTrashCount = 6;
        private int _trashType = 2;

        public override EOrderState StateName => EOrderState.CleanTrash;

        public static event Action<int, int> OnItemCleaned;

        [Header("CAMERA DRAG CONFIG")]
        [SerializeField] private float _dragCameraSpeed = 8f;
        [SerializeField] private float _holdingFurDragCamSpeed = 0.2f;
        [SerializeField] private Vector2 _minBounds = new(-5, 4);
        [SerializeField] private Vector2 _maxBounds = new(5, 8);
        [SerializeField] private float _edgeOffset = 150f;
        private Vector3 _dragOrigin;

        private List<ETrashType> _listTrashType = new();

        // TrashBoxUI 
        private OrderProgressTrashBoxUI _trashBoxUI;

        // Trash
        private List<FurnitureController> _trashList = new();
        private FurnitureController _curTrash;
        private int _trashClearCount = 0;

        // Etc
        private Tween _delayShowHighlightTrash;
        private bool _isUpdate;

        // PROPERTIES
        public List<FurnitureController> AllFurList => _trashList.Concat(_orderRoomController.FurnitureList).ToList();
        public int TotalMessyCount => _orderTrashCount + _orderRoomController.FurnitureList.Count;
        public int TrashClearCount => _trashClearCount;


        public FurnitureController CurTrash
        {
            get => _curTrash;
            set
            {
                if (value == null)
                {
                    _curTrash = value;
                    foreach (var trash in _trashList)
                    {
                        trash.VisualController?.Normalize();
                        trash.SpriteRenderer.sortingLayerName = Define.SortingLayerName.Default;
                        trash.SpriteRenderer.sortingOrder += 20;
                    }
                }
                else
                {
                    AudioManager.PlaySound(ESound.PickUp);
                    _curTrash = value;
                    RoomHelper.RemoveNested(_curTrash.NestedController.Parent, _curTrash);
                    _curTrash.VisualController.Highlight(false);
                    _curTrash.SpriteRenderer.sortingLayerName = Define.SortingLayerName.AlwaysOnTop;
                    _curTrash.SpriteRenderer.sortingOrder += 20;
                }
            }
        }

        #region OrderModeState

        public override void Enter()
        {
            _isUpdate = true;
            CameraController.Instance.SetPosition(0, 6);
            CameraController.Instance.SetSize(Define.Size.ORDER_REMOVE_TRASH_SIZE);

            var orderId = OrderModeManager.Instance.GetCurBotOrderData().OrderId;

            // Handle spawn trash
            _trashType = orderId == Define.DefaultId.TUTORIAL_ORDER_ID ? Define.ConstValue.ORDER_TRASH_TYPE_TUTORIAL : Define.ConstValue.ORDER_TRASH_TYPE;
            _orderTrashCount = orderId == Define.DefaultId.TUTORIAL_ORDER_ID ? Define.ConstValue.ORDER_TRASH_AMOUNT_TUTORIAL : Define.ConstValue.ORDER_TRASH_AMOUNT;
            _listTrashType = GetRandomTrashType(_trashType);
            SpawnTrashes(_listTrashType, _orderTrashCount);

            if (UIManager.Instance.IsSpecificViewShown(Define.UIName.ORDER_PROGRESS_POPUP, out var view))
            {
                var progressPopupUI = view as OrderProgressPopupUI;
                _trashBoxUI = progressPopupUI.TrashBoxUI;
            }
            OnItemCleaned?.Invoke(0, TotalMessyCount);

            this.InvokeDelayFrame(1, () =>
            {
                var needShowTutorial = UserData.Instance.CurTutorialIndex >= Define.TutorialIndex.NEW_PLAYER;
                ShowTutorial(needShowTutorial);
            });
        }

        public override void OnUpdate()
        {
            if (!_isUpdate) return;

            if (UIManager.Instance.IsPointerOverUIObject())
            {
                if (CurTrash != null)
                {
                    PlaceToValidPosition(CurTrash);
                    CurTrash = null;
                }
                return;
            }

            if (Input.touchCount <= 0) return;

            var inputPosition = _isoWorld.TouchIsoPosition(0);
            var touch = Input.GetTouch(0);
            var mainCamera = CameraController.Instance.Camera;


            // Begin select handle
            if (touch.phase == TouchPhase.Began)
            {
                _dragOrigin = mainCamera.ScreenToViewportPoint(touch.position);
                CurTrash = RoomHelper.GetFurnitureOnTouch(inputPosition, _orderRoomController.RoomBounds, _trashList, _curTrash);
            }

            if (CurTrash != null)
            {
                // Move camera
                var touchPosition = touch.position;
                if (touchPosition.x <= _edgeOffset || touchPosition.x >= Screen.width - _edgeOffset ||
                    touchPosition.y <= _edgeOffset || touchPosition.y >= Screen.height - _edgeOffset)
                {
                    Vector3 move = new Vector3();

                    if (touchPosition.x <= _edgeOffset)
                        move.x = -_holdingFurDragCamSpeed;
                    else if (touchPosition.x >= Screen.width - _edgeOffset)
                        move.x = _holdingFurDragCamSpeed;

                    if (touchPosition.y <= _edgeOffset)
                        move.y = -_holdingFurDragCamSpeed;
                    else if (touchPosition.y >= Screen.height - _edgeOffset)
                        move.y = _holdingFurDragCamSpeed;

                    var camera = Camera.main;
                    var cameraTransform = camera.transform;
                    cameraTransform.Translate(move, Space.World);
                    Vector3 pos = cameraTransform.position;
                    pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
                    pos.y = Mathf.Clamp(pos.y, _minBounds.y, _maxBounds.y);
                    cameraTransform.position = pos;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    OnMoveFurniture(touch.position, inputPosition, CurTrash);
                    ShowTutorial(false);
                }
                if (touch.phase == TouchPhase.Ended)
                {
                    OnDropFurniture(CurTrash);
                    ShowTutorial(_trashClearCount == 0);
                }
            }
            else
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    var touchPosition = mainCamera.ScreenToViewportPoint(touch.position);
                    var move = (_dragOrigin - touchPosition) * _dragCameraSpeed;

                    Vector3 targetPosition = mainCamera.transform.position + move;
                    targetPosition.x = Mathf.Clamp(targetPosition.x, _minBounds.x, _maxBounds.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, _minBounds.y, _maxBounds.y);

                    mainCamera.transform.position = targetPosition;
                    _dragOrigin = mainCamera.ScreenToViewportPoint(touch.position);
                    ShowTutorial(_trashClearCount == 0);
                }
            }
        }

        #endregion // OrderModeState

        private void OnMoveFurniture(Vector3 touchPosition, Vector3 inputPosition, FurnitureController trashMove)
        {
            var movePos = LandHelper.GetMovePosition(inputPosition, trashMove, Vector3.positiveInfinity, Vector3.negativeInfinity, moveOffset: 0);
            var isPositionValid =
                RoomHelper.IsPositionValid(movePos, trashMove.Size, trashMove, AllFurList, _orderRoomController.RoomBounds);
            trashMove.MoveController.SetPosition(movePos);
            trashMove.VisualController.IsValid = isPositionValid;

            // Handle trash box 
            var isWithinBounds = _trashBoxUI.IsWithinBounds(touchPosition);
            if (isWithinBounds)
            {
                _trashBoxUI.Highlight();
            }
            else
            {
                _trashBoxUI.Normalize();
            }
        }

        private void OnDropFurniture(FurnitureController trashDrop)
        {
            AudioManager.PlaySound(ESound.Drop);
            CurTrash = null;

            if (_trashBoxUI.IsHighlight)
            {
                RemoveTrash(trashDrop);
                _trashBoxUI.Normalize();
                return;
            }


            // Is current trash are on other parent.
            if (trashDrop.Config.CanPlaceOnOthers)
            {
                var parentFur = LandHelper.GetFurnitureOnSurface(trashDrop.Position, trashDrop.Size, trashDrop, _orderRoomController.FurnitureList);
                if (parentFur != null)
                {
                    RoomHelper.NestFurniture(parentFur, trashDrop, true);
                    return;
                }
            }

            PlaceToValidPosition(trashDrop);
        }

        private void PlaceToValidPosition(FurnitureController curFur)
        {
            var validPos = RoomHelper.GetValidPosition(curFur.Position, curFur.Size, curFur.CurDirection, curFur,
                _orderRoomController.RoomBounds, AllFurList);

            if (validPos != Vector3.one * 100)
            {
                curFur.MoveController.SetPosition(validPos);
                curFur.VisualController.IsValid = true;
            }
        }

        public void RemoveTrash(FurnitureController trashDrop)
        {
            // Handle trash drop
            _trashList.Remove(trashDrop);
            trashDrop.SpriteRenderer.sortingLayerName = Define.SortingLayerName.AlwaysOnTop;
            RoomHelper.RemoveNested(trashDrop.NestedController.Parent, trashDrop);
            trashDrop.VisualController.Highlight(false);
            trashDrop.VisualController.SetScale(0, true, 0.5f, () =>
            {
                FurnitureManager.Instance.Release(trashDrop);
            });
            _trashClearCount++;
            OnItemCleaned?.Invoke(_trashClearCount, TotalMessyCount);

            // Check complete
            if (IsCleanAllTrash())
            {
                // Next step
                _orderRoomController.ChangeState(EOrderState.RemoveOldFurniture);
            }
        }

        public bool IsCleanAllTrash() => _trashClearCount == _orderTrashCount;

        public async void CleanAllRemainingTrashes()
        {
            _isUpdate = false;
            var animTask = new List<UniTask>();

            ShowTutorial(false);

            foreach (var trash in _trashList)
            {
                var task = FlyToTrashBox(trash);
                animTask.Add(task);
            }

            await UniTask.WhenAll(animTask);

            _trashList.Clear();
            OnItemCleaned?.Invoke(TotalMessyCount, TotalMessyCount);
            _orderRoomController.ChangeState(EOrderState.RemoveOldFurniture);

            async UniTask FlyToTrashBox(FurnitureController trash)
            {
                var trashPosition = trash.Position;

                trash.VisualController.SetScale(0, true, 1f);

                await DOTween.To(() => trashPosition, x =>
                {
                    trashPosition = x;
                    x.z = 0;
                    trash.MoveController.SetPosition(_isoWorld.ScreenToIso(x));
                }, _trashBoxUI.Position, 1f).SetEase(Ease.OutQuad);

                RoomHelper.RemoveNested(trash.NestedController.Parent, trash);
                FurnitureManager.Instance.Release(trash);
            }
        }

        private List<ETrashType> GetRandomTrashType(int typeCount)
        {
            var values = Enum.GetValues(typeof(ETrashType));
            var allValues = values.Cast<ETrashType>().ToList();
            allValues.Remove(ETrashType.None);

            // Shuffle the list
            for (var i = allValues.Count - 1; i > 0; i--)
            {
                var j = Random.Range(0, i + 1);
                (allValues[i], allValues[j]) = (allValues[j], allValues[i]);
            }

            // Take the first 'amount' elements
            return allValues.Take(typeCount).ToList();
        }

        private void SpawnTrashes(List<ETrashType> listTypes, int maxTrash)
        {
            var amountPerType = maxTrash / listTypes.Count;
            foreach (var type in listTypes)
            {
                for (var i = 0; i < amountPerType;)
                {
                    var trashConfigSO = FurnitureManager.Instance.GetRandomTrash(type);
                    var trashInstance = FurnitureManager.Instance.Get(trashConfigSO.Config.Id);
                    var randomPosition = new Vector3(Random.Range(0, _orderRoomController.RoomBounds.x),
                        Random.Range(0, _orderRoomController.RoomBounds.y));

                    trashInstance.gameObject.SetActive(true);
                    trashInstance.transform.SetParent(_orderRoomController.IsoWorld.transform);
                    trashInstance.MoveController.SetPosition(randomPosition);
                    trashInstance.VisualController.Normalize(false);
                    trashInstance.SpriteRenderer.sortingOrder += 20;

                    var parentFur = RoomHelper.GetFurnitureOnSurface(randomPosition, trashInstance.Size, trashInstance,
                        _orderRoomController.FurnitureList);
                    if (parentFur != null)
                    {
                        RoomHelper.NestFurniture(parentFur, trashInstance, true);
                        _trashList.Add(trashInstance);
                        i++;
                        continue;
                    }

                    if (RoomHelper.IsPositionValid(randomPosition, trashInstance.Size, trashInstance, AllFurList, _orderRoomController.RoomBounds))
                    {
                        _trashList.Add(trashInstance);
                        i++;
                    }
                    else
                    {
                        FurnitureManager.Instance.Release(trashInstance);
                    }
                }
            }
        }

        private void ShowTutorial(bool isShow)
        {
            if (UIManager.Instance.IsSpecificViewShown(Define.UIName.ORDER_PROGRESS_POPUP, out var view))
            {
                var progressPopup = view as OrderProgressPopupUI;
                if (!isShow)
                {
                    progressPopup.SetActiveHandTrashTutorial(false);
                }
                else
                {
                    var trash = _trashList.FirstOrDefault();
                    progressPopup.SetActiveHandTrashTutorial(true);
                    progressPopup.HandDragTutorial(_isoWorld.IsoToScreen(trash.Position));
                }
            }

        }
    }
}