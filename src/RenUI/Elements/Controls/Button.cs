using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Rendering;

namespace RenUI.Elements.Controls;

public class Button : UIElement
{
    private string _text = string.Empty;
    private SpriteFont? _font;
    private Vector2 _textSize;
    private bool _isPressed;

    public string Text
    {
        get => _text;
        set
        {
            if (_text != value)
            {
                _text = value ?? string.Empty;
                UpdateTextSize();
                MarkDirty();
            }
        }
    }

    public SpriteFont? Font
    {
        get => _font;
        set
        {
            if (_font != value)
            {
                _font = value;
                UpdateTextSize();
                MarkDirty();
            }
        }
    }

    public Color TextColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = new Color(70, 70, 70);
    public Color HoverColor { get; set; } = new Color(90, 90, 90);
    public Color PressedColor { get; set; } = new Color(50, 50, 50);
    public Color DisabledColor { get; set; } = new Color(45, 45, 45);
    public Color BorderColor { get; set; } = new Color(100, 100, 100);
    public int BorderThickness { get; set; } = 1;
    public int CornerRadius { get; set; } = 4;

    public Button() { }

    public Button(string text)
    {
        Text = text;
    }

    private void UpdateTextSize()
    {
        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            _textSize = _font.MeasureString(_text);
        }
        else
        {
            _textSize = Vector2.Zero;
        }
    }

    public override bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        var mousePos = inputState.MousePosition;
        bool wasHovered = IsHovered;
        IsHovered = Contains(mousePos);

        if (IsHovered && !wasHovered)
        {
            OnMouseEnter.Dispatch(new Core.Events.MouseEventArgs(this, mousePos));
        }
        else if (!IsHovered && wasHovered)
        {
            OnMouseLeave.Dispatch(new Core.Events.MouseEventArgs(this, mousePos));
            _isPressed = false;
        }

        if (IsHovered)
        {
            if (inputState.IsMouseButtonPressed(MouseButton.Left))
            {
                _isPressed = true;
                OnMouseDown.Dispatch(new Core.Events.MouseEventArgs(this, mousePos));
                return true;
            }

            if (inputState.IsMouseButtonReleased(MouseButton.Left) && _isPressed)
            {
                _isPressed = false;
                OnMouseUp.Dispatch(new Core.Events.MouseEventArgs(this, mousePos));
                OnClick.Dispatch(new Core.Events.MouseEventArgs(this, mousePos));
                return true;
            }
        }

        if (!inputState.IsMouseButtonDown(MouseButton.Left))
        {
            _isPressed = false;
        }

        return false;
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        var bounds = GetAbsoluteBounds();
        var bgColor = GetCurrentBackgroundColor();

        if (CornerRadius > 0)
        {
            PrimitiveRenderer.DrawRoundedRectangle(spriteBatch, bounds, bgColor, CornerRadius);
            if (BorderThickness > 0)
            {
                PrimitiveRenderer.DrawRoundedRectangleBorder(spriteBatch, bounds, BorderColor, CornerRadius, BorderThickness);
            }
        }
        else
        {
            PrimitiveRenderer.DrawRectangle(spriteBatch, bounds, bgColor);
            if (BorderThickness > 0)
            {
                PrimitiveRenderer.DrawRectangleBorder(spriteBatch, bounds, BorderColor, BorderThickness);
            }
        }

        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            var textPos = new Vector2(
                bounds.X + (bounds.Width - _textSize.X) / 2,
                bounds.Y + (bounds.Height - _textSize.Y) / 2);
            
            spriteBatch.DrawString(_font, _text, textPos, IsEnabled ? TextColor : Color.Gray);
        }
    }

    private Color GetCurrentBackgroundColor()
    {
        if (!IsEnabled) return DisabledColor;
        if (_isPressed) return PressedColor;
        if (IsHovered) return HoverColor;
        return BackgroundColor;
    }
}
