using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderModeMenuUI : BaseUIView
    {
        [SerializeField] private List<BotOrderNodePattern> _botOrderNodePatterns;
        [SerializeField] private Button _btnSetting;
        [SerializeField] private Button _btnHome;
        [SerializeField] private Button _btnPlay;
        [SerializeField] private Button _btnHistoricalsOrders;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _goTutorialPanel;
        [SerializeField] private BotOrderNode _botOrderNodeTutorial;
        [SerializeField] private Button _btnOrderNodeTutorial;

        private void Awake()
        {
            _btnHome.onClick.AddListener(OnButtonHomeClicked);
            _btnPlay.onClick.AddListener(OnButtonPlayClicked);
            _btnSetting.onClick.AddListener(OnButtonSettingClicked);
            _btnHistoricalsOrders.onClick.AddListener(OnButtonHistoricalsOrdersClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            SetData();
        }

        public override void OnClose()
        {
            base.OnClose();
            _btnPlay.transform.DOKill();
        }

        private void OnButtonHistoricalsOrdersClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.ORDER_HISTORY_POPUP);
        }

        private void OnButtonPlayClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            var firstNotDoneOrder = OrderModeData.Instance.GetFirstNotDoneOrder();

            if (firstNotDoneOrder == null)
                return;

            UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(firstNotDoneOrder);
        }

        private void OnButtonHomeClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Close(Define.UIName.ORDER_MODE_MENU);
        }

        private void OnButtonSettingClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        private void SetData()
        {
            OrderModeManager.Instance.TryLoadMap();

            var mapData = OrderModeData.Instance.OrderMapData;
            _botOrderNodePatterns.ForEach(_ => _.gameObject.SetActive(false));
            _botOrderNodePatterns[mapData.MapId].SetData(mapData.BotOrderDatas);
            _btnPlay.transform.SetLocalScale(1f);
            _btnPlay.transform.DOScale(1.03f, 1f)
                .SetEase(Ease.OutQuad)
                .SetLoops(-1, LoopType.Yoyo);
            _btnHistoricalsOrders.gameObject.SetActive(OrderModeData.Instance.OrderHistoryDatas.Count > 0);
            if (UserData.Instance.CurTutorialIndex == 1)
            {
                PlayTutorial();
            }
        }

        private void PlayTutorial()
        {
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("Oh, there are quite a few money-making jobs in this town, let's check them out");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                DialogController.Instance.PlayDialogAnimation("Click on the resident's avatar for more job information!");
                DialogController.Instance.NextDialogEvent.AddListener(() =>
                {
                    AudioManager.PlaySound(ESound.Click);
                    DialogController.Instance.Hide();
                    DialogController.Instance.NextDialogEvent.RemoveAllListeners();

                    var mapData = OrderModeData.Instance.OrderMapData;
                    var firstNotDoneOrder = OrderModeData.Instance.GetFirstNotDoneOrder();
                    var botOrderNode = _botOrderNodePatterns[mapData.MapId].GetBotOrderNodeBaseOnData(firstNotDoneOrder.OrderId);

                    _goTutorialPanel.SetActive(true);
                    _botOrderNodeTutorial.transform.position = botOrderNode.transform.position;
                    _botOrderNodeTutorial.SetData(firstNotDoneOrder);

                    botOrderNode.gameObject.SetActive(false);

                    _btnOrderNodeTutorial.onClick.RemoveAllListeners();
                    _btnOrderNodeTutorial.onClick.AddListener(() =>
                    {
                        AudioManager.PlaySound(ESound.Click);
                        UIManager.Instance.Open<OrderNotePopupUI>(Define.UIName.ORDER_NOTE_POPUP).SetData(firstNotDoneOrder);
                        _goTutorialPanel.SetActive(false);
                    });
                });
            });

            DialogController.Instance.SkipEvent.AddListener(OnSkipDialog);

            void OnSkipDialog()
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                _btnOrderNodeTutorial.onClick.RemoveAllListeners();
                _goTutorialPanel.SetActive(false);
            }
        }
    }
}