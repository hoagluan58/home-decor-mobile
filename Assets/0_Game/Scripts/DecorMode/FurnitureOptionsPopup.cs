using UnityEngine;
using UnityEngine.UI;

namespace YoyoDesign
{
    public class FurnitureOptionsPopup : MonoBehaviour
    {
        [SerializeField] private Canvas _canvas;
        [SerializeField] private Image _doneButton;
        [SerializeField] private Image _rotateButton;
        [SerializeField] private Image _clearButton;

        private FurnitureController _currentFurnitureFollow;

        private void Start()
        {
            _canvas.worldCamera = Camera.main;
        }

        private void FixedUpdate()
        {
            if (_currentFurnitureFollow == null)
            {
                gameObject.SetActive(false);
                return;
            }
            transform.position = GetFollowPosition
            (
                _currentFurnitureFollow.Position,
                _currentFurnitureFollow.Size
            );
            _rotateButton.gameObject.SetActive
            (
                _currentFurnitureFollow.Config.CanFlip &&
                !_currentFurnitureFollow.Config.IsAutoFlip
            );
        }

        public void Follow(FurnitureController fur)
        {
            gameObject.SetActive(true);
            _currentFurnitureFollow = fur;
        }

        public void Release()
        {
            _currentFurnitureFollow = null;
            gameObject.SetActive(false);
        }

        public Vector3 GetFollowPosition(Vector3 furPos, Vector3 furSize)
        {
            var result = Vector3.zero;

            if (furSize.x > furSize.y)
            {
                furSize.x /= 2;
            }
            else if (furSize.y > furSize.x)
            {
                furSize.y /= 2;
            }

            // (x - y) * 0.59f;
            result.x = (furPos.x + furSize.x - furPos.y - furSize.y) * 0.59f;
            // (x + y + 2z) * 0.3363f;
            result.y = (furPos.x + furSize.x + furPos.y + furSize.y + 2 * (furPos.z + furSize.z + 2)) * 0.3363f;

            result.x = Mathf.Clamp(result.x, -4.44f, 4.44f);

            return result;
        }
    }
}