using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Core.Interfaces;

public interface IDrawable
{
    bool IsVisible { get; set; }
    int DrawOrder { get; set; }
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
