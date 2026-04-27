using DG.Tweening;
using JSAM;
using NFramework;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderHistoryPopupUI : BaseUIView
    {
        [SerializeField] private RectTransform _rectPopup;
        [SerializeField] private Button _btnClose;
        [SerializeField] private OrderHistoryItemUI _orderHistoryItemPf;
        [SerializeField] private ScrollRect _scrollRect;
        [SerializeField] private GameObject _goEmpty;
        [SerializeField] private GameObject _goLoading;

        private ObjectPool<OrderHistoryItemUI> _poolOrderHistoryItem;

        private void Awake()
        {
            _btnClose.onClick.AddListener(OnButtonCloseClicked);
            _poolOrderHistoryItem = new(
                () => Instantiate(_orderHistoryItemPf, _scrollRect.content),
                item => item.gameObject.SetActive(true),
                item => item.gameObject.SetActive(false),
                item => Destroy(item.gameObject));
        }

        public override void OnOpen()
        {
            base.OnOpen();

            _rectPopup.DOKill();
            _rectPopup.PunchScalePopup(Vector3.one * 0.1f, 0.2f);

            SetData();
        }

        private void OnButtonCloseClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }

        private async void SetData()
        {
            _scrollRect.content.DestroyAllChildren();
            var orderHistoryDatas = OrderModeData.Instance.OrderHistoryDatas;
            _goEmpty.SetActive(orderHistoryDatas.Count == 0);
            _goLoading.SetActive(false);

            if (orderHistoryDatas.Count == 0)
                return;

            _scrollRect.gameObject.SetActive(false);
            _goLoading.SetActive(true);
            foreach (var orderHistory in orderHistoryDatas)
            {
                var item = _poolOrderHistoryItem.Get();
                await item.SetData(orderHistory);
            }
            _goLoading.SetActive(false);
            _scrollRect.gameObject.SetActive(true);
        }
    }
}
