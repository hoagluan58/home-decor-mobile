using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using JSAM;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class LevelUpRewardItemUI : MonoBehaviour
    {
        public UnityAction<int> Claim;

        [Header("UI")]
        [SerializeField] private TextMeshProUGUI _levelTMP;

        [Header("CLAIMABLE")]
        [SerializeField] private GameObject _claimableOverlayPanel;

        [Header("CLAIMED")]
        [SerializeField] private GameObject _claimedPanel;
        [SerializeField] private Image _claimedOverlayImage;

        [Header("REWARD")]
        [SerializeField] private Button _claimRewardButton;
        [SerializeField] private Image _specialRewardImage;
        [SerializeField] private List<Image> _normalRewardImages;

        private int _level;

        public int Level => _level;

        private void Start()
        {
            _claimRewardButton.onClick.AddListener(OnClaimButtonClick);
        }

        public void LoadConfig(LevelUpRewardsConfig rewardsConfig)
        {
            _level = rewardsConfig.Level;
            _levelTMP.text = $"Level {_level}";
            _specialRewardImage.sprite = GetRewardSprite(rewardsConfig.SpecialReward);
            _specialRewardImage.SetNativeSize();
            _specialRewardImage.rectTransform.FitTo(_specialRewardImage.rectTransform.parent.GetComponent<RectTransform>(), 14f);
            for (var index = 0; index < rewardsConfig.NormalRewards.Count; index++)
            {
                _normalRewardImages[index].sprite = GetRewardSprite(rewardsConfig.NormalRewards[index]);
                _normalRewardImages[index].SetNativeSize();
                _normalRewardImages[index].rectTransform.FitTo(_normalRewardImages[index].rectTransform.parent.GetComponent<RectTransform>(), 14f);
            }

            return;

            Sprite GetRewardSprite(LevelUpRewardConfig rewardConfig)
            {
                return rewardConfig.RewardType switch
                {
                    ERewardType.Outfit => AllConfig.Instance.OutfitConfigDic.Values
                        .FirstOrDefault(outfit => outfit.Id == rewardConfig.RewardId)!.IconSprite,
                    ERewardType.Furniture => FurnitureManager.Instance
                        .GetFurnitureConfig(rewardConfig.RewardId).Config.Sprite,
                    _ => null
                };
            }
        }

        public void LoadData(int playerLevel, bool isClaimed)
        {
            _claimedOverlayImage.gameObject.SetActive(isClaimed);
            _claimedPanel.SetActive(isClaimed);
            _claimableOverlayPanel.SetActive(playerLevel >= _level && !isClaimed);
        }

        private void OnClaimButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            Claim?.Invoke(_level);
            _claimableOverlayPanel.SetActive(false);
            _claimedOverlayImage.gameObject.SetActive(true);
            _claimedPanel.SetActive(true);
        }
    }
}