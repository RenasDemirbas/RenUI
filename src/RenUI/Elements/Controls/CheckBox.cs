using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Events;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Rendering;

namespace RenUI.Elements.Controls;

public class CheckBox : UIElement
{
    private bool _isChecked;
    private SpriteFont? _font;

    public bool IsChecked
    {
        get => _isChecked;
        set
        {
            if (_isChecked != value)
            {
                _isChecked = value;
                OnCheckedChanged.Dispatch(new CheckedChangedEventArgs(this, value));
                MarkDirty();
            }
        }
    }

    public string Text { get; set; } = string.Empty;

    public SpriteFont? Font
    {
        get => _font;
        set
        {
            _font = value;
            MarkDirty();
        }
    }

    public int BoxSize { get; set; } = 18;
    public int Spacing { get; set; } = 8;
    public Color TextColor { get; set; } = Color.White;
    public Color BoxColor { get; set; } = new Color(50, 50, 50);
    public Color BoxHoverColor { get; set; } = new Color(70, 70, 70);
    public Color CheckColor { get; set; } = new Color(0, 120, 215);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderThickness { get; set; } = 1;

    public EventDispatcher<CheckedChangedEventArgs> OnCheckedChanged { get; } = new();

    public CheckBox() { }

    public CheckBox(string text, bool isChecked = false)
    {
        Text = text;
        _isChecked = isChecked;
    }

    public override bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        var mousePos = inputState.MousePosition;
        IsHovered = Contains(mousePos);

        if (IsHovered && inputState.IsMouseButtonPressed(MouseButton.Left))
        {
            IsChecked = !IsChecked;
            OnClick.Dispatch(new MouseEventArgs(this, mousePos));
            return true;
        }

        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        var bounds = GetAbsoluteBounds();
        var boxBounds = new Rectangle(bounds.X, bounds.Y + (bounds.Height - BoxSize) / 2, BoxSize, BoxSize);

        PrimitiveRenderer.DrawRectangle(spriteBatch, boxBounds, IsHovered ? BoxHoverColor : BoxColor);
        PrimitiveRenderer.DrawRectangleBorder(spriteBatch, boxBounds, BorderColor, BorderThickness);

        if (_isChecked)
        {
            var checkBounds = new Rectangle(
                boxBounds.X + 4,
                boxBounds.Y + 4,
                boxBounds.Width - 8,
                boxBounds.Height - 8);
            PrimitiveRenderer.DrawRectangle(spriteBatch, checkBounds, CheckColor);
        }

        if (_font != null && !string.IsNullOrEmpty(Text))
        {
            var textPos = new Vector2(
                bounds.X + BoxSize + Spacing,
                bounds.Y + (bounds.Height - _font.LineSpacing) / 2);
            spriteBatch.DrawString(_font, Text, textPos, IsEnabled ? TextColor : Color.Gray);
        }
    }
}

public class CheckedChangedEventArgs : UIEventArgs
{
    public bool IsChecked { get; }

    public CheckedChangedEventArgs(object? sender, bool isChecked)
        : base(sender)
    {
        IsChecked = isChecked;
    }
}
