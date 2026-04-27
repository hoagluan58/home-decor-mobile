using NFramework;
using System;

namespace YoyoDesign
{
    public class TutorialPhase2PopupUI : BaseUIView
    {
        public static event Action OnShowTutorialLand;
        public override void OnOpen()
        {
            base.OnOpen();
            HomeUnlockLandTutorial.OnUnlockLand += HomeUnlockLandTutorial_OnUnlockLand;
            SetData();
        }

        public override void OnClose()
        {
            base.OnClose();
            HomeUnlockLandTutorial.OnUnlockLand -= HomeUnlockLandTutorial_OnUnlockLand;
        }

        private void SetData()
        {
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("Level up! We can unlock a new room now. Come");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                DialogController.Instance.Hide();
                OnShowTutorialLand?.Invoke();
            });
        }

        private void HomeUnlockLandTutorial_OnUnlockLand()
        {
            DialogController.Instance.Show();
            DialogController.Instance.PlayDialogAnimation("You have successfully unlocked a new room");
            DialogController.Instance.NextDialogEvent.AddListener(() =>
            {
                DialogController.Instance.Hide();
                CloseSelf();
            });
        }

    }
}
