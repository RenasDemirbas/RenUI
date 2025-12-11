using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Core.Interfaces;

public interface IScene : IUpdatable, IDrawable
{
    string Name { get; }
    bool IsLoaded { get; }
    
    void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
    void UnloadContent();
    void OnEnter();
    void OnExit();
}
