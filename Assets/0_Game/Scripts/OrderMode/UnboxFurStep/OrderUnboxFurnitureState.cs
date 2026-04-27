using Cysharp.Threading.Tasks;
using DG.Tweening;
using IsoTools;
using IsoTools.Physics;
using JSAM;
using NFramework;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

namespace YoyoDesign
{
    public class OrderUnboxFurnitureState : OrderModeState
    {
        public static event Action OnDoneUnboxFurniture;
        public static event Action<int, int> OnUnboxedFurniture;

        [SerializeField] private OrderModeCartonBox _pfCartonBox;
        [SerializeField] private ParticleSystem _removeBoxFx;

        [Header("CONFIG")]
        [SerializeField] private float _offSetZoom;

        private Camera _camera;
        private float _baseOrthographicSize;
        private OrderModeRoomConfigData _roomConfig;
        private IsoRaycastHit[] _raycastBuffer = new IsoRaycastHit[1];
        private OrderModeCartonBox _curCartonBox;
        private FurnitureController _curFurUnboxed;
        private List<Vector3> _canSpawnBoxList = new List<Vector3>();
        private int _boxOpenedCount, _furSpawnedCount, _curIndexFurUnbox;
        private bool _donePlaceFurnitureStep;
        private Dictionary<int, FurnitureController> _unboxFurSpawnDic = new Dictionary<int, FurnitureController>();
        private Dictionary<int, int> _confirmedFurOptionDic = new Dictionary<int, int>();
        private bool _doneUnboxAllFur;

        private ObjectPool<OrderModeCartonBox> _poolCartonBox;

        public bool DoneUnboxAllFur => _doneUnboxAllFur;
        public override EOrderState StateName => EOrderState.UnboxFurniture;

        public override void Enter()
        {
            _roomConfig = OrderModeManager.Instance.GetCurOrderRoomConfigData();
            _orderRoomController.RoomWorldUI.ToggleRepaintingGroup(false);
            _poolCartonBox = new(
                () => Instantiate(_pfCartonBox, _isoWorld.transform),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _boxOpenedCount = 0;
            _baseOrthographicSize = CameraController.Instance.Camera.orthographicSize;
            _camera = CameraController.Instance.Camera;
            StartUnboxFurniture();
        }

        public override void OnUpdate()
        {
            if (UIManager.Instance.IsPointerOverUIObject())
                return;

            if (Input.GetMouseButtonDown(0))
            {
                var isoMousePos = _isoWorld.MouseIsoPosition();
                var ray = _isoWorld.RayFromIsoCameraToIsoPoint(isoMousePos);
                var hitCount = IsoPhysics.RaycastNonAlloc(ray, _raycastBuffer);

                for (var i = 0; i < hitCount; i++)
                {
                    _curCartonBox = _raycastBuffer[i].collider.GetComponent<OrderModeCartonBox>();

                    if (_curCartonBox == null)
                        return;

                    var canOpenBox = _curCartonBox.CanOpenBox;
                    if (canOpenBox)
                    {
                        _curCartonBox.OpenBoxStep();
                    }

                    if (_curCartonBox.IsBoxOpened)
                    {
                        SpawnBoxFurniture();
                        RemoveBox();
                    }
                }
            }
        }

        public override void Exit()
        {
            _orderRoomController.RoomWorldUI.ToggleRepaintingGroup(false);
            _orderRoomController.RoomWorldUI.ToggleGroup2Repainting(false);
            _orderRoomController.RoomWorldUI.ToggleUnboxFurGroup(false);
        }

        public async void StartUnboxFurniture()
        {
            _doneUnboxAllFur = false;
            while (_boxOpenedCount < _roomConfig.OrderOptions.UnboxOptions.Count)
            {
                var needPlayTutorial = _boxOpenedCount == 0;
                _donePlaceFurnitureStep = false;
                CalculatePosCanSpawnBox();
                var cartonBox = _poolCartonBox.Get();
                cartonBox.DropBox(_canSpawnBoxList.RandomItem(), needPlayTutorial);

                await UniTask.WaitForSeconds(cartonBox.DropBoxAnimTime, cancellationToken: destroyCancellationToken);

                var cameraTarget = new Vector3(cartonBox.transform.position.x, cartonBox.transform.position.y, _camera.transform.position.z);
                var zoomSize = _camera.orthographicSize - _offSetZoom;
                CameraController.Instance.MoveAndZoomAnimation(cameraTarget, zoomSize, 0.5f);

                await UniTask.WaitUntil(() => _donePlaceFurnitureStep == true);
            }
        }

        private void RemoveBox()
        {
            _removeBoxFx.transform.position = _curCartonBox.transform.position;
            _removeBoxFx.Play();
            Destroy(_curCartonBox.gameObject);
            _boxOpenedCount++;
            _curCartonBox = null;
        }

        public void SpawnBoxFurniture()
        {
            var index = _boxOpenedCount;
            var optionData = _roomConfig.OrderOptions.GetUnboxOptionAtIndex(index).FurnitureOption;
            var popup = UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP);
            var startIndex = _roomConfig.UnboxStartIndex;
            var adsItemIndex = OrderModeManager.Instance.GetAdsItemOptionAtIndex(index + startIndex);

            popup.SetFurnitureOptions(adsItemIndex, optionData,
            onSelectFurniture: (optionSelected) =>
            {
                var furData = optionData[optionSelected];
                AudioManager.PlaySound(ESound.Select);
                popup.NormalizeAllItems();
                OrderRoomController.Instance.UnboxFurnitureState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
            },
            onConfirmFurniture: (optionConfirmed) =>
            {
                var furData = optionData[optionConfirmed];
                AudioManager.PlaySound(ESound.Select);
                _donePlaceFurnitureStep = true;
                OrderRoomController.Instance.UnboxFurnitureState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
                OrderRoomController.Instance.UnboxFurnitureState.ConfirmSetFurniture(index, optionConfirmed);
                popup.CloseWithAnimation();
            });
            popup.ActiveFirstItemOrderOption(out var indexFur);

            var firstFurniture = optionData[indexFur];
            var furConfig = FurnitureManager.Instance.GetFurnitureConfig(firstFurniture.ConfigId);

            SelectFurniture(index, firstFurniture.ConfigId, firstFurniture.Position, firstFurniture.Direction);
            FurnitureAnimation();

            var cameraTarget = new Vector3(0, 6, _camera.transform.position.z);
            var zoomSize = _camera.orthographicSize + _offSetZoom;
            CameraController.Instance.MoveAndZoomAnimation(cameraTarget, zoomSize, 0.5f);

            void FurnitureAnimation()
            {
                var position = _curCartonBox.Position;
                DOTween.To(() => position, x =>
                {
                    position = x;
                    _curFurUnboxed.MoveController.SetPosition(position);
                }, firstFurniture.Position, 0.3f).SetEase(Ease.Flash);
            }
        }

