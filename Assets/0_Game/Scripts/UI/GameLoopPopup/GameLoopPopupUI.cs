using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using System;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class GameLoopPopupUI : BaseUIView
    {
        public static event Action OnCloseGameLoopPopup;

        [SerializeField] private Button _closeButton;
        [SerializeField] private Image _completeLevelImage;
        [SerializeField] private Image _firstArrowImage;
        [SerializeField] private Image _earnRewardImage;
        [SerializeField] private Image _secondArrowImage;
        [SerializeField] private Image _getDesignImage;
        [SerializeField] private Image _thirdArrowImage;
        [SerializeField] private Image _tapToContinueImage;

        private Sequence _animationSequence;

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _tapToContinueImage.gameObject.SetActive(false);
            _closeButton.gameObject.SetActive(false);

            _completeLevelImage.transform.SetLocalScale(0);
            _earnRewardImage.transform.SetLocalScale(0);
            _getDesignImage.transform.SetLocalScale(0);

            _firstArrowImage.fillAmount = 0;
            _secondArrowImage.fillAmount = 0;
            _thirdArrowImage.fillAmount = 0;

            _animationSequence?.Kill();
            _animationSequence = DOTween.Sequence();
            _animationSequence
                .AppendInterval(1f)
                .Append(_completeLevelImage.transform.DOScale(1f, 0.5f))
                .Append(_completeLevelImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f))
                .Append(_firstArrowImage.DOFillAmount(1, 0.5f))
                .Append(_earnRewardImage.transform.DOScale(1f, 0.5f))
                .Append(_earnRewardImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f))
                .Append(_secondArrowImage.DOFillAmount(1, 0.5f))
                .Append(_getDesignImage.transform.DOScale(1f, 0.5f))
                .Append(_getDesignImage.transform.DOPunchScale(Vector3.one * 0.1f, 0.2f))
                .Append(_thirdArrowImage.DOFillAmount(1, 0.5f))
                .AppendCallback(() =>
                {
                    _tapToContinueImage.gameObject.SetActive(true);
                    _closeButton.gameObject.SetActive(true);
                });
        }

        private void OnCloseButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
            OnCloseGameLoopPopup?.Invoke();
        }
    }
}