using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Input;

namespace RenUI.Scenes;

public abstract class SceneBase : IScene
{
    private readonly Container _rootContainer;
    private bool _isLoaded;

    public string Name { get; }
    public bool IsLoaded => _isLoaded;
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public int DrawOrder { get; set; }

    protected ContentManager? Content { get; private set; }
    protected GraphicsDevice? GraphicsDevice { get; private set; }
    protected Container Root => _rootContainer;

    protected SceneBase(string name)
    {
        Name = name;
        _rootContainer = new Container { Name = "Root" };
    }

    public virtual void LoadContent(ContentManager content, GraphicsDevice graphicsDevice)
    {
        Content = content;
        GraphicsDevice = graphicsDevice;
        
        _rootContainer.Bounds = new Rectangle(0, 0, 
            graphicsDevice.Viewport.Width, 
            graphicsDevice.Viewport.Height);
        
        OnLoadContent();
        _rootContainer.Initialize();
        _isLoaded = true;
    }

    public virtual void UnloadContent()
    {
        OnUnloadContent();
        _rootContainer.ClearChildren();
        _isLoaded = false;
    }

    public virtual void OnEnter()
    {
        IsEnabled = true;
        IsVisible = true;
    }

    public virtual void OnExit()
    {
        IsEnabled = false;
        IsVisible = false;
    }

    public virtual void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;

        var inputState = InputManager.Instance.State;
        _rootContainer.HandleMouseInput(inputState);
        _rootContainer.HandleKeyboardInput(inputState);
        _rootContainer.Update(gameTime);

        OnUpdate(gameTime);
    }

    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        _rootContainer.Draw(spriteBatch, gameTime);
        OnDraw(spriteBatch, gameTime);
    }

    public void UpdateViewport(int width, int height)
    {
        _rootContainer.Bounds = new Rectangle(0, 0, width, height);
    }

    protected abstract void OnLoadContent();
    protected virtual void OnUnloadContent() { }
    protected virtual void OnUpdate(GameTime gameTime) { }
    protected virtual void OnDraw(SpriteBatch spriteBatch, GameTime gameTime) { }

    protected void AddElement(IUIElement element)
    {
        _rootContainer.AddChild(element);
    }

    protected void RemoveElement(IUIElement element)
    {
        _rootContainer.RemoveChild(element);
    }

    protected T? GetElement<T>(string name) where T : class, IUIElement
    {
        return _rootContainer.GetChild<T>(name);
    }
}
