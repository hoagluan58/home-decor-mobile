using DG.Tweening;
using JSAM;
using NFramework;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Redcode.Extensions;

namespace YoyoDesign
{
    public class OrderNotePopupUI : BaseUIView
    {
        [SerializeField] private RectTransform _rectPopup;
        [SerializeField] private Image _imgRoomPic;
        [SerializeField] private TextMeshProUGUI _txtName;
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnStart;
        [SerializeField] private RewardItemUI _rewardItemUI;
        [SerializeField] private SkeletonGraphic _spineGraphicCharacter;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _goTutorialPanel;
        [SerializeField] private Button _btnStartTutorial;

        private BotOrderData _orderData;

        private void Awake()
        {
            _btnClose.onClick.AddListener(OnButtonCloseClicked);
            _btnStart.onClick.AddListener(OnButtonStartClicked);
            _btnStartTutorial.onClick.AddListener(OnButtonStartTutorialClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _rectPopup.DOKill();
            _rectPopup.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
        }

        public override void OnClose()
        {
            base.OnClose();
            _btnStart.transform.DOKill();
            _btnStartTutorial.transform.DOKill();
        }

        private void OnButtonStartClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Close(Define.UIName.ORDER_NOTE_POPUP);
            UIManager.Instance.Close(Define.UIName.ORDER_MODE_MENU);
            var prevLandId = LandController.Instance.CurLandEditing == null ? -1 : LandController.Instance.CurLandEditing.LandConfig.Id;
            OrderModeManager.Instance.TakeOrder(_orderData, prevLandId);
            FeatureNavigator.Instance.Go(EGameFeature.Order).Forget();
        }

        private void OnButtonStartTutorialClicked()
        {
            _goTutorialPanel.SetActive(false);
            OnButtonStartClicked();
        }

        private void OnButtonCloseClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }

        public void SetData(BotOrderData orderData)
        {
            _orderData = orderData;
            _txtName.text = _orderData.GetOrderName();
            _imgRoomPic.sprite = _orderData.GetDefaultRoomSprite();
            _goTutorialPanel.SetActive(UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_INTRO);
            _spineGraphicCharacter.ApplyCharacterOutfit(orderData.BotData.OutfitDatas);
            _btnStart.transform.SetLocalScale(1f);
            _btnStartTutorial.transform.SetLocalScale(1f);

            if (UserData.Instance.CurTutorialIndex != Define.TutorialIndex.DONE_INTRO)
            {
                _btnStart.transform.DOScale(1.1f, 1f)
                                   .SetEase(Ease.OutQuad)
                                   .SetLoops(-1, LoopType.Yoyo);
            }
            else
            {
                _btnStartTutorial.transform.DOScale(1.1f, 1f)
                                           .SetEase(Ease.OutQuad)
                                           .SetLoops(-1, LoopType.Yoyo);
            }
            _btnClose.gameObject.SetActive(UserData.Instance.CurTutorialIndex != Define.TutorialIndex.NEW_PLAYER);
        }
    }
}
