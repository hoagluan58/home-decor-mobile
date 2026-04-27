using Sirenix.OdinInspector;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace YoyoDesign
{
    public class ScreenshotHandler : MonoBehaviour
    {
        [SerializeField] private Camera _camera;

        public void TakeScreenshot(string fileName, int width, int height)
        {
            RenderTexture screenTexture = new RenderTexture(width, height, 16);
            _camera.targetTexture = screenTexture;
            RenderTexture.active = screenTexture;
            _camera.Render();

            Texture2D renderedTexture = new Texture2D(width, height);
            renderedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = null;

            byte[] byteArray = renderedTexture.EncodeToPNG();
            var savePath = Path.Combine(Application.persistentDataPath, fileName);
            File.WriteAllBytes(savePath, byteArray);
            Logger.Log(this, $"Screenshot saved! {savePath}");
        }

#if UNITY_EDITOR
        [SerializeField] private string _savePath;

        [Button]
        public void TakeScreenShotOrderMode(string fileName, int width = 1024, int height = 1024)
        {
            RenderTexture screenTexture = new RenderTexture(width, height, 16);
            _camera.targetTexture = screenTexture;
            RenderTexture.active = screenTexture;
            _camera.Render();

            Texture2D renderedTexture = new Texture2D(width, height);
            renderedTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            RenderTexture.active = null;

            byte[] byteArray = renderedTexture.EncodeToJPG();
            File.WriteAllBytes(_savePath + $"/{fileName}.png", byteArray);
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
#endif
    }
}