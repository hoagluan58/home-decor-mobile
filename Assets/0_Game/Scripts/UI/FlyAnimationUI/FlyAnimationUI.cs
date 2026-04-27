using AssetKits.ParticleImage;
using DG.Tweening;
using NFramework;
using Redcode.Extensions;
using TMPro;
using UnityEngine;

namespace YoyoDesign
{
    public class FlyAnimationUI : BaseUIView
    {
        [Header("DIAMOND")]
        [SerializeField] private ParticleImage _diamondParticle;
        [SerializeField] private RectTransform _baseDiamondPanelPos;
        [SerializeField] private RectTransform _fakeDiamondPanel;
        [SerializeField] private TextMeshProUGUI _diamondValueTMP;

        [Header("STAR")]
        [SerializeField] private ParticleImage _starParticle;
        [SerializeField] private RectTransform _baseStarPanel;
        [SerializeField] private RectTransform _fakeStarPanel;
        [SerializeField] private TextMeshProUGUI _starValueTMP;

        private Tween _autoCloseTween;

        public override void OnOpen()
        {
            _fakeDiamondPanel.gameObject.SetActive(false);
            _fakeStarPanel.gameObject.SetActive(false);
        }

        public void PlayStarAnim(float targetStarValue, float changedStarValue, Transform starPanel = null)
        {
            _starParticle.onParticleFinish.RemoveAllListeners();
            _starParticle.onLastParticleFinish.RemoveAllListeners();
            _fakeStarPanel.gameObject.SetActive(true);
            _fakeStarPanel.position = starPanel == null ? _baseStarPanel.position : starPanel.position;

            var oldValue = targetStarValue - changedStarValue <= 0 ? 0 : targetStarValue - changedStarValue;
            _starValueTMP.text = $"{oldValue}";
            _starParticle.SetBurst(0, 0, changedStarValue <= 10 ? (int)changedStarValue : 10);
            _starParticle.onParticleFinish.AddListener(() =>
            {
                VibrationManager.Vibrate(0.05f);
                _starValueTMP.transform.DOKill();
                _starValueTMP.transform.SetLocalScale(1);
                _starValueTMP.transform.DOPunchScale(Vector3.one * 0.1f, 0.05f);
                _starValueTMP.text = $"{int.Parse(_starValueTMP.text) + (changedStarValue >= 10 ? (int)changedStarValue / 10 : 1)}";
            });
            _starParticle.onLastParticleFinish.AddListener(() =>
            {
                _starValueTMP.transform.DOKill();
                _starValueTMP.transform.SetLocalScale(1);
                _starValueTMP.text = $"{targetStarValue}";
            });
            _starParticle.Play();
            _autoCloseTween?.Kill();
            _autoCloseTween = DOVirtual.DelayedCall(2f, () => CloseSelf());
        }

        public void PlayDiamondAnim(float targetDiamondValue, float changedDiamondValue, Transform diamondPanel = null)
        {
            _diamondParticle.onParticleFinish.RemoveAllListeners();
            _diamondParticle.onLastParticleFinish.RemoveAllListeners();
            _fakeDiamondPanel.gameObject.SetActive(true);
            _fakeDiamondPanel.position = diamondPanel == null ? _baseDiamondPanelPos.position : diamondPanel.position;

            var oldValue = targetDiamondValue - changedDiamondValue <= 0 ? 0 : targetDiamondValue - changedDiamondValue;
            _diamondValueTMP.text = $"{oldValue}";
            _diamondParticle.SetBurst(0, 0, changedDiamondValue <= 10 ? (int)changedDiamondValue : 10);
            _diamondParticle.onParticleFinish.AddListener(() =>
            {
                VibrationManager.Vibrate(0.05f);
                _diamondValueTMP.transform.DOKill();
                _diamondValueTMP.transform.SetLocalScale(1);
                _diamondValueTMP.transform.DOPunchScale(Vector3.one * 0.1f, 0.05f);
                _diamondValueTMP.text = $"{int.Parse(_diamondValueTMP.text) + (changedDiamondValue >= 10 ? (int)changedDiamondValue / 10 : 1)}";
            });
            _diamondParticle.onLastParticleFinish.AddListener(() =>
            {
                _diamondValueTMP.transform.DOKill();
                _diamondValueTMP.transform.SetLocalScale(1);
                _diamondValueTMP.text = $"{targetDiamondValue}";
            });
            _diamondParticle.Play();
            _autoCloseTween?.Kill();
            _autoCloseTween = DOVirtual.DelayedCall(2f, () => CloseSelf());
        }
    }
}