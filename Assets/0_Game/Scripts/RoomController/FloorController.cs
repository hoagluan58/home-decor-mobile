using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class FloorController : MonoBehaviour
    {
        [InfoBox("Always even number")]
        [SerializeField] private Vector2 _curFloorSize;
        [SerializeField] private FloorTile _tilePrefab;
        [SerializeField] private Transform _tileContainer;
        [SerializeField] private List<FloorTile> _floorTiles;

        private string _curFloorId;
        public string CurFloorId => _curFloorId;

        [Button]
        public void SpawnFloor()
        {
            DestroyFloor();
            for (var i = 0; i < _curFloorSize.x; i += 2)
            {
                for (var j = 0; j < _curFloorSize.y; j += 2)
                {
                    var tile = Instantiate(_tilePrefab, _tileContainer);
                    tile.OnCreate(new Vector3(i, j, -0.5f));
                    _floorTiles.Add(tile);
                }
            }
        }

        public void DestroyFloor()
        {
            foreach (var f in _floorTiles)
            {
#if UNITY_EDITOR
                DestroyImmediate(f.gameObject);
#else
             Destroy(f.gameObject);
#endif
            }
            _floorTiles.Clear();
        }

        public void ChangeFloor(string floorId, Sprite sprite)
        {
            _curFloorId = floorId;
            foreach (var f in _floorTiles)
            {
                f.ChangeSprite(sprite);
            }
        }

        public void SetActiveFloor(bool isActive)
        {
            foreach (var tile in _floorTiles)
            {
                tile.gameObject.SetActive(isActive);
            }
        }

        public void SetSortingOrder(int sortingOrder)
        {
            foreach (var tile in _floorTiles)
            {
                tile.SetSortingOrder(sortingOrder);
            }
        }

        public void SetVisibleFloor(bool isVisible)
        {
            foreach (var tile in _floorTiles)
            {
                tile.SetAlpha(isVisible ? 1 : 0f);
            }
        }
    }
}