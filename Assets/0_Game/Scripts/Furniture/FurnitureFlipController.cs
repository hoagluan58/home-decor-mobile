using Redcode.Extensions;
using Sirenix.OdinInspector;
using UnityEngine;

namespace YoyoDesign
{
    public class FurnitureFlipController : MonoBehaviour
    {
        [SerializeField] private FurnitureController _controller;
        [SerializeField] private FurnitureDirection _curDirection = FurnitureDirection.Left;
        private FurnitureDirection _baseDirection => _controller.Config.BaseDirection;
        public FurnitureDirection CurDirection => _curDirection;
        public bool IsDefault => _curDirection == _baseDirection;


#if UNITY_EDITOR
        public void OnCreate(FurnitureController controller)
        {
            _controller = controller;
            _curDirection = _controller.Config.BaseDirection;
        }
#endif

        public void AutoFlip(FurnitureDirection direction)
        {
            if (!_controller.Config.IsAutoFlip) return;
            if(direction == _curDirection) return;
            FlipTo(direction);
            FlipChild();
        }

        [Button]
        public void FlipTo(FurnitureDirection flipTo)
        {
            if (!_controller.Config.CanFlip) return;
            _curDirection = flipTo;

            // Flip size
            (_controller.IsoObject.sizeX, _controller.IsoObject.sizeY) = _curDirection == _baseDirection
                ? (_controller.Config.Size.x, _controller.Config.Size.y)
                : (_controller.Config.Size.y, _controller.Config.Size.x);
            
            // Flip renderer
            _controller.VisualController.OnFlip(_curDirection, _controller.Config.BaseDirection);
        }

        public void FlipChild()
        {
            // Flip child with parent
            foreach (var c in _controller.NestedController.Childs)
            {
                c.FlipController.FlipNegative();
                c.NestedController.FlipLocalPosition();
                c.MoveController.SetPosition(_controller.Position - c.LocalPosition);
            }
        }

        public void FlipNegative()
        {
            if (!_controller.Config.CanFlip) return;
            var negativeDirection = _curDirection == FurnitureDirection.Left
                ? FurnitureDirection.Right
                : FurnitureDirection.Left;
            FlipTo(negativeDirection);
            FlipChild(); 
        }
    }
}