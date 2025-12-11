using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace RenUI.Core.Interfaces;

public enum MouseButton
{
    Left,
    Right,
    Middle
}

public interface IInputState
{
    MouseState CurrentMouseState { get; }
    MouseState PreviousMouseState { get; }
    KeyboardState CurrentKeyboardState { get; }
    KeyboardState PreviousKeyboardState { get; }
    
    Point MousePosition { get; }
    Point MouseDelta { get; }
    int ScrollWheelDelta { get; }
    
    bool IsMouseButtonPressed(MouseButton button);
    bool IsMouseButtonReleased(MouseButton button);
    bool IsMouseButtonDown(MouseButton button);
    
    bool IsKeyPressed(Keys key);
    bool IsKeyReleased(Keys key);
    bool IsKeyDown(Keys key);
}
