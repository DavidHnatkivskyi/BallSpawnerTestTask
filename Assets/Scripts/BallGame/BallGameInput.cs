using UnityEngine.InputSystem;

namespace BallGame
{
    internal static class BallGameInput
    {
        public static bool IsShotPressed()
        {
            return Touchscreen.current != null && Touchscreen.current.primaryTouch.press.isPressed
                || Mouse.current != null && Mouse.current.leftButton.isPressed;
        }
    }
}
