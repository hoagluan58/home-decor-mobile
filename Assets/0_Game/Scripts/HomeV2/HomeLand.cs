using Cysharp.Threading.Tasks;
using IsoTools;
using NFramework;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace YoyoDesign
{
    public class HomeLand : MonoBehaviour
    {
        [SerializeField] private HomeLandWorldUI _worldUI;
        [SerializeField] private IsoWorld _isoWorld;
        [SerializeField] private WallController _wallController;
        [SerializeField] private FloorController _floorController;

        [Header("LOCK LAND")]
        [SerializeField] private Transform _lockRoomTf;
        [SerializeField] private SpriteRenderer _lockRoomSr;
        [SerializeField] private SpriteRenderer _unlockRoomSr;

        [Header("FX")]
        [SerializeField] private SortingGroup _fxSortingGroup;
        [SerializeField] private ParticleSystem _smokeFX;

        [Header("CONTROLLERS")]
        [SerializeField] private List<HomeLandSubController> _controllers;
        [SerializeField] private HomeLandCharacterController _characterController;
        [SerializeField] private HomeLandTutorialController _tutorialController;

        [Header("INFO")]
        [SerializeField] private Vector3 _roomSize;
        [SerializeField] private LandBounds _bounds;

        [Header("MAPPING FURNITURE CONTROLLER")]
        public SerializableDictionaryBase<string, FurnitureController> FurInstanceByIdMap = new SerializableDictionaryBase<string, FurnitureController>();
        public SerializableDictionaryBase<int, FurnitureController> FurnitureOptionMapping = new SerializableDictionaryBase<int, FurnitureController>(); // Map for Unlock and Repainting Controller

        public HomeLandWorldUI WorldUI => _worldUI;
        public Vector3 RoomCenterPos => _wallController.transform.position;
        public Vector3 RoomSize => _roomSize;
        public LandBounds Bounds => _bounds;
        public IsoWorld IsoWorld => _isoWorld;
        public LandConfigData LandConfig => _landConfig;
        public LandProgress LandProgress => HomeLandData.Instance.GetLandProgress(_landConfig.Id);
        public HomeLandDecorController DecorController => _decorController;

        private HomeLandUnlockFurController _unlockController => (HomeLandUnlockFurController)_controllers.FirstOrDefault(x => x.Type == EHomeLandControllerType.Unlock);
        private HomeLandRepaintingController _repaintingController => (HomeLandRepaintingController)_controllers.FirstOrDefault(x => x.Type == EHomeLandControllerType.Repainting);
        private HomeLandDecorController _decorController => (HomeLandDecorController)_controllers.FirstOrDefault(x => x.Type == EHomeLandControllerType.Decor);
        private LandConfigData _landConfig;

        public async UniTaskVoid Init(int id)
        {
            _landConfig = AllConfig.Instance.LandConfigDic[id];
            HomeLandData.Instance.InitLandSaveData(_landConfig.Id);

            await UniTask.Yield();
            await UniTask.Yield();

            MoveLand(_landConfig.Position.x, _landConfig.Position.y);
            RefreshLand();
            LoadRoom();
            _fxSortingGroup.sortingOrder += _landConfig.AdditionSortingValue;

            _worldUI.OnInit(this);
            foreach (var controller in _controllers)
            {
                controller.Init(this);
            }
            _tutorialController.Init(this);

            UpdateLandSortingOrder(_landConfig.SubtractionSortingValue);
            this.gameObject.name = $"Land_{id}";
        }

        public void OnEnter()
        {
            _worldUI.OnEnter();
            foreach (var controller in _controllers)
            {
                controller.OnEnter();
            }
            _characterController.OnEnter(this);
            UpdateLandSortingOrder(_landConfig.AdditionSortingValue);
            PlaceCharacter();

            if (UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_ORDER)
            {
                _tutorialController.PlayTutorial();
            }
        }

        public void OnExit()
        {
            UpdateLandSortingOrder(_landConfig.SubtractionSortingValue);
            _worldUI.OnExit();
            _characterController.OnExit();
            foreach (var controller in _controllers)
            {
                controller.OnExit();
            }
            _tutorialController.OnExit();
        }

        public void OnUpdate()
        {
            foreach (var controller in _controllers)
            {
                controller.OnUpdate();
            }
        }

        public async void OnRefresh()
        {
            _worldUI.Refresh();

            foreach (var controller in _controllers)
            {
                controller.OnRefresh();
            }
            PlaceCharacter();
            var unlockFurBtnPos = _worldUI.UnlockFurButtonPos;
            if (unlockFurBtnPos == Vector3.zero)
                return;

            var zoomPos = LandController.Instance.GetClampedCameraPosition(unlockFurBtnPos, Define.Size.LAND_CAMERA_SIZE);
            await LandController.Instance.ZoomCamera(zoomPos, Define.Size.LAND_CAMERA_SIZE, 0.8f);
        }

        public bool TryUnlockLand()
        {
            if (!LandProgress.IsUnlock)
            {
                var canUnlock = HomeLandData.Instance.CanUnlockLand(_landConfig.Id);
                if (canUnlock)
                {
                    LandProgress.UnlockLand();
                    RefreshLand();
                    return true;
                }
            }
            return false;
        }

        public void SaveLandData()
        {
            var tempData = new SerializableDictionaryBase<FurnitureController, string>();
            var allFurInstanceMap = new Dictionary<string, FurnitureController>();

            FurInstanceByIdMap.ForEach(pair => allFurInstanceMap.TryAdd(pair.Key, pair.Value));
            _decorController.DecorInstanceByIdMap.ForEach(pair => allFurInstanceMap.TryAdd(pair.Key, pair.Value));

            LandProgress.FurnitureData = new List<RoomFurnitureData>();

            foreach (var fur in allFurInstanceMap.Values)
            {
                var newFurData = new RoomFurnitureData(fur.Config.Id, LandHelper.GetBaseFurniturePos(fur.Position, _landConfig.Position), fur.CurDirection);
                if (allFurInstanceMap.TryGetKeyByValue(fur, out var furId))
                {
                    newFurData.Id = furId;
                }
                LandProgress.FurnitureData.Add(newFurData);
                tempData.TryAdd(fur, newFurData.Id);
            }

            foreach (var fur in allFurInstanceMap.Values)
            {
                if (fur.NestedController.Parent != null)
                {
                    var furData = LandProgress.FurnitureData.FirstOrDefault(f => f.Id == tempData[fur]);
                    furData.ParentId = tempData[fur.NestedController.Parent];
                }
            }
            LandProgress.DataChanged = true;
        }

        public void PlaySmokeFX(Vector3 position)
        {
            _smokeFX.transform.position = position;
            _smokeFX.Play();
        }

        private void LoadRoom()
        {
            FurInstanceByIdMap = new SerializableDictionaryBase<string, FurnitureController>();
            FurnitureOptionMapping = new SerializableDictionaryBase<int, FurnitureController>();

            var roomFurData = HomeLandData.Instance.GetLandProgress(_landConfig.Id).FurnitureData;
            foreach (var furData in roomFurData)
            {
                SpawnFurniture(furData);
            }

            foreach (var furData in roomFurData)
            {
                if (furData.ParentId == "") continue;

                var furChild = FurInstanceByIdMap[furData.Id];
                var furParent = FurInstanceByIdMap[furData.ParentId];
                RoomHelper.NestFurniture(furParent, furChild, false);
            }
        }

        private void SetWall(string wallId)
        {
            var wallConfig = AllConfig.Instance.WallConfigDic[wallId];
            _wallController.ChangeWall(wallConfig.Id, wallConfig.Sprite);
        }

        private void SetFloor(string floorId)
        {
            var floorConfig = AllConfig.Instance.FloorConfigDic[floorId];
            _floorController.ChangeFloor(floorConfig.Id, floorConfig.Sprite);
        }

        private void RefreshLand()
        {
            if (LandProgress.IsUnlock)
            {
                ToggleWall(true);
                ToggleFurniture(true);
                _floorController.SetVisibleFloor(true);
                _lockRoomTf.gameObject.SetActive(false);
                _lockRoomSr.gameObject.SetActive(false);
                SetWall(_landConfig.DefaultWallId);
                SetFloor(_landConfig.DefaultFloorId);
            }
            else
            {
                ToggleWall(false);
                ToggleFurniture(false);
                _floorController.SetVisibleFloor(false);

                var canUnlockLand = HomeLandData.Instance.CanUnlockLand(_landConfig.Id);
                _lockRoomTf.gameObject.SetActive(true);
                _lockRoomSr.gameObject.SetActive(!canUnlockLand);
                _unlockRoomSr.gameObject.SetActive(canUnlockLand);
            }
        }

        public FurnitureController SpawnFurniture(RoomFurnitureData data)
        {
            var furInstance = FurnitureManager.Instance.Get(data.ConfigId);
            furInstance.transform.SetParent(_isoWorld.transform);
            furInstance.gameObject.SetActive(true);
            furInstance.FlipController.FlipTo(data.Direction);
            furInstance.MoveController.SetPosition(LandHelper.GetOffsetFurniturePos(data.Position, _landConfig.Position));
            furInstance.SpriteRenderer.sortingOrder = furInstance.Config.IsCarpet ? Define.SortingOrder.CARPET :
                                                      furInstance.Config.IsWindow ? Define.SortingOrder.WINDOW : Define.SortingOrder.NORMAL;
            furInstance.SpriteRenderer.sortingOrder += _landConfig.AdditionSortingValue;
            FurInstanceByIdMap.TryAdd(data.Id, furInstance);
            return furInstance;
        }

        public void ReleaseFurniture(FurnitureController furRemove)
        {
            // Remove parent
            RoomHelper.RemoveNested(furRemove.NestedController.Parent, furRemove);

            // Remove all childs
            var childs = furRemove.NestedController.Childs;
            foreach (var child in childs)
            {
                if (FurInstanceByIdMap.TryGetKeyByValue(child, out var childFurId))
                {
                    FurInstanceByIdMap.Remove(childFurId);
                    FurnitureManager.Instance.Release(child);
                }
            }
            if (FurInstanceByIdMap.TryGetKeyByValue(furRemove, out var furRemoveId))
            {
                FurInstanceByIdMap.Remove(furRemoveId);
                FurnitureManager.Instance.Release(furRemove);
            }
        }

        public HashSet<FurnitureController> ReleaseFurnitureAndRetrieveChilds(FurnitureController furRemove)
        {
            // Remove parent
            RoomHelper.RemoveNested(furRemove.NestedController.Parent, furRemove);

            var childs = new HashSet<FurnitureController>();
            foreach (var child in furRemove.NestedController.Childs)
            {
                childs.Add(child);
            }

            if (FurInstanceByIdMap.TryGetKeyByValue(furRemove, out var furRemoveId))
            {
                FurInstanceByIdMap.Remove(furRemoveId);
                FurnitureManager.Instance.Release(furRemove);
            }
            return childs;
        }

        public void ReleaseAllFurniture()
        {
            foreach (var fur in FurInstanceByIdMap.Values)
            {
                fur.NestedController.OnRelease();
                FurnitureManager.Instance.Release(fur);
            }
            FurInstanceByIdMap.Clear();
        }

        public void ToggleWall(bool isShow)
        {
            if (!LandProgress.IsUnlock)
            {
                isShow = false;
            }
            _wallController.gameObject.SetActive(isShow);
        }

        public void ToggleFurniture(bool isShow)
        {
            if (!LandProgress.IsUnlock)
            {
                isShow = false;
            }
            var allFurList = FurInstanceByIdMap.Values;
            foreach (var fur in allFurList)
            {
                fur.gameObject.SetActive(isShow);
            }
        }

        public void PlaceCharacter()
        {
            var allFurList = FurInstanceByIdMap.Values.ToList();
            var isHaveDecorBox = _decorController.CurFurDecorBox != null;
            var isInsideBound = _bounds.IsInside(_characterController.Position);
            var isPositionValid = LandHelper.IsPositionValid(_characterController.Position, _characterController.Size,
                                                             _characterController.CharFurnitureController,
                                                             allFurList, _bounds.Max, _bounds.Min);
            if (isPositionValid && isHaveDecorBox)
            {
                isPositionValid = _characterController.Position == _decorController.DecorBoxDropPosition ? false : true;
            }
            if (isPositionValid && isInsideBound) return;

            var floorPositionList = LandHelper.GetFloorPositionList(_bounds.Min, _bounds.Max, _characterController.Size);
            var result = new List<Vector3>();

            foreach (var pos in floorPositionList)
            {
                if (LandHelper.IsPositionValid(pos, _characterController.Size,
                                               _characterController.CharFurnitureController, allFurList,
                                               _bounds.Max, _bounds.Min))
                {
                    result.Add(pos);
                }
            }

            // Remove invalid position from controller
            var invalidPosition = new List<Vector3>() { _unlockController.InvalidPosition };
            invalidPosition.AddRange(_repaintingController.InvalidPositionList);
            result.RemoveAll(item => invalidPosition.Contains(item));

            _characterController.SetPosition(result.RandomItem());
        }

        [Button]
        public void MoveLand(float x, float y)
        {
            var offsetX = 0.59f;
            var offsetY = 0.3363f;
            var baseWallY = 8.5f;
            var landPosition = new Vector2(x, y);

            _floorController.transform.position = new Vector2(landPosition.x * offsetX, landPosition.y * offsetY);
            _lockRoomTf.position = _floorController.transform.position;
            _wallController.transform.position = new Vector2(offsetX * landPosition.x, baseWallY + (offsetY * landPosition.y));
            GetLandBounds(_roomSize, landPosition);
        }

        private void GetLandBounds(Vector3 roomSize, Vector2 landPos)
        {
            var minPos = Vector2.zero;
            var newFurPos = LandHelper.GetOffsetFurniturePos(minPos, landPos);

            var minBound = new Vector3(newFurPos.x, newFurPos.y, 0);
            var maxBound = new Vector3(newFurPos.x + roomSize.x, newFurPos.y + roomSize.y, newFurPos.z + roomSize.z);
            _bounds = new LandBounds(minBound, maxBound);
        }

        private void UpdateLandSortingOrder(int sortingOrder)
        {
            _wallController.SetSortingOrder(Define.SortingOrder.WALL + sortingOrder);
            _floorController.SetSortingOrder(Define.SortingOrder.FLOOR + sortingOrder);
            _lockRoomSr.sortingOrder = Define.SortingOrder.FLOOR + sortingOrder;
            _unlockRoomSr.sortingOrder = Define.SortingOrder.FLOOR + sortingOrder;
            foreach (var fur in FurInstanceByIdMap.Values)
            {
                fur.SpriteRenderer.sortingOrder = fur.Config.IsCarpet ? Define.SortingOrder.CARPET :
                                                      fur.Config.IsWindow ? Define.SortingOrder.WINDOW : Define.SortingOrder.NORMAL;
                fur.SpriteRenderer.sortingOrder += sortingOrder;
            }
        }
    }

    [System.Serializable]
    public class LandBounds
    {
        public Vector3 Min;
        public Vector3 Max;

        public LandBounds(Vector3 min, Vector3 max)
        {
            Min = min;
            Max = max;
        }

        public bool IsInside(Vector2 pos)
        {
            return pos.x >= Min.x && pos.x <= Max.x
                && pos.y >= Min.y && pos.y <= Max.y;
        }

        public HashSet<Vector2Int> GetBorder()
        {
            var result = new HashSet<Vector2Int>();
            for (var x = Min.x; x <= Max.x; x++)
            {
                result.Add(new Vector2Int((int)x, (int)Min.y - 1));
            }

            for (var x = Max.x; x >= Min.x; x--)
            {
                result.Add(new Vector2Int((int)x, (int)Max.y + 1));
            }

            for (var y = Min.y; y <= Max.y; y++)
            {
                result.Add(new Vector2Int((int)Min.x - 1, (int)y));
            }

            for (var y = Max.y; y >= Min.y; y--)
            {
                result.Add(new Vector2Int((int)Max.x + 1, (int)y));
            }
            return result;
        }
    }

    public enum EHomeLandControllerType
    {
        None,
        Repainting,
        Unlock,
        Decor,
    }
}
