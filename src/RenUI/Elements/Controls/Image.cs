using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Elements.Base;

namespace RenUI.Elements.Controls;

public class Image : UIElement
{
    private Texture2D? _texture;
    private Rectangle? _sourceRectangle;
    private Vector2 _origin;

    public Texture2D? Texture
    {
        get => _texture;
        set
        {
            if (_texture != value)
            {
                _texture = value;
                UpdateSourceRectangle();
                MarkDirty();
            }
        }
    }

    public Rectangle? SourceRectangle
    {
        get => _sourceRectangle;
        set
        {
            _sourceRectangle = value;
            MarkDirty();
        }
    }

    public Color Tint { get; set; } = Color.White;
    public float Rotation { get; set; } = 0f;
    public Vector2 Scale { get; set; } = Vector2.One;
    public SpriteEffects Effects { get; set; } = SpriteEffects.None;
    public float LayerDepth { get; set; } = 0f;

    public ImageStretchMode StretchMode { get; set; } = ImageStretchMode.Uniform;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Center;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Center;

    public bool PreserveAspectRatio { get; set; } = true;
    public bool AutoSize { get; set; } = false;

    public Image() { }

    public Image(Texture2D texture)
    {
        Texture = texture;
    }

    private void UpdateSourceRectangle()
    {
        if (_texture != null && _sourceRectangle == null)
        {
            if (AutoSize)
            {
                Width = _texture.Width;
                Height = _texture.Height;
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible || _texture == null) return;

        var bounds = GetAbsoluteBounds();
        var sourceRect = _sourceRectangle ?? new Rectangle(0, 0, _texture.Width, _texture.Height);
        var destRect = CalculateDestinationRectangle(bounds, sourceRect);

        spriteBatch.Draw(
            _texture,
            destRect,
            sourceRect,
            Tint,
            Rotation,
            _origin,
            Effects,
            LayerDepth);
    }

    private Rectangle CalculateDestinationRectangle(Rectangle bounds, Rectangle source)
    {
        switch (StretchMode)
        {
            case ImageStretchMode.None:
                return CalculateAlignedRectangle(bounds, source.Width, source.Height);

            case ImageStretchMode.Fill:
                return bounds;

            case ImageStretchMode.Uniform:
                return CalculateUniformRectangle(bounds, source.Width, source.Height);

            case ImageStretchMode.UniformToFill:
                return CalculateUniformToFillRectangle(bounds, source.Width, source.Height);

            default:
                return bounds;
        }
    }

    private Rectangle CalculateAlignedRectangle(Rectangle bounds, int imageWidth, int imageHeight)
    {
        int x = HorizontalAlignment switch
        {
            HorizontalAlignment.Left => bounds.X,
            HorizontalAlignment.Center => bounds.X + (bounds.Width - imageWidth) / 2,
            HorizontalAlignment.Right => bounds.X + bounds.Width - imageWidth,
            _ => bounds.X
        };

        int y = VerticalAlignment switch
        {
            VerticalAlignment.Top => bounds.Y,
            VerticalAlignment.Center => bounds.Y + (bounds.Height - imageHeight) / 2,
            VerticalAlignment.Bottom => bounds.Y + bounds.Height - imageHeight,
            _ => bounds.Y
        };

        return new Rectangle(x, y, imageWidth, imageHeight);
    }

    private Rectangle CalculateUniformRectangle(Rectangle bounds, int imageWidth, int imageHeight)
    {
        float scaleX = (float)bounds.Width / imageWidth;
        float scaleY = (float)bounds.Height / imageHeight;
        float scale = Math.Min(scaleX, scaleY);

        int newWidth = (int)(imageWidth * scale);
        int newHeight = (int)(imageHeight * scale);

        return CalculateAlignedRectangle(bounds, newWidth, newHeight);
    }

    private Rectangle CalculateUniformToFillRectangle(Rectangle bounds, int imageWidth, int imageHeight)
    {
        float scaleX = (float)bounds.Width / imageWidth;
        float scaleY = (float)bounds.Height / imageHeight;
        float scale = Math.Max(scaleX, scaleY);

        int newWidth = (int)(imageWidth * scale);
        int newHeight = (int)(imageHeight * scale);

        int x = bounds.X + (bounds.Width - newWidth) / 2;
        int y = bounds.Y + (bounds.Height - newHeight) / 2;

        return new Rectangle(x, y, newWidth, newHeight);
    }

    public void SetSpriteSheet(int column, int row, int frameWidth, int frameHeight)
    {
        SourceRectangle = new Rectangle(column * frameWidth, row * frameHeight, frameWidth, frameHeight);
    }

    public void SetSpriteSheetFrame(int frameIndex, int frameWidth, int frameHeight, int columns)
    {
        int column = frameIndex % columns;
        int row = frameIndex / columns;
        SetSpriteSheet(column, row, frameWidth, frameHeight);
    }
}

public enum ImageStretchMode
{
    None,
    Fill,
    Uniform,
    UniformToFill
}

public enum HorizontalAlignment
{
    Left,
    Center,
    Right
}

public enum VerticalAlignment
{
    Top,
    Center,
    Bottom
}
