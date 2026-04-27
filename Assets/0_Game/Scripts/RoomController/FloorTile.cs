using IsoTools;
using NFramework;
using UnityEngine;

namespace YoyoDesign
{
    public class FloorTile : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;
        [SerializeField] private IsoObject _isoObject;

        public void OnCreate(Vector3 position)
        {
            _isoObject.position = position;
        }

        public void ChangeSprite(Sprite sprite)
        {
            _renderer.sprite = sprite;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _renderer.sortingOrder = sortingOrder;
        }

        public void SetAlpha(float value) => _renderer.SetAlpha(value);
    }
}