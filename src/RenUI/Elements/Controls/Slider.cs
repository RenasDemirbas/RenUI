using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Events;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Rendering;

namespace RenUI.Elements.Controls;

public class Slider : UIElement
{
    private float _value;
    private bool _isDragging;

    public float Value
    {
        get => _value;
        set
        {
            var clamped = Math.Clamp(value, MinValue, MaxValue);
            if (Math.Abs(_value - clamped) > float.Epsilon)
            {
                _value = clamped;
                OnValueChanged.Dispatch(new ValueChangedEventArgs(this, _value));
                MarkDirty();
            }
        }
    }

    public float MinValue { get; set; } = 0f;
    public float MaxValue { get; set; } = 100f;
    public float Step { get; set; } = 1f;
    public Orientation Orientation { get; set; } = Orientation.Horizontal;

    public int TrackThickness { get; set; } = 4;
    public int ThumbSize { get; set; } = 16;
    public Color TrackColor { get; set; } = new Color(50, 50, 50);
    public Color TrackFillColor { get; set; } = new Color(0, 120, 215);
    public Color ThumbColor { get; set; } = new Color(200, 200, 200);
    public Color ThumbHoverColor { get; set; } = Color.White;
    public Color ThumbDragColor { get; set; } = new Color(0, 120, 215);

    public EventDispatcher<ValueChangedEventArgs> OnValueChanged { get; } = new();

    public Slider()
    {
        Height = 20;
        Width = 200;
    }

    public override bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        var mousePos = inputState.MousePosition;
        var thumbBounds = GetThumbBounds();
        
        IsHovered = Contains(mousePos) || thumbBounds.Contains(mousePos);

        if (inputState.IsMouseButtonPressed(MouseButton.Left))
        {
            if (thumbBounds.Contains(mousePos) || Contains(mousePos))
            {
                _isDragging = true;
                UpdateValueFromMouse(mousePos);
                return true;
            }
        }

        if (_isDragging)
        {
            if (inputState.IsMouseButtonDown(MouseButton.Left))
            {
                UpdateValueFromMouse(mousePos);
                return true;
            }
            else
            {
                _isDragging = false;
            }
        }

        return false;
    }

    private void UpdateValueFromMouse(Point mousePos)
    {
        var bounds = GetAbsoluteBounds();
        float percentage;

        if (Orientation == Orientation.Horizontal)
        {
            var trackStart = bounds.X + ThumbSize / 2;
            var trackLength = bounds.Width - ThumbSize;
            percentage = Math.Clamp((mousePos.X - trackStart) / (float)trackLength, 0, 1);
        }
        else
        {
            var trackStart = bounds.Y + ThumbSize / 2;
            var trackLength = bounds.Height - ThumbSize;
            percentage = 1 - Math.Clamp((mousePos.Y - trackStart) / (float)trackLength, 0, 1);
        }

        var rawValue = MinValue + percentage * (MaxValue - MinValue);
        
        if (Step > 0)
        {
            rawValue = (float)Math.Round(rawValue / Step) * Step;
        }

        Value = rawValue;
    }

    private Rectangle GetThumbBounds()
    {
        var bounds = GetAbsoluteBounds();
        float percentage = (Value - MinValue) / (MaxValue - MinValue);

        if (Orientation == Orientation.Horizontal)
        {
            var trackLength = bounds.Width - ThumbSize;
            var thumbX = bounds.X + (int)(percentage * trackLength);
            var thumbY = bounds.Y + (bounds.Height - ThumbSize) / 2;
            return new Rectangle(thumbX, thumbY, ThumbSize, ThumbSize);
        }
        else
        {
            var trackLength = bounds.Height - ThumbSize;
            var thumbX = bounds.X + (bounds.Width - ThumbSize) / 2;
            var thumbY = bounds.Y + (int)((1 - percentage) * trackLength);
            return new Rectangle(thumbX, thumbY, ThumbSize, ThumbSize);
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        var bounds = GetAbsoluteBounds();
        DrawTrack(spriteBatch, bounds);
        DrawThumb(spriteBatch);
    }

    private void DrawTrack(SpriteBatch spriteBatch, Rectangle bounds)
    {
        float percentage = (Value - MinValue) / (MaxValue - MinValue);

        if (Orientation == Orientation.Horizontal)
        {
            var trackY = bounds.Y + (bounds.Height - TrackThickness) / 2;
            var fullTrack = new Rectangle(bounds.X, trackY, bounds.Width, TrackThickness);
            var fillWidth = (int)(bounds.Width * percentage);
            var fillTrack = new Rectangle(bounds.X, trackY, fillWidth, TrackThickness);

            PrimitiveRenderer.DrawRectangle(spriteBatch, fullTrack, TrackColor);
            PrimitiveRenderer.DrawRectangle(spriteBatch, fillTrack, TrackFillColor);
        }
        else
        {
            var trackX = bounds.X + (bounds.Width - TrackThickness) / 2;
            var fullTrack = new Rectangle(trackX, bounds.Y, TrackThickness, bounds.Height);
            var fillHeight = (int)(bounds.Height * percentage);
            var fillTrack = new Rectangle(trackX, bounds.Y + bounds.Height - fillHeight, TrackThickness, fillHeight);

            PrimitiveRenderer.DrawRectangle(spriteBatch, fullTrack, TrackColor);
            PrimitiveRenderer.DrawRectangle(spriteBatch, fillTrack, TrackFillColor);
        }
    }

    private void DrawThumb(SpriteBatch spriteBatch)
    {
        var thumbBounds = GetThumbBounds();
        var thumbColor = _isDragging ? ThumbDragColor : (IsHovered ? ThumbHoverColor : ThumbColor);
        
        PrimitiveRenderer.DrawCircle(spriteBatch, 
            new Vector2(thumbBounds.X + thumbBounds.Width / 2f, thumbBounds.Y + thumbBounds.Height / 2f),
            ThumbSize / 2f, thumbColor);
    }
}

public class ValueChangedEventArgs : UIEventArgs
{
    public float Value { get; }

    public ValueChangedEventArgs(object? sender, float value)
        : base(sender)
    {
        Value = value;
    }
}

public enum Orientation
{
    Horizontal,
    Vertical
}
