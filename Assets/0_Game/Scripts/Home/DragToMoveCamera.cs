using NFramework;
using System;
using UnityEngine;

namespace YoyoDesign
{
    public class DragToMoveCamera : MonoBehaviour
    {
        // Reference to the background sprite renderer
        [SerializeField] private SpriteRenderer _backgroundSpriteRenderer;

        private Vector3 _origin;
        private Vector3 _difference;
        private bool _isDragging = false, _canDrag = true;
        private Camera _cam;
        private Vector3 _previousMousePosition, _currentMousePosition;

        // Bounds for camera movement
        private float _minX, _maxX, _minY, _maxY;

        public bool IsDragging { get => _isDragging; set => _isDragging = value; }
        public bool CanDrag { get => _canDrag; set => _canDrag = value; }

        private void Start() => _cam = Camera.main; // Get the main camera

        private void Update()
        {
            if (UIManager.Instance.IsPointerOverUIObject()) return;

            CalculateBounds();

            if (!_canDrag)
                return;

            if (Input.GetMouseButtonDown(0))
            {
                _previousMousePosition = Input.mousePosition;
                _isDragging = false;
            }

            // On left mouse button press or touch
            if (Input.GetMouseButton(0))
            {
                _currentMousePosition = Input.mousePosition;
                var delta = Vector3.Distance(_previousMousePosition, _currentMousePosition);

                if (delta > 0.1f)
                {
                    _difference = _cam.ScreenToWorldPoint(Input.mousePosition) - _cam.transform.position;

                    if (_isDragging == false)
                    {
                        _isDragging = true;
                        _origin = _cam.ScreenToWorldPoint(Input.mousePosition);
                    }
                }
                else
                {
                    _isDragging = false;
                }

            }
            else
            {
                _isDragging = false;
            }

            // If dragging, update camera position
            if (_isDragging)
            {
                var newPosition = _origin - _difference;
                // Clamp the camera position to stay within bounds
                newPosition.x = Mathf.Clamp(newPosition.x, _minX, _maxX);
                newPosition.y = Mathf.Clamp(newPosition.y, _minY, _maxY);
                _cam.transform.position = newPosition;
            }
        }

        private void CalculateBounds()
        {
            // Get the bounds of the sprite renderer
            var bounds = _backgroundSpriteRenderer.bounds;

            // Calculate the orthographic bounds for the camera
            var vertExtent = _cam.orthographicSize;
            var horzExtent = vertExtent * Screen.width / Screen.height;

            // Set the bounds based on the sprite renderer's bounds and camera's orthographic size
            _minX = bounds.min.x + horzExtent;
            _maxX = bounds.max.x - horzExtent;
            _minY = bounds.min.y + vertExtent;
            _maxY = bounds.max.y - vertExtent;
        }
    }
}
