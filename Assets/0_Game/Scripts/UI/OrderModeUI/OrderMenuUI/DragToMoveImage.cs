using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.EventSystems;

namespace YoyoDesign
{
    public class DragToMoveImage : MonoBehaviour, IDragHandler
    {
        [SerializeField] private RectTransform _parentRectTransform;
        private RectTransform _imageRectTransform;

        void Start()
        {
            _imageRectTransform = GetComponent<RectTransform>();
            ScaleImage();
        }

        void ScaleImage()
        {
            float parentWidth = _parentRectTransform.rect.width;
            float parentHeight = _parentRectTransform.rect.height;

            float imageWidth = _imageRectTransform.rect.width;
            float imageHeight = _imageRectTransform.rect.height;

            float widthScale = parentWidth / imageWidth;
            float heightScale = parentHeight / imageHeight;

            float minScale = Mathf.Max(widthScale, heightScale);

            _imageRectTransform.localScale = new Vector3(minScale + 0.1f, minScale + 0.1f, 1f); // Ensure it's always slightly larger
        }

        public void OnDrag(PointerEventData eventData)
        {
            Vector2 newPos = _imageRectTransform.anchoredPosition + eventData.delta;

            float minX = _parentRectTransform.rect.width / 2 - _imageRectTransform.rect.width * _imageRectTransform.localScale.x / 2;
            float maxX = _imageRectTransform.rect.width * _imageRectTransform.localScale.x / 2 - _parentRectTransform.rect.width / 2;
            float minY = _parentRectTransform.rect.height / 2 - _imageRectTransform.rect.height * _imageRectTransform.localScale.y / 2;
            float maxY = _imageRectTransform.rect.height * _imageRectTransform.localScale.y / 2 - _parentRectTransform.rect.height / 2;

            newPos.x = Mathf.Clamp(newPos.x, minX, maxX);
            newPos.y = Mathf.Clamp(newPos.y, minY, maxY);

            _imageRectTransform.anchoredPosition = newPos;
        }
    }
}
