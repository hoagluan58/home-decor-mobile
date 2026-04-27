
using System;
using DG.Tweening;
using JSAM;
using Redcode.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class CategoryButtonUI : MonoBehaviour
    {
        public static UnityEvent<EFurnitureCategory> ClickEvent = new();
        
        [Header("CONFIG")]
        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private RectTransform _selectedPanel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _selectedIconImage;
        [SerializeField] private TextMeshProUGUI _categoryNameTMP;
            
        
        private EFurnitureCategory _category;
        public EFurnitureCategory Category => _category;

        private void OnEnable()
        {
           _button.onClick.AddListener(OnButtonClick);
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClick);
        }
        
        public void OnCreate(EFurnitureCategory categoryKey, Sprite iconSprite, Sprite selectedIconSprite, string categoryName)
        {
            _category = categoryKey;
            _categoryNameTMP.text = categoryName;
            
            _iconImage.sprite = iconSprite;
            _iconImage.SetNativeSize();
            
            _selectedIconImage.sprite = selectedIconSprite;
            _selectedIconImage.SetNativeSize();
        }

        public void SetHighlight(bool isHighlight)
        {
            _selectedPanel.gameObject.SetActive(isHighlight);

            if (isHighlight)
            {
                _selectedPanel.DOKill();
                _selectedPanel.SetLocalScale(1);
                _selectedPanel.DOPunchScale(Vector3.one * 0.1f, 0.2f);
            }
        }
        
        public void SetInteractable(bool isInteractable)
        {
            _button.interactable = isInteractable;
        }

        private void OnButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            ClickEvent?.Invoke(_category);
        }
    }
}