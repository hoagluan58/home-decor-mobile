using JSAM;
using NFramework;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class UserExperienceBarUI : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _tmpExp;
        [SerializeField] private TextMeshProUGUI _tmpLevel;
        [SerializeField] private Image _imgFill;
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        [SerializeField] private Button _dressUpButton;

        private void OnEnable()
        {
            UpdatePlayerAvatar();
            UpdateLevelText();
            UpdateExpBar();
            UserData.OnUserExpChanged += UserData_OnUserExpChanged;
        }

        private void OnDisable() => UserData.OnUserExpChanged -= UserData_OnUserExpChanged;

        private void Start()
        {
            _dressUpButton.onClick.AddListener(OnDressUpButtonClick);
        }

        private void OnDressUpButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.LEVEL_UP_REWARD_POPUP);
        }

        private void UserData_OnUserExpChanged()
        {
            _characterSkeletonGraphic.AnimationState.SetAnimation(0, "fun", false);
            _characterSkeletonGraphic.AnimationState.AddAnimation(0, "idle", true, 0);

            UpdateLevelText();
            UpdateExpBar();
        }

        private void UpdatePlayerAvatar()
        {
            _characterSkeletonGraphic.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
        }

        private void UpdateLevelText() => _tmpLevel.text = $"Level {UserData.Instance.CurLevel}";

        private void UpdateExpBar()
        {
            var config = UserManager.Instance.GetUserLevelConfigData(UserData.Instance.CurLevel);
            var curLevelExp = UserData.Instance.CurLevelExp;
            var curLevel = UserData.Instance.CurLevel;

            _tmpExp.text = UserManager.Instance.IsMaxLevel(curLevel) ? "MAX" : $"{curLevelExp}/{config.ExpNeed}";
            _imgFill.fillAmount = curLevelExp / config.ExpNeed;
        }
    }
}