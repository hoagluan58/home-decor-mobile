using JSAM;
using NFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class LikeRewardItemUI : MonoBehaviour
    {
        [SerializeField] private Button _btnClaim;
        [SerializeField] private TextMeshProUGUI _tmpLike;
        [SerializeField] private GameObject _goDisable;
        [SerializeField] private Image _claimedImage;

        private RectTransform _rectTransform;
        private LikeRewardConfigData _data;

        private void Awake()
        {
            _btnClaim.onClick.AddListener(OnButtonClaimClicked);
            _rectTransform = GetComponent<RectTransform>();
        }

        private void OnButtonClaimClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open<RewardPopupUI>(Define.UIName.REWARD_POPUP).SetData(_data.Reward, multiply =>
            {
                HouseVisitManager.Instance.ClaimLikeReward(_data.Id, multiply);
            });
        }

        public void SetData(LikeRewardConfigData data, float maxWidth)
        {
            _data = data;
            _tmpLike.text = $"{_data.Like}";
            _rectTransform.anchoredPosition = new Vector2(maxWidth * _data.PositionOnBar, 0);
            UpdateButtonClaim();
        }

        public void ReloadData() => UpdateButtonClaim();

        private void UpdateButtonClaim()
        {
            _goDisable.SetActive(!HouseVisitManager.Instance.CanClaimLikeReward(_data.Id));
            _claimedImage.gameObject.SetActive(HouseVisitManager.Instance.IsLikeRewardClaimed(_data.Id));
        }
            
            
    }
}
