using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class WallController : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer _renderer;

        private string _curWallId;
        public string CurWallId => _curWallId; 

        [Button]
        public void OnSpawn(Vector3 roomSize)
        {
            SetSortingOrder(Define.SortingOrder.WALL);
            _renderer.transform.position = new Vector3(0, (roomSize.x + roomSize.y) * 0.3363f);
        }

        public void ChangeWall(string wallId, Sprite sprite)
        {
            _curWallId = wallId;
            _renderer.sprite = sprite;
        }

        public void SetSortingOrder(int sortingOrder)
        {
            _renderer.sortingOrder = sortingOrder;
        }
    }
}
