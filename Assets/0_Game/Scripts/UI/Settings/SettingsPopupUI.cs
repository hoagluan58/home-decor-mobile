using DG.Tweening;
using JSAM;
using NFramework;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class SettingsPopupUI : BaseUIView
    {
        [Title("CONTENT PANEL ")]
        [SerializeField] private RectTransform _contentPanel;
        [SerializeField] private Button _closeButton;

        [Title("SETTINGS PANEL")]
        [SerializeField] private SwitchButton _soundSwitch;
        [SerializeField] private SwitchButton _musicSwitch;
        [SerializeField] private SwitchButton _vibrateSwitch;

        #region UNITY METHODS

        private void OnEnable()
        {
            _closeButton.onClick.AddListener(OnCloseButtonClick);
            _soundSwitch.onClick.AddListener(OnSoundSwitchClick);
            _musicSwitch.onClick.AddListener(OnMusicSwitchClick);
            _vibrateSwitch.onClick.AddListener(OnVibrateSwitchClick);
        }

        private void OnDisable()
        {
            _closeButton.onClick.RemoveListener(OnCloseButtonClick);
            _soundSwitch.onClick.RemoveListener(OnSoundSwitchClick);
            _musicSwitch.onClick.RemoveListener(OnMusicSwitchClick);
            _vibrateSwitch.onClick.RemoveListener(OnVibrateSwitchClick);
        }

        #endregion

        #region OVERRIDES

        public override void OnOpen()
        {
            base.OnOpen();
            
            _soundSwitch.IsOn = !AudioManager.SoundMuted;
            _musicSwitch.IsOn = !AudioManager.MusicMuted;
            _vibrateSwitch.IsOn = VibrationManager.isVibrateOn;

            _contentPanel.DOKill();
            _contentPanel.PunchScalePopup(Vector3.one * 0.1f, 0.2f);
        }

        public override void OnClose()
        {
        }

        #endregion

        #region EVENT LISTENERS

        private void OnCloseButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            CloseSelf();
        }

        private void OnSoundSwitchClick()
        {
            AudioManager.SoundMuted = !AudioManager.SoundMuted;
            _soundSwitch.IsOn = !AudioManager.SoundMuted;
            UserData.Instance.IsSoundOn = !AudioManager.SoundMuted;
        }

        private void OnMusicSwitchClick()
        {
            AudioManager.MusicMuted = !AudioManager.MusicMuted;
            _musicSwitch.IsOn = !AudioManager.MusicMuted;
            UserData.Instance.IsMusicOn = !AudioManager.MusicMuted;
        }

        private void OnVibrateSwitchClick()
        {
            VibrationManager.isVibrateOn = !VibrationManager.isVibrateOn;
            _vibrateSwitch.IsOn = VibrationManager.isVibrateOn;
            UserData.Instance.IsVibrateOn = VibrationManager.isVibrateOn;
        }

        #endregion
    }
}