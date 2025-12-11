# RenUI Diğer Modüller API Referansı

## İçindekiler
- [UIManager](#uimanager)
- [Input Modülü](#input-modülü)
- [Scenes Modülü](#scenes-modülü)
- [Styling Modülü](#styling-modülü)
- [Rendering Modülü](#rendering-modülü)
  - [PrimitiveRenderer](#primitiverenderer)
  - [FontManager](#fontmanager)
  - [SpriteManager](#spritemanager)
- [Serialization Modülü](#serialization-modülü)

---

## UIManager

Merkezi UI yönetim singleton'ı (Facade Pattern).

```csharp
public sealed class UIManager
{
    public static UIManager Instance { get; }
    
    // Alt sistemler
    public InputManager Input { get; }
    public SceneManager Scenes { get; }
    public ThemeManager Themes { get; }
    public UILayoutManager Layouts { get; }
    public FontManager Fonts { get; }
    public SpriteManager Sprites { get; }
    
    // Durum
    public bool IsInitialized { get; }
    public GraphicsDevice? GraphicsDevice { get; }
    public ContentManager? Content { get; }
    
    // Metodlar
    public void Initialize(GraphicsDevice graphicsDevice, ContentManager content);
    public void Update(GameTime gameTime);
    public void Draw(GameTime gameTime);
    public void OnWindowResize(int width, int height);
    public void Dispose();
    public static void Reset();
}
```

**Kullanım:**
```csharp
// Alt sistemlere erişim
var input = UIManager.Instance.Input;
var scenes = UIManager.Instance.Scenes;
var themes = UIManager.Instance.Themes;
var fonts = UIManager.Instance.Fonts;
var sprites = UIManager.Instance.Sprites;

// RenUIGame içinde kısayol
public class MyGame : RenUIGame
{
    protected override void OnUIInitialized()
    {
        // Font yükleme
        UI.Fonts.LoadFont("default", "Fonts/DefaultFont");
        UI.Fonts.LoadFont("title", "Fonts/TitleFont");
        
        // Sprite yükleme
        UI.Sprites.LoadTexture("logo", "Textures/Logo");
        UI.Sprites.LoadTexture("icons", "Textures/IconSheet");
        
        UI.Scenes.RegisterScene("Menu", new MenuScene());
        UI.Themes.SetTheme("Dark");
    }
}
```

---

## Input Modülü

### InputManager

```csharp
public sealed class InputManager : IUpdatable
{
    public static InputManager Instance { get; }
    public IInputState State { get; }
    public bool IsEnabled { get; set; }
    
    public void Update(GameTime gameTime);
    public static void Reset();
}
```

### InputState

```csharp
public sealed class InputState : IInputState
{
    // Mouse
    public MouseState CurrentMouseState { get; }
    public MouseState PreviousMouseState { get; }
    public Point MousePosition { get; }
    public Point MouseDelta { get; }
    public int ScrollWheelDelta { get; }
    
    // Keyboard
    public KeyboardState CurrentKeyboardState { get; }
    public KeyboardState PreviousKeyboardState { get; }
    
    // Mouse metodları
    bool IsMouseButtonPressed(MouseButton button);  // Bu frame basıldı
    bool IsMouseButtonReleased(MouseButton button); // Bu frame bırakıldı
    bool IsMouseButtonDown(MouseButton button);     // Şu an basılı
    
    // Keyboard metodları
    bool IsKeyPressed(Keys key);   // Bu frame basıldı
    bool IsKeyReleased(Keys key);  // Bu frame bırakıldı
    bool IsKeyDown(Keys key);      // Şu an basılı
}
```

**Input Durumu Farkları:**

| Metot | True Döner |
|-------|------------|
| `IsPressed` | Sadece ilk basış frame'i |
| `IsReleased` | Sadece bırakış frame'i |
| `IsDown` | Basılı olduğu her frame |

**Kullanım:**
```csharp
var input = InputManager.Instance.State;

// Tek tıklama
if (input.IsMouseButtonPressed(MouseButton.Left))
    HandleClick(input.MousePosition);

// Sürekli basılı (sürükleme)
if (input.IsMouseButtonDown(MouseButton.Left))
    HandleDrag(input.MouseDelta);

// Klavye kombinasyonu
if (input.IsKeyDown(Keys.LeftControl) && input.IsKeyPressed(Keys.S))
    SaveGame();

// Scroll
if (input.ScrollWheelDelta != 0)
    Zoom(input.ScrollWheelDelta > 0 ? 1.1f : 0.9f);
```

### KeyboardHelper

```csharp
public static class KeyboardHelper
{
    public static char? KeyToChar(Keys key, bool shift, bool capsLock);
    public static bool IsModifierKey(Keys key);
    public static bool IsNavigationKey(Keys key);
}
```

---

## Scenes Modülü

### SceneBase

```csharp
public abstract class SceneBase : IScene
{
    public string Name { get; }
    public bool IsLoaded { get; }
    public bool IsEnabled { get; set; }
    public bool IsVisible { get; set; }
    public int DrawOrder { get; set; }
    
    protected ContentManager? Content { get; }
    protected GraphicsDevice? GraphicsDevice { get; }
    protected Container Root { get; }
    
    protected SceneBase(string name);
    
    // Yaşam döngüsü
    public virtual void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
    public virtual void UnloadContent();
    public virtual void OnEnter();
    public virtual void OnExit();
    public virtual void Update(GameTime gameTime);
    public virtual void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    
    // Override edilmesi gerekenler
    protected abstract void OnLoadContent();
    protected virtual void OnUnloadContent() { }
    protected virtual void OnUpdate(GameTime gameTime) { }
    protected virtual void OnDraw(SpriteBatch spriteBatch, GameTime gameTime) { }
    
    // Yardımcılar
    protected void AddElement(IUIElement element);
    protected void RemoveElement(IUIElement element);
    protected T? GetElement<T>(string name) where T : class, IUIElement;
    public void UpdateViewport(int width, int height);
}
```

**Sahne Örneği:**
```csharp
public class MainMenuScene : SceneBase
{
    private SpriteFont _font;
    
    public MainMenuScene() : base("MainMenu") { }
    
    protected override void OnLoadContent()
    {
        _font = Content.Load<SpriteFont>("Fonts/Default");
        
        Root.LayoutStrategy = new StackLayout(Orientation.Vertical, 20)
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Root.Padding = new Padding(0, 150, 0, 0);
        
        AddElement(new Label("MY GAME") { Font = _font, TextColor = Color.Gold });
        
        var startBtn = new Button("Start") { Font = _font, Width = 200, Height = 50 };
        startBtn.OnClick.Subscribe(e => UIManager.Instance.Scenes.ChangeScene("Game"));
        AddElement(startBtn);
    }
    
    protected override void OnUpdate(GameTime gameTime)
    {
        if (InputManager.Instance.State.IsKeyPressed(Keys.Escape))
            Environment.Exit(0);
    }
}
```

### SceneManager

```csharp
public sealed class SceneManager : ISceneManager
{
    public static SceneManager Instance { get; }
    public IScene? CurrentScene { get; }
    
    public event Action<IScene?, IScene?>? SceneChanged;
    
    public void Initialize(ContentManager content, GraphicsDevice graphicsDevice);
    
    // Sahne kaydı
    public void RegisterScene(string name, IScene scene);
    public void UnregisterScene(string name);
    
    // Geçişler
    public void ChangeScene(string name);  // Stack temizle, yeni sahne
    public void PushScene(string name);    // Üstüne ekle
    public void PopScene();                // Üsttekini kaldır
    
    public void Update(GameTime gameTime);
    public void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    public void UpdateViewport(int width, int height);
    public static void Reset();
}
```

**Sahne Geçiş Stratejileri:**

| Metot | Kullanım |
|-------|----------|
| `ChangeScene` | Menu → Game (temiz geçiş) |
| `PushScene` | Game → Pause (game arka planda) |
| `PopScene` | Pause → Game (geri dön) |

```csharp
// Kayıt
UI.Scenes.RegisterScene("Menu", new MenuScene());
UI.Scenes.RegisterScene("Game", new GameScene());
UI.Scenes.RegisterScene("Pause", new PauseScene());

// Geçişler
UI.Scenes.ChangeScene("Menu");    // Başlangıç
UI.Scenes.ChangeScene("Game");    // Oyuna geç
UI.Scenes.PushScene("Pause");     // Pause aç
UI.Scenes.PopScene();             // Oyuna dön

// Event dinle
UI.Scenes.SceneChanged += (old, @new) =>
    Console.WriteLine($"{old?.Name} → {@new?.Name}");
```

---

## Styling Modülü

### Theme

```csharp
public class Theme
{
    public string Name { get; set; }
    
    // Temel renkler
    public Color PrimaryColor { get; set; }
    public Color SecondaryColor { get; set; }
    public Color AccentColor { get; set; }
    
    // Arka plan
    public Color BackgroundColor { get; set; }
    public Color SurfaceColor { get; set; }
    public Color PanelColor { get; set; }
    
    // Metin
    public Color TextColor { get; set; }
    public Color TextSecondaryColor { get; set; }
    public Color TextDisabledColor { get; set; }
    
    // Kenarlık
    public Color BorderColor { get; set; }
    public Color BorderFocusedColor { get; set; }
    
    // Kontrol
    public Color ControlBackground { get; set; }
    public Color ControlHover { get; set; }
    public Color ControlPressed { get; set; }
    public Color ControlDisabled { get; set; }
    
    // Seçim
    public Color SelectionColor { get; set; }
    
    // Fontlar
    public SpriteFont? DefaultFont { get; set; }
    public SpriteFont? HeadingFont { get; set; }
    public SpriteFont? SmallFont { get; set; }
    
    // Varsayılanlar
    public int DefaultSpacing { get; set; } = 8;
    public int DefaultPadding { get; set; } = 8;
    public int DefaultBorderThickness { get; set; } = 1;
    public int DefaultCornerRadius { get; set; } = 4;
    
    // Hazır temalar
    public static Theme Dark { get; }
    public static Theme Light { get; }
}
```

### ThemeManager

```csharp
public sealed class ThemeManager
{
    public static ThemeManager Instance { get; }
    public Theme CurrentTheme { get; }
    
    public event Action<Theme>? ThemeChanged;
    
    public void RegisterTheme(Theme theme);
    public void UnregisterTheme(string name);
    public void SetTheme(string name);
    public void SetTheme(Theme theme);
    public Theme? GetTheme(string name);
    public IEnumerable<string> GetAvailableThemes();
    public static void Reset();
}
```

**Özel Tema:**
```csharp
var neonTheme = new Theme
{
    Name = "Neon",
    PrimaryColor = new Color(0, 255, 128),
    BackgroundColor = new Color(10, 10, 20),
    TextColor = Color.Cyan,
    BorderColor = new Color(0, 200, 255),
    ControlBackground = new Color(20, 20, 40),
    ControlHover = new Color(40, 40, 80)
};

ThemeManager.Instance.RegisterTheme(neonTheme);
ThemeManager.Instance.SetTheme("Neon");
```

### ElementStyle

```csharp
public class ElementStyle
{
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public Color? BorderColor { get; set; }
    public int? BorderThickness { get; set; }
    public int? CornerRadius { get; set; }
    public float? Opacity { get; set; }
    
    public ElementStyle Merge(ElementStyle? other);
}
```

---

## Rendering Modülü

### PrimitiveRenderer

```csharp
public static class PrimitiveRenderer
{
    public static void Initialize(GraphicsDevice graphicsDevice);
    public static void Dispose();
    
    // Dikdörtgen
    public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle bounds, Color color);
    public static void DrawRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, 
                                            Color color, int thickness = 1);
    
    // Yuvarlak köşeli
    public static void DrawRoundedRectangle(SpriteBatch spriteBatch, Rectangle bounds, 
                                             Color color, int cornerRadius);
    public static void DrawRoundedRectangleBorder(SpriteBatch spriteBatch, Rectangle bounds, 
                                                   Color color, int cornerRadius, int thickness = 1);
    
    // Çizgi
    public static void DrawLine(SpriteBatch spriteBatch, Vector2 start, Vector2 end, 
                                Color color, int thickness = 1);
    
    // Daire
    public static void DrawCircle(SpriteBatch spriteBatch, Vector2 center, float radius, 
                                  Color color, int segments = 32);
}
```

**Kullanım:**
```csharp
public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
{
    var bounds = GetAbsoluteBounds();
    
    // Arka plan
    PrimitiveRenderer.DrawRoundedRectangle(spriteBatch, bounds, Color.DarkGray, 8);
    
    // Kenarlık
    PrimitiveRenderer.DrawRoundedRectangleBorder(spriteBatch, bounds, Color.White, 8, 2);
    
    // Progress fill
    var fillRect = new Rectangle(bounds.X, bounds.Y, (int)(bounds.Width * 0.7f), bounds.Height);
    PrimitiveRenderer.DrawRectangle(spriteBatch, fillRect, Color.Green);
    
    // Divider
    PrimitiveRenderer.DrawLine(spriteBatch,
        new Vector2(bounds.X, bounds.Bottom),
        new Vector2(bounds.Right, bounds.Bottom),
        Color.Gray, 1);
}
```

---

### FontManager

Font (yazı tipi) yönetim singleton'ı. TTF fontlarını MonoGame SpriteFont formatında yükler ve yönetir.

```csharp
public sealed class FontManager
{
    public static FontManager Instance { get; }
    
    // Varsayılan font
    public SpriteFont? DefaultFont { get; set; }
    
    // Başlatma
    public void Initialize(ContentManager content);
    
    // Font yükleme
    public SpriteFont? LoadFont(string fontId, string contentPath);
    public SpriteFont? LoadFontWithSizes(string fontFamily, string basePath, params int[] sizes);
    
    // Font kayıt
    public void RegisterFont(string fontId, SpriteFont font, string? sourcePath = null);
    public void UnregisterFont(string fontId);
    
    // Font erişimi
    public SpriteFont? GetFont(string fontId);
    public SpriteFont? GetFontOrDefault(string? fontId);
    public SpriteFont GetFontRequired(string fontId);
    public bool HasFont(string fontId);
    
    // Font bilgisi
    public FontInfo? GetFontInfo(string fontId);
    public IEnumerable<string> GetFontIds();
    public IEnumerable<FontInfo> GetAllFontInfos();
    
    // Yardımcı metodlar
    public Vector2 MeasureString(string fontId, string text);
    public Vector2 MeasureString(string text);  // DefaultFont ile
    public float GetLineSpacing(string fontId);
    
    // Çizim yardımcıları
    public void DrawString(SpriteBatch spriteBatch, string fontId, string text, 
                          Vector2 position, Color color);
    public void DrawString(SpriteBatch spriteBatch, string fontId, string text, 
                          Vector2 position, Color color, float rotation, 
                          Vector2 origin, float scale, SpriteEffects effects, float layerDepth);
    
    public void Clear();
    public static void Reset();
}

public class FontInfo
{
    public string Id { get; }
    public string SourcePath { get; }
    public SpriteFont Font { get; }
    public int LineSpacing { get; }
    public float Spacing { get; }
}
```

**TTF Font Kullanımı:**

MonoGame'de TTF fontları doğrudan kullanılamaz. Önce `.spritefont` dosyasına dönüştürülmesi gerekir:

1. **Content Pipeline ile:**
```xml
<!-- Content/Fonts/Arial.spritefont -->
<?xml version="1.0" encoding="utf-8"?>
<XnaContent xmlns:Graphics="Microsoft.Xna.Framework.Content.Pipeline.Graphics">
  <Asset Type="Graphics:FontDescription">
    <FontName>Arial</FontName>
    <Size>16</Size>
    <Spacing>0</Spacing>
    <UseKerning>true</UseKerning>
    <Style>Regular</Style>
    <CharacterRegions>
      <CharacterRegion>
        <Start>&#32;</Start>
        <End>&#126;</End>
      </CharacterRegion>
      <!-- Türkçe karakterler -->
      <CharacterRegion>
        <Start>&#199;</Start>
        <End>&#199;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#231;</Start>
        <End>&#231;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#286;</Start>
        <End>&#287;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#304;</Start>
        <End>&#305;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#214;</Start>
        <End>&#214;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#246;</Start>
        <End>&#246;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#350;</Start>
        <End>&#351;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#220;</Start>
        <End>&#220;</End>
      </CharacterRegion>
      <CharacterRegion>
        <Start>&#252;</Start>
        <End>&#252;</End>
      </CharacterRegion>
    </CharacterRegions>
  </Asset>
</XnaContent>
```

2. **Farklı boyutlar için:**
```csharp
// Fonts/Roboto_12.spritefont, Fonts/Roboto_16.spritefont, Fonts/Roboto_24.spritefont
UI.Fonts.LoadFontWithSizes("Roboto", "Fonts/Roboto", 12, 16, 24);

// Erişim
var smallFont = UI.Fonts.GetFont("Roboto_12");
var normalFont = UI.Fonts.GetFont("Roboto_16");
var largeFont = UI.Fonts.GetFont("Roboto_24");
```

**Kullanım Örnekleri:**

```csharp
// Başlatma
protected override void OnUIInitialized()
{
    // Fontları yükle
    UI.Fonts.LoadFont("default", "Fonts/OpenSans");
    UI.Fonts.LoadFont("title", "Fonts/Montserrat_Bold");
    UI.Fonts.LoadFont("mono", "Fonts/JetBrainsMono");
    
    // Varsayılan font ayarla
    UI.Fonts.DefaultFont = UI.Fonts.GetFont("default");
}

// UI elemanlarında kullanım
var label = new Label("Merhaba Dünya!")
{
    Font = UI.Fonts.GetFont("title"),
    TextColor = Color.Gold
};

var button = new Button("Başla")
{
    Font = UI.Fonts.GetFontOrDefault("default"),
    Width = 200,
    Height = 50
};

// Metin ölçümü
var textSize = UI.Fonts.MeasureString("title", "Game Over");
Console.WriteLine($"Metin boyutu: {textSize.X}x{textSize.Y}");

// Özel çizim
public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
{
    UI.Fonts.DrawString(spriteBatch, "mono", "Score: 1000", 
        new Vector2(10, 10), Color.White);
}
```

**Font Listesi:**
```csharp
// Yüklü fontları listele
foreach (var fontId in UI.Fonts.GetFontIds())
{
    var info = UI.Fonts.GetFontInfo(fontId);
    Console.WriteLine($"Font: {info.Id}, Path: {info.SourcePath}, LineSpacing: {info.LineSpacing}");
}
```

---

### SpriteManager

Sprite (görüntü) ve texture yönetim singleton'ı. Sprite sheet ve atlas desteği içerir.

```csharp
public sealed class SpriteManager
{
    public static SpriteManager Instance { get; }
    
    // Başlatma
    public void Initialize(ContentManager content, GraphicsDevice graphicsDevice);
    
    // Texture yükleme
    public Texture2D? LoadTexture(string textureId, string contentPath);
    public Texture2D? LoadTextureFromFile(string textureId, string filePath);
    
    // Texture kayıt
    public void RegisterTexture(string textureId, Texture2D texture);
    public void UnregisterTexture(string textureId);
    
    // Texture erişimi
    public Texture2D? GetTexture(string textureId);
    public bool HasTexture(string textureId);
    
    // Sprite Sheet
    public SpriteSheet CreateSpriteSheet(string sheetId, string textureId, 
                                          int frameWidth, int frameHeight);
    public SpriteSheet CreateSpriteSheet(string sheetId, Texture2D texture, 
                                          int frameWidth, int frameHeight);
    public SpriteSheet? GetSpriteSheet(string sheetId);
    
    // Sprite Atlas
    public SpriteAtlas CreateAtlas(string atlasId, string textureId);
    public SpriteAtlas? GetAtlas(string atlasId);
    public Sprite? GetSprite(string atlasId, string spriteName);
    
    // Listeleme
    public IEnumerable<string> GetTextureIds();
    public IEnumerable<string> GetSpriteSheetIds();
    public IEnumerable<string> GetAtlasIds();
    
    public void Clear();
    public static void Reset();
}
```

**SpriteSheet Sınıfı:**

Eşit boyutlu karelere bölünmüş sprite sheet'ler için.

```csharp
public class SpriteSheet
{
    public string Id { get; }
    public Texture2D Texture { get; }
    public int FrameWidth { get; }
    public int FrameHeight { get; }
    public int Columns { get; }
    public int Rows { get; }
    public int TotalFrames { get; }
    
    public Rectangle GetFrameRectangle(int frameIndex);
    public Rectangle GetFrameRectangle(int column, int row);
}
```

**SpriteAtlas Sınıfı:**

Farklı boyutlarda sprite'lar içeren atlas'lar için.

```csharp
public class SpriteAtlas
{
    public string Id { get; }
    public Texture2D Texture { get; }
    
    public void DefineSprite(string name, Rectangle sourceRectangle, 
                             Vector2? origin = null, Vector2? pivot = null);
    public void DefineSprite(string name, int x, int y, int width, int height, 
                             Vector2? origin = null, Vector2? pivot = null);
    
    public Sprite? GetSprite(string name);
    public bool HasSprite(string name);
    public IEnumerable<string> GetSpriteNames();
    public IEnumerable<Sprite> GetAllSprites();
}
```

**Sprite Sınıfı:**

```csharp
public class Sprite
{
    public string Name { get; }
    public Texture2D Texture { get; }
    public Rectangle SourceRectangle { get; }
    public Vector2 Origin { get; }
    public Vector2 Pivot { get; }
    public int Width { get; }
    public int Height { get; }
    
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color);
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, 
                    float rotation, float scale, SpriteEffects effects, float layerDepth);
    public void Draw(SpriteBatch spriteBatch, Rectangle destinationRectangle, Color color);
}
```

**Kullanım Örnekleri:**

```csharp
// Başlatma
protected override void OnUIInitialized()
{
    // Texture yükleme
    UI.Sprites.LoadTexture("logo", "Textures/GameLogo");
    UI.Sprites.LoadTexture("background", "Textures/MenuBackground");
    UI.Sprites.LoadTexture("icons", "Textures/IconSheet");
    UI.Sprites.LoadTexture("ui_atlas", "Textures/UIAtlas");
    
    // Sprite Sheet oluşturma (64x64 kareler)
    var iconSheet = UI.Sprites.CreateSpriteSheet("icon_sheet", "icons", 64, 64);
    
    // Atlas oluşturma ve sprite tanımlama
    var atlas = UI.Sprites.CreateAtlas("ui", "ui_atlas");
    atlas.DefineSprite("button_normal", 0, 0, 200, 50);
    atlas.DefineSprite("button_hover", 0, 50, 200, 50);
    atlas.DefineSprite("button_pressed", 0, 100, 200, 50);
    atlas.DefineSprite("checkbox_unchecked", 200, 0, 24, 24);
    atlas.DefineSprite("checkbox_checked", 224, 0, 24, 24);
    atlas.DefineSprite("slider_track", 200, 24, 200, 8);
    atlas.DefineSprite("slider_thumb", 200, 32, 16, 16);
}

// Image elementi ile kullanım
var logo = new Image
{
    Texture = UI.Sprites.GetTexture("logo"),
    Width = 400,
    Height = 200,
    StretchMode = ImageStretchMode.Uniform
};

// Sprite Sheet frame kullanımı
var iconImage = new Image
{
    Texture = UI.Sprites.GetTexture("icons"),
    Width = 64,
    Height = 64
};
iconImage.SetSpriteSheetFrame(5, 64, 64, 8); // 5. frame, 8 sütunlu sheet

// Atlas sprite kullanımı
var atlas = UI.Sprites.GetAtlas("ui");
var buttonSprite = atlas?.GetSprite("button_normal");
if (buttonSprite != null)
{
    buttonSprite.Draw(spriteBatch, new Vector2(100, 100), Color.White);
}

// Özel çizim
public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
{
    var sheet = UI.Sprites.GetSpriteSheet("icon_sheet");
    if (sheet != null)
    {
        var frameRect = sheet.GetFrameRectangle(_currentFrame);
        spriteBatch.Draw(sheet.Texture, Position.ToVector2(), frameRect, Color.White);
    }
}
```

**Animasyon Örneği:**

```csharp
public class AnimatedImage : Image
{
    private SpriteSheet? _spriteSheet;
    private int _currentFrame;
    private float _frameTime;
    private float _frameTimer;
    private int _startFrame;
    private int _endFrame;
    private bool _isLooping = true;
    
    public void SetAnimation(string sheetId, int startFrame, int endFrame, float fps)
    {
        _spriteSheet = SpriteManager.Instance.GetSpriteSheet(sheetId);
        if (_spriteSheet != null)
        {
            Texture = _spriteSheet.Texture;
            _startFrame = startFrame;
            _endFrame = endFrame;
            _currentFrame = startFrame;
            _frameTime = 1f / fps;
            _frameTimer = 0;
            UpdateSourceRectangle();
        }
    }
    
    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        
        if (_spriteSheet == null) return;
        
        _frameTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_frameTimer >= _frameTime)
        {
            _frameTimer = 0;
            _currentFrame++;
            
            if (_currentFrame > _endFrame)
            {
                _currentFrame = _isLooping ? _startFrame : _endFrame;
            }
            
            UpdateSourceRectangle();
        }
    }
    
    private void UpdateSourceRectangle()
    {
        if (_spriteSheet != null)
        {
            SourceRectangle = _spriteSheet.GetFrameRectangle(_currentFrame);
        }
    }
}
```

---

## Serialization Modülü

### UILayoutData

```csharp
public class UILayoutData
{
    public string Version { get; set; } = "1.0";
    public string Name { get; set; }
    public string Description { get; set; }
    public int Width { get; set; } = 1920;
    public int Height { get; set; } = 1080;
    public ResourcesData Resources { get; set; }
    public List<UIElementData> Elements { get; set; }
    public Dictionary<string, string> Metadata { get; set; }
}

public class ResourcesData
{
    public Dictionary<string, string> Textures { get; set; }
    public Dictionary<string, string> Fonts { get; set; }
    public Dictionary<string, string> Sounds { get; set; }
}
```

### UIElementData

```csharp
public class UIElementData
{
    public string Id { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
    public string? TextureId { get; set; }
    public string? FontId { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public int DrawOrder { get; set; }
    public string Anchor { get; set; } = "TopLeft";
    public SpacingData Margin { get; set; }
    public SpacingData Padding { get; set; }
    public Dictionary<string, object> Properties { get; set; }
    public List<UIElementData> Children { get; set; }
}
```

### UISerializer

```csharp
public static class UISerializer
{
    public static string Serialize(UILayoutData layout);
    public static UILayoutData? Deserialize(string json);
    public static async Task SaveAsync(UILayoutData layout, string filePath);
    public static async Task<UILayoutData?> LoadAsync(string filePath);
    public static UILayoutData Save(string filePath, UILayoutData layout);
    public static UILayoutData? Load(string filePath);
    public static UIElementData ToData(UIElement element);
}
```

### UIBuilder

```csharp
public class UIBuilder
{
    public IReadOnlyDictionary<string, UIElement> Elements { get; }
    
    public UIBuilder(ContentManager content);
    public Container Build(UILayoutData layout);
    public void ApplyLayout(Container root, UILayoutData layout);
    public T? GetElement<T>(string id) where T : UIElement;
    public Texture2D? GetTexture(string id);
    public SpriteFont? GetFont(string id);
}
```

**JSON'dan UI Yükleme:**
```csharp
// JSON dosyasından yükle
var layout = UISerializer.Load("Content/UI/MainMenu.ui.json");
if (layout != null)
{
    var builder = new UIBuilder(Content);
    var root = builder.Build(layout);
    
    // Butonlara event bağla
    var startBtn = builder.GetElement<Button>("btn_start");
    startBtn?.OnClick.Subscribe(e => StartGame());
}
```

**JSON Formatı Örneği:**
```json
{
  "version": "1.0",
  "name": "MainMenu",
  "width": 1920,
  "height": 1080,
  "resources": {
    "fonts": { "default": "Fonts/DefaultFont" }
  },
  "elements": [
    {
      "id": "btn_start",
      "type": "Button",
      "name": "StartButton",
      "fontId": "default",
      "x": 860, "y": 400,
      "width": 200, "height": 50,
      "properties": {
        "text": "Start Game",
        "backgroundColor": "#0078D7FF",
        "cornerRadius": 8
      }
    }
  ]
}
```
