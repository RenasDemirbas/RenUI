using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Events;
using RenUI.Core.Interfaces;
using RenUI.Core.Primitives;
using RenUI.Styling;
using MouseButton = RenUI.Core.Interfaces.MouseButton;

namespace RenUI.Elements.Base;

public abstract class UIElement : IUIElement, IInputHandler
{
    private Rectangle _bounds;
    private bool _isDirty = true;
    private static int _idCounter = 0;

    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public IUIElement? Parent { get; set; }
    
    public string? TextureId { get; set; }
    public string? FontId { get; set; }
    
    public Rectangle Bounds
    {
        get => _bounds;
        set
        {
            if (_bounds != value)
            {
                _bounds = value;
                _isDirty = true;
                OnBoundsChanged();
            }
        }
    }

    public Point Position
    {
        get => Bounds.Location;
        set => Bounds = new Rectangle(value, Bounds.Size);
    }

    public Point Size
    {
        get => Bounds.Size;
        set => Bounds = new Rectangle(Bounds.Location, value);
    }

    public int Width
    {
        get => Bounds.Width;
        set => Size = new Point(value, Height);
    }

    public int Height
    {
        get => Bounds.Height;
        set => Size = new Point(Width, value);
    }

    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsFocused { get; set; }
    public bool IsHovered { get; set; }
    public int DrawOrder { get; set; }

    public Anchor Anchor { get; set; } = Anchor.TopLeft;
    public Margin Margin { get; set; } = Margin.Zero;
    public Padding Padding { get; set; } = Padding.Zero;

    public Size MinSize { get; set; } = Core.Primitives.Size.Zero;
    public Size MaxSize { get; set; } = new Size(int.MaxValue, int.MaxValue);

    public ElementStyle Style { get; set; } = new();

    public EventDispatcher<MouseEventArgs> OnClick { get; } = new();
    public EventDispatcher<MouseEventArgs> OnMouseEnter { get; } = new();
    public EventDispatcher<MouseEventArgs> OnMouseLeave { get; } = new();
    public EventDispatcher<MouseEventArgs> OnMouseDown { get; } = new();
    public EventDispatcher<MouseEventArgs> OnMouseUp { get; } = new();
    public EventDispatcher<FocusEventArgs> OnFocusChanged { get; } = new();

    protected bool IsDirty => _isDirty;

    public virtual void Initialize() { }

    public virtual void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;
        _isDirty = false;
    }

    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

    public virtual bool Contains(Point point) => Bounds.Contains(point);

    public Rectangle GetAbsoluteBounds()
    {
        if (Parent == null) return Bounds;
        
        var parentBounds = Parent.Bounds;
        return new Rectangle(
            parentBounds.X + Bounds.X,
            parentBounds.Y + Bounds.Y,
            Bounds.Width,
            Bounds.Height);
    }

    public virtual void OnFocusGained()
    {
        IsFocused = true;
        OnFocusChanged.Dispatch(new FocusEventArgs(this, true));
    }

    public virtual void OnFocusLost()
    {
        IsFocused = false;
        OnFocusChanged.Dispatch(new FocusEventArgs(this, false));
    }

    protected virtual void OnBoundsChanged() { }

    public virtual bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        var mousePos = inputState.MousePosition;
        bool wasHovered = IsHovered;
        IsHovered = Contains(mousePos);

        if (IsHovered && !wasHovered)
        {
            OnMouseEnter.Dispatch(new MouseEventArgs(this, mousePos));
        }
        else if (!IsHovered && wasHovered)
        {
            OnMouseLeave.Dispatch(new MouseEventArgs(this, mousePos));
        }

        if (IsHovered)
        {
            if (inputState.IsMouseButtonPressed(MouseButton.Left))
            {
                OnMouseDown.Dispatch(new MouseEventArgs(this, mousePos, default, MouseButton.Left));
                return true;
            }

            if (inputState.IsMouseButtonReleased(MouseButton.Left))
            {
                OnMouseUp.Dispatch(new MouseEventArgs(this, mousePos, default, MouseButton.Left));
                OnClick.Dispatch(new MouseEventArgs(this, mousePos, default, MouseButton.Left));
                return true;
            }
        }

        return false;
    }

    public virtual bool HandleKeyboardInput(IInputState inputState) => false;

    protected void MarkDirty() => _isDirty = true;
}
