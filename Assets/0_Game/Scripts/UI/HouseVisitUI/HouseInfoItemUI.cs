using EnhancedUI.EnhancedScroller;
using JSAM;
using NFramework;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HouseInfoItemUI : EnhancedScrollerCellView
    {
        [SerializeField] private SkeletonGraphic _imgAvatar;
        [SerializeField] private TextMeshProUGUI _txtName;
        [SerializeField] private TextMeshProUGUI _txtLikeAmount;
        [SerializeField] private Button _btnVisit;
        [SerializeField] private RectTransform _rectTransform;

        private BotConfigData _botData;
        private BotHouseConfigData _houseData;

        public float Height => _rectTransform.rect.height;

        private void Awake() => _btnVisit.onClick.AddListener(OnButtonVisitClicked);

        private void OnButtonVisitClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            HouseVisitManager.Instance.VisitBotHouse(_houseData.Id);
            UIManager.Instance.Close(Define.UIName.HOUSES_LIST_POPUP);
            FeatureNavigator.Instance.Go(EGameFeature.SquareBotHouse).Forget();
        }

        public void SetData(BotHouseConfigData data)
        {
            _houseData = data;
            _botData = BotManager.Instance.GetBotConfigData(_houseData.BotId);
            _txtName.text = _botData.Name;
            _imgAvatar.ApplyCharacterOutfit(_botData.OutfitDatas);
            _txtLikeAmount.text = $"{_houseData.Like}";
        }
    }
}