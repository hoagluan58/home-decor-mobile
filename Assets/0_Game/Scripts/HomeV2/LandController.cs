using Cysharp.Threading.Tasks;
using IsoTools;
using NFramework;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace YoyoDesign
{
    public class LandController : SingletonMono<LandController>
    {
        [SerializeField] private HomeLand _landPf;
        [SerializeField] private DragToMoveCamera _dragToMoveCamera;

        [Header("Background Sprite")]
        [SerializeField] private SpriteRenderer _backgroundSprite;

        private Vector3 _cachePosition;

        public CameraBound LandCamBound
        {
            get
            {
                _cameraBound.CalculateBounds(_backgroundSprite.bounds, Define.Size.LAND_CAMERA_SIZE);
                return _cameraBound;
            }
        }

        private CameraBound _cameraBound;
        private bool _isZoomingCam;
        private List<HomeLand> _lands = new List<HomeLand>();
        private IsoWorld _isoWorldDetectInput;
        private HomeLand _curLand;
        private Camera _camera;

        public HomeLand CurLandEditing
        {
            get => _curLand;
            set
            {
                if (value == _curLand)
                    return;

                HandleOnLandChanged();

                async void HandleOnLandChanged()
                {
                    if (_curLand != null)
                    {
                        _curLand.OnExit();
                        ToggleNearbyLand(_curLand.LandConfig.Id, true);
                    }

                    if (value == null)
                    {
                        _curLand = value;
                        var firstNotUnlockedLand = GetFirstNotUnlockedLand();
                        var zoomPosition = GetClampedCameraPosition(_camera.transform.position, Define.Size.HOME_CAMERA_SIZE);
                        if (firstNotUnlockedLand != null)
                        {
                            zoomPosition = GetClampedCameraPosition(firstNotUnlockedLand.RoomCenterPos, Define.Size.HOME_CAMERA_SIZE);
                        }
                        await ZoomCamera(zoomPosition, Define.Size.HOME_CAMERA_SIZE, 1f);
                        _dragToMoveCamera.CanDrag = true;
                    }
                    else
                    {
                        _curLand = value;
                        ToggleNearbyLand(_curLand.LandConfig.Id, false);
                        _dragToMoveCamera.CanDrag = false;
                        _curLand.OnEnter();
                        var zoomPosition = GetClampedCameraPosition(_curLand.RoomCenterPos, Define.Size.LAND_CAMERA_SIZE);
                        ZoomCamera(zoomPosition, Define.Size.LAND_CAMERA_SIZE, 1f).Forget();
                        UIManager.Instance.Close(Define.UIName.HOME_V2_MENU);

                        // Open Land Menu UI
                        if (UIManager.Instance.IsSpecificViewShown(Define.UIName.LAND_MENU, out var view))
                        {
                            var landMenu = view as LandMenuUI;
                            landMenu.SetData(_curLand.LandConfig.Id);
                        }
                        else
                        {
                            UIManager.Instance.Open<LandMenuUI>(Define.UIName.LAND_MENU).SetData(_curLand.LandConfig.Id);
                        }
                    }
                }
            }
        }

        private void Start()
        {
            var configs = AllConfig.Instance.LandConfigDic;

            _camera = CameraController.Instance.Camera;
            foreach (var id in configs.Keys)
            {
                var land = Instantiate(_landPf, this.transform);
                land.Init(id).Forget();
                _lands.Add(land);
            }
            if (_isoWorldDetectInput == null)
                _isoWorldDetectInput = GetFirstActiveIsoWorld();

            _cameraBound = new CameraBound();
        }

        private void Update()
        {
            if (_isZoomingCam) return;

            if (CurLandEditing != null)
            {
                CurLandEditing.OnUpdate();
            }

            if (UIManager.Instance.IsPointerOverUIObject()) return;
            if (Input.touchCount <= 0) return;
            if (_dragToMoveCamera.IsDragging) return;

            var inputPosition = _isoWorldDetectInput.TouchIsoPosition(0);
            var touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                _cachePosition = inputPosition;
            }

            if (touch.phase == TouchPhase.Ended)
            {
                var endPosition = _isoWorldDetectInput.TouchIsoPosition(0);
                if (endPosition != _cachePosition) return;

                if (CurLandEditing == null)
                {
                    var landOnTouch = LandHelper.GetLandOnTouch(inputPosition, _lands);
                    if (landOnTouch != null)
                    {
                        if (!landOnTouch.LandProgress.IsUnlock)
                        {
                            var isUnlock = landOnTouch.TryUnlockLand();
                            if (isUnlock)
                            {
                                CurLandEditing = landOnTouch;
                            }
                        }
                        else
                        {
                            CurLandEditing = landOnTouch;
                        }
                    }
                }
            }
        }

        public void UnlockTutorialLand()
        {
            var tutorialLand = _lands.FirstOrDefault(x => x.LandConfig.Id == Define.DefaultId.TUTORIAL_UNLOCK_LAND_ID);
            if (tutorialLand != null)
            {
                tutorialLand.TryUnlockLand();
                CurLandEditing = tutorialLand;
            }
        }

        public void FocusTutorialLand()
        {
            var land = _lands.FirstOrDefault(x => x.LandConfig.Id == Define.DefaultId.TUTORIAL_FOCUS_LAND_ID);
            if (land != null)
            {
                CurLandEditing = land;
            }
        }

        public async UniTask ZoomCamera(Vector3 target, float zoomSize, float zoomTime)
        {
            _isZoomingCam = true;
            CameraController.Instance.MoveAndZoomAnimation(target, zoomSize, zoomTime);
            await UniTask.WaitForSeconds(zoomTime, cancellationToken: destroyCancellationToken);
            _isZoomingCam = false;
        }

        private IsoWorld GetFirstActiveIsoWorld()
        {
            foreach (var land in _lands)
            {
                return land.IsoWorld;
            }
            return null;
        }

        private void ToggleNearbyLand(int landId, bool isShow)
        {
            var landConfig = AllConfig.Instance.LandConfigDic[landId];
            foreach (var id in landConfig.NearbyLandList)
            {
                var land = _lands.FirstOrDefault(x => x.LandConfig.Id == id);
                land.ToggleWall(isShow);
                land.ToggleFurniture(isShow);
            }
        }

        public void GoToLandId(int landId, bool isCheckNull = false)
        {
            if (isCheckNull && _curLand == null)
                return;

            var land = _lands.FirstOrDefault(x => x.LandConfig.Id == landId);
            CurLandEditing = land;
        }

        public HomeLand GetFirstNotUnlockedLand()
        {
            foreach (var land in _lands)
            {
                var isLandUnlocked = HomeLandData.Instance.GetLandProgress(land.LandConfig.Id);
                var canUnlockLand = HomeLandData.Instance.CanUnlockLand(land.LandConfig.Id);
                if (canUnlockLand && !isLandUnlocked.IsUnlock)
                    return land;
            }
            return null;
        }

        public HomeLand GetLastUnlockedLand()
            => _lands.LastOrDefault(x => HomeLandData.Instance.GetLandProgress(x.LandConfig.Id).IsUnlock);

        public bool CanGoToLand(int landId)
        {
            var land = _lands.FirstOrDefault(x => x.LandConfig.Id == landId);
            return land != null && land.LandProgress.IsUnlock;
        }

        public Vector3 GetClampedCameraPosition(Vector3 targetPosition, float orthographicSize)
        {
            _cameraBound.CalculateBounds(_backgroundSprite.bounds, orthographicSize);
            var clampedX = Mathf.Clamp(targetPosition.x, _cameraBound.MinX, _cameraBound.MaxX);
            var clampedY = Mathf.Clamp(targetPosition.y, _cameraBound.MinY, _cameraBound.MaxY);

            return new Vector3(clampedX, clampedY, targetPosition.z);
        }

        public async void OnSceneLoadCameraMovement()
        {
            // Check and go to last furniture in land
            if (OrderModeManager.Instance.IsLastFurnitureInLand)
            {
                var lastLand = GetLastUnlockedLand();
                GoToLandId(lastLand.LandConfig.Id, false);
                OrderModeManager.Instance.IsLastFurnitureInLand = false;
                return;
            }

            // Check and go to first not unlocked land
            var firstNotUnlockedLand = GetFirstNotUnlockedLand();
            if (firstNotUnlockedLand != null)
            {
                var zoomPosition = GetClampedCameraPosition(firstNotUnlockedLand.RoomCenterPos, Define.Size.HOME_CAMERA_SIZE);
                await ZoomCamera(zoomPosition, Define.Size.HOME_CAMERA_SIZE, 1f);
                return;
            }

            // Else go to prev land
            GoToLandId(OrderModeManager.Instance.PrevLandId, false);
        }

        public class CameraBound
        {
            public float MinX;
            public float MinY;
            public float MaxX;
            public float MaxY;

            public void CalculateBounds(Bounds bounds, Camera camera) => CalculateBounds(bounds, camera.orthographicSize);

            public void CalculateBounds(Bounds bounds, float camOrthoSize)
            {
                // Calculate the orthographic bounds for the camera
                var vertExtent = camOrthoSize;
                var horzExtent = vertExtent * Screen.width / Screen.height;

                // Set the bounds based on the sprite renderer's bounds and camera's orthographic size
                MinX = bounds.min.x + horzExtent;
                MaxX = bounds.max.x - horzExtent;
                MinY = bounds.min.y + vertExtent;
                MaxY = bounds.max.y - vertExtent;
            }
        }
    }
}
