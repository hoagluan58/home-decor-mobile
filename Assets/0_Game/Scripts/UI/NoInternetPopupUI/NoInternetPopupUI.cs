using DG.Tweening;
using JSAM;
using NFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class NoInternetPopupUI : BaseUIView
    {
        [Title("CONTENT PANEL")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _okButton;

        private void OnEnable() => _okButton.onClick.AddListener(OnOkButtonClicked);

        private void OnDisable() => _okButton.onClick.RemoveListener(OnOkButtonClicked);

        public override void OnOpen()
        {
            base.OnOpen();
            _contentPanel.DOKill();
            _contentPanel.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
        }

        private void OnOkButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            if (InternetChecker.Instance.InternetStatus)
            {
                CloseSelf();
            }
            else
            {
                DeviceInfo.OpenDeviceWifiSetting();
            }
        }
    }
}
