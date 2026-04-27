using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandUnlockFurController : HomeLandSubController
    {
        public override EHomeLandControllerType Type => EHomeLandControllerType.Unlock;

        public Vector3 InvalidPosition => _invalidPosition;

        private Vector3 _invalidPosition = new Vector3();
        private Dictionary<int, int> _adsItemDic = new Dictionary<int, int>();

        public override void Init(HomeLand homeLand)
        {
            base.Init(homeLand);
            InitFurOptionMapping();
        }

        public override void OnEnter()
        {
            base.OnEnter();
            InitFurOptionMapping();
            SetData();
        }

        public override void OnExit()
        {
            base.OnExit();
            _homeLand.WorldUI.ReleaseButtons();
        }

        public override void OnRefresh()
        {
            base.OnRefresh();
            InitFurOptionMapping();
            SetData();
        }

        private void SetData()
        {
            var index = HomeLandData.Instance.GetLandNextUnlockIndex(_homeLand.LandConfig.Id);

            if (_homeLand.LandConfig.UnlockFurnitureOptions.IsIndexOutOfList(index))
                return;

            // Configs
            var optionData = _homeLand.LandConfig.UnlockFurnitureOptions[index].FurnitureOption;
            var optionPrices = _homeLand.LandConfig.UnlockFurniturePrices[index].Prices;
            var starPrice = _homeLand.LandConfig.UnlockFurnitureStarPrices[index];
            var boughtOptions = _homeLand.LandProgress.GetBoughtOptions(index);
            var furnitureData = optionData[0];
            var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furnitureData.ConfigId);

            var realFurniturePosition = LandHelper.GetOffsetFurniturePos(furnitureData.Position, _homeLand.LandConfig.Position);
            var buttonPosition = RoomHelper.GetFurnitureCanvasPos(_homeLand.IsoWorld, realFurniturePosition, furConfig.Config.Size);

            _invalidPosition = realFurniturePosition;
            _homeLand.WorldUI.ShowUnlockFurButton(buttonPosition, starPrice, OnUnlockFur, OnUnlockFur);

            void OnUnlockFur()
            {
                var userStar = UserData.Instance.GetCurrencyAmount(ECurrencyType.Star);
                if (userStar < starPrice)
                    return;

                UserData.Instance.ModifyCurrencyDic(ECurrencyType.Star, -starPrice);

                var popup = UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP);
                var adsItemIndex = GetAdsItemOptionIndex(index);

                popup.SetFurnitureOptionsWithPrice(adsItemIndex, optionData, optionPrices, boughtOptions, OnSelectFurniture, OnConfirmFurniture);
                popup.ActiveItemOrderOptionAtIndex(GetConfirmedFurOption(index));
                if (UserData.Instance.CurTutorialIndex >= Define.TutorialIndex.DONE_UNLOCK_FURNITURE)
                {
                    popup.ShowButtonClose(OnCloseButtonClicked);
                }
                popup.ShowTutorial(UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_DECOR_FURNITURE);

                void OnSelectFurniture(int optionSelected)
                {
                    var furData = optionData[optionSelected];
                    var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furData.ConfigId);
                    AudioManager.PlaySound(ESound.Select);
                    SelectFurniture(index, furData);
                    popup.NormalizeAllItems();
                    popup.ShowTutorial(false);
                }

                async void OnConfirmFurniture(int optionConfirmed)
                {
                    var furData = optionData[optionConfirmed];
                    var price = optionPrices[optionConfirmed];
                    var userDiamond = UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond);

                    AudioManager.PlaySound(ESound.Select);

                    if (adsItemIndex != optionConfirmed)
                    {
                        if (userDiamond < price) return;
                        if (!boughtOptions.Contains(optionConfirmed))
                        {
                            UserData.Instance.ModifyCurrencyDic(ECurrencyType.Diamond, -price);
                        }
                    }

                    SelectFurniture(index, furData);
                    ConfirmFurniture(index, optionConfirmed, furData.Id);
                    popup.CloseWithAnimation().Forget();

                    var needShowTutorial = UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_DECOR_FURNITURE;

                    if (needShowTutorial)
                    {
                        _homeLand.WorldUI.SetActive(false);
                        await UniTask.WaitForSeconds(0.5f, cancellationToken: destroyCancellationToken);
                        var popup = UIManager.Instance.Open(Define.UIName.GAME_LOOP_POPUP);
                        await UniTask.WaitUntil(() => popup == null || !popup.gameObject.activeSelf);
                        UserData.Instance.CurTutorialIndex = Define.TutorialIndex.DONE_UNLOCK_FURNITURE;
                    }

                    _homeLand.OnRefresh();
                    _homeLand.SaveLandData();
                }

                void OnCloseButtonClicked()
                {
                    RemoveTempFur(index);
                    ConfirmFurniture(index, -1, "");
                    _homeLand.OnRefresh();
                    _homeLand.SaveLandData();
                }
            }
        }

        private void InitFurOptionMapping()
        {
            var unlockedFurDic = HomeLandData.Instance.GetLandUnlockFurData(_homeLand.LandConfig.Id);

            if (unlockedFurDic == null)
                return;

            foreach (var pair in unlockedFurDic)
            {
                if (pair.Value.OptionUsing == -1)
                    continue;

                _homeLand.FurnitureOptionMapping[pair.Key] = _homeLand.FurInstanceByIdMap[pair.Value.FurId];
            }
        }

        private void RemoveTempFur(int index)
        {
            var furnitureMapping = _homeLand.FurnitureOptionMapping;
            if (furnitureMapping.ContainsKey(index) && furnitureMapping[index] != null)
            {
                _homeLand.ReleaseFurniture(furnitureMapping[index]);
            }
        }
        private void SelectFurniture(int index, RoomFurnitureData furData)
        {
            RemoveTempFur(index);
            var furnitureMapping = _homeLand.FurnitureOptionMapping;
            var furInstance = _homeLand.SpawnFurniture(furData);
            furnitureMapping[index] = furInstance;
        }

        private int GetAdsItemOptionIndex(int index)
        {
            if (_adsItemDic.ContainsKey(index))
            {
                return _adsItemDic[index];
            }
            var rndAdsIndex = UnityEngine.Random.Range(0, 3);
            _adsItemDic.Add(index, rndAdsIndex);
            return rndAdsIndex;
        }

        private void ConfirmFurniture(int index, int optionConfirmed, string furId)
            => HomeLandData.Instance.UnlockNewLandFur(_homeLand.LandConfig.Id, index, optionConfirmed, furId);

        private int GetConfirmedFurOption(int index)
            => HomeLandData.Instance.GetLandUnlockFurOption(_homeLand.LandConfig.Id, index);
    }
}
