using EnhancedUI.EnhancedScroller;
using JSAM;
using NFramework;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HouseListPopupUI : BaseUIView, IEnhancedScrollerDelegate
    {
        [SerializeField] private Button _btnClose;
        [SerializeField] private TextMeshProUGUI _tmpPlayerLike;
        [SerializeField] private HouseInfoItemUI _houseInfoPf;
        [SerializeField] private LikeRewardItemUI _likeRewardItemPf;
        [SerializeField] private EnhancedScroller _scroller;
        [SerializeField] private RectTransform _rtRewardBar;
        [SerializeField] private Image _imgRewardProgress;

        private List<LikeRewardItemUI> _rewardItems = new List<LikeRewardItemUI>();
        private List<BotHouseConfigData> _datas;

        private void Awake()
        {
            _btnClose.onClick.AddListener(OnButtonCloseClicked);
            _scroller.Delegate = this;
        }

        public override void OnOpen()
        {
            base.OnOpen();
            HouseVisitData.OnCurrentLikeChanged += HouseVisitData_OnCurrentLikeChanged;
            UserData.OnUserCurrencyChanged += UserData_OnUserCurrencyChanged;
        }

        public override void OnClose()
        {
            base.OnClose();
            HouseVisitData.OnCurrentLikeChanged -= HouseVisitData_OnCurrentLikeChanged;
            UserData.OnUserCurrencyChanged -= UserData_OnUserCurrencyChanged;
        }

        private void OnButtonCloseClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }

        private void UserData_OnUserCurrencyChanged() => UpdateRewardBar();

        private void HouseVisitData_OnCurrentLikeChanged()
        {
            UpdatePlayerLikeText();
            UpdateRewardBar();
        }

        public void SetData(bool getNewData)
        {
            if (getNewData || _datas == null)
            {
                _datas = HouseVisitManager.Instance.GetRandomBotConfigDatas(4);
            }
            _scroller.ReloadData();
            UpdatePlayerLikeText();
            UpdateRewardBar();
        }

        private void UpdateRewardBar()
        {
            var config = AllConfig.Instance.LikeRewardConfigDic;

            if (_rewardItems.Count > 0)
            {
                _rewardItems.ForEach(x => x.ReloadData());
            }
            else
            {
                foreach (var pair in config)
                {
                    var data = pair.Value;
                    var item = Instantiate(_likeRewardItemPf, _rtRewardBar);
                    item.SetData(data, _rtRewardBar.rect.width);
                    _rewardItems.Add(item);
                }
            }

            var maxAmount = HouseVisitManager.Instance.GetLikeRewardConfigData(config.Keys.Last()).Like;
            _imgRewardProgress.fillAmount = (float)HouseVisitData.Instance.CurrentLike / (float)maxAmount;
        }

        private void UpdatePlayerLikeText() => _tmpPlayerLike.text = HouseVisitData.Instance.CurrentLike.ToString();

        #region EnhancedScroller

        public int GetNumberOfCells(EnhancedScroller scroller) => _datas.Count;

        public float GetCellViewSize(EnhancedScroller scroller, int dataIndex) => _houseInfoPf.Height;

        public EnhancedScrollerCellView GetCellView(EnhancedScroller scroller, int dataIndex, int cellIndex)
        {
            var cellView = scroller.GetCellView(_houseInfoPf) as HouseInfoItemUI;
            cellView.name = "Cell " + dataIndex.ToString();
            cellView.SetData(_datas[dataIndex]);
            return cellView;
        }

        #endregion
    }
}
