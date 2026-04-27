using DG.Tweening;
using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class CameraController : SingletonMono<CameraController>
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private float _baseAspect = 16f / 9;

        public Camera Camera => _camera;

        public void SetPosition(float x, float y)
        {
            transform.SetPositionX(x);
            transform.SetPositionY(y);
        }

        public void SetSize(float size) => _camera.orthographicSize = size;

        public void MoveAndZoomAnimation(Vector3 target, float zoomSize, float animTime)
        {
            transform.DOKill();
            transform.DOMove(target, animTime);
            DOVirtual.Float(_camera.orthographicSize, zoomSize, animTime, x => { SetSize(x); });
        }

        [Button]
        public void Adapt() => SetSize(GetAdaptSize(_camera.orthographicSize));

        public float GetAdaptSize(float baseSize)
        {
            var aspect = 0f;

            if (Screen.width > Screen.height) // Landscape
                aspect = (float)Screen.width / Screen.height;
            else // Portrait
                aspect = (float)Screen.height / Screen.width;

            var aspectScale = aspect / _baseAspect;

            return baseSize * aspectScale;
        }
    }
}