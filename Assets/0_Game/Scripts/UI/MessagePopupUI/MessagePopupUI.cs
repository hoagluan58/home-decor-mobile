using DG.Tweening;
using JSAM;
using NFramework;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class MessagePopupUI : BaseUIView
    {
        [Header("UI OBJECTS")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _okButton;
        [SerializeField] private TextMeshProUGUI _titleTMP;
        [SerializeField] private TextMeshProUGUI _messageTMP;

        [Header("CHARACTER")]
        [SerializeField] private SkeletonGraphic _characterSkeletonGraphic;

        private void OnEnable()
        {
            _okButton.onClick.AddListener(OnOkButtonClick);
        }

        private void OnDisable()
        {
            _okButton.onClick.RemoveListener(OnOkButtonClick);
        }

        public override void OnOpen()
        {
            base.OnOpen();
            _characterSkeletonGraphic.ApplyCharacterOutfit(DressUpData.Instance.GetCharacterOutfitSaveDatas());
            _contentPanel.DOKill();
            _contentPanel.RotateScalePopup(0.2f);
        }

        private void OnOkButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }

        public void SetData(string title, string message)
        {
            _titleTMP.text = title;
            _messageTMP.text = message;
        }
    }
}