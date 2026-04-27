using DG.Tweening;
using JSAM;
using Redcode.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OutfitTypeSelectButton : MonoBehaviour
    {
        #region PARAM

        public static UnityEvent<int, OutfitType> ClickEvent = new();
        public static UnityEvent<OutfitTypeSelectButton> BtnClickEvent = new();

        [Header("DATA")]
        [SerializeField] private int _index;
        [SerializeField] private OutfitType _outfitType;
        [Header("UI")]
        [SerializeField] private Button _button;
        [SerializeField] private Image _imageIconOff;
        [SerializeField] private Image _imageIconOn;
        [SerializeField] private Image _selectImg;
        [SerializeField] private TextMeshProUGUI _textMeshProName;
        [Header("ANIM")]
        [SerializeField] private Ease _selectEase;
        [SerializeField] private float _selectTime;
        [SerializeField] private float _deSelectTime;

        public int Index => _index;

        #endregion

        #region UNITY METHODS
        public void Awake()
        {
            if(_outfitType == OutfitType.Hair)
            {
                BtnClickEvent?.Invoke(this);
            }
        }

        private void OnEnable()
        {
            _button.onClick.AddListener(OnButtonClicked);
            BtnClickEvent.AddListener(SetSelect);

            if(_outfitType == OutfitType.Hair)
            {
                Select();
            }
            else
            {
                DeSelect();
            }
        }

        private void OnDisable()
        {
            _button.onClick.RemoveListener(OnButtonClicked);
            BtnClickEvent.RemoveListener(SetSelect);
        }
        #endregion

        #region FEATURE METHODS

        private void SetSelect(OutfitTypeSelectButton outfitTypeSelectButton)
        {
            if(outfitTypeSelectButton == this)
            {
                Select();
            }
            else
            {
                DeSelect();
            }
        }

        private void Select()
        {
            _imageIconOff.gameObject.SetActive(false);
            _textMeshProName.gameObject.SetActive(true);

            _selectImg.transform.DOKill();
            _selectImg.transform.SetLocalScale(1);
            _selectImg.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f);
        }

        private void DeSelect()
        {
            _textMeshProName.gameObject.SetActive(false);

            _selectImg.transform.DOKill();
            _selectImg.transform
                .DOScale(0f, _deSelectTime)
                .OnComplete(() => _imageIconOff.gameObject.SetActive(true));
        }

        #endregion

        #region ONCLICK METHODS

        private void OnButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            ClickEvent?.Invoke(_index, _outfitType);
            BtnClickEvent?.Invoke(this);
        }
        #endregion
    }
}