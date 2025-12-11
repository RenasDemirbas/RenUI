using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RenUI.Core.Interfaces;

namespace RenUI.Input;

public sealed class InputState : IInputState
{
    public MouseState CurrentMouseState { get; private set; }
    public MouseState PreviousMouseState { get; private set; }
    public KeyboardState CurrentKeyboardState { get; private set; }
    public KeyboardState PreviousKeyboardState { get; private set; }

    public Point MousePosition => CurrentMouseState.Position;
    public Point MouseDelta => new(
        CurrentMouseState.X - PreviousMouseState.X,
        CurrentMouseState.Y - PreviousMouseState.Y);
    
    public int ScrollWheelDelta => CurrentMouseState.ScrollWheelValue - PreviousMouseState.ScrollWheelValue;

    public void Update()
    {
        PreviousMouseState = CurrentMouseState;
        PreviousKeyboardState = CurrentKeyboardState;
        CurrentMouseState = Mouse.GetState();
        CurrentKeyboardState = Keyboard.GetState();
    }

    public bool IsMouseButtonPressed(MouseButton button) => button switch
    {
        MouseButton.Left => CurrentMouseState.LeftButton == ButtonState.Pressed && 
                           PreviousMouseState.LeftButton == ButtonState.Released,
        MouseButton.Right => CurrentMouseState.RightButton == ButtonState.Pressed && 
                            PreviousMouseState.RightButton == ButtonState.Released,
        MouseButton.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed && 
                             PreviousMouseState.MiddleButton == ButtonState.Released,
        _ => false
    };

    public bool IsMouseButtonReleased(MouseButton button) => button switch
    {
        MouseButton.Left => CurrentMouseState.LeftButton == ButtonState.Released && 
                           PreviousMouseState.LeftButton == ButtonState.Pressed,
        MouseButton.Right => CurrentMouseState.RightButton == ButtonState.Released && 
                            PreviousMouseState.RightButton == ButtonState.Pressed,
        MouseButton.Middle => CurrentMouseState.MiddleButton == ButtonState.Released && 
                             PreviousMouseState.MiddleButton == ButtonState.Pressed,
        _ => false
    };

    public bool IsMouseButtonDown(MouseButton button) => button switch
    {
        MouseButton.Left => CurrentMouseState.LeftButton == ButtonState.Pressed,
        MouseButton.Right => CurrentMouseState.RightButton == ButtonState.Pressed,
        MouseButton.Middle => CurrentMouseState.MiddleButton == ButtonState.Pressed,
        _ => false
    };

    public bool IsKeyPressed(Keys key) => 
        CurrentKeyboardState.IsKeyDown(key) && !PreviousKeyboardState.IsKeyDown(key);

    public bool IsKeyReleased(Keys key) => 
        !CurrentKeyboardState.IsKeyDown(key) && PreviousKeyboardState.IsKeyDown(key);

    public bool IsKeyDown(Keys key) => CurrentKeyboardState.IsKeyDown(key);
}
