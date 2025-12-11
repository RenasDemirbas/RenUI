# RenUI Diğer Modüller API Referansı

## İçindekiler
- [UIManager](#uimanager)
- [Input Modülü](#input-modülü)
- [Scenes Modülü](#scenes-modülü)
- [Styling Modülü](#styling-modülü)
- [Rendering Modülü](#rendering-modülü)
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

// RenUIGame içinde kısayol
public class MyGame : RenUIGame
{
    protected override void OnUIInitialized()
    {
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
