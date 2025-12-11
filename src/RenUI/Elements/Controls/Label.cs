using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Elements.Base;

namespace RenUI.Elements.Controls;

public class Label : UIElement
{
    private string _text = string.Empty;
    private SpriteFont? _font;
    private Vector2 _textSize;

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
    public TextAlignment HorizontalAlignment { get; set; } = TextAlignment.Start;
    public TextAlignment VerticalAlignment { get; set; } = TextAlignment.Center;
    public bool AutoSize { get; set; } = true;

    public Label() { }

    public Label(string text)
    {
        Text = text;
    }

    private void UpdateTextSize()
    {
        if (_font != null && !string.IsNullOrEmpty(_text))
        {
            _textSize = _font.MeasureString(_text);
            if (AutoSize)
            {
                Width = (int)_textSize.X;
                Height = (int)_textSize.Y;
            }
        }
        else
        {
            _textSize = Vector2.Zero;
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible || _font == null || string.IsNullOrEmpty(_text)) return;

        var bounds = GetAbsoluteBounds();
        var textPosition = CalculateTextPosition(bounds);

        spriteBatch.DrawString(_font, _text, textPosition, TextColor);
    }

    private Vector2 CalculateTextPosition(Rectangle bounds)
    {
        float x = HorizontalAlignment switch
        {
            TextAlignment.Start => bounds.X,
            TextAlignment.Center => bounds.X + (bounds.Width - _textSize.X) / 2,
            TextAlignment.End => bounds.X + bounds.Width - _textSize.X,
            _ => bounds.X
        };

        float y = VerticalAlignment switch
        {
            TextAlignment.Start => bounds.Y,
            TextAlignment.Center => bounds.Y + (bounds.Height - _textSize.Y) / 2,
            TextAlignment.End => bounds.Y + bounds.Height - _textSize.Y,
            _ => bounds.Y
        };

        return new Vector2((int)x, (int)y);
    }
}

public enum TextAlignment
{
    Start,
    Center,
    End
}
