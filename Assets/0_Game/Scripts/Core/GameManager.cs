using Cysharp.Threading.Tasks;
using JSAM;
using NFramework;
using UnityEngine;
using YoyoDesign;

public class GameManager : SingletonMono<GameManager>
{
    [SerializeField] private GameObject _goIngameDebugConsole;

    public ObservableValue<EGameState> CurGameState = new ObservableValue<EGameState>(EGameState.Init);

    public void Start() => Initialize().Forget();

    private async UniTaskVoid Initialize()
    {
        // Wait a frame after start to prevent bugs.
        await UniTask.Yield();

        // Open init screen
        var initScreen = await UIManager.Instance.OpenAsync(Define.UIName.INIT) as InitUI;

        // Init gameplay
        Application.targetFrameRate = 60;
        Input.multiTouchEnabled = false;

        await PoolManager.Instance.Init();

        AllConfig.Instance.Init();

        await UniTask.Yield();

        RegisterSaveData();

        // Init settings
        AudioManager.SoundMuted = !UserData.Instance.IsSoundOn;
        AudioManager.MusicMuted = !UserData.Instance.IsMusicOn;
        VibrationManager.isVibrateOn = UserData.Instance.IsVibrateOn;
        AudioManager.PlayMusic(EMusic.BackgroundMusic);

        // Init controllers & managers
        HouseVisitData.Instance.StartAutoUpdateLike();
        UserData.Instance.CanClaimNPCReward = UserData.Instance.CanClaimNPCReward;

        // Complete init
        initScreen?.RunLoadingAnimation(Define.TimeLength.INIT);
        await UniTask.WaitForSeconds(Define.TimeLength.INIT + 0.5f);
        initScreen?.GoToHome();

        if (DeviceInfo.IsDevelopment)
        {
            _goIngameDebugConsole.SetActive(true);
        }
        else
        {
            Destroy(_goIngameDebugConsole);
        }

        InternetChecker.OnStatusChanged += InternetChecker_OnStatusChanged;
        return;

        void RegisterSaveData()
        {
            SaveManager.Instance.RegisterSaveData(UserData.Instance);
            SaveManager.Instance.RegisterSaveData(HomeLandData.Instance);
            SaveManager.Instance.RegisterSaveData(DressUpData.Instance);
            SaveManager.Instance.RegisterSaveData(HouseVisitData.Instance);
            SaveManager.Instance.RegisterSaveData(DecorModeData.Instance);
            SaveManager.Instance.RegisterSaveData(OrderModeData.Instance);
            SaveManager.Instance.RegisterSaveData(LevelUpRewardData.Instance);
            SaveManager.Instance.Load();
        }
    }

    private void InternetChecker_OnStatusChanged(bool status)
    {
        if (status)
        {
            UIManager.Instance.Close(Define.UIName.NO_INTERNET_POPUP);
        }
        else
        {
            UIManager.Instance.Open(Define.UIName.NO_INTERNET_POPUP);
        }
    }
}

public enum EGameState
{
    Init,
    Playing,
    Pause,
}