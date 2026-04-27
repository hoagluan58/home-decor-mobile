using Cysharp.Threading.Tasks;
using DG.Tweening;
using JSAM;
using NFramework;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class HomeMenuUI : BaseUIView
    {
        [Header("UI")]
        [SerializeField] private UserExperienceBarUI _userExperienceBar;
        [SerializeField] private Button _playDecorButton;
        [SerializeField] private Button _playDressUpButton;
        [SerializeField] private Button _btnSquare;
        [SerializeField] private Button _btnOrder;
        [SerializeField] private Button _settingsButton;

        [Header("TUTORIAL")]
        [SerializeField] private GameObject _tutorialOverlay;
        [SerializeField] private Button _decorTutorialButton;
        [SerializeField] private Button _orderTutorialButton;

        [Header("FX")]
        [SerializeField] private RectTransform _midScreen;
        [SerializeField] private RectTransform _starFlyTarget;
        [SerializeField] private RectTransform _diamondFlyTarget;

        private void OnEnable()
        {
            _btnSquare.onClick.AddListener(OnButtonSquareClicked);
            _btnOrder.onClick.AddListener(OnButtonOrderClicked);
            _playDecorButton.onClick.AddListener(OnDecorButtonClick);
            _playDressUpButton.onClick.AddListener(OnPlayDressUpButtonClicked);
            _settingsButton.onClick.AddListener(OnSettingsButtonClick);
            UserData.OnUserLevelUp += OnPlayerLevelUp;
        }

        private void OnDisable()
        {
            _btnSquare.onClick.RemoveListener(OnButtonSquareClicked);
            _btnOrder.onClick.RemoveListener(OnButtonOrderClicked);
            _playDecorButton.onClick.RemoveListener(OnDecorButtonClick);
            _playDressUpButton.onClick.RemoveListener(OnPlayDressUpButtonClicked);
            _settingsButton.onClick.RemoveListener(OnSettingsButtonClick);
            UserData.OnUserLevelUp -= OnPlayerLevelUp;
        }

        private void OnPlayerLevelUp(int level)
        {
            if (level >= 5 && UserData.Instance.CurTutorialIndex == 3)
            {
                UIManager.Instance.CloseAllInLayer(EUILayer.Popup);
                PlayTutorialUnlockNewRoom().Forget();
            }
            else if (UserData.Instance.CurTutorialIndex != 2)
            {
                //DOVirtual.DelayedCall(2f, () => { UIManager.Instance.Open(Define.UIName.LEVEL_UP_REWARD_POPUP); });
            }
        }

        public override void OnOpen()
        {
            base.OnOpen();
            TutorialCheck();
        }

        #region TUTORIAL

        public void TutorialCheck()
        {
            switch (UserData.Instance.CurTutorialIndex)
            {
                case 0:
                    PlayTutorialDecor();
                    break;
                case 1:
                    PlayTutorialOrder();
                    break;
                case 2:
                    DOVirtual.DelayedCall(2f, () =>
                    {
                        UIManager.Instance.CloseAllInLayer(EUILayer.Popup);
                        UIManager.Instance.Open(Define.UIName.GAME_LOOP_POPUP);
                    });
                    break;
                case 3:
                    if (UserData.Instance.CurLevel >= 5)
                    {
                        UIManager.Instance.CloseAllInLayer(EUILayer.Popup);
                        PlayTutorialUnlockNewRoom().Forget();
                    }
                    break;
            }
        }

        public void PlayTutorialDecor()
        {
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation(
                "I have finally moved towards my dream town, I am really looking forward to the future here");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                _tutorialOverlay.SetActive(true);
                _decorTutorialButton.gameObject.SetActive(true);
                _decorTutorialButton.onClick.AddListener(() =>
                {
                    AudioManager.PlaySound(ESound.Click);
                    _tutorialOverlay.SetActive(false);
                    _decorTutorialButton.onClick.RemoveAllListeners();
                    _decorTutorialButton.gameObject.SetActive(false);
                    FeatureNavigator.Instance.Go(EGameFeature.DecorTutorial).Forget();
                });
            });
            DialogController.Instance.SkipEvent.AddListener(OnSkipEvent);

            void OnSkipEvent()
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                _tutorialOverlay.SetActive(false);
                _decorTutorialButton.onClick.RemoveAllListeners();
                _decorTutorialButton.gameObject.SetActive(false);
                UserData.Instance.CurTutorialIndex++;
            }
        }

        public void PlayTutorialOrder()
        {
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("Go to work and earn money so you can buy more items for the house.");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                _tutorialOverlay.SetActive(true);

                _orderTutorialButton.gameObject.SetActive(true);
                _orderTutorialButton.onClick.AddListener(() =>
                {
                    _tutorialOverlay.SetActive(false);
                    _orderTutorialButton.onClick.RemoveAllListeners();
                    _orderTutorialButton.gameObject.SetActive(false);

                    AudioManager.PlaySound(ESound.Click);
                    UIManager.Instance.Open(Define.UIName.ORDER_MODE_MENU);
                });
            });

            DialogController.Instance.SkipEvent.AddListener(OnSkipEvent);

            void OnSkipEvent()
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                _tutorialOverlay.SetActive(false);
                _orderTutorialButton.onClick.RemoveAllListeners();
                _orderTutorialButton.gameObject.SetActive(false);
                if (UserData.Instance.CurTutorialIndex == 1)
                {
                    UserData.Instance.CurTutorialIndex = 2;
                }
            }
        }

        public async UniTaskVoid PlayTutorialUnlockNewRoom()
        {
            if (UIManager.Instance.IsSpecificViewShown(Define.UIName.FLY_ANIMATION, out var popup))
            {
                await UniTask.WaitForSeconds(1.5f);
            }
            else
            {
                await UniTask.Yield();
            }

            CameraController.Instance.SetPosition(0, 10);
            CameraController.Instance.SetSize(Define.Size.HOME_CAMERA_SIZE);

            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("Hmmm, there's still too little furniture");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                _tutorialOverlay.SetActive(true);
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                UnlockNewRoomAnimator.Instance.PlayUnlockNewRoomAnimation(() =>
                {
                    DialogController.Instance.Show();
                    DialogController.Instance.PlayDialogAnimation("Wow, you just extended your house. Let's add more decorations to it now!");
                    DialogController.Instance.SkipEvent.AddListener(OnSkipEvent);
                    DialogController.Instance.NextDialogEvent.AddListener(() =>
                    {
                        _tutorialOverlay.SetActive(true);
                        AudioManager.PlaySound(ESound.Click);
                        DialogController.Instance.NextDialogEvent.RemoveAllListeners();
                        DialogController.Instance.Hide();
                        _decorTutorialButton.gameObject.SetActive(true);
                        _decorTutorialButton.onClick.AddListener(() =>
                        {
                            AudioManager.PlaySound(ESound.Click);
                            _tutorialOverlay.SetActive(false);
                            _decorTutorialButton.onClick.RemoveAllListeners();
                            _decorTutorialButton.gameObject.SetActive(false);
                            UserData.Instance.CurTutorialIndex++;
                            FeatureNavigator.Instance.Go(EGameFeature.Decor).Forget();
                        });
                    });
                }).Forget();
            });
            DialogController.Instance.SkipEvent.AddListener(OnSkipEvent);

            void OnSkipEvent()
            {
                AudioManager.PlaySound(ESound.Click);
                DialogController.Instance.Hide();
                _tutorialOverlay.SetActive(false);
                _decorTutorialButton.onClick.RemoveAllListeners();
                _decorTutorialButton.gameObject.SetActive(false);
                UserData.Instance.CurTutorialIndex++;
                HomeSceneManager.Instance.SecondFloorRoom.UnlockRoom();
            }
        }

        #endregion // TUTORIAL

        #region EVENT LISTENERS

        private void OnSettingsButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.SETTINGS_POPUP);
        }

        private void OnDecorButtonClick()
        {
            AudioManager.PlaySound(ESound.Click);
            FeatureNavigator.Instance.Go(EGameFeature.Decor).Forget();
        }

        private void OnPlayDressUpButtonClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            FeatureNavigator.Instance.Go(EGameFeature.DressUp).Forget();
        }

        private void OnButtonSquareClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open<HouseListPopupUI>(Define.UIName.HOUSES_LIST_POPUP).SetData(true);
        }

        private void OnButtonOrderClicked()
        {
            AudioManager.PlaySound(ESound.Click);
            UIManager.Instance.Open(Define.UIName.ORDER_MODE_MENU);
        }

        #endregion // EVENT LISTENERS
    }
}