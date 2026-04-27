using JSAM;
using NFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class LandMenuUI : BaseUIView
    {
        [SerializeField] private Button _backButton;
        [SerializeField] private Button _orderButton;
        [SerializeField] private CurrencyBarUI _starCurrencyBarUI;
        [SerializeField] private CurrencyBarUI _diamondCurrencyBarUI;
        [SerializeField] private Image _progressBarIMG;
        [SerializeField] private TextMeshProUGUI _progressTMP;
        [SerializeField] private GameObject _progressBar;
        [SerializeField] private GameObject _tutorialHand;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _tutorialOverlay;
        [SerializeField] private Button _tutorialOrderButton;

        private LandProgress _landProgress;
        private LandConfigData _landConfig;

        private void Awake()
        {
            _backButton.onClick.AddListener(OnBackButtonClicked);
            _orderButton.onClick.AddListener(OnOrderButtonClicked);
            _tutorialOrderButton.onClick.AddListener(OnTutorialOrderButtonClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            GameLoopPopupUI.OnCloseGameLoopPopup += GameLoopPopupUI_OnCloseGameLoopPopup;
            UnlockFurButton.OnNotEnoughStar += UnlockFurButton_OnNotEnoughStar;
        }

        public override void OnClose()
        {
            base.OnClose();
            GameLoopPopupUI.OnCloseGameLoopPopup -= GameLoopPopupUI_OnCloseGameLoopPopup;
            UnlockFurButton.OnNotEnoughStar -= UnlockFurButton_OnNotEnoughStar;
        }

        private void OnBackButtonClicked()
        {
            LandController.Instance.CurLandEditing = null;
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
            UIManager.Instance.Open(Define.UIName.HOME_V2_MENU);
        }

        private void OnOrderButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderModeManager.Instance.TryLoadMap();
            _tutorialHand.SetActive(false);
            var firstNotDoneOrder = OrderModeData.Instance.GetFirstNotDoneOrder();

            if (firstNotDoneOrder == null)
                return;

            UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(firstNotDoneOrder);
        }

        private void OnTutorialOrderButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            _tutorialOverlay.SetActive(false);
            OrderModeManager.Instance.TryLoadMap();

            var firstNotDoneOrder = OrderModeData.Instance.GetFirstNotDoneOrder();

            if (firstNotDoneOrder == null)
                return;

            UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(firstNotDoneOrder);
            CheckActiveButton();
        }

        private void GameLoopPopupUI_OnCloseGameLoopPopup() => _tutorialOverlay.SetActive(true);

        private void UnlockFurButton_OnNotEnoughStar() => _tutorialHand.SetActive(true);

        public void SetData(int landId)
        {
            _landProgress = HomeLandData.Instance.GetLandProgress(landId);
            _landConfig = AllConfig.Instance.LandConfigDic[landId];
            _tutorialOverlay.SetActive(false);
            _tutorialHand.SetActive(false);
            UpdateProgressBar();
            CheckActiveButton();
        }

        private void UpdateProgressBar()
        {
            var isDone = _landProgress.UnlockedDecorFurniture.Count == _landConfig.DecorFurnitureData.Count;
            _progressBar.SetActive(!isDone);
            _progressTMP.text = $"{_landProgress.UnlockedDecorFurniture.Count}/{_landConfig.DecorFurnitureData.Count}";
            _progressBarIMG.fillAmount = (float)_landProgress.UnlockedDecorFurniture.Count / _landConfig.DecorFurnitureData.Count;
        }

        private void CheckActiveButton()
        {
            var isTutorial = UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_ORDER;
            _backButton.gameObject.SetActive(!isTutorial);
            _orderButton.gameObject.SetActive(!isTutorial);
        }
    }
}
