using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Input;
using RenUI.Rendering;
using RenUI.Scenes;
using RenUI.Serialization;
using RenUI.Styling;

namespace RenUI;

public sealed class UIManager
{
    private static UIManager? _instance;
    private static readonly object _lock = new();

    public static UIManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new UIManager();
                }
            }
            return _instance;
        }
    }

    private SpriteBatch? _spriteBatch;
    private GraphicsDevice? _graphicsDevice;
    private ContentManager? _content;
    private bool _isInitialized;

    public InputManager Input => InputManager.Instance;
    public SceneManager Scenes => SceneManager.Instance;
    public ThemeManager Themes => ThemeManager.Instance;
    public UILayoutManager Layouts => UILayoutManager.Instance;
    public FontManager Fonts => FontManager.Instance;
    public SpriteManager Sprites => SpriteManager.Instance;

    public bool IsInitialized => _isInitialized;
    public GraphicsDevice? GraphicsDevice => _graphicsDevice;
    public ContentManager? Content => _content;

    private UIManager() { }

    public void Initialize(GraphicsDevice graphicsDevice, ContentManager content)
    {
        if (_isInitialized) return;

        _graphicsDevice = graphicsDevice;
        _content = content;
        _spriteBatch = new SpriteBatch(graphicsDevice);

        PrimitiveRenderer.Initialize(graphicsDevice);
        FontManager.Instance.Initialize(content);
        SpriteManager.Instance.Initialize(content, graphicsDevice);
        SceneManager.Instance.Initialize(content, graphicsDevice);
        UILayoutManager.Instance.Initialize(content);

        _isInitialized = true;
    }

    public void Update(GameTime gameTime)
    {
        if (!_isInitialized) return;

        Input.Update(gameTime);
        Scenes.Update(gameTime);
    }

    public void Draw(GameTime gameTime)
    {
        if (!_isInitialized || _spriteBatch == null) return;

        _spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.AlphaBlend,
            SamplerState.PointClamp,
            null,
            null);

        Scenes.Draw(_spriteBatch, gameTime);

        _spriteBatch.End();
    }

    public void OnWindowResize(int width, int height)
    {
        Scenes.UpdateViewport(width, height);
    }

    public void Dispose()
    {
        PrimitiveRenderer.Dispose();
        _spriteBatch?.Dispose();
        _spriteBatch = null;
        _isInitialized = false;
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance?.Dispose();
            _instance = null;
            InputManager.Reset();
            SceneManager.Reset();
            ThemeManager.Reset();
            UILayoutManager.Reset();
            FontManager.Reset();
            SpriteManager.Reset();
        }
    }
}
