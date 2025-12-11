using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Rendering;

public static class SpriteBatchExtensions
{
    public static void DrawStringCentered(this SpriteBatch spriteBatch, SpriteFont font, string text, Rectangle bounds, Color color)
    {
        var textSize = font.MeasureString(text);
        var position = new Vector2(
            bounds.X + (bounds.Width - textSize.X) / 2,
            bounds.Y + (bounds.Height - textSize.Y) / 2);
        
        spriteBatch.DrawString(font, text, position, color);
    }

    public static void DrawStringShadowed(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, Color shadowColor, Vector2 shadowOffset)
    {
        spriteBatch.DrawString(font, text, position + shadowOffset, shadowColor);
        spriteBatch.DrawString(font, text, position, color);
    }

    public static void DrawStringOutlined(this SpriteBatch spriteBatch, SpriteFont font, string text, Vector2 position, Color color, Color outlineColor, int outlineThickness = 1)
    {
        for (int x = -outlineThickness; x <= outlineThickness; x++)
        {
            for (int y = -outlineThickness; y <= outlineThickness; y++)
            {
                if (x != 0 || y != 0)
                {
                    spriteBatch.DrawString(font, text, position + new Vector2(x, y), outlineColor);
                }
            }
        }
        
        spriteBatch.DrawString(font, text, position, color);
    }
}
