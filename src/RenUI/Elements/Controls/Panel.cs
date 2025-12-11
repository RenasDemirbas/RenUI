using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Elements.Base;
using RenUI.Rendering;

namespace RenUI.Elements.Controls;

public class Panel : Container
{
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40);
    public Color BorderColor { get; set; } = new Color(60, 60, 60);
    public int BorderThickness { get; set; } = 1;
    public int CornerRadius { get; set; } = 0;

    protected override void DrawSelf(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var bounds = GetAbsoluteBounds();

        if (CornerRadius > 0)
        {
            PrimitiveRenderer.DrawRoundedRectangle(spriteBatch, bounds, BackgroundColor, CornerRadius);
            if (BorderThickness > 0)
            {
                PrimitiveRenderer.DrawRoundedRectangleBorder(spriteBatch, bounds, BorderColor, CornerRadius, BorderThickness);
            }
        }
        else
        {
            PrimitiveRenderer.DrawRectangle(spriteBatch, bounds, BackgroundColor);
            if (BorderThickness > 0)
            {
                PrimitiveRenderer.DrawRectangleBorder(spriteBatch, bounds, BorderColor, BorderThickness);
            }
        }
    }
}
