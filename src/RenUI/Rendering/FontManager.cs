using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Rendering;

public sealed class FontManager
{
    private static FontManager? _instance;
    private static readonly object _lock = new();

    public static FontManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new FontManager();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, SpriteFont> _fonts = new();
    private readonly Dictionary<string, FontInfo> _fontInfos = new();
    private ContentManager? _content;
    private SpriteFont? _defaultFont;

    private FontManager() { }

    public SpriteFont? DefaultFont
    {
        get => _defaultFont;
        set => _defaultFont = value;
    }

    public void Initialize(ContentManager content)
    {
        _content = content;
    }

    public SpriteFont? LoadFont(string fontId, string contentPath)
    {
        if (_content == null)
            throw new InvalidOperationException("FontManager not initialized");

        if (_fonts.TryGetValue(fontId, out var existingFont))
            return existingFont;

        try
        {
            var font = _content.Load<SpriteFont>(contentPath);
            _fonts[fontId] = font;
            _fontInfos[fontId] = new FontInfo(fontId, contentPath, font);

            if (_defaultFont == null)
                _defaultFont = font;

            return font;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Failed to load font '{fontId}' from '{contentPath}': {ex.Message}");
            return null;
        }
    }

    public SpriteFont? LoadFontWithSizes(string fontFamily, string basePath, params int[] sizes)
    {
        SpriteFont? firstFont = null;

        foreach (var size in sizes)
        {
            var fontId = $"{fontFamily}_{size}";
            var contentPath = $"{basePath}_{size}";
            var font = LoadFont(fontId, contentPath);

            if (firstFont == null)
                firstFont = font;
        }

        return firstFont;
    }

    public void RegisterFont(string fontId, SpriteFont font, string? sourcePath = null)
    {
        _fonts[fontId] = font;
        _fontInfos[fontId] = new FontInfo(fontId, sourcePath ?? "registered", font);

        if (_defaultFont == null)
            _defaultFont = font;
    }

    public void UnregisterFont(string fontId)
    {
        _fonts.Remove(fontId);
        _fontInfos.Remove(fontId);
    }

    public SpriteFont? GetFont(string fontId)
    {
        if (_fonts.TryGetValue(fontId, out var font))
            return font;

        return _defaultFont;
    }

    public SpriteFont? GetFontOrDefault(string? fontId)
    {
        if (string.IsNullOrEmpty(fontId))
            return _defaultFont;

        return GetFont(fontId);
    }

    public SpriteFont GetFontRequired(string fontId)
    {
        if (_fonts.TryGetValue(fontId, out var font))
            return font;

        if (_defaultFont != null)
            return _defaultFont;

        throw new InvalidOperationException($"Font '{fontId}' not found and no default font is set");
    }

    public bool HasFont(string fontId)
    {
        return _fonts.ContainsKey(fontId);
    }

    public FontInfo? GetFontInfo(string fontId)
    {
        return _fontInfos.TryGetValue(fontId, out var info) ? info : null;
    }

    public IEnumerable<string> GetFontIds()
    {
        return _fonts.Keys;
    }

    public IEnumerable<FontInfo> GetAllFontInfos()
    {
        return _fontInfos.Values;
    }

    public Vector2 MeasureString(string fontId, string text)
    {
        var font = GetFont(fontId);
        return font?.MeasureString(text) ?? Vector2.Zero;
    }

    public Vector2 MeasureString(string text)
    {
        return _defaultFont?.MeasureString(text) ?? Vector2.Zero;
    }

    public float GetLineSpacing(string fontId)
    {
        var font = GetFont(fontId);
        return font?.LineSpacing ?? 0f;
    }

    public void DrawString(SpriteBatch spriteBatch, string fontId, string text, Vector2 position, Color color)
    {
        var font = GetFont(fontId);
        if (font != null)
        {
            spriteBatch.DrawString(font, text, position, color);
        }
    }

    public void DrawString(SpriteBatch spriteBatch, string fontId, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, float scale, SpriteEffects effects, float layerDepth)
    {
        var font = GetFont(fontId);
        if (font != null)
        {
            spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }
    }

    public void DrawString(SpriteBatch spriteBatch, string fontId, string text, Vector2 position, Color color,
        float rotation, Vector2 origin, Vector2 scale, SpriteEffects effects, float layerDepth)
    {
        var font = GetFont(fontId);
        if (font != null)
        {
            spriteBatch.DrawString(font, text, position, color, rotation, origin, scale, effects, layerDepth);
        }
    }

    public void Clear()
    {
        _fonts.Clear();
        _fontInfos.Clear();
        _defaultFont = null;
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

public class FontInfo
{
    public string Id { get; }
    public string SourcePath { get; }
    public SpriteFont Font { get; }
    public int LineSpacing => Font.LineSpacing;
    public float Spacing => Font.Spacing;

    public FontInfo(string id, string sourcePath, SpriteFont font)
    {
        Id = id;
        SourcePath = sourcePath;
        Font = font;
    }
}