        public void SelectFurniture(int index, string furId, Vector3 isoPosition, FurnitureDirection direction)
        {
            _furSpawnedCount = _unboxFurSpawnDic.Keys.Count;
            _curIndexFurUnbox = index;

            var roomFurList = _orderRoomController.FurnitureList;
            var isoWorld = _orderRoomController.IsoWorld;

            TryRemoveFurnitureAtIndex(_curIndexFurUnbox);
            _curFurUnboxed = FurnitureManager.Instance.Get(furId);
            _curFurUnboxed.transform.SetParent(isoWorld.transform);
            _curFurUnboxed.gameObject.SetActive(true);
            _curFurUnboxed.FlipController.FlipTo(direction);
            _curFurUnboxed.MoveController.SetPosition(isoPosition);
            _curFurUnboxed.MoveController.ToggleMoveable(false);
            roomFurList.Add(_curFurUnboxed);
            AddFurnitureToDicAtIndex(_curIndexFurUnbox);
        }

        public int GetOptionConfirmedAtIndex(int index)
        {
            if (_confirmedFurOptionDic.TryGetValue(index, out var confirmedOption))
            {
                return confirmedOption;
            }

            return -1;
        }

        public void ConfirmSetFurniture(int index, int optionConfirmed)
        {
            _curFurUnboxed = null;
            var orderOption = _roomConfig.OrderOptions;
            var startIndex = _roomConfig.UnboxStartIndex;
            var adsItemIndex = OrderModeManager.Instance.GetAdsItemOptionAtIndex(index + startIndex);
            var isAdsItem = optionConfirmed == adsItemIndex;

            if (isAdsItem)
            {
                OrderModeManager.Instance.AddToWatchedItemKey(startIndex + index);
            }
            AddFurnitureOptionConfirmedToDicAtIndex(index, optionConfirmed);

            if (_doneUnboxAllFur)
                return;

            if (_furSpawnedCount != orderOption.UnboxOptions.Count)
            {
                // 2 is wall and floor option count
                OnUnboxedFurniture?.Invoke(_furSpawnedCount + startIndex + 2, _roomConfig.OrderOptions.OptionCount());
            }

            if (_furSpawnedCount == orderOption.UnboxOptions.Count)
            {
                _doneUnboxAllFur = true;
                _orderRoomController.RoomWorldUI.SetDataUnboxFurniture();
                OnUnboxedFurniture?.Invoke(_roomConfig.OrderOptions.OptionCount(), _roomConfig.OrderOptions.OptionCount());
                OnDoneUnboxFurniture?.Invoke();
            }
        }

        private void CalculatePosCanSpawnBox()
        {
            var roomBounds = _orderRoomController.RoomBounds;
            var listPosToCheck = RoomHelper.GetFloorPositionList(roomBounds, _pfCartonBox.Size);
            var isoObjectList = new List<IsoObject>();

            _canSpawnBoxList = new List<Vector3>();
            _orderRoomController.FurnitureList.ForEach(x => isoObjectList.Add(x.IsoObject));

            foreach (var checkPos in listPosToCheck)
            {
                var isPositionValid = RoomHelper.IsPositionValid(checkPos, _pfCartonBox.Size, _pfCartonBox.IsoObject, isoObjectList);
                if (isPositionValid)
                {
                    _canSpawnBoxList.Add(checkPos);
                }
            }

            if (_canSpawnBoxList.Count == 0)
            {
                _canSpawnBoxList.Add(Vector3.zero);
            }
        }

        private void TryRemoveFurnitureAtIndex(int index)
        {
            if (_unboxFurSpawnDic.TryGetValue(index, out var fur))
            {
                _orderRoomController.ReleaseFurniture(fur);
            }
        }

        private void AddFurnitureToDicAtIndex(int index)
        {
            if (_unboxFurSpawnDic.ContainsKey(index))
            {
                _unboxFurSpawnDic[index] = _curFurUnboxed;
            }
            else
            {
                _unboxFurSpawnDic.Add(index, _curFurUnboxed);
                _furSpawnedCount++;
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
