using Coffee.UIEffects;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class RewardFurniturePopupUI : BaseUIView
    {
        [SerializeField] private GameObject _giftBox;
        [SerializeField] private UIParticle _openBoxParticle;
        [SerializeField] private RectTransform _rewardPanel;
        [SerializeField] private RectTransform _giftOpenedPanel;
        [SerializeField] private Image _rewardImage;
        [SerializeField] private UIShiny _shinyEffect;

        public void SetData(RewardData rewardData)
        {
            _rewardImage.sprite = rewardData.icon;
            _rewardImage.SetNativeSize();
            _rewardImage.rectTransform.FitTo(_rewardImage.rectTransform.parent.GetComponent<RectTransform>(), 14f);
        }

        public async UniTask PlayAnimation()
        {
            _giftOpenedPanel.gameObject.SetActive(false);
            _giftBox.gameObject.SetActive(true);
            await UniTask.WaitForSeconds(1f, cancellationToken: destroyCancellationToken);
            AudioManager.PlaySound(ESound.AdsItem);
            _giftOpenedPanel.gameObject.SetActive(true);
            _openBoxParticle.Play();
            _giftBox.gameObject.SetActive(false);
            await _rewardPanel.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            _shinyEffect.Play();
            await UniTask.WaitForSeconds(3.5f, cancellationToken: destroyCancellationToken);
            CloseSelf();
        }
    }
}
