using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;

namespace RenUI.Input;

public sealed class InputManager : IUpdatable
{
    private static InputManager? _instance;
    private static readonly object _lock = new();

    public static InputManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new InputManager();
                }
            }
            return _instance;
        }
    }

    private readonly InputState _inputState;
    
    public IInputState State => _inputState;
    public bool IsEnabled { get; set; } = true;

    private InputManager()
    {
        _inputState = new InputState();
    }

    public void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;
        _inputState.Update();
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
