using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderResultPopupUI : BaseUIView
    {
        [SerializeField] private Image _imgRoomPic;
        [SerializeField] private Image _imgHeart;
        [SerializeField] private TextMeshProUGUI _txtLikeAmount;
        [SerializeField] private TextMeshProUGUI _txtRewardNormal;
        [SerializeField] private TextMeshProUGUI _txtRewardAds;
        [SerializeField] private TextMeshProUGUI _txtRewardAdsFree;
        [SerializeField] private Button _btnAdsFree;
        [SerializeField] private Button _btnAds;
        [SerializeField] private Button _btnContinue;
        [SerializeField] private OrderResultStarsGroupUI _starsGroup;

        [Header("FX")]
        [SerializeField] private UIParticle _fxHeart;
        [SerializeField] private UIParticle _fxSparkleBlue;

        private RewardData _diamondReward, _expReward;

        private void Awake()
        {
            _btnAds.onClick.AddListener(OnButtonAdsClicked);
            _btnContinue.onClick.AddListener(OnButtonContinueClicked);
            _btnAdsFree.onClick.AddListener(OnButtonAdsFreeClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            AudioManager.PlaySound(ESound.Complete);
            SetData();
        }

        public override void OnClose()
        {
            base.OnClose();
            _btnAds.transform.DOKill();
            _btnAdsFree.transform.DOKill();
        }

        private void OnButtonContinueClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
            var (diamondReward, starReward, furConfigId) = OrderModeManager.Instance.DoneOrder();
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        private void OnButtonAdsClicked()
        {
            AudioManager.PlaySound(ESound.AdsItem);
            CloseSelf();
            var (diamondReward, starReward, furConfigId) = OrderModeManager.Instance.DoneOrder(true);
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        private void OnButtonAdsFreeClicked()
        {
            AudioManager.PlaySound(ESound.AdsItem);
            CloseSelf();
            var (diamondReward, starReward, furConfigId) = OrderModeManager.Instance.DoneOrder(true);
            FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
        }

        public async void SetData()
        {
            var curOrderData = OrderModeManager.Instance.GetCurBotOrderData();
            curOrderData.NeedReloadRoomSprite = true;
            _diamondReward = OrderModeManager.Instance.GetDiamondRewardData();
            _expReward = OrderModeManager.Instance.GetExperienceRewardData();
            _starsGroup.PlayAnim(curOrderData.StarRating);
            _imgRoomPic.sprite = await curOrderData.GetRoomScreenshotSprite();
            _txtRewardNormal.text = $"x{_diamondReward.amount}";
            _txtRewardAds.text = _txtRewardAdsFree.text = $"x{_diamondReward.amount * Define.ConstValue.ORDER_REWARD_MULTIPLY}";
            _fxSparkleBlue.Play();
            PlayLikeTextAnim();
            PlayHeartAnim();
            DelayShowButtons();
        }

        private void PlayHeartAnim()
        {
            _fxHeart.Play();
            _imgHeart.transform.DOKill();
            _imgHeart.transform.SetLocalScale(1);
            _imgHeart.transform.DOPunchScale(Vector3.one * 0.1f, 2f);
        }

        private void PlayLikeTextAnim()
        {
            var likeAmount = UnityEngine.Random.Range(300, 500);
            _txtLikeAmount.DOKill();
            _txtLikeAmount.DOCounter(0, likeAmount, 2f);
        }

        private async void DelayShowButtons()
        {
            _btnAds.gameObject.SetActive(false);
            _btnAdsFree.gameObject.SetActive(false);
            _btnContinue.gameObject.SetActive(false);
            await UniTask.Delay(2000, cancellationToken: destroyCancellationToken);

            var btnAds = UserData.Instance.CurTutorialIndex == Define.TutorialIndex.NEW_PLAYER ? _btnAdsFree : _btnAds;
            btnAds.gameObject.SetActive(true);
            btnAds.transform.SetLocalScale(1f);
            await btnAds.transform.DOScale(Vector3.zero, 1f)
                .From()
                .SetEase(Ease.OutExpo)
                .WithCancellation(cancellationToken: destroyCancellationToken);
            btnAds.transform
                .DOScale(1.1f, 1f).SetEase(Ease.OutQuad)
                .SetLoops(-1, LoopType.Yoyo);

            await UniTask.Delay(1000, cancellationToken: destroyCancellationToken);
            _btnContinue.gameObject.SetActive(true);
        }
    }
}