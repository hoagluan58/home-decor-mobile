using System;
using JSAM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class SubCategoryButtonUI : MonoBehaviour
    {
        public static UnityEvent<EFurnitureSubCategory> ClickEvent = new();
        
        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _highlightImage;
        
        private EFurnitureCategory _mainCategory;
        private EFurnitureSubCategory _subCategory;
        
        public EFurnitureCategory MainCategory => _mainCategory;
        public EFurnitureSubCategory SubCategory => _subCategory;

        private void OnEnable()
        {
            _button.onClick.AddListener(OnSubCategoryButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnSubCategoryButtonClick);
        }

        public void OnCreate(EFurnitureCategory mainCategory, EFurnitureSubCategory subCategoryKey, Sprite iconSprite)
        {
            _mainCategory = mainCategory;
            _subCategory = subCategoryKey;
            _iconImage.sprite = iconSprite;
            _iconImage.SetNativeSize();
        }
        
        public void SetHighlight(bool isHighlight)
        {
            _highlightImage.gameObject.SetActive(isHighlight);   
        }

        public void OnSubCategoryButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            ClickEvent?.Invoke(SubCategory);
        }

        public void SetInteractable(bool canInteractable)
        {
            _button.interactable = canInteractable;
        }
    }
}