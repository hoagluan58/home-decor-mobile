using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Coffee.UIEffects;
using Coffee.UIExtensions;
using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class ClaimLevelUpRewardAnimator : MonoBehaviour
    {
        [SerializeField] private GameObject _giftBox;
        [SerializeField] private UIParticle _openBoxParticle;
        [SerializeField] private RectTransform _rewardPanel;
        [SerializeField] private RectTransform _giftOpenedPanel;
        [SerializeField] private Image _specialRewardImage;
        [SerializeField] private List<Image> _normalRewardImages;
        [SerializeField] private List<UIShiny> _shinnyEffects;

        public void LoadData(LevelUpRewardsConfig rewardsConfig)
        {
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
        
        public async UniTaskVoid PlayAnimation()
        {
            gameObject.SetActive(true);
            _giftOpenedPanel.gameObject.SetActive(false);
            _giftBox.gameObject.SetActive(true);
            await UniTask.WaitForSeconds(1f, cancellationToken: destroyCancellationToken);
            AudioManager.PlaySound(ESound.AdsItem);
            _giftOpenedPanel.gameObject.SetActive(true);
            _openBoxParticle.Play();
            _giftBox.gameObject.SetActive(false);
            await _rewardPanel.DOPunchScale(Vector3.one * 0.1f, 0.3f);
            _shinnyEffects.ForEach(fx => fx.Play());
            await UniTask.WaitForSeconds(3.5f, cancellationToken: destroyCancellationToken);
            gameObject.SetActive(false);
        }
    }
}
