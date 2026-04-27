using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class RateUsPopupUI : BaseUIView
    {
        [Title("CONTENT PANEL")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _submitButton;
        [SerializeField] private Button[] _rateButtons;
        [SerializeField] private Image[] _starImages;

        private int _currentStarIndex = -1;

        private void OnEnable()
        {
            _submitButton.onClick.AddListener(OnSubmitButtonClicked);
            for (var i = 0; i < _rateButtons.Length; i++)
            {
                var index = i;
                _rateButtons[i].onClick.AddListener(() => OnStarButtonClicked(index));
            }
        }

        private void OnDisable()
        {
            _submitButton.onClick.RemoveListener(OnSubmitButtonClicked);
            _rateButtons.ForEach(x => x.onClick.RemoveAllListeners());
        }

        private void OnSubmitButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            Logger.Log(this, $"Rate {_currentStarIndex + 1} stars");
            CloseSelf();
        }

        private void OnStarButtonClicked(int index) => UpdateStar(index);

        public override void OnOpen()
        {
            base.OnOpen();
            _contentPanel.DOKill();
            _contentPanel.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
            _currentStarIndex = -1;
            DisableAllStar();
        }

        private void UpdateStar(int index)
        {
            if (index == _currentStarIndex) return;

            DisableAllStar();
            _currentStarIndex = index;
            StartCoroutine(CRStarAnim());

            IEnumerator CRStarAnim()
            {
                var waiter = new WaitForSeconds(0.1f);
                for (var i = 0; i <= _currentStarIndex; i++)
                {
                    _starImages[i].SetAlpha(1);
                    yield return waiter;
                }
            }
        }

        private void DisableAllStar() => _starImages.ForEach(star => star.SetAlpha(0));
    }
}
