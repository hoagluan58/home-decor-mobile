using Cysharp.Threading.Tasks;
using System;
using UnityEngine.SceneManagement;

public class SceneLoader : SingletonMono<SceneLoader>
{
    public async UniTask LoadScene(string sceneName, LoadSceneMode loadSceneMode = LoadSceneMode.Additive, bool setActive = false)
    {
        await UniTask.Yield();
        var async = SceneManager.LoadSceneAsync(sceneName, loadSceneMode);

        if (async == null)
        {
            this.LogError("Load scene async invalid!");
            return;
        }

        var scene = SceneManager.GetSceneByName(sceneName);
        async.allowSceneActivation = false;
        while (async.progress < 0.9f)
        {
            await UniTask.Yield();
        }
        await UniTask.Yield();
        async.allowSceneActivation = true;
        await UniTask.Yield();

        if (setActive)
        {
            SceneManager.SetActiveScene(scene);
        }
    }

    public async UniTask UnloadScene(string sceneName)
    {
        var async = SceneManager.UnloadSceneAsync(sceneName);
        while (async.progress < 0.9f)
        {
            await UniTask.Yield();
        }
    }
}