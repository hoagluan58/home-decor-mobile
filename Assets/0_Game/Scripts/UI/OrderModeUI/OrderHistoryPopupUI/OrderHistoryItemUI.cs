using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderHistoryItemUI : MonoBehaviour
    {
        [SerializeField] private GameObject _goParent;
        [SerializeField] private TextMeshProUGUI _txtName;
        [SerializeField] private Image _imgRoom;
        [SerializeField] private List<GameObject> _goStars;
        [SerializeField] private Button _btnTakeOrder;

        private OrderHistoryData _data;

        private void Awake() => _btnTakeOrder.onClick.AddListener(OnButtonTakeOrderClicked);

        private void OnButtonTakeOrderClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Close(Define.UIName.ORDER_MODE_MENU);
            UIManager.Instance.Close(Define.UIName.ORDER_HISTORY_POPUP);
            OrderModeManager.Instance.RetakeOrder(_data.Id);
            FeatureNavigator.Instance.Go(EGameFeature.Order).Forget();
        }

        public async UniTask SetData(OrderHistoryData orderHistoryData)
        {
            _data = orderHistoryData;
            _goParent.SetActive(_data != null);

            if (_data == null)
                return;

            _txtName.text = $"{_data.OrderData.GetOrderName()}";
            _imgRoom.sprite = await _data.OrderData.GetRoomScreenshotSprite();
            _goStars.ForEach(x => x.SetActive(false));

            for (var i = 0; i < _data.OrderData.StarRating; i++)
            {
                _goStars[i].SetActive(true);
            }
        }
    }
}
