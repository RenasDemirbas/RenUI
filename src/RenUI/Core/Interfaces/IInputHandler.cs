namespace RenUI.Core.Interfaces;

public interface IInputHandler
{
    bool HandleMouseInput(IInputState inputState);
    bool HandleKeyboardInput(IInputState inputState);
}
