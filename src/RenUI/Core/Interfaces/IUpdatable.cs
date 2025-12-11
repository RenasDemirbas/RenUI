using Microsoft.Xna.Framework;

namespace RenUI.Core.Interfaces;

public interface IUpdatable
{
    bool IsEnabled { get; set; }
    void Update(GameTime gameTime);
}
