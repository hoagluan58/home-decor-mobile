using JSAM;
using NFramework;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class DressUpMenuUI : BaseUIView
    {
        #region PARAM

        [Header("=====", order = 0)]
        [Header("TOP PANEL")]
        [SerializeField] private CharacterOutfitControllerUI _characterOutfitControllerUI;
        [SerializeField] private ParticleSystem _applyOutfitFx;
        [SerializeField] private Button _buttonHome;
        [SerializeField] private GameObject _goSkinColor;
        [SerializeField] private Transform _tfPrefabSkinColor;
        [Header("BOT PANEL")]
        [SerializeField] private Button _saveButton;
        [SerializeField] private Button _buttonDelete;
        [SerializeField] private Button _buttonSetting;
        [SerializeField] private ScrollRect _outfitSelectButtonsScrollRect;
        [SerializeField] private List<OutfitTypeSelectButton> _outfitTypeSelectButtons;
        [SerializeField] private ScrollRect _itemsScrollRect;
        [SerializeField] private OutfitControllerView _outfitControllerView;
        [SerializeField] private OutfitItemUI _transformOutfitPrefab;
        [SerializeField] private ScrollRect _scrollRectOutfit;


        private List<OutfitItemUI> _outfitItems = new();
        private ObjectPool<OutfitItemUI> _outfitItemPool;

        #endregion

        #region UNITY METHODS

        private void Awake()
        {
            LoadSkinColor();
            // Init pool.
            _outfitItemPool = new(
                () => Instantiate(_transformOutfitPrefab, _scrollRectOutfit.content),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
        }

        private void Start()
        {

        }

        private void OnEnable()
        {
            OutfitTypeSelectButton.ClickEvent.AddListener(OnOutfitTypeSelectButtonClicked);

            _buttonHome.onClick.AddListener(OnBtnHome);
            _buttonDelete.onClick.AddListener(OnBtnDeleteCurOutfit);
            _saveButton.onClick.AddListener(OnSaveButtonClicked);
            _buttonSetting.onClick.AddListener(OnBtnSetting);

        }

        private void OnDisable()
        {
            OutfitTypeSelectButton.ClickEvent.RemoveListener(OnOutfitTypeSelectButtonClicked);

            _buttonHome.onClick.RemoveListener(OnBtnHome);
            _buttonDelete.onClick.RemoveListener(OnBtnDeleteCurOutfit);
            _saveButton.onClick.RemoveListener(OnSaveButtonClicked);
            _buttonSetting.onClick.RemoveListener(OnBtnSetting);

        }

        #endregion // UNITY METHODS

        #region FEATURE METHODS
        private void LoadSkinColor()
        {
            DressUpManager.Instance.GetDressUpConfigByType(OutfitType.Skin).ForEach(x =>
            {
                OutfitItemUI outfitItem = Instantiate(_tfPrefabSkinColor, _goSkinColor.transform).GetComponent<OutfitItemUI>();
                outfitItem.OnInit(x, _characterOutfitControllerUI);
            });
        }

        private void ShowCategoryPool(OutfitType outfitType)
        {
            //Hide outfit item
            _outfitItems.ForEach(x => _outfitItemPool.Release(x));
            _outfitItems.Clear();

            //Active Skin color when select face
            _goSkinColor.SetActive(outfitType == OutfitType.Face);

            if (outfitType == OutfitType.Face) _scrollRectOutfit.GetComponent<RectTransform>().SetTop(145);
            else _scrollRectOutfit.GetComponent<RectTransform>().SetTop(20);

            //Load outfitItem scroller
            foreach(var item in DressUpManager.Instance.GetDressUpConfigByType(outfitType))
            {
                OutfitItemUI outfitItem = _outfitItemPool.Get();
                outfitItem.transform.SetAsLastSibling();
                outfitItem.OnInit(item, _characterOutfitControllerUI);
                _outfitItems.Add(outfitItem);
            }

        }

        private void ShowCategory(OutfitType outfitType)
        {
            //Active Skin color when select face
            _goSkinColor.SetActive(outfitType == OutfitType.Face);

            if (outfitType == OutfitType.Face)
                _itemsScrollRect.GetComponent<RectTransform>().SetTop(145);
            else _itemsScrollRect.GetComponent<RectTransform>().SetTop(20);

            //Load EnhanceScroll Data
            _outfitControllerView.LoadData(DressUpManager.Instance.GetDressUpConfigByType(outfitType));
        }

        #endregion

        #region BASE VIEW METHODS

        public override void OnOpen()
        {
            base.OnOpen();

            _characterOutfitControllerUI.LoadCurrentCharacter();

            //Reset OutfitItemButton
            //ShowCategory(OutfitType.Hair);
            ShowCategoryPool(OutfitType.Hair);

            _outfitSelectButtonsScrollRect.horizontalNormalizedPosition = 0;
        }

        #endregion

        #region ONCLICK METHODS

        private void OnBtnDeleteCurOutfit()
        {
            AudioManager.PlaySound(ESound.Click);
            _characterOutfitControllerUI.LoadCurrentOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
        }

        private void OnOutfitTypeSelectButtonClicked(int index, OutfitType outfitType)
        {
            // Fit the scroll rect
            //_outfitSelectButtonsScrollRect.horizontalNormalizedPosition =
            //(index < _outfitTypeSelectButtons.Count / 2f) ? 1 : 0;

            // Reload list outfits

            ShowCategoryPool(outfitType);
            //ShowCategory(outfitType);
        }

        private void OnSaveButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            DressUpData.Instance.SaveOutfit(_characterOutfitControllerUI.CurrentOutfitData);
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        private void OnBtnHome()
        {
            AudioManager.PlaySound(ESound.Click);
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        private void OnBtnSetting()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        #endregion
    }
}