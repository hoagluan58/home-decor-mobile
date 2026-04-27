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
    public class OrderProgressPopupUI : BaseUIView
    {
        [SerializeField] private Transform _tfRoomInfoPanel;
        [SerializeField] private TextMeshProUGUI _txtRoomInfo;
        [SerializeField] private TextMeshProUGUI _txtFreeCleanRoom;
        [SerializeField] private Button _btnHome;
        [SerializeField] private Button _btnRoomInfo;
        [SerializeField] private Button _btnFastCleanRoomAds;
        [SerializeField] private Button _btnFastCleanRoomFree;
        [SerializeField] private Button _btnDoneRemoveOldFurniture;
        [SerializeField] private Button _btnUndoRepainting;
        [SerializeField] private Button _btnDoneRepainting;
        [SerializeField] private Button _btnDoneDecorate;
        [SerializeField] private OrderProgressBarUI _orderProgressBarUI;
        [SerializeField] private GameObject _btnGroup;

        [Header("VFX")]
        [SerializeField] private GameObject _particleConfetti;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _handTutorialCleanRoom;

        [Header("TRASH BOX")]
        [SerializeField] private OrderProgressTrashBoxUI _trashBoxUI;
        [SerializeField] private RectTransform _handTutorialTrash;

        public OrderProgressTrashBoxUI TrashBoxUI => _trashBoxUI;

        private void Awake()
        {
            _btnHome.onClick.AddListener(OnButtonHomeClicked);
            _btnRoomInfo.onClick.AddListener(OnButtonRoomInfoClicked);
            _btnFastCleanRoomAds.onClick.AddListener(OnButtonFastCleanRoomAdsClicked);
            _btnFastCleanRoomFree.onClick.AddListener(OnButtonFastCleanRoomFreeClicked);
            _btnDoneRemoveOldFurniture.onClick.AddListener(OnButtonDoneRemoveFurnitureClicked);
            _btnUndoRepainting.onClick.AddListener(OnButtonUndoRepaintingClicked);
            _btnDoneRepainting.onClick.AddListener(OnButtonDoneUnboxFurnitureClicked);
            _btnDoneDecorate.onClick.AddListener(OnButtonDoneDecorateClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            Init();

            OrderRoomController.OnOrderStateChanged += OrderRoomController_OnOrderStateChanged;
            OrderCleanRoomState.OnItemCleaned += OrderCleanRoomController_OnTrashCleared;
            OrderRemoveOldFurState.OnDoneCleanRoom += OrderCleanRoomController_OnDoneCleanRoom;
            OrderUnboxFurnitureState.OnDoneUnboxFurniture += OrderRepaintingRoomController_OnDoneUnboxFurniture;
            OrderDecorState.OnAllFurniturePutOut += OrderDecorateController_OnAllFurniturePutOut;
            OrderOptionPopupUI.OnShownPopup += OrderOptionPopupUI_OnShownPopup;
        }

        public override void OnClose()
        {
            base.OnClose();
            OrderRoomController.OnOrderStateChanged -= OrderRoomController_OnOrderStateChanged;
            OrderCleanRoomState.OnItemCleaned -= OrderCleanRoomController_OnTrashCleared;
            OrderRemoveOldFurState.OnDoneCleanRoom -= OrderCleanRoomController_OnDoneCleanRoom;
            OrderUnboxFurnitureState.OnDoneUnboxFurniture -= OrderRepaintingRoomController_OnDoneUnboxFurniture;
            OrderDecorState.OnAllFurniturePutOut -= OrderDecorateController_OnAllFurniturePutOut;
            OrderOptionPopupUI.OnShownPopup -= OrderOptionPopupUI_OnShownPopup;
        }

        private void OrderRoomController_OnOrderStateChanged(EOrderState state)
        {
            if (state == EOrderState.RemoveOldFurniture)
            {
                _btnFastCleanRoomAds.gameObject.SetActive(false);
                _btnFastCleanRoomFree.gameObject.SetActive(false);
                _trashBoxUI.SetActive(false);
            }
        }

        private async void OrderCleanRoomController_OnDoneCleanRoom()
        {
            await UniTask.WaitUntil(() => !_orderProgressBarUI.IsBarTweening);
            _btnDoneRemoveOldFurniture.gameObject.SetActive(true);
        }

        private void OrderRepaintingRoomController_OnDoneUnboxFurniture()
        {
            _btnUndoRepainting.gameObject.SetActive(true);
            _btnDoneRepainting.gameObject.SetActive(true);
        }

        private void OrderDecorateController_OnAllFurniturePutOut() => _btnDoneDecorate.gameObject.SetActive(true);

        private void OrderOptionPopupUI_OnShownPopup(bool isShow) => ToggleButtonGroup(!isShow);

        private void OrderCleanRoomController_OnTrashCleared(int cleanedCount, int maxCount) => UpdateButtonFastCleanRoom();

        private void OnButtonHomeClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            var popup = UIManager.Instance.Open<ConfirmReturnHomePopupUI>(Define.UIName.CONFIRM_RETURN_HOME_POPUP);
            popup.SetData(OnYesButtonClicked, OnNoButtonClicked);

            void OnYesButtonClicked()
            {
                OrderModeManager.Instance.QuitOrder();
                AudioManager.PlaySound(ESound.Click);
                FeatureNavigator.Instance.Go(EGameFeature.Home).Forget();
                popup.CloseSelf();
                CloseSelf();
            }

            void OnNoButtonClicked()
            {
                AudioManager.PlaySound(ESound.Click);
                popup.CloseSelf();
            }
        }

        private void OnButtonRoomInfoClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            ShowRoomInfoPanel();
        }

        private void OnButtonFastCleanRoomAdsClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderRoomController.Instance.CleanRoomState.CleanAllRemainingTrashes();
            _btnFastCleanRoomAds.gameObject.SetActive(false);
        }

        private void OnButtonFastCleanRoomFreeClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UserData.Instance.ModifyCurrencyDic(ECurrencyType.OrderCleanTrash, -1);
            OrderRoomController.Instance.CleanRoomState.CleanAllRemainingTrashes();
            _btnFastCleanRoomFree.gameObject.SetActive(false);
        }

        private void OnButtonDoneRemoveFurnitureClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderRoomController.Instance.RoomWorldUI.SetDataRepainting();
            OrderRoomController.Instance.ChangeState(EOrderState.Repainting);
            _btnDoneRemoveOldFurniture.gameObject.SetActive(false);
        }

        private void OnButtonDoneUnboxFurnitureClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderRoomController.Instance.ChangeState(EOrderState.Decorate);
            _btnUndoRepainting.gameObject.SetActive(false);
            _btnDoneRepainting.gameObject.SetActive(false);
        }

        private void OnButtonUndoRepaintingClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            OrderRoomController.Instance.RoomWorldUI.ToggleRepaintingGroup(true);
            OrderRoomController.Instance.RoomWorldUI.ToggleGroup2Repainting(true);
            OrderRoomController.Instance.RoomWorldUI.ToggleUnboxFurGroup(true);
        }

        private async void OnButtonDoneDecorateClicked()
        {
            VibrationManager.Vibrate(0.1f);
            UIManager.Instance.DisableInteract(this);
            AudioManager.PlaySound(ESound.Click);
            _btnDoneDecorate.gameObject.SetActive(false);
            OrderRoomController.Instance.DecorRoomState.Exit();
            OrderRoomController.Instance.TakeScreenshot();

            var curBotOrderData = OrderModeManager.Instance.GetCurBotOrderData();
            curBotOrderData.NeedReloadRoomSprite = true;
            await curBotOrderData.GetRoomScreenshotSprite();

            _particleConfetti.SetActive(true);
            var camera = CameraController.Instance.Camera;
            var zoomSize = camera.orthographicSize + 5f;
            var animTime = 1.5f;
            CameraController.Instance.MoveAndZoomAnimation(Vector3.zero, zoomSize, animTime);
            await UniTask.WaitForSeconds(animTime + 0.5f, cancellationToken: destroyCancellationToken);

            CloseSelf();
            UIManager.Instance.EnableInteract(this);
            UIManager.Instance.Open(Define.UIName.ORDER_RESULT_POPUP);
        }

        private void ShowRoomInfoPanel()
        {
            var curOrderData = OrderModeManager.Instance.GetCurBotOrderData();
            _txtRoomInfo.text = curOrderData.GetOrderName();
            _tfRoomInfoPanel.DOKill();
            _tfRoomInfoPanel.SetLocalScale(1f);
            var sequence = DOTween.Sequence();
            sequence.Append(_tfRoomInfoPanel.DOScale(new Vector3(0f, 0f, 0f), 0.5f).SetEase(Ease.OutExpo).From())
                .AppendInterval(1f)
                .Append(_tfRoomInfoPanel.DOScale(Vector3.zero, 0.5f).SetEase(Ease.Flash));
        }

        private void Init()
        {
            var canShowButton = UserData.Instance.CurTutorialIndex != Define.TutorialIndex.NEW_PLAYER && UserData.Instance.CurTutorialIndex != Define.TutorialIndex.DONE_INTRO;
            _btnHome.gameObject.SetActive(canShowButton);
            _btnRoomInfo.gameObject.SetActive(canShowButton);
            _btnDoneRemoveOldFurniture.gameObject.SetActive(false);
            _btnUndoRepainting.gameObject.SetActive(false);
            _btnDoneRepainting.gameObject.SetActive(false);
            _btnDoneDecorate.gameObject.SetActive(false);
            _particleConfetti.SetActive(false);
            _tfRoomInfoPanel.SetLocalScale(0f);

            _trashBoxUI.SetActive(true);
            UpdateButtonFastCleanRoom();
            ToggleButtonGroup(true);
        }

        private void ToggleButtonGroup(bool value) => _btnGroup.SetActive(value);

        private void UpdateButtonFastCleanRoom()
        {
            var cleanTrashFreeCount = UserData.Instance.GetCurrencyAmount(ECurrencyType.OrderCleanTrash);
            var isTutorial = UserData.Instance.CurTutorialIndex < Define.TutorialIndex.DONE_INTRO;

            if (isTutorial)
            {
                var trashCleanedCount = OrderRoomController.IsAlive ? OrderRoomController.Instance.CleanRoomState.TrashClearCount : 0;
                _btnFastCleanRoomAds.gameObject.SetActive(false);
                _btnFastCleanRoomFree.gameObject.SetActive(trashCleanedCount >= 2);
                _handTutorialCleanRoom.SetActive(trashCleanedCount >= 2);
            }
            else
            {
                _btnFastCleanRoomFree.gameObject.SetActive(cleanTrashFreeCount != 0);
                _btnFastCleanRoomAds.gameObject.SetActive(cleanTrashFreeCount == 0);
                _handTutorialCleanRoom.SetActive(false);
            }
            _txtFreeCleanRoom.text = $"{cleanTrashFreeCount}";
        }

        public void SetActiveHandTrashTutorial(bool value) => _handTutorialTrash.gameObject.SetActive(value);

        public void HandDragTutorial(Vector3 startPos)
        {
            _handTutorialTrash.position = startPos;
            _handTutorialTrash.DOKill();
            _handTutorialTrash.DOMove(_trashBoxUI.Position, 2f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Restart);
        }
    }
}