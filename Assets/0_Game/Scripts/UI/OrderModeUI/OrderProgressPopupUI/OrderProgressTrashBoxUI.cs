using Redcode.Extensions;
using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class OrderProgressTrashBoxUI : MonoBehaviour
    {
        [SerializeField] private Image _trashBoxImage;

        private bool _isHighlight = false;
        private RectTransform _rectTransform;

        public Vector3 Position => _rectTransform.position;

        public bool IsHighlight => _isHighlight;

        private void Awake() => _rectTransform = _trashBoxImage.GetComponent<RectTransform>();

        public bool IsWithinBounds(Vector3 mousePosition)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(_rectTransform, mousePosition, CameraController.Instance.Camera, out var localPoint);
            return _rectTransform.rect.Contains(localPoint);
        }

        public void Highlight()
        {
            _isHighlight = true;
            transform.SetLocalScale(1.05f);
        }

        public void Normalize()
        {
            _isHighlight = false;
            transform.SetLocalScale(1f);
        }

        public void SetActive(bool value) => this.gameObject.SetActive(value);
    }
}
