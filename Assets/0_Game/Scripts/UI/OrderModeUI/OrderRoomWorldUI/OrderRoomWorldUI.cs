using Cysharp.Threading.Tasks;
using DG.Tweening;
using IsoTools;
using JSAM;
using Redcode.Extensions;
using NFramework;
using System;
using UnityEngine;
using UnityEngine.Pool;
using System.Collections.Generic;

namespace YoyoDesign
{
    public class OrderRoomWorldUI : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private RepaintingButton _pfRemoveOldFurBtn;
        [SerializeField] private RepaintingButton _pfRepaintingBtn;
        [SerializeField] private Transform _tfRemoveOldFurBtnParent;
        [SerializeField] private Transform _tfRepaintingBtnParent;
        [SerializeField] private Transform _tfGroup2Repainting;
        [SerializeField] private Transform _tfUnboxBtnParent;
        [SerializeField] private Transform _tfTierEmojiParent;
        [SerializeField] private RepaintingButton _btnChangeWall;
        [SerializeField] private RepaintingButton _btnChangeFloor;
        [SerializeField] private GameObject _goTierEmoji;
        [SerializeField] private OrderRoomTutorialWorldUI _tutorialUI;

        private List<FurnitureController> _furShowEmoji = new List<FurnitureController>();
        private IsoWorld _isoWorld;
        private OrderModeRoomConfigData _roomConfig;
        private ObjectPool<RepaintingButton> _poolRemoveOldFurBtn;
        private ObjectPool<RepaintingButton> _poolRepaintingPlaceBtn;
        private ObjectPool<RepaintingButton> _poolUnboxPlaceBtn;
        private ObjectPool<GameObject> _poolTierEmoji;

        public Canvas Canvas => _canvas;
        public OrderRoomTutorialWorldUI TutorialUI => _tutorialUI;

        private void Start()
        {
            _canvas.worldCamera = Camera.main;
            _poolRemoveOldFurBtn = new(
                () => Instantiate(_pfRemoveOldFurBtn, _tfRemoveOldFurBtnParent),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _poolRepaintingPlaceBtn = new(
                () => Instantiate(_pfRepaintingBtn, _tfGroup2Repainting),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _poolUnboxPlaceBtn = new(
                () => Instantiate(_pfRepaintingBtn, _tfUnboxBtnParent),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _poolTierEmoji = new(
                () => Instantiate(_goTierEmoji, _tfTierEmojiParent),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
            _isoWorld = OrderRoomController.Instance.IsoWorld;
        }

        private void OnEnable() => Init();

        private void OnButtonChangeFloorClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            var floorDatas = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.FloorOptions;
            ToggleRepaintingGroup(false);
            ToggleUnboxFurGroup(false);
            UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP).SetFloorOptions(floorDatas, () =>
            {
                ToggleRepaintingGroup(true);
                ToggleUnboxFurGroup(true);

                var isDoneUnbox = OrderRoomController.Instance.UnboxFurnitureState.DoneUnboxAllFur;
                if (isDoneUnbox)
                {
                    ToggleGroup2Repainting(true);
                }
            });
        }

        private void OnButtonChangeWallClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            var wallDatas = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.WallOptions;
            ToggleRepaintingGroup(false);
            ToggleUnboxFurGroup(false);
            UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP).SetWallOptions(wallDatas, () =>
            {
                ToggleRepaintingGroup(true);

                var isDoneUnbox = OrderRoomController.Instance.UnboxFurnitureState.DoneUnboxAllFur;
                if (isDoneUnbox)
                {
                    ToggleGroup2Repainting(true);
                    ToggleUnboxFurGroup(true);
                }
            });
        }

        public void ToggleRepaintingGroup(bool value) => _tfRepaintingBtnParent.gameObject.SetActive(value);

        public void ToggleGroup2Repainting(bool value) => _tfGroup2Repainting.gameObject.SetActive(value);

        public void ToggleUnboxFurGroup(bool value) => _tfUnboxBtnParent.gameObject.SetActive(value);

        public void SpawnOldRemoveFurButton(Vector3 furPos, Vector3 furSize, Action onClickRemoveFur = null)
        {
            var btn = _poolRemoveOldFurBtn.Get();
            btn.transform.position = GetFurnitureCanvasPos(furPos, furSize);
            btn.SetData(new RepaintingButton.ButtonModel(OnClickRemoveFur, OnClickRemoveFur));

            void OnClickRemoveFur()
            {
                onClickRemoveFur?.Invoke();
                _poolRemoveOldFurBtn.Release(btn);
            }
        }

