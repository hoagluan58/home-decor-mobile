using UnityEngine;
using IsoTools;

namespace YoyoDesign
{
    public static class InputHelper
    {
        public static bool HasInput()
        {
            return Input.touchCount > 0 || Input.GetMouseButton(0) || Input.GetMouseButtonUp(0) || Input.GetMouseButtonDown(0);
        }

        public static Vector3 GetIsoPosition(IsoWorld isoWorld)
        {
            if (Input.touchCount > 0)
                return isoWorld.TouchIsoPosition(0);
            return isoWorld.MouseIsoPosition();
        }

        public static Vector2 GetScreenPosition()
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).position;
            return Input.mousePosition;
        }

        public static TouchPhase GetTouchPhase()
        {
            if (Input.touchCount > 0)
                return Input.GetTouch(0).phase;
            
            if (Input.GetMouseButtonDown(0))
                return TouchPhase.Began;
            if (Input.GetMouseButtonUp(0))
                return TouchPhase.Ended;
            if (Input.GetMouseButton(0))
                return TouchPhase.Moved;
            
            return TouchPhase.Canceled;
        }
    }
}
