using System;
using IsoTools;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureMoveController : MonoBehaviour
    {
        [SerializeField] private FurnitureController _controller;
        [SerializeField] private bool _canMove = true;

        public bool CanMove => _canMove;

#if UNITY_EDITOR
        public void OnCreate(FurnitureController controller)
        {
            _controller = controller;
            _controller.IsoObject.position = Vector3.zero;
        }
#endif

        public void SetPosition(Vector3 newPosition)
        {
            _controller.IsoObject.position = newPosition;

            foreach (var c in _controller.NestedController.Childs)
            {
                c.MoveController.SetPosition(newPosition - c.LocalPosition);
            }
        }

        public void ToggleMoveable(bool canMove) => _canMove = canMove;
    }
}