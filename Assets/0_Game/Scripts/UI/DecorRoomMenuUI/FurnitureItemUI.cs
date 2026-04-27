using JSAM;
using NFramework;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class FurnitureItemUI : MonoBehaviour
    {
        public static UnityEvent<FurnitureItemUI> ClickEvent = new();

        [SerializeField] private Image _furImage;
        [SerializeField] private Image _highlightOverlayImg;
        [SerializeField] private Button _itemButton;
        [SerializeField] private Image _adsLockPanel;
        [SerializeField] private Image _priceLockPanel;
        [SerializeField] private TextMeshProUGUI _priceTMP;

        private string _furId;
        private EFurnitureSubCategory _subCategory;
        private FurnitureUnlockType _unlockType;
        private bool _isSpecial;
        private bool _isLock;
        private float _price;

        public string FurId => _furId;
        public EFurnitureSubCategory SubCategory => _subCategory;
        public FurnitureUnlockType UnlockType => _unlockType;
        public bool IsLock => _isLock;
        public float Price => _price;

        private void OnEnable()
        {
            _itemButton.onClick.AddListener(OnItemClick);
        }

        private void OnDisable()
        {
            _itemButton.onClick.RemoveListener(OnItemClick);
        }

        public void LoadConfig(string id, Sprite sprite, FurnitureDirection direction, EFurnitureSubCategory subCategory,
            FurnitureUnlockType unlockType, float price)
        {
            _furId = id;
            _subCategory = subCategory;
            _unlockType = unlockType;
            _price = price;
            
            _furImage.sprite = sprite;
            _furImage.SetNativeSize();  
            _furImage.rectTransform.FitTo(_itemButton.image.rectTransform, 24f);
            _furImage.rectTransform.SetEulerAngleY(direction == FurnitureDirection.Left ? 0 : 180);
            _priceTMP.text = _price.ToString();
        }

        public void LoadData(bool isUnlock)
        {
            if (_unlockType == FurnitureUnlockType.Free || isUnlock)
            {
                Unlock();
            }
            else
            {
                Lock();
            }
        }

        private void OnItemClick()
        {
            ClickEvent?.Invoke(this);
        }

        public void OnShow()
        {
            gameObject.SetActive(true);
        }

        public void Lock()
        {
            _isLock = true;

            switch (_unlockType)
            {
                case FurnitureUnlockType.Free:
                    break;
                case FurnitureUnlockType.Ads:
                    _priceLockPanel.gameObject.SetActive(false);
                    _adsLockPanel.gameObject.SetActive(true);
                    break;
                case FurnitureUnlockType.Coin:
                    _adsLockPanel.gameObject.SetActive(false);
                    _priceLockPanel.gameObject.SetActive(true);
                    break;
            }
        }

        public void Unlock()
        {
            _isLock = false;
            _adsLockPanel.gameObject.SetActive(false);
            _priceLockPanel.gameObject.SetActive(false);
        }

        public void SetInteractable(bool canInteractable)
        {
            _itemButton.interactable = canInteractable;
        }

        public void SetHighlight(bool isHighlight)
        {
            _highlightOverlayImg.gameObject.SetActive(isHighlight);
        }
    }
}