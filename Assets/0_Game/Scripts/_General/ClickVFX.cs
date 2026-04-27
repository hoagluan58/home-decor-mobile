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
            if (Input.touchCount <= 0) return;
            var firstTouch = Input.GetTouch(0);
            if (firstTouch.phase == TouchPhase.Ended)
            {
                _vfx.transform.position = _camera.ScreenToWorldPoint(firstTouch.position);
                _vfx.RefreshParticles();
                _vfx.Play();
            }
        }
    }
}
