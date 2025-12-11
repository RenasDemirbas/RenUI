using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;

namespace RenUI.Core.Events;

public class UIEventArgs : EventArgs
{
    public object? Sender { get; }
    public GameTime? GameTime { get; }
    public bool Handled { get; set; }

    public UIEventArgs(object? sender, GameTime? gameTime = null)
    {
        Sender = sender;
        GameTime = gameTime;
        Handled = false;
    }
}

public class MouseEventArgs : UIEventArgs
{
    public Point Position { get; }
    public Point Delta { get; }
    public MouseButton Button { get; }

    public MouseEventArgs(object? sender, Point position, Point delta = default, MouseButton button = MouseButton.Left, GameTime? gameTime = null)
        : base(sender, gameTime)
    {
        Position = position;
        Delta = delta;
        Button = button;
    }
}

public class KeyEventArgs : UIEventArgs
{
    public Microsoft.Xna.Framework.Input.Keys Key { get; }
    public bool IsRepeat { get; }

    public KeyEventArgs(object? sender, Microsoft.Xna.Framework.Input.Keys key, bool isRepeat = false, GameTime? gameTime = null)
        : base(sender, gameTime)
    {
        Key = key;
        IsRepeat = isRepeat;
    }
}

public class FocusEventArgs : UIEventArgs
{
    public bool IsFocused { get; }

    public FocusEventArgs(object? sender, bool isFocused, GameTime? gameTime = null)
        : base(sender, gameTime)
    {
        IsFocused = isFocused;
    }
}
