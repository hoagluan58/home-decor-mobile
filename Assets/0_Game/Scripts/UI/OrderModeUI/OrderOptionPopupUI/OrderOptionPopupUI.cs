using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using System;
using System.Collections.Generic;
using System.Xml;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderOptionPopupUI : BaseUIView
    {
        public static event Action<bool> OnShownPopup;

        [SerializeField] private List<OrderOptionItemUI> _orderOptionItems;
        [SerializeField] private Button _closeButton;

        private void OnEnable()
        {
            _closeButton.gameObject.SetActive(false);
            PlayShowAnimation();
            OnShownPopup?.Invoke(true);
        }

        public override void OnClose()
        {
            base.OnClose();
            OnShownPopup?.Invoke(false);
        }

        public void SetWallOptions(List<string> wallDatas, Action onChangeWallCompleted = null)
        {
            for (var i = 0; i < wallDatas.Count; i++)
            {
                var optionIndex = i;
                var id = wallDatas[i];
                var config = AllConfig.Instance.WallConfigDic[id];
                var model = new OrderOptionItemUI.OrderOptionItemModel(optionIndex, id, false, FurnitureDirection.Left, config.Sprite, OnSelectWall, OnConfirmWall);
                _orderOptionItems[i].SetData(model);

                void OnSelectWall(int optionIndex)
                {
                    AudioManager.PlaySound(ESound.Select);
                    _orderOptionItems.ForEach(x => x.Normalize());
                    OrderRoomController.Instance.SetWall(id);
                }

                async void OnConfirmWall(int optionIndex)
                {
                    AudioManager.PlaySound(ESound.Select);
                    OrderRoomController.Instance.SetWall(id);
                    OrderRoomController.Instance.RepaintingRoomState.ConfirmSetWall();
                    await CloseWithAnimation();
                    onChangeWallCompleted?.Invoke();
                }
            }
        }

        public void SetFloorOptions(List<string> floorDatas, Action onChangeFloorCompleted = null)
        {
            for (var i = 0; i < floorDatas.Count; i++)
            {
                var optionIndex = i;
                var id = floorDatas[i];
                var config = AllConfig.Instance.FloorConfigDic[id];
                var model = new OrderOptionItemUI.OrderOptionItemModel(optionIndex, id, false, FurnitureDirection.Left, config.Sprite, OnSelectFloor, OnConfirmFloor);
                _orderOptionItems[i].SetData(model);

                void OnSelectFloor(int opitonIndex)
                {
                    AudioManager.PlaySound(ESound.Select);
                    _orderOptionItems.ForEach(x => x.Normalize());
                    OrderRoomController.Instance.SetFloor(id);
                }

                async void OnConfirmFloor(int optionIndex)
                {
                    AudioManager.PlaySound(ESound.Select);
                    OrderRoomController.Instance.SetFloor(id);
                    OrderRoomController.Instance.RepaintingRoomState.ConfirmSetFloor();
                    await CloseWithAnimation();
                    onChangeFloorCompleted?.Invoke();
                }
            }
        }

        public void SetFurnitureOptions(int adsItemIndex, List<RoomFurnitureData> furDatas, Action<int> onSelectFurniture = null, Action<int> onConfirmFurniture = null)
        {
            for (var i = 0; i < furDatas.Count; i++)
            {
                var optionIndex = i;
                var isAds = i == adsItemIndex;
                var data = furDatas[i];
                var furConfig = FurnitureManager.Instance.GetFurnitureConfig(data.ConfigId);
                var model = new OrderOptionItemUI.OrderOptionItemModel(optionIndex, data.ConfigId, isAds, furConfig.Config.BaseDirection, furConfig.Config.Sprite, onSelectFurniture, onConfirmFurniture);
                _orderOptionItems[i].SetData(model);
            }
        }

        public void SetFurnitureOptionsWithPrice(int adsItemIndex, List<RoomFurnitureData> furDatas, List<int> prices, List<int> unlockedList, Action<int> onSelectFurniture = null, Action<int> onConfirmFurniture = null)
        {
            for (var i = 0; i < furDatas.Count; i++)
            {
                var optionIndex = i;
                var isUnlocked = unlockedList.Contains(optionIndex);
                var isAds = i == adsItemIndex && !isUnlocked;
                var data = furDatas[i];
                var furConfig = FurnitureManager.Instance.GetFurnitureConfig(data.ConfigId);
                var price = prices[i];
                var type = isUnlocked ? OrderOptionItemUI.EOrderOptionItemType.Select : OrderOptionItemUI.EOrderOptionItemType.Price;
                var model = new OrderOptionItemUI.OrderOptionItemModel(optionIndex,
                    type, data.ConfigId, isAds, furConfig.Config.BaseDirection,
                    price, furConfig.Config.Sprite, onSelectFurniture, onConfirmFurniture);

                _orderOptionItems[i].SetData(model);
            }
        }

        public void NormalizeAllItems() => _orderOptionItems.ForEach(x => x.Normalize());

        private async void PlayShowAnimation()
        {
            UIManager.Instance.DisableInteract(this);

            _orderOptionItems.ForEach(x => x.transform.SetLocalScale(0f));
            foreach (var item in _orderOptionItems)
            {
                await item.transform.DOScale(Vector3.one, 0.15f).SetEase(Ease.InOutQuad);
            }

            UIManager.Instance.EnableInteract(this);
        }

        public async UniTask CloseWithAnimation()
        {
            UIManager.Instance.DisableInteract(this);
            foreach (var item in _orderOptionItems)
            {
                await item.transform.DOScale(Vector3.zero, 0.15f).SetEase(Ease.InOutQuad);
            }
            UIManager.Instance.EnableInteract(this);
            CloseSelf();
        }

        public void ActiveFirstItemOrderOption(out int indexFur)
        {
            indexFur = 0;
            for (int i = 0; i < _orderOptionItems.Count; i++)
            {
                var item = _orderOptionItems[i];
                if (!item.Model.IsAds)
                {
                    item.ToggleActiveState();
                    indexFur = i;
                    break;
                }
            }
        }

        public void ActiveItemOrderOptionAtIndex(int index, bool isCheckMark = false)
        {
            if (_orderOptionItems.IsIndexOutOfList(index))
                return;

            _orderOptionItems[index].ToggleActiveState();
            _orderOptionItems[index].ShowCheckmark(isCheckMark);
        }

        public void ShowButtonClose(Action onCloseButtonClicked = null)
        {
            _closeButton.gameObject.SetActive(true);
            _closeButton.onClick.RemoveAllListeners();
            _closeButton.onClick.AddListener(async () =>
            {
                AudioManager.PlaySound(ESound.Click);
                onCloseButtonClicked?.Invoke();
                await CloseWithAnimation();
            });
        }

        public void ShowTutorial(bool isShow)
        {
            _orderOptionItems.ForEach(x => x.ShowTutorial(false));

            foreach (var item in _orderOptionItems)
            {
                if (item.Model.IsAds) continue;
                item.ShowTutorial(isShow);
                break;
            }
        }
    }
}
