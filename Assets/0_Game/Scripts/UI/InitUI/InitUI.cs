using DG.Tweening;
using JSAM;
using NFramework;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class InitUI : BaseUIView
    {
        [Header("INIT UI")]
        [SerializeField] private TextMeshProUGUI _loadingPercentTMP;
        [SerializeField] private RectTransform _carRectTransform;
        [SerializeField] private RectTransform _characterRectTransform;
        [SerializeField] private SkeletonGraphic _characterSpine;

        public override void OnClose()
        {
            base.OnClose();
            _carRectTransform.DOKill();
        }

        public void RunLoadingAnimation(float loadingLength)
        {
            DOVirtual.Float(0, 100, loadingLength, percent => _loadingPercentTMP.text = "Loading " + percent.ToString("F0") + "%")
                .SetEase(Ease.Linear);

            _carRectTransform.DOAnchorPosX(293.33f, 2f)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    _carRectTransform.DOAnchorPosX(2000f, 1f)
                        .SetEase(Ease.Linear);
                });


            var characterAnimSequence = DOTween.Sequence();
            characterAnimSequence
                .Append(_characterRectTransform.DOAnchorPosX(0, 2f).SetEase(Ease.Linear))
                .AppendCallback(() => _characterRectTransform.DOScaleY(0.4f, 0.1f))
                .AppendCallback(() => _characterRectTransform.DOScaleY(0.55f, 0.2f))
                .Append(_characterRectTransform.DOAnchorPosY(_characterRectTransform.anchoredPosition.y + 300, 0.2f))
                .AppendCallback(() => _characterRectTransform.DOScaleY(0.5f, 0.1f))
                .Append(_characterRectTransform.DOAnchorPosY(-180, 0.2f))
                .Append(_characterRectTransform.DOScaleY(0.4f, 0.1f))
                .Append(_characterRectTransform.DOScaleY(0.5f, 0.15f));
            characterAnimSequence.Play();
        }

        public void GoToHome()
        {
            GameManager.Instance.CurGameState.Value = EGameState.Playing;
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }
    }
}