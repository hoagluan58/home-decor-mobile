using System;
using System.Collections.Generic;
using System.Linq;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class OrderDecorState : OrderModeState
    {
        public override EOrderState StateName => EOrderState.Decorate;

        public static event Action OnAllFurniturePutOut;
        public static event Action<int, int> OnDecorFurPutOut;
        public static event Action<float> OnDecorTierChanged;

        private readonly Vector3 BOX_POSITION = Vector3.zero;
        private const float BASE_POINT = 0.8f;

        [Title("FX")]
        [SerializeField] private ParticleSystem _removeBoxFx;

        [Title("CONFIG")]
        [SerializeField] private int _newPosZ;
        [SerializeField] private float _offSetZoom;

        [Title("CAMERA DRAG CONFIG")]
        [SerializeField] private float _dragCameraSpeed = 8f;
        [SerializeField] private float _holdingFurDragCamSpeed = 0.2f;
        [SerializeField] private Vector2 _minBounds = new(-4, 3);
        [SerializeField] private Vector2 _maxBounds = new(4, 7);
        [SerializeField] private float _edgeOffset = 150f;

        private List<FurnitureController> _wrongTierFurnitureList;
        private int _curTotalDecorFur, _curDecorFurPutOut;
        private Camera _camera;
        private float _baseOrthographicSize;
        private List<RoomFurnitureData> _decorFurData;
        private List<FurnitureController> _curDecorFurList;
        private FurnitureController _curFurniture, _curFurnitureBox;
        private OrderModeDecorBox _curDecorBox;
        private Dictionary<RoomFurnitureData, FurnitureController> _mappingFurToDataDic;
        private bool _isAnimSpawnFur, _hasPlaceLastFurniture;
        private Vector3 _dragOrigin;
        private bool _isUpdate;

        public Vector3 RoomBounds => _orderRoomController.RoomBounds;
        public List<FurnitureController> AllFurList => _orderRoomController.FurnitureList.Concat(_curDecorFurList).ToList();

        public FurnitureController CurFurniture
        {
            get => _curFurniture;
            set
            {
                if (value == null)
                {
                    _curFurniture = value;
                    foreach (var fur in _curDecorFurList)
                    {
                        fur.VisualController.Normalize();
                    }
                }
                else
                {
                    AudioManager.PlaySound(ESound.PickUp);
                    _curFurniture = value;
                    _curFurniture.VisualController.Highlight();
                }
            }
        }

        public override void Enter()
        {
            _isUpdate = _hasPlaceLastFurniture = false;
            var decorFurDataList = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.DecorOptions;
            _decorFurData = new List<RoomFurnitureData>();
            _curDecorFurList = new List<FurnitureController>();
            _mappingFurToDataDic = new Dictionary<RoomFurnitureData, FurnitureController>();

            foreach (var data in decorFurDataList)
            {
                _decorFurData.Add(data);
            }

            _curTotalDecorFur = _decorFurData.Count;
            _curDecorFurPutOut = 0;
            _camera = CameraController.Instance.Camera;
            SpawnDecorBox(BOX_POSITION).Forget();
            OnDecorFurPutOut?.Invoke(_curDecorFurPutOut, _curTotalDecorFur);
        }

        public override void OnUpdate()
        {
            if (!_isUpdate) return;
            if (UIManager.Instance.IsPointerOverUIObject()) return;
            if (!InputHelper.HasInput()) return;

            var inputPosition = InputHelper.GetIsoPosition(_isoWorld);
            var touchPhase = InputHelper.GetTouchPhase();
            var screenPosition = InputHelper.GetScreenPosition();
            var mainCamera = CameraController.Instance.Camera;

            // Begin select handle
            if (touchPhase == TouchPhase.Began)
            {
                var furOnTouch = RoomHelper.GetFurnitureOnTouch(inputPosition, RoomBounds, _curDecorFurList, _curFurniture);
                _dragOrigin = mainCamera.ScreenToViewportPoint(screenPosition);

                if (furOnTouch == null)
                {
                    if (IsClickOnBox(inputPosition))
                    {
                        VibrationManager.Vibrate(0.05f);
                        if (!_curDecorBox.IsOpenBox)
                        {
                            _curDecorBox.PlayHandTutorial(false);
                            _curDecorBox.OpenBox();
                        }
                        else
                        {
                            SpawnNewDecorFurniture();
                        }
                    }
                    else
                    {
                        CurFurniture = null;
                    }
                }
                else
                {
                    CurFurniture = furOnTouch;
                }
            }

            if (CurFurniture != null)
            {
                // Move camera
                var touchPosition = screenPosition;
                if (touchPosition.x <= _edgeOffset || touchPosition.x >= Screen.width - _edgeOffset ||
                    touchPosition.y <= _edgeOffset || touchPosition.y >= Screen.height - _edgeOffset)
                {
                    var move = new Vector3();

                    if (touchPosition.x <= _edgeOffset)
                        move.x = -_holdingFurDragCamSpeed;
                    else if (touchPosition.x >= Screen.width - _edgeOffset)
                        move.x = _holdingFurDragCamSpeed;

                    if (touchPosition.y <= _edgeOffset)
                        move.y = -_holdingFurDragCamSpeed;
                    else if (touchPosition.y >= Screen.height - _edgeOffset)
                        move.y = _holdingFurDragCamSpeed;

                    var cameraTransform = mainCamera.transform;
                    cameraTransform.Translate(move, Space.World);
                    var pos = cameraTransform.position;
                    pos.x = Mathf.Clamp(pos.x, _minBounds.x, _maxBounds.x);
                    pos.y = Mathf.Clamp(pos.y, _minBounds.y, _maxBounds.y);
                    cameraTransform.position = pos;
                }

                if (touchPhase == TouchPhase.Moved)
                {
                    OnMoveFurniture(inputPosition, _curFurniture);
                }

                if (touchPhase == TouchPhase.Ended)
                {
                    OnDropFurniture(_curFurniture);
                }
            }
            else
            {
                if (touchPhase == TouchPhase.Moved)
                {
                    var touchPositionCam = mainCamera.ScreenToViewportPoint(screenPosition);
                    var move = (_dragOrigin - touchPositionCam) * _dragCameraSpeed;

                    Vector3 targetPosition = mainCamera.transform.position + move;
                    targetPosition.x = Mathf.Clamp(targetPosition.x, _minBounds.x, _maxBounds.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, _minBounds.y, _maxBounds.y);

                    mainCamera.transform.position = targetPosition;
                    _dragOrigin = mainCamera.ScreenToViewportPoint(screenPosition);
                }
            }
        }

        public override void Exit()
        {
            _isUpdate = false;
        }

        public bool IsClickOnBox(Vector3 input)
        {
            if (_curFurnitureBox == null)
                return false;

            // Fix offset
            input.x -= 0.5f;
            input.y -= 0.5f;

            if (input.x > RoomBounds.x || input.y > RoomBounds.y)
            {
                if (input.x > input.y)
                {
                    input.z = input.x - RoomBounds.x;
                    input.y -= input.z;
                    input.x = RoomBounds.x;
                }
                else
                {
                    input.z = input.y - RoomBounds.y;
                    input.x -= input.z;
                    input.y = RoomBounds.y;
                }
            }

            return RoomHelper.IsOverlap(input, Vector3.one * 2, _curFurnitureBox.Position, _curFurnitureBox.Size);
        }

        private void OnMoveFurniture(Vector3 inputPosition, FurnitureController furMove)
        {
            RoomHelper.RemoveNested(furMove.NestedController.Parent, furMove);
            furMove.FlipController.AutoFlip(inputPosition.x > inputPosition.y
                ? FurnitureDirection.Right
                : FurnitureDirection.Left);
            furMove.MoveController.SetPosition(RoomHelper.GetMovePosition(inputPosition, furMove, RoomBounds));
            furMove.VisualController.IsValid = RoomHelper.IsPositionValid(furMove.Position, furMove.Size, furMove, AllFurList, RoomBounds);
        }

        private void OnDropFurniture(FurnitureController furDrop)
        {
            AudioManager.PlaySound(ESound.Drop);
            if (furDrop.VisualController.IsValid)
            {
                // If current furniture can place other -> check if furniture is on surface of any. -> Nest them.
                if (furDrop.Config.CanPlaceOnOthers)
                {
                    var parentFur = RoomHelper.GetFurnitureOnSurface(furDrop.Position, furDrop.Size, furDrop, AllFurList);
                    if (parentFur != null)
                    {
                        RoomHelper.NestFurniture(parentFur, furDrop, true);
                        furDrop.VisualController.IsValid = true;
                    }
                }
                furDrop.VisualController.Normalize();
            }
            else // If current furniture overlap any other fur -> Move to valid place
            {
                PlaceToValidPosition(furDrop);
                furDrop.VisualController.IsValid = true;
                furDrop.VisualController.Normalize();
            }

            if (IsDecorBoxEmpty())
            {
                CheckForFiveStars();
                if (!_hasPlaceLastFurniture)
                {
                    _hasPlaceLastFurniture = true;
                    OnAllFurniturePutOut?.Invoke();
                    _mappingFurToDataDic.Values.ForEach(fur => OutlineFurniture(fur));
                }
                OutlineFurniture(furDrop);
            }
        }

        private bool PlaceToValidPosition(FurnitureController curFur)
        {
            var (newPosition, newDirection, newParent) = RoomHelper.GetFurnitureValidPlace(curFur, RoomBounds, AllFurList);

            if (newPosition == Vector3.back)
            {
                return false;
            }

            curFur.MoveController.SetPosition(newPosition);

            if (newDirection != curFur.CurDirection)
            {
                curFur.FlipController.FlipTo(newDirection);
                curFur.FlipController.FlipChild();
            }

            RoomHelper.RemoveNested(curFur.NestedController.Parent, curFur);
            if (newParent != null)
            {
                RoomHelper.NestFurniture(newParent, curFur, false);
                curFur.NestedController.UpdateLocalPosition();
            }

            curFur.VisualController.IsValid = true;
            return true;
        }

        private async UniTaskVoid SpawnDecorBox(Vector3 position)
        {
            var instance = FurnitureManager.Instance.Get(Define.DefaultId.DECOR_BOX);
            instance.transform.SetParent(_isoWorld.transform);
            instance.gameObject.SetActive(true);
            instance.FlipController.FlipTo(instance.Config.BaseDirection);
            instance.MoveController.SetPosition(position);
            instance.MoveController.ToggleMoveable(false);
            _curFurnitureBox = instance;
            _curDecorBox = instance.GetComponent<OrderModeDecorBox>();
            _curDecorBox.DropBox(position, _decorFurData.Count, true);

            await UniTask.WaitForSeconds(_curDecorBox.DropBoxAnimTime, cancellationToken: destroyCancellationToken);
            CameraController.Instance.MoveAndZoomAnimation(new Vector3(0, 6, 0), 13f, 0.3f);
            _isUpdate = true;
        }

        public void SpawnNewDecorFurniture()
        {
            if (_curDecorFurList.Any(fur => RoomHelper.IsOverlap(
                    Vector3.forward * _newPosZ,
                    Vector3.one,
                    fur.Position,
                    fur.Size))) return;

            if (_decorFurData.Count == 0) return;
            if (_isAnimSpawnFur) return;
            _isAnimSpawnFur = true;

            CurFurniture = null;

            var firstIndex = 0;
            var furData = _decorFurData[firstIndex];
            var furInstance = FurnitureManager.Instance.Get(furData.ConfigId);

            AudioManager.PlaySound(ESound.Release);
            _curDecorFurPutOut++;
            _mappingFurToDataDic.Add(furData, furInstance);
            _decorFurData.RemoveAt(firstIndex);

            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.gameObject.SetActive(true);
            furInstance.MoveController.SetPosition(Vector3.one);
            furInstance.FlipController.FlipTo(furData.Direction);
            _curDecorBox.FoamAnim(_decorFurData.Count);
            DOVirtual.Float(0, _newPosZ, 0.3f, v => { furInstance.MoveController.SetPosition(Vector3.forward * v); })
                .OnComplete(() =>
                {
                    _curDecorFurList.Add(furInstance);
                    AllFurList.Add(furInstance);
                    _isAnimSpawnFur = false;
                })
                .SetDelay(0.1f);

            OnDecorFurPutOut?.Invoke(_curDecorFurPutOut, _curTotalDecorFur);

            if (IsDecorBoxEmpty())
                RemoveDecorBox();
        }

        private void RemoveDecorBox()
        {
            _removeBoxFx.transform.position = _curDecorBox.transform.position;
            _removeBoxFx.Play();
            AudioManager.PlaySound(ESound.Whoosh);
            _orderRoomController.ReleaseFurniture(_curFurnitureBox);
            _curFurnitureBox = null;
            _curDecorBox = null;
        }

        private void CheckForFiveStars()
        {
            var numCorrectFur = 0;
            var curRoomConfig = OrderModeManager.Instance.GetCurOrderRoomConfigData();
            _wrongTierFurnitureList = new List<FurnitureController>();
            foreach (var kvp in _mappingFurToDataDic)
            {
                var tier = curRoomConfig.GetRoomFurnitureDecorTier(kvp.Key.Id);
                if (tier == EOrderDecorTier.Tier3)
                {
                    numCorrectFur++;
                    continue;
                }

                var curTier = CheckFurnitureTier(kvp.Value);
                var isCorrectTier = tier == curTier;
                if (isCorrectTier)
                {
                    numCorrectFur++;
                }
                else
                {
                    _wrongTierFurnitureList.Add(kvp.Value);
                }
            }

            var value = BASE_POINT + (numCorrectFur / (float)_mappingFurToDataDic.Keys.Count * 0.2f);
            var curOrder = OrderModeManager.Instance.GetCurBotOrderData();

            curOrder.StarRating = value == 1f ? 5 : 4;
            OnDecorTierChanged?.Invoke(value);
        }


        private void OutlineFurniture(FurnitureController fur)
        {
            if (fur == null)
                return;

            if (_wrongTierFurnitureList.Contains(fur))
            {
                fur.VisualController.Outline(EFurnitureMaterial.OutlineYellow);
                _orderRoomController.RoomWorldUI.InitEmojiTier(fur);
            }
            else
            {
                fur.VisualController.Outline(EFurnitureMaterial.Default);
            }
        }

        private EOrderDecorTier CheckFurnitureTier(FurnitureController furnitureController)
        {
            var result = EOrderDecorTier.Tier3;
            var hasChild = furnitureController.NestedController.HasChild();
            var hasParent = furnitureController.NestedController.HasParent();

            if (!hasParent)
            {
                result = EOrderDecorTier.Tier1;
                return result;
            }

            else if (hasParent && !hasChild)
            {
                result = EOrderDecorTier.Tier2;
                return result;
            }

            return result;
        }

        private bool IsDecorBoxEmpty() => _decorFurData.Count == 0;
    }
}