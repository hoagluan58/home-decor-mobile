using System;
using JSAM;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OutfitItemUI : MonoBehaviour
    {
        #region PARAM

        public static Action CheckEquipEvent;

        [Header("UI")]
        [SerializeField] private Image _iconImage;
        [SerializeField] private Button _selfButton;
        [SerializeField] private GameObject _goHighlight;
        [SerializeField] private GameObject _goAds;
        [SerializeField] private GameObject _goDiamond;
        [SerializeField] private TextMeshProUGUI _textDiamondCost;

        [SerializeField] private OutfitConfigData _currentConfigData;
        private bool _isUnlocked;
        public OutfitConfigData Config => _currentConfigData;

        private DressUpData _dressUpData;
        private CharacterOutfitControllerUI _characterOutfitController;

        #endregion

        #region UNITY METHODS

        private void Awake()
        {
            _dressUpData = DressUpData.Instance;
        }

        private void OnEnable()
        {
            _selfButton.onClick.AddListener(OnItemClicked);
            CheckEquipEvent += CheckIsEquip;
        }

        private void OnDisable()
        {
            _selfButton.onClick.RemoveListener(OnItemClicked);
            CheckEquipEvent -= CheckIsEquip;
        }

        #endregion

        #region FEATURE METHODS

        public void OnInit(OutfitConfigData configData, CharacterOutfitControllerUI characterOutfitController)
        {
            // Load configs
            if (configData == null) return;

            transform.name = configData.Id;
            _currentConfigData = configData;
            _iconImage.sprite = configData.IconSprite;
            _characterOutfitController = characterOutfitController;
            _textDiamondCost.text = $"{configData.Price}";

            _isUnlocked = _dressUpData.CheckIsUnlock(_currentConfigData.Id);
            _goAds.SetActive(!_isUnlocked && _currentConfigData.UnlockType == UnlockType.Ads);
            _goDiamond.SetActive(!_isUnlocked && _currentConfigData.UnlockType == UnlockType.Diamond);

            ActiveHighlight(_characterOutfitController.CurrentOutfitData.Find(x => x.OutfitType == configData.OutfitType &&
                                                                                   x.OutfitId == configData.Id) != null);
        }

        public bool Unlock()
        {
            switch (Config.UnlockType)
            {
                case UnlockType.Ads:
                    // TODO: Add ads
                    _isUnlocked = true;
                    _goAds.SetActive(!_isUnlocked);
                    DressUpData.Instance.UnlockOutfit(Config.Id);
                    return true;

                case UnlockType.Diamond:
                    float diamond = UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond);
                    if (diamond < _currentConfigData.Price) return false;

                    float value = diamond - _currentConfigData.Price;
                    UserData.Instance.ModifyCurrencyDic(ECurrencyType.Diamond, -value);
                    _isUnlocked = true;
                    _goDiamond.SetActive(!_isUnlocked);
                    DressUpData.Instance.UnlockOutfit(Config.Id);
                    return true;
            }
            return false;
        }

        private void CheckIsEquip()
        {
            CharacterOutfitData characterOutfitData =
                _characterOutfitController.CurrentOutfitData.Find(x => x.OutfitType == _currentConfigData.OutfitType);
            if (characterOutfitData == null) return;

            ActiveHighlight(characterOutfitData.OutfitId == _currentConfigData.Id);
        }

        private void ActiveHighlight(bool isOn)
        {
            _goHighlight.SetActive(isOn);
        }

        #endregion

        #region ONCLICK METHODS

        public void OnItemClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            if (_isUnlocked)
            {
                _characterOutfitController.PlayAnimEquipOutfit();
                _characterOutfitController.ApplyOutfit(Config.OutfitType, Config.Id);
            }
            else if (Unlock())
            {
                _characterOutfitController.PlayAnimEquipOutfit();
                _characterOutfitController.ApplyOutfit(Config.OutfitType, Config.Id);
            }

            //CheckEquipEvent?.Invoke();
        }

        #endregion
    }
}