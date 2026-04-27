using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandDecorController : HomeLandSubController
    {
        public static event Action<bool> OnDragFurniture;

        [Header("CAMERA DRAG CONFIG")]
        [SerializeField] private float _dragCameraSpeed;
        [SerializeField] private float _holdingFurDragCamSpeed;
        [SerializeField] private float _edgeOffset = 150f;

        public override EHomeLandControllerType Type => EHomeLandControllerType.Decor;

        public FurnitureController CurFurniture
        {
            get => _curFur;
            set
            {
                if (value == null)
                {
                    _curFur = value;
                    foreach (var fur in _curDecorFurList)
                    {
                        fur.VisualController.Normalize();
                    }
                    foreach (var fur in _curDecorFurList)
                    {
                        fur.SpriteRenderer.sortingOrder += _homeLand.LandConfig.AdditionSortingValue;
                    }
                }
                else
                {
                    AudioManager.PlaySound(ESound.PickUp);
                    _curFur = value;
                    _curFur.VisualController.Highlight(additionSortingOrder: _curFur.SpriteRenderer.sortingOrder);
                }
            }
        }

        public Dictionary<string, FurnitureController> DecorInstanceByIdMap => _decorInstanceByIdMap;
        public Vector3 DecorBoxDropPosition => LandHelper.GetOffsetFurniturePos(Vector2.zero, _homeLand.LandConfig.Position);
        public FurnitureController CurFurDecorBox => _curFurDecorBox;
        public OrderModeDecorBox CurDecorBox => _curDecorBox;
        public bool IsPlacedFur => _isPlacedFur;

        private List<FurnitureController> _allFurList => _homeLand.FurInstanceByIdMap.Values.ToList();
        private Dictionary<string, FurnitureController> _decorInstanceByIdMap = new Dictionary<string, FurnitureController>();
        private Camera _camera;
        private Vector3 _dragOrigin;
        private Vector2 _minCameraBounds, _maxCameraBounds;
        private OrderModeDecorBox _curDecorBox;
        private FurnitureController _curFur, _curFurDecorBox;
        private List<FurnitureController> _curDecorFurList;
        private List<RoomFurnitureData> _decorFurData;
        private readonly float _newPosZ = 3;
        private bool _isAnimSpawnFur, _isPlacedFur;

        public override void Init(HomeLand homeLand)
        {
            base.Init(homeLand);
            _camera = CameraController.Instance.Camera;
        }

        public override void OnEnter()
        {
            base.OnEnter();

            _isPlacedFur = false;
            CalculateBounds();

            _decorFurData = new List<RoomFurnitureData>();
            _decorInstanceByIdMap = new Dictionary<string, FurnitureController>();
            _curDecorFurList = new List<FurnitureController>();

            _landProgress.GetDecorBoxFurList().ForEach(x =>
            {
                _decorFurData.Add(x);
            });

            if (_decorFurData.Count > 0)
            {
                SpawnDecorBox(DecorBoxDropPosition).Forget();
            }
            MapValidDecorFurniture();
        }

        public override void OnExit()
        {
            base.OnExit();

            RemoveInvalidDecorFurniture();
            if (_curFurDecorBox != null)
            {
                RemoveDecorBox();
            }
        }

        public override void OnUpdate()
        {
            base.OnUpdate();

            if (UIManager.Instance.IsPointerOverUIObject())
                return;

            if (Input.touchCount <= 0)
                return;

            var inputPosition = _homeLand.IsoWorld.TouchIsoPosition(0);
            var touch = Input.GetTouch(0);

            // Begin select handle
            if (touch.phase == TouchPhase.Began)
            {
                var furOnTouch = LandHelper.GetFurnitureOnTouch(inputPosition, _homeLand.Bounds.Min, _homeLand.Bounds.Max, _homeLand.RoomSize, _curDecorFurList, _curFur);
                _dragOrigin = _camera.ScreenToViewportPoint(touch.position);

                if (furOnTouch == null)
                {
                    if (IsClickOnBox(inputPosition))
                    {
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
                    OnDragFurniture?.Invoke(true);
                }
            }

            if (_curFur != null)
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
                    pos.x = Mathf.Clamp(pos.x, _minCameraBounds.x, _maxCameraBounds.x);
                    pos.y = Mathf.Clamp(pos.y, _minCameraBounds.y, _maxCameraBounds.y);
                    cameraTransform.position = pos;
                }

                if (touch.phase == TouchPhase.Moved)
                {
                    OnMoveFurniture(inputPosition, _curFur);
                }

                if (touch.phase == TouchPhase.Ended)
                {
                    OnDropFurniture(_curFur);
                }
            }
            else
            {
                if (touch.phase == TouchPhase.Moved)
                {
                    var touchPosition = _camera.ScreenToViewportPoint(touch.position);
                    var move = (_dragOrigin - touchPosition) * _dragCameraSpeed;

                    Vector3 targetPosition = _camera.transform.position + move;
                    targetPosition.x = Mathf.Clamp(targetPosition.x, _minCameraBounds.x, _maxCameraBounds.x);
                    targetPosition.y = Mathf.Clamp(targetPosition.y, _minCameraBounds.y, _maxCameraBounds.y);

                    _camera.transform.position = targetPosition;
                    _dragOrigin = _camera.ScreenToViewportPoint(touch.position);
                }
            }
        }

        private void OnMoveFurniture(Vector3 inputPosition, FurnitureController furMove)
        {
            var normalizedPosition = LandHelper.NormalizedInputPositionWithBounds(inputPosition, _bounds.Max, _bounds.Min);

            RoomHelper.RemoveNested(furMove.NestedController.Parent, furMove);
            furMove.FlipController.AutoFlip(normalizedPosition.x > normalizedPosition.y
                ? FurnitureDirection.Right
                : FurnitureDirection.Left);
            furMove.MoveController.SetPosition(LandHelper.GetMovePosition(inputPosition, furMove, _homeLand.Bounds.Max, _homeLand.Bounds.Min));
            furMove.VisualController.IsValid = LandHelper.IsPositionValid(furMove.Position, furMove.Size, furMove, _allFurList, _bounds.Max, _bounds.Min);
        }

        private void OnDropFurniture(FurnitureController furDrop)
        {
            AudioManager.PlaySound(ESound.Drop);
            if (furDrop.VisualController.IsValid)
            {
                // If current furniture can place other -> check if furniture is on surface of any. -> Nest them.
                if (furDrop.Config.CanPlaceOnOthers)
                {
                    var parentFur = RoomHelper.GetFurnitureOnSurface(furDrop.Position, furDrop.Size, furDrop, _allFurList);
                    if (parentFur != null)
                    {
                        RoomHelper.NestFurniture(parentFur, furDrop, true);
                        furDrop.VisualController.IsValid = true;
                    }
                }
                furDrop.VisualController.Normalize();
                SetSortingOrder(furDrop);
                ConfirmDecorPosition(furDrop);
            }
            else // If current furniture overlap any other fur -> Move to valid place
            {
                PlaceToValidPosition(furDrop);
                furDrop.VisualController.Normalize();
                SetSortingOrder(furDrop);
            }
            _isPlacedFur = true;
            _curFur = null;
            _homeLand.PlaceCharacter();
            OnDragFurniture?.Invoke(false);

            void SetSortingOrder(FurnitureController fur)
            {
                fur.SpriteRenderer.sortingOrder += _homeLand.LandConfig.AdditionSortingValue;
                foreach (var child in fur.NestedController.Childs)
                {
                    child.SpriteRenderer.sortingOrder += _homeLand.LandConfig.AdditionSortingValue;
                }
            }
        }

        private async UniTaskVoid SpawnDecorBox(Vector3 position)
        {
            var instance = FurnitureManager.Instance.Get(Define.DefaultId.DECOR_BOX);
            instance.transform.SetParent(_homeLand.IsoWorld.transform);
            instance.gameObject.SetActive(true);
            instance.FlipController.FlipTo(instance.Config.BaseDirection);
            instance.MoveController.SetPosition(position);
            instance.MoveController.ToggleMoveable(false);
            _curDecorBox = instance.GetComponent<OrderModeDecorBox>();
            _curFurDecorBox = instance;
            _curDecorBox.DropBox(position, _decorFurData.Count, true);
            _curDecorBox.SetSortingOrder(_homeLand.LandConfig.AdditionSortingValue);
            await UniTask.WaitForSeconds(_curDecorBox.DropBoxAnimTime, cancellationToken: destroyCancellationToken);
        }

        private void MapValidDecorFurniture()
        {
            var validDecorList = _landProgress.GetRoomFurDataValidDecorList();

            foreach (var data in validDecorList)
            {
                if (_homeLand.FurInstanceByIdMap.TryGetValue(data.Id, out var furInstance))
                {
                    _decorInstanceByIdMap.Add(data.Id, furInstance);
                    _curDecorFurList.Add(furInstance);
                }
            }
        }

        private void SpawnNewDecorFurniture()
        {
            if (_curDecorFurList.Any(fur => RoomHelper.IsOverlap(
                    new Vector3(_curFurDecorBox.Position.x, _curFurDecorBox.Position.y, _newPosZ),
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

            _decorInstanceByIdMap.Add(furData.Id, furInstance);
            AudioManager.PlaySound(ESound.Release);
            _decorFurData.RemoveAt(firstIndex);

            furInstance.transform.SetParent(_homeLand.IsoWorld.transform);
            furInstance.gameObject.SetActive(true);
            furInstance.MoveController.SetPosition(_curFurDecorBox.Position);
            furInstance.FlipController.FlipTo(furData.Direction);
            furInstance.SpriteRenderer.sortingOrder += _homeLand.LandConfig.AdditionSortingValue;

            _curDecorBox.FoamAnim(_decorFurData.Count);
            DOVirtual.Float(0, _newPosZ, 0.3f, v => { furInstance.MoveController.SetPosition(new Vector3(_curFurDecorBox.Position.x, _curFurDecorBox.Position.y, v)); })
                .OnComplete(() =>
                {
                    _curDecorFurList.Add(furInstance);
                    _homeLand.FurInstanceByIdMap.TryAdd(furData.Id, furInstance);
                    _isAnimSpawnFur = false;
                })
                .SetDelay(0.1f);

            if (IsDecorBoxEmpty())
                RemoveDecorBox();
        }

        private bool PlaceToValidPosition(FurnitureController curFur)
        {
            var (newPosition, newDirection, newParent) = LandHelper.GetFurnitureValidPlace(curFur, _bounds.Min, _bounds.Max, _allFurList);

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
            ConfirmDecorPosition(curFur);
            return true;
        }

        private bool IsClickOnBox(Vector3 input)
        {
            if (_curFurDecorBox == null)
                return false;

            // Fix offset
            input.x -= 0.5f;
            input.y -= 0.5f;

            var maxBounds = _homeLand.Bounds.Max;
            var minBounds = _homeLand.Bounds.Min;

            if (input.x > maxBounds.x || input.y > maxBounds.y || input.x < minBounds.x || input.y < minBounds.y)
            {
                if (input.x > maxBounds.x || input.x < minBounds.x)
                {
                    if (input.x > maxBounds.x)
                    {
                        input.z = input.x - maxBounds.x;
                        input.y -= input.z;
                        input.x = maxBounds.x;
                    }
                    else
                    {
                        input.z = minBounds.x - input.x;
                        input.y += input.z;
                        input.x = minBounds.x;
                    }
                }
                else if (input.y > maxBounds.y || input.y < minBounds.y)
                {
                    if (input.y > maxBounds.y)
                    {
                        input.z = input.y - maxBounds.y;
                        input.x -= input.z;
                        input.y = maxBounds.y;
                    }
                    else
                    {
                        input.z = minBounds.y - input.y;
                        input.x += input.z;
                        input.y = minBounds.y;
                    }
                }
            }

            return LandHelper.IsOverlap(input, Vector3.one * 2, _curFurDecorBox.Position, _curFurDecorBox.Size);
        }

        private bool IsDecorBoxEmpty() => _decorFurData.Count == 0;

        private void RemoveDecorBox()
        {
            _homeLand.PlaySmokeFX(_curDecorBox.transform.position);
            AudioManager.PlaySound(ESound.Whoosh);
            FurnitureManager.Instance.Release(_curFurDecorBox);
            _curFurDecorBox = null;
            _curDecorBox = null;
        }

        private void RemoveInvalidDecorFurniture()
        {
            var validFurList = _landProgress.ValidDecorFurList;
            _decorInstanceByIdMap.ForEach(pair =>
            {
                if (!validFurList.Contains(pair.Key))
                {
                    FurnitureManager.Instance.Release(pair.Value);
                }
            });
        }

        private void ConfirmDecorPosition(FurnitureController furDrop)
        {
            if (_decorInstanceByIdMap.TryGetKeyByValue(furDrop, out var key))
            {
                _landProgress.SaveValidDecorFur(key, furDrop.Position, furDrop.CurDirection);
                _homeLand.FurInstanceByIdMap.TryAdd(key, furDrop);
                _homeLand.SaveLandData();
            }
        }

        private void CalculateBounds()
        {
            var landCamBound = LandController.Instance.LandCamBound;
            _minCameraBounds = new Vector2(_homeLand.RoomCenterPos.x - 7f, _homeLand.RoomCenterPos.y - 3f);
            _maxCameraBounds = new Vector2(_homeLand.RoomCenterPos.x + 7f, _homeLand.RoomCenterPos.y + 3f);

            // Ensure the minimum bounds do not exceed the land camera bounds
            _minCameraBounds.x = Mathf.Max(landCamBound.MinX, _minCameraBounds.x);
            _minCameraBounds.y = Mathf.Max(landCamBound.MinY, _minCameraBounds.y);

            // Ensure the maximum bounds do not exceed the land camera bounds
            _maxCameraBounds.x = Mathf.Min(landCamBound.MaxX, _maxCameraBounds.x);
            _maxCameraBounds.y = Mathf.Min(landCamBound.MaxY, _maxCameraBounds.y);
        }
    }
}
