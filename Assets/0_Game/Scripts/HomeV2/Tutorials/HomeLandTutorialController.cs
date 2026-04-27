using Cysharp.Threading.Tasks;
using System;
using System.Threading;
using UnityEngine;

namespace YoyoDesign
{
    public class HomeLandTutorialController : MonoBehaviour
    {
        private CancellationTokenSource _token = new CancellationTokenSource();
        private HomeLand _land;

        public void Init(HomeLand land) => _land = land;

        public void OnExit() => _token?.Cancel();

        private void OnDestroy()
        {
            _token?.Cancel();
        }

        public async void PlayTutorial()
        {
            try
            {
                _token = new CancellationTokenSource();

                if (UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_ORDER)
                {
                    _land.WorldUI.SetActive(false);
                    var decorBoxFurList = _land.LandProgress.GetDecorBoxFurList();
                    if (decorBoxFurList.Count > 0)
                    {
                        _land.DecorController.CurDecorBox.PlayHandTutorial(true);
                    }
                    await UniTask.WaitUntil(() => _land.DecorController.CurDecorBox == null && _land.DecorController.IsPlacedFur, cancellationToken: _token.Token);
                    await UniTask.WaitForSeconds(0.5f, cancellationToken: _token.Token);
                    UserData.Instance.CurTutorialIndex = Define.TutorialIndex.DONE_DECOR_FURNITURE;
                }

                if (UserData.Instance.CurTutorialIndex == Define.TutorialIndex.DONE_DECOR_FURNITURE)
                {
                    _land.WorldUI.SetActive(true);

                    var unlockFurBtnPos = _land.WorldUI.UnlockFurButtonPos;
                    if (unlockFurBtnPos == Vector3.zero)
                        return;

                    var zoomPos = LandController.Instance.GetClampedCameraPosition(unlockFurBtnPos, Define.Size.LAND_CAMERA_SIZE);
                    await LandController.Instance.ZoomCamera(zoomPos, Define.Size.LAND_CAMERA_SIZE, 0.8f);
                }
            }
            catch (Exception ex)
            {
                Logger.LogWarning(this, ex);
            }

        }
    }
}
