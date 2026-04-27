using DG.Tweening;
using NFramework;
using Spine.Unity;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class ConfirmReturnHomePopupUI : BaseUIView
    {
        [Header("UI OBJECTS")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _yesBtn;
        [SerializeField] private Button _noBtn;

        [Header("CHARACTER")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;

        private Action _onYesClicked, _onNoClicked;

        private void OnEnable()
        {
            _yesBtn.onClick.AddListener(OnYesButtonClick);
            _noBtn.onClick.AddListener(OnNoButtonClick);
        }

        private void OnDisable()
        {
            _yesBtn.onClick.RemoveListener(OnYesButtonClick);
            _noBtn.onClick.RemoveListener(OnNoButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _characterSkeletonGraphic.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            _contentPanel.DOKill();
            _contentPanel.RotateScalePopup(0.2f);
        }

        public void SetData(Action onYesClicked = null, Action onNoClicked = null)
        {
            _onYesClicked = onYesClicked;
            _onNoClicked = onNoClicked;
        }

        private void OnYesButtonClick() => _onYesClicked?.Invoke();

        private void OnNoButtonClick() => _onNoClicked?.Invoke();
    }
}