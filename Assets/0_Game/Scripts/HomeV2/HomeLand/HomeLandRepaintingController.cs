using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using System.Collections.Generic;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandRepaintingController : HomeLandSubController
    {
        public override EHomeLandControllerType Type => EHomeLandControllerType.Repainting;

        public List<Vector3> InvalidPositionList => _invalidPositionList;

        private List<Vector3> _invalidPositionList = new List<Vector3>();
        private Dictionary<int, int> _adsItemDic = new Dictionary<int, int>();

        public override void OnEnter()
        {
            base.OnEnter();
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
            SetData();
        }

        private void SetData()
        {
            var curUnlockIndex = HomeLandData.Instance.GetLandCurUnlockIndex(_homeLand.LandConfig.Id);
            _invalidPositionList.Clear();

            for (var i = 0; i <= curUnlockIndex; i++)
            {
                // Configs
                var index = i;
                var optionData = _homeLand.LandConfig.UnlockFurnitureOptions[index].FurnitureOption;
                var optionPrices = _homeLand.LandConfig.UnlockFurniturePrices[index].Prices;
                var boughtOptions = _homeLand.LandProgress.GetBoughtOptions(index);
                var furnitureData = optionData[0];
                var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furnitureData.ConfigId);

                var realFurniturePos = LandHelper.GetOffsetFurniturePos(furnitureData.Position, _homeLand.LandConfig.Position);
                var buttonPosition = RoomHelper.GetFurnitureCanvasPos(_homeLand.IsoWorld, realFurniturePos, furConfig.Config.Size);

                if (boughtOptions.Count > 0)
                    continue;

                _homeLand.WorldUI.ShowRepaintingFurButton(buttonPosition, OnRepaintingFur, OnRepaintingFur);
                _invalidPositionList.Add(realFurniturePos);

                void OnRepaintingFur()
                {
                    var popup = UIManager.Instance.Open<OrderOptionPopupUI>(Define.UIName.ORDER_OPTION_POPUP);
                    var adsItemIndex = GetAdsItemOptionIndex(index, boughtOptions);
                    var userConfirmedFurOption = GetConfirmedFurOption(index);

                    popup.SetFurnitureOptionsWithPrice(adsItemIndex, optionData, optionPrices, boughtOptions, OnSelectFurniture, OnConfirmFurniture);
                    popup.ActiveItemOrderOptionAtIndex(userConfirmedFurOption);
                    popup.ShowButtonClose(OnClosePopup);

                    void OnSelectFurniture(int optionSelected)
                    {
                        var furData = optionData[optionSelected];
                        var furConfig = FurnitureManager.Instance.GetFurnitureConfig(furData.ConfigId);
                        AudioManager.PlaySound(ESound.Select);
                        SelectFurniture(index, furData);
                        popup.NormalizeAllItems();
                    }

                    void OnConfirmFurniture(int optionConfirmed)
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
                        _homeLand.SaveLandData();
                        _homeLand.OnRefresh();
                        popup.CloseWithAnimation().Forget();
                    }

                    void OnClosePopup()
                    {
                        _homeLand.WorldUI.ShowAllRepaintingButton();
                        if (optionData.IsIndexOutOfList(userConfirmedFurOption))
                        {
                            RemoveTempFur(index);
                            return;
                        }

                        var furData = optionData[userConfirmedFurOption];
                        SelectFurniture(index, furData);
                    }
                }
            }
        }

        private void SelectFurniture(int index, RoomFurnitureData furData)
        {
            var optionMapping = _homeLand.FurnitureOptionMapping;
            var childs = new HashSet<FurnitureController>();
            if (optionMapping.ContainsKey(index) && optionMapping[index] != null)
            {
                childs = _homeLand.ReleaseFurnitureAndRetrieveChilds(optionMapping[index]);
            }
            var furInstance = _homeLand.SpawnFurniture(furData);
            foreach (var child in childs)
            {
                RoomHelper.RemoveNested(child.NestedController.Parent, child);
                RoomHelper.NestFurniture(furInstance, child, true);
            }
            optionMapping[index] = furInstance;
        }

        private int GetAdsItemOptionIndex(int index, List<int> boughtOptions)
        {
            var maxOption = 3;
            if (boughtOptions.Count == maxOption)
                return -1;

            if (_adsItemDic.ContainsKey(index))
            {
                return _adsItemDic[index];
            }
            var retryCount = 0;
            var rndAdsIndex = UnityEngine.Random.Range(0, maxOption);
            while (boughtOptions.Contains(rndAdsIndex))
            {
                rndAdsIndex = UnityEngine.Random.Range(0, maxOption);
                retryCount++;
                if (retryCount > 100)
                {
                    break;
                }
            }
            _adsItemDic.Add(index, rndAdsIndex);
            return rndAdsIndex;
        }

        private void RemoveTempFur(int index)
        {
            var furnitureMapping = _homeLand.FurnitureOptionMapping;
            if (furnitureMapping.ContainsKey(index) && furnitureMapping[index] != null)
            {
                _homeLand.ReleaseFurniture(furnitureMapping[index]);
            }
        }

        private void ConfirmFurniture(int index, int optionConfirmed, string furId)
            => HomeLandData.Instance.UnlockNewLandFur(_homeLand.LandConfig.Id, index, optionConfirmed, furId);

        private int GetConfirmedFurOption(int index)
            => HomeLandData.Instance.GetLandUnlockFurOption(_homeLand.LandConfig.Id, index);
    }
}
