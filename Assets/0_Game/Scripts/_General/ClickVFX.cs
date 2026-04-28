using System.Collections;
using System.Collections.Generic;
using Coffee.UIExtensions;
using UnityEngine;

namespace YoyoDesign
{
    public class ClickVFX : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        [SerializeField] private UIParticle _vfx;

        private void Update()
        {
            if (!InputHelper.HasInput()) return;
            if (InputHelper.GetTouchPhase() == TouchPhase.Ended)
            {
                var screenPosition = InputHelper.GetScreenPosition();
                _vfx.transform.position = _camera.ScreenToWorldPoint(screenPosition);
                _vfx.RefreshParticles();
                _vfx.Play();
            }
        }
    }
}
