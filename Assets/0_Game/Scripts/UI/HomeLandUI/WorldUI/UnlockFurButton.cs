using DG.Tweening;
using Redcode.Extensions;
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class UnlockFurButton : MonoBehaviour
    {
        public static event Action OnNotEnoughStar;

        [SerializeField] private Button _btnUnlock;
        [SerializeField] private Button _btnDone;
        [SerializeField] private TextMeshProUGUI _priceTMP;
        [SerializeField] private TextMeshProUGUI _notEnoughStarTMP;
        [SerializeField] private GameObject _tutorialGroup;

        private Tween _tweenText;
        private ButtonModel _model;

        private void Awake()
        {
            _btnUnlock.onClick.AddListener(OnButtonUnlockClicked);
            _btnDone.onClick.AddListener(OnButtonDoneClicked);
        }

        private void OnButtonDoneClicked()
        {
            _model.OnClickDone?.Invoke();
        }

        private void OnButtonUnlockClicked()
        {
            if (!_model.IsEnough)
            {
                VibrationManager.Vibrate(0.05f);
                ShowNotEnoughStarText();
                OnNotEnoughStar?.Invoke();
                return;
            }
            _model.OnClickUnlock?.Invoke();
        }

        public void SetData(ButtonModel model, bool isDone = false)
        {
            _model = model;
            _btnDone.gameObject.SetActive(isDone);
            _notEnoughStarTMP.transform.SetLocalScale(0f);
            _notEnoughStarTMP.SetColorA(0f);
            UpdatePriceText();
            ShowTutorial(false);
        }

        public void ShowTutorial(bool isShow) => _tutorialGroup.SetActive(isShow);

        private void ShowNotEnoughStarText()
        {
            _notEnoughStarTMP.transform.SetLocalScale(0f);
            _notEnoughStarTMP.SetColorA(1f);

            _tweenText?.Kill();
            _tweenText = _notEnoughStarTMP.transform.DOScale(1f, 0.7f).SetEase(Ease.OutBack).OnComplete(() =>
            {
                _notEnoughStarTMP.DOFade(0f, 0.3f);
                _notEnoughStarTMP.transform.DOScale(Vector3.zero, 0.3f).SetEase(Ease.InBack);
            });
        }

        private void UpdatePriceText()
        {
            var colorCode = _model.IsEnough ? Define.ColorCode.DEEP_CHESTNUT : Define.ColorCode.RED;
            _priceTMP.text = $"<color={colorCode}>{_model.Price}</color>";
        }

        public class ButtonModel
        {
            public bool IsEnough;
            public int Price;
            public Action OnClickUnlock;
            public Action OnClickDone;

            public ButtonModel(Action onClickUnlock = null, Action onClickDone = null)
            {
                OnClickUnlock = onClickUnlock;
                OnClickDone = onClickDone;
            }
        }
    }
}
