using System.Collections.Generic;
using DG.Tweening;
using JSAM;
using NFramework;
using Redcode.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class LevelUpRewardPopupUI : BaseUIView
    {
        [Header("UI")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _closeButton;
        [SerializeField] private ScrollRect _itemsScrollRect;
        [SerializeField] private ClaimLevelUpRewardAnimator _claimLevelUpRewardAnimator;

        [Header("ASSETS")]
        [SerializeField] private LevelUpRewardItemUI _itemPrefab;

        private Dictionary<int, LevelUpRewardItemUI> _itemsDic;

        private void Start()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            SpawnRewardItems();
            LoadData();
            _itemsScrollRect.verticalNormalizedPosition = UserData.Instance.CurLevel <= 3 ? 0 :
                UserData.Instance.CurLevel <= 5 ? (UserData.Instance.CurLevel - 1) / 10f :
                UserData.Instance.CurLevel / 10f;
            _contentPanel.DOKill();
            _contentPanel.SetLocalScale(1f);
            _contentPanel.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
        }

        public void SpawnRewardItems()
        {
            if (_itemsDic != null) return;
            _itemsDic = new();
            var configs = AllConfig.Instance.LevelUpRewardsConfigDic;
            foreach (var cfg in configs.Values)
            {
                var newItem = Instantiate(_itemPrefab, _itemsScrollRect.content);
                newItem.LoadConfig(cfg);
                _itemsDic.Add(cfg.Level, newItem);
                newItem.Claim += OnClaimItem;
                newItem.transform.SetAsFirstSibling();
            }
        }

        public void LoadData()
        {
            var curPlayerLevel = UserData.Instance.CurLevel;
            foreach (var item in _itemsDic.Values)
            {
                var isClaimed = LevelUpRewardData.Instance.IsLevelRewarded(item.Level);
                item.LoadData(curPlayerLevel, isClaimed);
            }
        }

        public void OnClaimItem(int level)
        {
            // Save to data
            var rewardConfig = AllConfig.Instance.LevelUpRewardsConfigDic[level];
            ClaimReward(rewardConfig.SpecialReward);
            foreach (var cfg in rewardConfig.NormalRewards)
            {
                ClaimReward(cfg);
            }
            LevelUpRewardData.Instance.ClaimReward(level);

            // Play animation
            _claimLevelUpRewardAnimator.LoadData(rewardConfig);
            _claimLevelUpRewardAnimator.PlayAnimation().Forget();

            return;

            void ClaimReward(LevelUpRewardConfig rewardConfig)
            {
                switch (rewardConfig.RewardType)
                {
                    case ERewardType.Outfit:
                        DressUpData.Instance.UnlockOutfit(rewardConfig.RewardId);
                        break;
                    case ERewardType.Furniture:
                        UserData.Instance.UnlockFurniture(rewardConfig.RewardId);
                        break;
                }
            }
        }

        private void OnCloseButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }
    }
}