using JSAM;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeNPCController : MonoBehaviour
    {
        [SerializeField] private string _skinName;
        [SerializeField] private string _npcName;
        [SerializeField] private GameObject _threeDots;
        [SerializeField] private GameObject _star;

        private bool _isHaveReward;

        public void OnMouseDown()
        {
            if (UIManager.Instance.IsPointerOverUIObject()) return;

            AudioManager.PlaySound(ESound.Click);
            DialogController.Instance.Show(_npcName, false, _skinName);

            if (_isHaveReward)
            {
                DialogController.Instance.PlayDialogAnimation(
                    "I'm thrilled to see another wonderful person in town! I have a special gift just for you.");
                DialogController.Instance.NextDialogEvent.AddListener(OnSkipDialog);
                DialogController.Instance.SkipEvent.AddListener(OnSkipDialog);

                void OnSkipDialog()
                {
                    SetReward(false);
                    AudioManager.PlaySound(ESound.Click);
                    DialogController.Instance.Hide();
                    UserData.Instance.ModifyCurrencyDic(ECurrencyType.Star, 2);
                    var flyAnimationUI = UIManager.Instance.Open<FlyAnimationUI>(Define.UIName.FLY_ANIMATION);
                    flyAnimationUI.PlayStarAnim(UserData.Instance.GetCurrencyAmount(ECurrencyType.Star), 2);
                }
            }
            else
            {
                DialogController.Instance.PlayDialogAnimation("Welcome to the new town, enjoy life with everyone here!");
                DialogController.Instance.NextDialogEvent.AddListener(OnSkipDialog);
                DialogController.Instance.SkipEvent.AddListener(OnSkipDialog);

                void OnSkipDialog()
                {
                    AudioManager.PlaySound(ESound.Click);
                    DialogController.Instance.Hide();
                }
            }
        }

        public void SetReward(bool isHaveReward)
        {
            _isHaveReward = isHaveReward;
            _threeDots.gameObject.SetActive(!_isHaveReward);
            _star.gameObject.SetActive(_isHaveReward);
        }
    }
}