using DG.Tweening;
using JSAM;
using Redcode.Extensions;
using RotaryHeart.Lib.SerializableDictionary;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderProgressBarUI : MonoBehaviour
    {
        [SerializeField] private SerializableDictionaryBase<EOrderState, Sprite> _stateImageDic;
        [SerializeField] private Image _imgFillBar;
        [SerializeField] private Image _imgState;
        [SerializeField] private GameObject _goFullBar;
        [SerializeField] private Sprite _spriteStarOn;
        [SerializeField] private Sprite _spriteStarOff;
        [SerializeField] private List<Image> _starImageList;
        [SerializeField] private List<GameObject> _starParticleList;
        [SerializeField] private TextMeshProUGUI _txtState;
        [SerializeField] private Transform _tfStateBG;

        private int _oldStarCount = 0;
        private Tween _tweenFillBar;

        public bool IsBarTweening => _tweenFillBar.IsComplete();

        private void OnEnable()
        {
            Init();
            OrderRoomController.OnOrderStateChanged += OrderRoomController_OnOrderStateChanged;
            OrderCleanRoomState.OnItemCleaned += OrderCleanRoomController_OnTrashCleared;
            OrderRemoveOldFurState.OnItemCleaned += OrderCleanRoomController_OnTrashCleared;
            OrderUnboxFurnitureState.OnUnboxedFurniture += OrderUnboxFurnitureState_OnUnboxedFurniture;
            OrderRepaintingRoomState.OnRepaintedFurniture += OrderRepaintingRoomController_OnRepaintedFurniture;
            OrderDecorState.OnDecorFurPutOut += OrderDecorateController_OnDecorFurPutOut;
            OrderDecorState.OnDecorTierChanged += OrderDecorState_OnDecorTierChanged;
        }

        private void OnDisable()
        {
            OrderRoomController.OnOrderStateChanged -= OrderRoomController_OnOrderStateChanged;
            OrderCleanRoomState.OnItemCleaned -= OrderCleanRoomController_OnTrashCleared;
            OrderRemoveOldFurState.OnItemCleaned -= OrderCleanRoomController_OnTrashCleared;
            OrderUnboxFurnitureState.OnUnboxedFurniture -= OrderUnboxFurnitureState_OnUnboxedFurniture;
            OrderRepaintingRoomState.OnRepaintedFurniture -= OrderRepaintingRoomController_OnRepaintedFurniture;
            OrderDecorState.OnDecorFurPutOut -= OrderDecorateController_OnDecorFurPutOut;
        }

        private void OrderRoomController_OnOrderStateChanged(EOrderState state)
        {
            if (state == EOrderState.UnboxFurniture)
                return;

            UpdateStateImage(state);
            UpdateStateText(state);
            UpdateStar();
        }

        private void OrderCleanRoomController_OnTrashCleared(int curValue, int maxValue) => UpdateBarProgress(curValue / (float)maxValue);

        private void OrderRepaintingRoomController_OnRepaintedFurniture(int curValue, int maxValue) => UpdateBarProgress(curValue / (float)maxValue);

        private void OrderUnboxFurnitureState_OnUnboxedFurniture(int curValue, int maxValue) => UpdateBarProgress(curValue / (float)maxValue);

        private void OrderDecorateController_OnDecorFurPutOut(int curValue, int maxValue) => UpdateBarProgress(curValue / (float)maxValue);

        private void OrderDecorState_OnDecorTierChanged(float curValue) => UpdateBarProgress(curValue);

        private void Init()
        {
            _imgFillBar.fillAmount = 0;
            _oldStarCount = 0;
            UpdateStar();
        }

        private void UpdateStateImage(EOrderState state) => _imgState.sprite = _stateImageDic[state];

        private void UpdateBarProgress(float value)
        {
            var curFill = _imgFillBar.fillAmount;

            _tweenFillBar?.Kill();

            if (value == 0)
            {
                _goFullBar.SetActive(false);
                _imgFillBar.fillAmount = 0;
                UpdateStar();
                return;
            }

            _imgFillBar.fillAmount = curFill;
            _tweenFillBar = _imgFillBar.DOFillAmount(value, 0.3f).OnComplete(() =>
            {
                UpdateStar();
                _goFullBar.SetActive(value == 1f);
            }).SetEase(Ease.Linear);
        }

        private void UpdateStateText(EOrderState state)
        {
            _txtState.text = GetStateText(state);
            _tfStateBG.DOKill();
            _tfStateBG.SetLocalScale(1f);
            var sequence = DOTween.Sequence();
            sequence.Append(_tfStateBG.DOScale(new Vector3(0f, 0f, 0f), 0.5f).SetEase(Ease.OutExpo).From())
                .AppendInterval(2f)
                .Append(_tfStateBG.DOScale(Vector3.zero, 0.5f).SetEase(Ease.InExpo));
        }

        private void UpdateStar()
        {
            var value = _imgFillBar.fillAmount;
            var starCount = Mathf.FloorToInt(value * _starImageList.Count);
            _starParticleList.ForEach(x => x.SetActive(false));
            if (starCount > _oldStarCount)
            {
                AudioManager.PlaySound(ESound.ReachStar);
                VibrationManager.Vibrate(0.05f);
                _starParticleList[starCount - 1].SetActive(true);
            }
            _oldStarCount = starCount;
            _starImageList.ForEach(x => x.sprite = _spriteStarOff);
            for (var i = 0; i < starCount; i++)
            {
                _starImageList[i].sprite = _spriteStarOn;
            }
        }

        private string GetStateText(EOrderState state) => state switch
        {
            EOrderState.None => "",
            EOrderState.CleanTrash => "Garbage Sorting",
            EOrderState.RemoveOldFurniture => "Old Furniture Removing",
            EOrderState.Repainting => "Repainting",
            EOrderState.Decorate => "New Furniture Adding",
            _ => "",
        };
    }
}