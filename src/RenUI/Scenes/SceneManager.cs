using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Interfaces;

namespace RenUI.Scenes;

public sealed class SceneManager : ISceneManager
{
    private static SceneManager? _instance;
    private static readonly object _lock = new();

    public static SceneManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SceneManager();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, IScene> _scenes = new();
    private readonly Stack<IScene> _sceneStack = new();
    private ContentManager? _content;
    private GraphicsDevice? _graphicsDevice;

    public IScene? CurrentScene => _sceneStack.Count > 0 ? _sceneStack.Peek() : null;

    public event Action<IScene?, IScene?>? SceneChanged;

    private SceneManager() { }

    public void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;
    }

    public void RegisterScene(string name, IScene scene)
    {
        _scenes[name] = scene;
    }

    public void UnregisterScene(string name)
    {
        if (_scenes.TryGetValue(name, out var scene))
        {
            if (_sceneStack.Contains(scene))
            {
                throw new InvalidOperationException($"Cannot unregister active scene: {name}");
            }
            _scenes.Remove(name);
        }
    }

    public void ChangeScene(string name)
    {
        if (!_scenes.TryGetValue(name, out var newScene))
        {
            throw new ArgumentException($"Scene not found: {name}");
        }

        var oldScene = CurrentScene;

        while (_sceneStack.Count > 0)
        {
            var scene = _sceneStack.Pop();
            scene.OnExit();
            scene.UnloadContent();
        }

        LoadAndPushScene(newScene);
        SceneChanged?.Invoke(oldScene, newScene);
    }

    public void PushScene(string name)
    {
        if (!_scenes.TryGetValue(name, out var newScene))
        {
            throw new ArgumentException($"Scene not found: {name}");
        }

        var oldScene = CurrentScene;
        oldScene?.OnExit();

        LoadAndPushScene(newScene);
        SceneChanged?.Invoke(oldScene, newScene);
    }

    public void PopScene()
    {
        if (_sceneStack.Count == 0) return;

        var oldScene = _sceneStack.Pop();
        oldScene.OnExit();
        oldScene.UnloadContent();

        var newScene = CurrentScene;
        newScene?.OnEnter();

        SceneChanged?.Invoke(oldScene, newScene);
    }

    public void Update(GameTime gameTime)
    {
        CurrentScene?.Update(gameTime);
    }

    public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        CurrentScene?.Draw(spriteBatch, gameTime);
    }

    public void UpdateViewport(int width, int height)
    {
        foreach (var scene in _sceneStack)
        {
            if (scene is SceneBase sceneBase)
            {
                sceneBase.UpdateViewport(width, height);
            }
        }
    }

    private void LoadAndPushScene(IScene scene)
    {
        if (_content == null || _graphicsDevice == null)
        {
            throw new InvalidOperationException("SceneManager not initialized");
        }

        if (!scene.IsLoaded)
        {
            scene.LoadContent(_content, _graphicsDevice);
        }

        _sceneStack.Push(scene);
        scene.OnEnter();
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
