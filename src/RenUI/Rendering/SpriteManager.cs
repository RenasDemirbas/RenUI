using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Rendering;

public sealed class SpriteManager
{
    private static SpriteManager? _instance;
    private static readonly object _lock = new();

    public static SpriteManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new SpriteManager();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, SpriteSheet> _spriteSheets = new();
    private readonly Dictionary<string, SpriteAtlas> _atlases = new();
    private ContentManager? _content;
    private GraphicsDevice? _graphicsDevice;

    private SpriteManager() { }

    public void Initialize(ContentManager content, GraphicsDevice graphicsDevice)
    {
        _content = content;
        _graphicsDevice = graphicsDevice;
    }

    public Texture2D? LoadTexture(string textureId, string contentPath)
    {
        if (_content == null)
            throw new InvalidOperationException("SpriteManager not initialized");

        if (_textures.TryGetValue(textureId, out var existingTexture))
            return existingTexture;

        try
        {
            var texture = _content.Load<Texture2D>(contentPath);
            _textures[textureId] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load texture '{textureId}' from '{contentPath}': {ex.Message}");
            return null;
        }
    }

    public Texture2D? LoadTextureFromFile(string textureId, string filePath)
    {
        if (_graphicsDevice == null)
            throw new InvalidOperationException("SpriteManager not initialized");

        if (_textures.TryGetValue(textureId, out var existingTexture))
            return existingTexture;

        try
        {
            using var stream = File.OpenRead(filePath);
            var texture = Texture2D.FromStream(_graphicsDevice, stream);
            _textures[textureId] = texture;
            return texture;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load texture '{textureId}' from file '{filePath}': {ex.Message}");
            return null;
        }
    }

    public void RegisterTexture(string textureId, Texture2D texture)
    {
        _textures[textureId] = texture;
    }

    public void UnregisterTexture(string textureId)
    {
        _textures.Remove(textureId);
    }

    public Texture2D? GetTexture(string textureId)
    {
        return _textures.TryGetValue(textureId, out var texture) ? texture : null;
    }

    public bool HasTexture(string textureId)
    {
        return _textures.ContainsKey(textureId);
    }

    public SpriteSheet CreateSpriteSheet(string sheetId, string textureId, int frameWidth, int frameHeight)
    {
        var texture = GetTexture(textureId);
        if (texture == null)
            throw new InvalidOperationException($"Texture '{textureId}' not found");

        var spriteSheet = new SpriteSheet(sheetId, texture, frameWidth, frameHeight);
        _spriteSheets[sheetId] = spriteSheet;
        return spriteSheet;
    }

    public SpriteSheet CreateSpriteSheet(string sheetId, Texture2D texture, int frameWidth, int frameHeight)
    {
        var spriteSheet = new SpriteSheet(sheetId, texture, frameWidth, frameHeight);
        _spriteSheets[sheetId] = spriteSheet;
        return spriteSheet;
    }

    public SpriteSheet? GetSpriteSheet(string sheetId)
    {
        return _spriteSheets.TryGetValue(sheetId, out var sheet) ? sheet : null;
    }

    public SpriteAtlas CreateAtlas(string atlasId, string textureId)
    {
        var texture = GetTexture(textureId);
        if (texture == null)
            throw new InvalidOperationException($"Texture '{textureId}' not found");

        var atlas = new SpriteAtlas(atlasId, texture);
        _atlases[atlasId] = atlas;
        return atlas;
    }

    public SpriteAtlas? GetAtlas(string atlasId)
    {
        return _atlases.TryGetValue(atlasId, out var atlas) ? atlas : null;
    }

    public Sprite? GetSprite(string atlasId, string spriteName)
    {
        var atlas = GetAtlas(atlasId);
        return atlas?.GetSprite(spriteName);
    }

    public IEnumerable<string> GetTextureIds()
    {
        return _textures.Keys;
    }

    public IEnumerable<string> GetSpriteSheetIds()
    {
        return _spriteSheets.Keys;
    }

    public IEnumerable<string> GetAtlasIds()
    {
        return _atlases.Keys;
    }

    public void Clear()
    {
        _textures.Clear();
        _spriteSheets.Clear();
        _atlases.Clear();
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance?.Clear();
            _instance = null;
        }
    }
}

public class SpriteSheet
{
    public string Id { get; }
    public Texture2D Texture { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public int TotalFrames { get; }

    public SpriteSheet(string id, Texture2D texture, int frameWidth, int frameHeight)
    {
        Id = id;
        Texture = texture;
        FrameWidth = frameWidth;
        FrameHeight = frameHeight;
        Columns = texture.Width / frameWidth;
        Rows = texture.Height / frameHeight;
        TotalFrames = Columns * Rows;
    }

    public Rectangle GetFrameRectangle(int frameIndex)
    {
        if (frameIndex < 0 || frameIndex >= TotalFrames)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));

        int column = frameIndex % Columns;
        int row = frameIndex / Columns;
        return new Rectangle(column * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
    }

    public Rectangle GetFrameRectangle(int column, int row)
    {
        return new Rectangle(column * FrameWidth, row * FrameHeight, FrameWidth, FrameHeight);
    }
}

public class SpriteAtlas
{
    public string Id { get; }
    public Texture2D Texture { get; }
    private readonly Dictionary<string, Sprite> _sprites = new();

    public SpriteAtlas(string id, Texture2D texture)
    {
        Id = id;
        Texture = texture;
    }

    public void DefineSprite(string name, Rectangle sourceRectangle, Vector2? origin = null, Vector2? pivot = null)
    {
        _sprites[name] = new Sprite(name, Texture, sourceRectangle, origin ?? Vector2.Zero, pivot ?? new Vector2(0.5f, 0.5f));
    }

    public void DefineSprite(string name, int x, int y, int width, int height, Vector2? origin = null, Vector2? pivot = null)
    {
        DefineSprite(name, new Rectangle(x, y, width, height), origin, pivot);
    }

    public Sprite? GetSprite(string name)
    {
        return _sprites.TryGetValue(name, out var sprite) ? sprite : null;
    }

    public bool HasSprite(string name)
    {
        return _sprites.ContainsKey(name);
    }

    public IEnumerable<string> GetSpriteNames()
    {
        return _sprites.Keys;
    }

    public IEnumerable<Sprite> GetAllSprites()
    {
        return _sprites.Values;
    }
}

public class Sprite
{
    public string Name { get; }
    public Texture2D Texture { get; }
    public Rectangle SourceRectangle { get; }
    public Vector2 Origin { get; }
    public Vector2 Pivot { get; }
    public int Width => SourceRectangle.Width;
    public int Height => SourceRectangle.Height;

    public Sprite(string name, Texture2D texture, Rectangle sourceRectangle, Vector2 origin, Vector2 pivot)
    {
        Name = name;
        Texture = texture;
        SourceRectangle = sourceRectangle;
        Origin = origin;
        Pivot = pivot;
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color)
    {
        spriteBatch.Draw(Texture, position, SourceRectangle, color);
    }

    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, float rotation, float scale, SpriteEffects effects, float layerDepth)
    {
        spriteBatch.Draw(Texture, position, SourceRectangle, color, rotation, Origin, scale, effects, layerDepth);
    }

    public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color)
    {
        spriteBatch.Draw(Texture, destinationRectangle, SourceRectangle, color);
    }
}
