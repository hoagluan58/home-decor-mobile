using UnityEngine;

namespace YoyoDesign
{
    public class DecorModeBackground : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;

        public void Start()
        {
            _canvas.worldCamera = Camera.main;
        }
    }
}
