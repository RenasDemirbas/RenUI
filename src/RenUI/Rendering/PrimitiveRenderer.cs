using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Rendering;

public static class PrimitiveRenderer
{
    private static Texture2D? _pixelTexture;
    private static GraphicsDevice? _graphicsDevice;

    public static void Initialize(GraphicsDevice graphicsDevice)
    {
        _graphicsDevice = graphicsDevice;
        _pixelTexture = new Texture2D(graphicsDevice, 1, 1);
        _pixelTexture.SetData(new[] { Color.White });
    }

    public static void Dispose()
    {
        _pixelTexture?.Dispose();
        _pixelTexture = null;
        _graphicsDevice = null;
    }

    public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color)
    {
        if (_pixelTexture == null) return;
        spriteBatch.Draw(_pixelTexture, bounds, color);
    }

    public static void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int thickness = 1)
    {
        if (_pixelTexture == null) return;

        // Top
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, bounds.Width, thickness), color);
        // Bottom
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Bottom - thickness, bounds.Width, thickness), color);
        // Left
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.X, bounds.Y, thickness, bounds.Height), color);
        // Right
        spriteBatch.Draw(_pixelTexture, new Rectangle(bounds.Right - thickness, bounds.Y, thickness, bounds.Height), color);
    }

    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, Color color, int thickness = 1)
    {
        if (_pixelTexture == null) return;

        var delta = end - start;
        var angle = (float)Math.Atan2(delta.Y, delta.X);
        var length = delta.Length();

        spriteBatch.Draw(_pixelTexture,
            start,
            null,
            color,
            angle,
            Vector2.Zero,
            new Vector2(length, thickness),
            SpriteEffects.None,
            0);
    }

    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, Color color, int segments = 32)
    {
        if (_pixelTexture == null || _graphicsDevice == null) return;

        var circleTexture = CreateCircleTexture((int)(radius * 2), color);
        if (circleTexture == null) return;

        spriteBatch.Draw(circleTexture,
            new Rectangle((int)(center.X - radius), (int)(center.Y - radius), (int)(radius * 2), (int)(radius * 2)),
            Color.White);
        
        circleTexture.Dispose();
    }

    public static void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color, int cornerRadius)
    {
        if (_pixelTexture == null || cornerRadius <= 0)
        {
            DrawRectangle(spriteBatch, bounds, color);
            return;
        }

        cornerRadius = Math.Min(cornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);

        // Center rectangle
        var centerRect = new Rectangle(
            bounds.X + cornerRadius,
            bounds.Y,
            bounds.Width - cornerRadius * 2,
            bounds.Height);
        DrawRectangle(spriteBatch, centerRect, color);

        // Left rectangle
        var leftRect = new Rectangle(
            bounds.X,
            bounds.Y + cornerRadius,
            cornerRadius,
            bounds.Height - cornerRadius * 2);
        DrawRectangle(spriteBatch, leftRect, color);

        // Right rectangle
        var rightRect = new Rectangle(
            bounds.Right - cornerRadius,
            bounds.Y + cornerRadius,
            cornerRadius,
            bounds.Height - cornerRadius * 2);
        DrawRectangle(spriteBatch, rightRect, color);

        // Corners
        DrawFilledCorner(spriteBatch, new Vector2(bounds.X + cornerRadius, bounds.Y + cornerRadius), cornerRadius, color);
        DrawFilledCorner(spriteBatch, new Vector2(bounds.Right - cornerRadius, bounds.Y + cornerRadius), cornerRadius, color);
        DrawFilledCorner(spriteBatch, new Vector2(bounds.X + cornerRadius, bounds.Bottom - cornerRadius), cornerRadius, color);
        DrawFilledCorner(spriteBatch, new Vector2(bounds.Right - cornerRadius, bounds.Bottom - cornerRadius), cornerRadius, color);
    }

    public static void DrawRoundedRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, Color color, int cornerRadius, int thickness = 1)
    {
        if (_pixelTexture == null || cornerRadius <= 0)
        {
            DrawRectangleBorder(spriteBatch, bounds, color, thickness);
            return;
        }

        cornerRadius = Math.Min(cornerRadius, Math.Min(bounds.Width, bounds.Height) / 2);

        // Top edge
        DrawRectangle(spriteBatch, new Rectangle(bounds.X + cornerRadius, bounds.Y, bounds.Width - cornerRadius * 2, thickness), color);
        // Bottom edge
        DrawRectangle(spriteBatch, new Rectangle(bounds.X + cornerRadius, bounds.Bottom - thickness, bounds.Width - cornerRadius * 2, thickness), color);
        // Left edge
        DrawRectangle(spriteBatch, new Rectangle(bounds.X, bounds.Y + cornerRadius, thickness, bounds.Height - cornerRadius * 2), color);
        // Right edge
        DrawRectangle(spriteBatch, new Rectangle(bounds.Right - thickness, bounds.Y + cornerRadius, thickness, bounds.Height - cornerRadius * 2), color);

        // Corner arcs
        DrawCornerArc(spriteBatch, new Vector2(bounds.X + cornerRadius, bounds.Y + cornerRadius), cornerRadius, color, thickness, 0);
        DrawCornerArc(spriteBatch, new Vector2(bounds.Right - cornerRadius, bounds.Y + cornerRadius), cornerRadius, color, thickness, 1);
        DrawCornerArc(spriteBatch, new Vector2(bounds.X + cornerRadius, bounds.Bottom - cornerRadius), cornerRadius, color, thickness, 2);
        DrawCornerArc(spriteBatch, new Vector2(bounds.Right - cornerRadius, bounds.Bottom - cornerRadius), cornerRadius, color, thickness, 3);
    }

    private static void DrawFilledCorner(SpriteBatch spriteBatch, Vector2 center, int radius, Color color)
    {
        if (_graphicsDevice == null) return;

        var texture = CreateCircleTexture(radius * 2, color);
        if (texture == null) return;

        spriteBatch.Draw(texture,
            new Rectangle((int)(center.X - radius), (int)(center.Y - radius), radius * 2, radius * 2),
            Color.White);
        
        texture.Dispose();
    }

    private static void DrawCornerArc(SpriteBatch spriteBatch, Vector2 center, int radius, Color color, int thickness, int corner)
    {
        const int segments = 8;
        float startAngle = corner switch
        {
            0 => MathHelper.Pi,
            1 => -MathHelper.PiOver2,
            2 => MathHelper.PiOver2,
            3 => 0,
            _ => 0
        };

        for (int i = 0; i < segments; i++)
        {
            float angle1 = startAngle + (i / (float)segments) * MathHelper.PiOver2;
            float angle2 = startAngle + ((i + 1) / (float)segments) * MathHelper.PiOver2;

            var p1 = center + new Vector2((float)Math.Cos(angle1), (float)Math.Sin(angle1)) * radius;
            var p2 = center + new Vector2((float)Math.Cos(angle2), (float)Math.Sin(angle2)) * radius;

            DrawLine(spriteBatch, p1, p2, color, thickness);
        }
    }

    private static Texture2D? CreateCircleTexture(int diameter, Color color)
    {
        if (_graphicsDevice == null) return null;

        var texture = new Texture2D(_graphicsDevice, diameter, diameter);
        var data = new Color[diameter * diameter];
        var radius = diameter / 2f;
        var center = new Vector2(radius, radius);

        for (int y = 0; y < diameter; y++)
        {
            for (int x = 0; x < diameter; x++)
            {
                var distance = Vector2.Distance(new Vector2(x, y), center);
                data[y * diameter + x] = distance <= radius ? color : Color.Transparent;
            }
        }

        texture.SetData(data);
        return texture;
    }
}
