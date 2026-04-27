using Cysharp.Threading.Tasks;
using DG.Tweening;
using NFramework;
using Redcode.Extensions;
using System;
using System.Threading;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class RewardPopupUI : BaseUIView
    {
        [SerializeField] private Button _btnContinue;
        [SerializeField] private Button _btnClaimX2;
        [SerializeField] private RewardItemUI _rewardItemUI;
        [SerializeField] private CurrencyBarUI _currencyBarUI;
        [SerializeField] private OrderResultStarsGroupUI _starsUI;

        private RewardData _data;
        private Action<float> _onClaimCallback; // multiply reward

        private Tween _delayShowButtonContinue;

        private void OnEnable()
        {
            _btnContinue.onClick.AddListener(OnButtonContinueClicked);
            _btnClaimX2.onClick.AddListener(OnButtonClaimClicked);
        }

        private void OnDisable()
        {
            _btnContinue.onClick.RemoveListener(OnButtonContinueClicked);
            _btnClaimX2.onClick.RemoveListener(OnButtonClaimClicked);
        }

        public override void OnClose()
        {
            base.OnClose();
            _delayShowButtonContinue?.Kill();
        }

        private void OnButtonContinueClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            _onClaimCallback?.Invoke(1);
            PlayFlyCurrencyAnim(1);
            CloseSelf();
        }

        private void OnButtonClaimClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            _onClaimCallback?.Invoke(2);
            PlayFlyCurrencyAnim(2);
            CloseSelf();
        }

        public void SetData(RewardData data, Action<float> onClaimCallback = null)
        {
            _data = data;
            _onClaimCallback = onClaimCallback;
            _starsUI.PlayAnim(5);
            _rewardItemUI.SetData(data.amount.ToString());

            _btnContinue.gameObject.SetActive(false);
            _delayShowButtonContinue?.Kill();
            _delayShowButtonContinue = DOVirtual.DelayedCall(2f, () => _btnContinue.gameObject.SetActive(true));
        }

        private void PlayFlyCurrencyAnim(int multiply = 1)
        {
            var amount = multiply * _data.amount;
            var flyAnimationUI = UIManager.Instance.Open<FlyAnimationUI>(Define.UIName.FLY_ANIMATION);
            flyAnimationUI.PlayDiamondAnim(UserData.Instance.GetCurrencyAmount(ECurrencyType.Diamond), amount, _currencyBarUI.transform);
        }
    }
}