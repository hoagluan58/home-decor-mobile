using IsoTools;
using UnityEngine;

namespace YoyoDesign
{
    public abstract class OrderModeState : MonoBehaviour
    {
        public abstract EOrderState StateName { get; }

        protected IsoWorld _isoWorld;
        protected OrderRoomController _orderRoomController;

        public virtual void Init(IsoWorld isoWorld, OrderRoomController orderRoomController)
        {
            _isoWorld = isoWorld;
            _orderRoomController = orderRoomController;
        }

        public virtual void Enter()
        {
        }

        public virtual void Exit()
        {
        }

        public virtual void OnUpdate()
        {
        }
    }
}