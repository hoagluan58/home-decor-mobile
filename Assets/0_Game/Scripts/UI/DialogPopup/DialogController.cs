using DG.Tweening;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class DialogController : SingletonMono<DialogController>
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Button _nextDialogButton;
        [SerializeField] private Button _nextButton;
        [SerializeField] private Button _skipButton;
        [SerializeField] private TextMeshProUGUI _dialogTMP;
        [SerializeField] private TextMeshProUGUI _nameTMP;

        [Header("CHARACTER")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;
        [SerializeField] private SkeletonGraphic _npcSkeletonGraphic;

        public UnityEvent NextDialogEvent => _nextDialogButton.onClick;
        public UnityEvent SkipEvent => _skipButton.onClick;

        private void Awake() => _nextButton.onClick.AddListener(OnNextButtonClicked);

        public void Show(string charName = "Jessica", bool isMainCharacter = true, string npcSkin = "NPC_1")
        {
            _characterSkeletonGraphic.gameObject.SetActive(false);
            _npcSkeletonGraphic.gameObject.SetActive(false);

            if (isMainCharacter)
            {
                _characterSkeletonGraphic.gameObject.SetActive(true);
                _characterSkeletonGraphic.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            }
            else
            {
                _npcSkeletonGraphic.gameObject.SetActive(true);
                _npcSkeletonGraphic.Skeleton.SetSkin(npcSkin);
                _npcSkeletonGraphic.Skeleton.SetSlotsToSetupPose();
                _npcSkeletonGraphic.AnimationState.SetAnimation(0, "idle", true);
            }

            _dialogTMP.text = "";
            _canvas.enabled = true;
            _nameTMP.text = charName;
        }

        public void Hide()
        {
            _canvas.enabled = false;
            NextDialogEvent.RemoveAllListeners();
            SkipEvent.RemoveAllListeners();
        }

        public void PlayDialogAnimation(string message)
        {
            _dialogTMP.DOKill();
            _dialogTMP.text = "";
            _dialogTMP.DOText(message, Define.TimeLength.DIALOG_TIME)
                .SetEase(Ease.Linear);
        }

        private void OnNextButtonClicked() => NextDialogEvent?.Invoke();
    }
}