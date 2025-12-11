using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI;

public abstract class RenUIGame : Game
{
    protected GraphicsDeviceManager Graphics { get; }
    protected UIManager UI => UIManager.Instance;

    protected RenUIGame()
    {
        Graphics = new GraphicsDeviceManager(this);
        Content.RootDirectory = "Content";
        IsMouseVisible = true;
        Window.AllowUserResizing = true;
        Window.ClientSizeChanged += OnClientSizeChanged;
    }

    protected override void Initialize()
    {
        base.Initialize();
    }

    protected override void LoadContent()
    {
        UI.Initialize(GraphicsDevice, Content);
        OnUIInitialized();
    }

    protected override void UnloadContent()
    {
        UI.Dispose();
        base.UnloadContent();
    }

    protected override void Update(GameTime gameTime)
    {
        UI.Update(gameTime);
        base.Update(gameTime);
    }

    protected override void Draw(GameTime gameTime)
    {
        GraphicsDevice.Clear(UI.Themes.CurrentTheme.BackgroundColor);
        UI.Draw(gameTime);
        base.Draw(gameTime);
    }

    private void OnClientSizeChanged(object? sender, EventArgs e)
    {
        UI.OnWindowResize(Window.ClientBounds.Width, Window.ClientBounds.Height);
    }

    protected abstract void OnUIInitialized();
}