        public void SetDataRepainting()
        {
            var datas = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.RepaintingOptions;

            _btnChangeWall.gameObject.SetActive(true);
            _btnChangeFloor.gameObject.SetActive(true);

            for (var i = 0; i < datas.Count; i++)
            {
                var index = i;
                var optionData = datas[index].FurnitureOption;
                var btn = _poolRepaintingPlaceBtn.Get();
                var furConfig = FurnitureManager.Instance.GetFurnitureConfig(optionData[0].ConfigId);

                btn.SetData(new RepaintingButton.ButtonModel(OnButtonRepaintingClicked, OnButtonRepaintingClicked));
                btn.transform.position = GetFurnitureCanvasPos(optionData[0].Position, furConfig.Config.Size);

                void OnButtonRepaintingClicked()
                {
                    var popup = UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP);
                    var adsItemIndex = OrderModeManager.Instance.GetAdsItemOptionAtIndex(index);
                    ToggleRepaintingGroup(false);
                    ToggleUnboxFurGroup(false);
                    popup.SetFurnitureOptions(adsItemIndex, optionData,
                    onSelectFurniture: (optionSelected) =>
                    {
                        var furData = optionData[optionSelected];
                        var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furData.ConfigId);
                        AudioManager.PlaySound(ESound.Select);
                        popup.NormalizeAllItems();
                        OrderRoomController.Instance.RepaintingRoomState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
                    },
                    onConfirmFurniture: (optionConfirmed) =>
                    {
                        var furData = optionData[optionConfirmed];
                        AudioManager.PlaySound(ESound.Select);
                        ToggleRepaintingGroup(true);
                        ToggleUnboxFurGroup(true);
                        OrderRoomController.Instance.RepaintingRoomState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
                        OrderRoomController.Instance.RepaintingRoomState.ConfirmSetFurniture(index, optionConfirmed);
                        popup.CloseWithAnimation().Forget();
                    });

                    var confirmedOption = OrderRoomController.Instance.RepaintingRoomState.GetOptionConfirmedAtIndex(index);
                    popup.ActiveItemOrderOptionAtIndex(confirmedOption, true);
                }
            }
        }

        public void SetDataUnboxFurniture()
        {
            var datas = OrderModeManager.Instance.GetCurOrderRoomConfigData().OrderOptions.UnboxOptions;

            for (var i = 0; i < datas.Count; i++)
            {
                var index = i;
                var optionData = datas[index].FurnitureOption;
                var btn = _poolUnboxPlaceBtn.Get();
                var furConfig = FurnitureManager.Instance.GetFurnitureConfig(optionData[0].ConfigId);

                btn.SetData(new RepaintingButton.ButtonModel(OnButtonRepaintingClicked, OnButtonRepaintingClicked), isDone: true);
                btn.transform.position = GetFurnitureCanvasPos(optionData[0].Position, furConfig.Config.Size);

                void OnButtonRepaintingClicked()
                {
                    var popup = UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP);
                    var startIndex = _roomConfig.UnboxStartIndex;
                    var adsItemIndex = OrderModeManager.Instance.GetAdsItemOptionAtIndex(index + startIndex);
                    ToggleUnboxFurGroup(false);
                    ToggleRepaintingGroup(false);
                    popup.SetFurnitureOptions(adsItemIndex, optionData,
                    onSelectFurniture: (optionSelected) =>
                    {
                        var furData = optionData[optionSelected];
                        var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furData.ConfigId);
                        AudioManager.PlaySound(ESound.Select);
                        popup.NormalizeAllItems();
                        OrderRoomController.Instance.UnboxFurnitureState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
                    },
                    onConfirmFurniture: (optionConfirmed) =>
                    {
                        var furData = optionData[optionConfirmed];
                        AudioManager.PlaySound(ESound.Select);
                        ToggleUnboxFurGroup(true);
                        ToggleRepaintingGroup(true);
                        OrderRoomController.Instance.UnboxFurnitureState.SelectFurniture(index, furData.ConfigId, furData.Position, furData.Direction);
                        OrderRoomController.Instance.UnboxFurnitureState.ConfirmSetFurniture(index, optionConfirmed);
                        popup.CloseWithAnimation().Forget();
                    });

                    var confirmedOption = OrderRoomController.Instance.UnboxFurnitureState.GetOptionConfirmedAtIndex(index);
                    popup.ActiveItemOrderOptionAtIndex(confirmedOption, true);
                }
            }
            ToggleUnboxFurGroup(false);
        }

        public async void InitEmojiTier(FurnitureController fur)
        {
            if (_furShowEmoji.Contains(fur))
                return;

            var obj = _poolTierEmoji.Get();
            obj.transform.position = GetFurnitureCanvasPos(fur.Position, fur.Config.Size);
            obj.transform.SetLocalScale(1f);
            obj.transform.DOScale(Vector3.zero, 0.2f).From().SetEase(Ease.OutExpo);

            _furShowEmoji.Add(fur);
            await UniTask.WaitForSeconds(1f, cancellationToken: destroyCancellationToken);

            obj.transform.DOScale(Vector3.zero, 0.2f).SetEase(Ease.OutExpo).OnComplete(() =>
            {
                _furShowEmoji.Remove(fur);
                Destroy(obj);
            });
        }

        private void Init()
        {
            _roomConfig = OrderModeManager.Instance.GetCurOrderRoomConfigData();
            _tfRemoveOldFurBtnParent.DestroyAllChildren();
            _tfGroup2Repainting.DestroyAllChildren();
            _tfUnboxBtnParent.DestroyAllChildren();
            _tfTierEmojiParent.DestroyAllChildren();
            _btnChangeFloor.gameObject.SetActive(false);
            _btnChangeWall.gameObject.SetActive(false);
            ToggleGroup2Repainting(false);
            ToggleUnboxFurGroup(false);
            _btnChangeFloor.SetData(new RepaintingButton.ButtonModel(OnButtonChangeFloorClicked, OnButtonChangeFloorClicked));
            _btnChangeWall.SetData(new RepaintingButton.ButtonModel(OnButtonChangeWallClicked, OnButtonChangeWallClicked));
        }

        private Vector3 GetFurnitureCanvasPos(Vector3 furPos, Vector3 furSize) => _isoWorld.IsoToScreen(furPos + furSize / 2);
    }
}
