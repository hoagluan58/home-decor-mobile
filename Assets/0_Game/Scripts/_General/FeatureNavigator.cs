using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using System;

namespace YoyoDesign
{
    public class FeatureNavigator : SingletonMono<FeatureNavigator>
    {
        private EGameFeature _curFeature = EGameFeature.Init;

        public EGameFeature CurFeature => _curFeature;

        public async UniTaskVoid Go(EGameFeature feature, Action onGoToFeature = null, Action onFullyShowLoading = null, bool isExitPrevious = true,
            bool isShowLoading = true)
        {
            if (_curFeature == feature)
                return;

            if (isShowLoading)
            {
                await TransitionController.Instance.Open();
                onFullyShowLoading?.Invoke();
            }

            await ExitFeature(_curFeature);

            _curFeature = feature;

            await StartFeature(_curFeature);

            if (isShowLoading)
            {
                await UniTask.WaitForSeconds(0.1f);
                await TransitionController.Instance.Close();
            }

            onGoToFeature?.Invoke();
        }

        private async UniTask StartFeature(EGameFeature feature)
        {
            switch (feature)
            {
                case EGameFeature.None:
                    break;
                case EGameFeature.Init:
                    break;
                case EGameFeature.Home:
                    CameraController.Instance.SetPosition(-36, -5);
                    CameraController.Instance.SetSize(Define.Size.HOME_CAMERA_SIZE);
                    await UIManager.Instance.OpenAsync(Define.UIName.HOME_V2_MENU);
                    await SceneLoader.Instance.LoadScene(Define.SceneName.HOME_V2);
                    await UniTask.Yield();
                    break;
                case EGameFeature.Decor:
                    AudioManager.StopMusic(EMusic.BackgroundMusic);
                    AudioManager.PlayMusic(EMusic.BackgroundDecor);
                    await SceneLoader.Instance.LoadScene(Define.SceneName.DECOR);
                    await UIManager.Instance.OpenAsync(Define.UIName.DECOR_MENU);
                    CameraController.Instance.SetPosition(0, 1);
                    CameraController.Instance.SetSize(Define.Size.DECOR_CAMERA_SIZE);
                    break;
                case EGameFeature.Order:
                    UIManager.Instance.Open(Define.UIName.ORDER_PROGRESS_POPUP);
                    await SceneLoader.Instance.LoadScene(Define.SceneName.ORDER_MODE);
                    await UniTask.Yield();
                    break;
                case EGameFeature.DressUp:
                    await SceneLoader.Instance.LoadScene(Define.SceneName.DRESS_UP);
                    await UIManager.Instance.OpenAsync(Define.UIName.DRESS_UP);
                    break;
                case EGameFeature.SquareBotHouse:
                    await SceneLoader.Instance.LoadScene(Define.SceneName.HOUSE_VISIT);
                    await UIManager.Instance.OpenAsync(Define.UIName.HOUSE_INFO_POPUP);
                    CameraController.Instance.SetPosition(0, 6);
                    CameraController.Instance.SetSize(Define.Size.SQUARE_CAMERA_SIZE);
                    break;
                case EGameFeature.SelectCharacter:
                    await UIManager.Instance.OpenAsync(Define.UIName.SELECTCHARACTER_MENU);
                    break;
                case EGameFeature.DecorTutorial:
                    await SceneLoader.Instance.LoadScene(Define.SceneName.DECOR_TUTOR);
                    CameraController.Instance.SetPosition(0, 0);
                    CameraController.Instance.SetSize(Define.Size.DECOR_CAMERA_SIZE);
                    break;
            }
        }

        private async UniTask ExitFeature(EGameFeature feature)
        {
            switch (feature)
            {
                case EGameFeature.None:
                    break;
                case EGameFeature.Init:
                    UIManager.Instance.Close(Define.UIName.INIT, true);
                    await UniTask.Yield();
                    break;
                case EGameFeature.Home:
                    UIManager.Instance.Close(Define.UIName.HOME_V2_MENU);
                    UIManager.Instance.Close(Define.UIName.LAND_MENU);
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.HOME_V2);
                    break;
                case EGameFeature.Decor:
                    AudioManager.StopMusic(EMusic.BackgroundDecor);
                    AudioManager.PlayMusic(EMusic.BackgroundMusic);
                    UIManager.Instance.Close(Define.UIName.DECOR_MENU);
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.DECOR);
                    break;
                case EGameFeature.Order:
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.ORDER_MODE);
                    break;
                case EGameFeature.DressUp:
                    UIManager.Instance.Close(Define.UIName.DRESS_UP);
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.DRESS_UP);
                    break;
                case EGameFeature.SquareBotHouse:
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.HOUSE_VISIT);
                    break;
                case EGameFeature.SelectCharacter:
                    UIManager.Instance.Close(Define.UIName.SELECTCHARACTER_MENU, true);
                    break;
                case EGameFeature.DecorTutorial:
                    await SceneLoader.Instance.UnloadScene(Define.SceneName.DECOR_TUTOR);
                    break;
            }
        }
    }

    public enum EGameFeature
    {
        None,
        Init,
        Home,
        Decor,
        Order,
        DressUp,
        SquareBotHouse,
        DecorTutorial,
        SelectCharacter
    }
}