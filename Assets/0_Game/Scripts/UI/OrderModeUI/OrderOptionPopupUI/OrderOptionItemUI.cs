using DG.Tweening;
using NFramework;
using Redcode.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderOptionItemUI : MonoBehaviour
    {
        [SerializeField] private Image _imgBg;
        [SerializeField] private Image _imgFur;
        [SerializeField] private RectTransform _rectImgFurHolder;
        [SerializeField] private Button _btnViewFur;
        [SerializeField] private Button _btnViewFurAds;
        [SerializeField] private Button _btnSelectAds;
        [SerializeField] private GameObject _goAdsGroup;
        [SerializeField] private Sprite _spriteNormal;
        [SerializeField] private Sprite _spriteSelected;
        [SerializeField] private Sprite _spriteAdsNormal;
        [SerializeField] private Sprite _spriteAdsSelect;

        [Header("SELECT")]
        [SerializeField] private Button _btnSelect;
        [SerializeField] private GameObject _goSelectText;
        [SerializeField] private GameObject _goCheckmark;

        [Header("PRICE")]
        [SerializeField] private Button _btnPrice;
        [SerializeField] private TextMeshProUGUI _priceTMP;
        [SerializeField] private TextMeshProUGUI _notEnoughDiamondTMP;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _handTutorialObject;

        private Sequence _sequenceNotEnoughDiamondText;
        private OrderOptionItemModel _model;
        public OrderOptionItemModel Model => _model;

        private void Awake()
        {
            _btnViewFur.onClick.AddListener(OnButtonViewFurClicked);
            _btnViewFurAds.onClick.AddListener(OnButtonViewFurAdsClicked);
            _btnSelect.onClick.AddListener(OnButtonSelectClicked);
            _btnSelectAds.onClick.AddListener(OnButtonSelectAdsClicked);
            _btnPrice.onClick.AddListener(OnButtonPriceClicked);
        }

        private void OnButtonSelectClicked()
        {
            _model.OnConfirm?.Invoke(_model.Index);
            ToggleActiveState();
        }

        private void OnButtonSelectAdsClicked()
        {
            _model.OnConfirm?.Invoke(_model.Index);
            ToggleActiveState();
        }

        private void OnButtonViewFurAdsClicked()
        {
            _model.OnSelect?.Invoke(_model.Index);
            ToggleActiveState();
        }

        private void OnButtonViewFurClicked()
        {
            _model.OnSelect?.Invoke(_model.Index);
            ToggleActiveState();
        }

        private void OnButtonPriceClicked()
        {
            if (!_model.IsEnough && !_model.IsAds)
            {
                VibrationManager.Vibrate(0.05f);
                ShowNotEnoughText();
            }
            _model.OnConfirm?.Invoke(_model.Index);
            ToggleActiveState();
        }

        public void SetData(OrderOptionItemModel model)
        {
            _model = model;
            var isNull = string.IsNullOrEmpty(_model.FurId);

            this.gameObject.SetActive(!isNull);
            if (isNull)
                return;

            _imgFur.sprite = _model.Sprite;
            _imgFur.SetNativeSize();
            _imgFur.rectTransform.FitTo(_rectImgFurHolder, 0f);
            _imgFur.rectTransform.SetEulerAngleY(_model.Direction == FurnitureDirection.Left ? 0 : 180);
            Normalize();
            ShowTutorial(false);
            ShowCheckmark(false);
        }

        public void Normalize()
        {
            _goAdsGroup.SetActive(_model.IsAds);
            _notEnoughDiamondTMP.transform.SetLocalScale(0f);
            _notEnoughDiamondTMP.SetColorA(0f);

            if (_model.IsAds)
            {
                _imgBg.sprite = _spriteAdsNormal;
                return;
            }
            _imgBg.sprite = _spriteNormal;

            _btnSelect.gameObject.SetActive(false);
            _btnPrice.gameObject.SetActive(false);

            switch (_model.Type)
            {
                case EOrderOptionItemType.Select:
                    _btnSelect.gameObject.SetActive(true);
                    //_goSelectText.SetActive(true);
                    //_goCheckmark.SetActive(false);
                    break;
                case EOrderOptionItemType.Price:
                    var colorCode = UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond) >= _model.Price ? Define.ColorCode.WHITE : Define.ColorCode.RED;
                    _btnPrice.gameObject.SetActive(true);
                    _priceTMP.text = $"<color={colorCode}>{_model.Price}</color>";
                    
                    break;
            }
        }

        public void ToggleActiveState()
        {
            _imgBg.sprite = _model.IsAds ? _spriteAdsSelect : _spriteSelected;
            switch (_model.Type)
            {
                case EOrderOptionItemType.Select:
                    //_goSelectText.SetActive(false);
                    //_goCheckmark.SetActive(true);
                    break;
                case EOrderOptionItemType.Price:
                    break;
            }
        }

        public void ShowCheckmark(bool isShow)
        {
            _goSelectText.SetActive(!isShow);
            _goCheckmark.SetActive(isShow);
        }

        public void ShowTutorial(bool isShow) => _handTutorialObject.SetActive(isShow);

        private void ShowNotEnoughText()
        {
            _sequenceNotEnoughDiamondText?.Kill();
            _notEnoughDiamondTMP.transform.SetLocalScale(0f);
            _notEnoughDiamondTMP.SetColorA(1f);
            _sequenceNotEnoughDiamondText?.Kill();
            _sequenceNotEnoughDiamondText = DOTween.Sequence();
            _sequenceNotEnoughDiamondText.Insert(0f, _notEnoughDiamondTMP.transform.DOScale(1f, 0.7f).SetEase(Ease.OutBack));
            _sequenceNotEnoughDiamondText.Insert(0.7f, _notEnoughDiamondTMP.DOFade(0f, 0.3f));
            _sequenceNotEnoughDiamondText.Insert(0.7f, _notEnoughDiamondTMP.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack));
        }

        public class OrderOptionItemModel
        {
            public int Index;
            public EOrderOptionItemType Type;
            public string FurId;
            public bool IsAds;
            public FurnitureDirection Direction;
            public int Price;
            public Sprite Sprite;
            public Action<int> OnSelect;
            public Action<int> OnConfirm;
            public bool IsEnough => UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond) >= Price;

            public OrderOptionItemModel(int index, EOrderOptionItemType type, string furId, bool isAds, FurnitureDirection direction, int price,
                                        Sprite sprite, Action<int> onSelect = null, Action<int> onConfirm = null)
            {
                Index = index;
                Type = type;
                FurId = furId;
                IsAds = isAds;
                Direction = direction;
                Price = price;
                Sprite = sprite;
                OnSelect = onSelect;
                OnConfirm = onConfirm;
            }

            public OrderOptionItemModel(int index, string id, bool isAds, FurnitureDirection direction, Sprite sprite, Action<int> onSelect = null, Action<int> onConfirm = null)
            {
                Index = index;
                FurId = id;
                IsAds = isAds;
                Direction = direction;
                Sprite = sprite;
                OnSelect = onSelect;
                OnConfirm = onConfirm;
            }

        }

        public enum EOrderOptionItemType
        {
            Select,
            Price,
        }
    }
}
