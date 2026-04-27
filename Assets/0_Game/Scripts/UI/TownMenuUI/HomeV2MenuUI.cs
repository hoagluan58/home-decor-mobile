using JSAM;
using NFramework;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HomeV2MenuUI : BaseUIView
    {
        [Header("UI")]
        [SerializeField] private Button _btnOrder;
        [SerializeField] private Button _settingsButton;
        [SerializeField] private GameObject _topPanel;
        [SerializeField] private GameObject _bottomPanel;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _tutorialOverlay;
        [SerializeField] private GameObject _raycastBlocker;
        [SerializeField] private Button _orderTutorialButton;

        [Header("FX")]
        [SerializeField] private RectTransform _starFlyTarget;
        [SerializeField] private RectTransform _diamondFlyTarget;

        private void OnEnable()
        {
            _btnOrder.onClick.AddListener(OnButtonOrderClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);
        }

        private void OnDisable()
        {
            _btnOrder.onClick.RemoveListener(OnButtonOrderClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _tutorialOverlay.SetActive(false);
            _raycastBlocker.SetActive(false);

            var isIntroTutorial = UserData.Instance.CurTutorialIndex == Define.TutorialIndex.NEW_PLAYER;
            _topPanel.SetActive(!isIntroTutorial);
            _bottomPanel.SetActive(!isIntroTutorial);

            HomeV2TutorialController.OnShowRayCastBlocker += HomeV2TutorialController_OnShowTutorial;
        }

        public override void OnClose()
        {
            base.OnClose();
            HomeV2TutorialController.OnShowRayCastBlocker -= HomeV2TutorialController_OnShowTutorial;
        }

        private void HomeV2TutorialController_OnShowTutorial(HomeV2TutorialController.ERaycastBlockType type)
        {
            _raycastBlocker.SetActive(type == HomeV2TutorialController.ERaycastBlockType.WhiteBlock);
            _tutorialOverlay.SetActive(type == HomeV2TutorialController.ERaycastBlockType.BlackBlock);
        }

        #region EVENT LISTENERS

        private void OnSettingsButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        private void OnButtonOrderClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderModeManager.Instance.TryLoadMap();

            var firstNotDoneOrder = OrderModeData.Instance.GetFirstNotDoneOrder();

            if (firstNotDoneOrder == null)
                return;

            UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(firstNotDoneOrder);
        }

        #endregion // EVENT LISTENERS
    }
}
