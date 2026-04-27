using DG.Tweening;
using JSAM;
using NFramework;
using Spine.Unity;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class ConfirmDeleteRoomPopupUI : BaseUIView
    {
        [Header("UI OBJECTS")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _yesBtn;
        [SerializeField] private Button _noBtn;
        
        [Header("CHARACTER")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;

        private void OnEnable()
        {
            _yesBtn.onClick.AddListener(OnYesButtonClick);
            _noBtn.onClick.AddListener(OnNoButtonClick);
        }

        private void OnDisable()
        {
            _yesBtn.onClick.RemoveListener(OnYesButtonClick);
            _noBtn.onClick.RemoveListener(OnNoButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _characterSkeletonGraphic.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            _contentPanel.DOKill();
            _contentPanel.RotateScalePopup(0.2f);
        }

        private void OnYesButtonClick()
        {
            AudioManager.PlaySound(ESound.Remove);
            DecorRoomManager.Instance.CurRoom.FurnitureController.ReleaseAll();
            CloseSelf();
          
        }

        private void OnNoButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }
        
    }
}