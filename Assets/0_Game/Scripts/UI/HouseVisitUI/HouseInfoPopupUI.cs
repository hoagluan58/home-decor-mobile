using JSAM;
using NFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HouseInfoPopupUI : BaseUIView
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private Button _btnLike;
        [SerializeField] private TextMeshProUGUI _txtBotName;
        [SerializeField] private TextMeshProUGUI _txtBotLike;

        private BotConfigData _botData;
        private BotHouseConfigData _houseData;

        private void Awake()
        {
            _btnClose.onClick.AddListener(OnButtonCloseClicked);
            _btnLike.onClick.AddListener(OnButtonLikeClicked);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            SetData();
        }

        private void OnButtonLikeClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            _btnLike.interactable = false;
            _txtBotLike.text = $"{_houseData.Like + 1}";
        }

        private void OnButtonCloseClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            HouseVisitManager.Instance.LeaveBotHouse();
            CloseSelf();
            FeatureNavigator.Instance.Go(EGameFeature.Home, onFullyShowLoading: () =>
            {
                UIManager.Instance.Open<HouseListPopupUI>(Define.UIName.HOUSES_LIST_POPUP).SetData(false);
            }).Forget();
        }

        public void SetData()
        {
            _houseData = HouseVisitManager.Instance.GetCurBotRoomConfigData();
            _botData = BotManager.Instance.GetBotConfigData(_houseData.BotId);
            _txtBotName.text = $"{_botData.Name}";
            _txtBotLike.text = $"{_houseData.Like}";
            _btnLike.interactable = true;
        }
    }
}
