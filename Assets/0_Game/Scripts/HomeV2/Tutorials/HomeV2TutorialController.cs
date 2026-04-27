using Cysharp.Threading.Tasks;
using NFramework;
using System;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeV2TutorialController : SingletonMono<HomeV2TutorialController>
    {
        public static event Action<ERaycastBlockType> OnShowRayCastBlocker;

        [SerializeField] private HomeIntroTutorial _introTutorial;
        [SerializeField] private HomeFocusLandTutorial _focusLandTutorial;
        [SerializeField] private HomeUnlockLandTutorial _unlockLandTutorial;

        public bool CheckTutorial()
        {
            switch (UserData.Instance.CurTutorialIndex)
            {
                case Define.TutorialIndex.NEW_PLAYER:
                    PlayTutorialIntro();
                    return true;
                case Define.TutorialIndex.DONE_ORDER:
                    PlayTutorialFocusLand();
                    return true;
                case Define.TutorialIndex.DONE_UNLOCK_FURNITURE:
                    var canUnlockLand = HomeLandData.Instance.CanUnlockLand(Define.DefaultId.TUTORIAL_UNLOCK_LAND_ID);
                    if (canUnlockLand)
                    {
                        PlayTutorialUnlockLand();
                        return true;
                    }
                    return false;
            }

            return false;
        }

        private async void PlayTutorialIntro()
        {
            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.WhiteBlock);

            await _introTutorial.StartTutorial();
            await UniTask.Delay(1000);

            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.None);

            var tutorialPhase1Popup = UIManager.Instance.Open(Define.UIName.TUTORIAL_PHASE_1_POPUP);
            await UniTask.WaitUntil(() => tutorialPhase1Popup == null || tutorialPhase1Popup.gameObject.activeSelf == false);
        }

        private async void PlayTutorialFocusLand()
        {
            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.BlackBlock);
            _focusLandTutorial.StartTutorial();
            await UniTask.WaitUntil(() => !_focusLandTutorial.IsUpdate);
            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.None);
        }

        private async void PlayTutorialUnlockLand()
        {
            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.BlackBlock);
            _unlockLandTutorial.StartTutorial();
            await UniTask.WaitUntil(() => _unlockLandTutorial.IsDoneTutorial);
            OnShowRayCastBlocker?.Invoke(ERaycastBlockType.None);
        }

        public enum ERaycastBlockType
        {
            None,
            WhiteBlock,
            BlackBlock,
        }
    }
}
