# RenUI Mimari Dokümantasyonu

## Genel Bakış

RenUI, MonoGame için tasarlanmış modüler bir UI kütüphanesidir. Temel tasarım prensipleri:

1. **Separation of Concerns**: Her modül tek bir sorumluluğa sahiptir
2. **Composition over Inheritance**: Davranışlar kompozisyon ile eklenir
3. **Interface Segregation**: Küçük, odaklı arayüzler
4. **Open/Closed**: Genişlemeye açık, değişikliğe kapalı

## Modül Yapısı

```
┌─────────────────────────────────────────────────────────────────┐
│                         RenUIGame                                │
│                    (Uygulama Giriş Noktası)                     │
└─────────────────────────────────────────────────────────────────┘
                                 │
                                 ▼
┌─────────────────────────────────────────────────────────────────┐
│                         UIManager                                │
│              (Merkezi Koordinasyon - Facade Pattern)            │
├─────────────────────────────────────────────────────────────────┤
│  ┌─────────────┐  ┌─────────────┐  ┌─────────────────────────┐  │
│  │InputManager │  │SceneManager │  │    ThemeManager         │  │
│  └─────────────┘  └─────────────┘  └─────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│     Input       │    │     Scenes      │    │    Styling      │
│    Module       │    │     Module      │    │    Module       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                 │
                                 ▼
                    ┌─────────────────────┐
                    │    SceneBase        │
                    │ (Ince Orkestrasyon) │
                    └─────────────────────┘
                                 │
         ┌───────────────────────┼───────────────────────┐
         ▼                       ▼                       ▼
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│    Elements     │    │     Layout      │    │   Rendering     │
│    Module       │    │     Module      │    │    Module       │
└─────────────────┘    └─────────────────┘    └─────────────────┘
```

## Core Modülü

### Interfaces

Temel soyutlamaları tanımlar:

- `IUpdatable`: Update döngüsüne katılım
- `IDrawable`: Draw döngüsüne katılım
- `IUIElement`: Temel UI eleman kontratı
- `IContainer`: Çocuk eleman yönetimi
- `IInputHandler`: Input işleme yeteneği
- `IScene`: Sahne yaşam döngüsü

### Events

Thread-safe event sistemi:

```csharp
public sealed class EventDispatcher<TEventArgs>
{
    void Subscribe(Action<TEventArgs> handler);
    void Unsubscribe(Action<TEventArgs> handler);
    void Dispatch(TEventArgs args);
}
```

### Primitives

Değer tipleri:
- `Padding` / `Margin`: Boşluk yönetimi
- `Anchor`: Konumlandırma
- `Size`: Boyut kısıtlamaları

## Elements Modülü

### Hiyerarşi

```
UIElement (abstract)
├── Label
├── Button  
├── TextBox
├── CheckBox
├── Slider
└── Container
    └── Panel
```

### UIElement Base

Tüm UI elemanlarının temel sınıfı:

```csharp
public abstract class UIElement : IUIElement, IInputHandler
{
    // Pozisyon ve boyut
    Rectangle Bounds { get; set; }
    
    // Durum
    bool IsEnabled { get; set; }
    bool IsVisible { get; set; }
    bool IsFocused { get; set; }
    bool IsHovered { get; set; }
    
    // Layout
    Anchor Anchor { get; set; }
    Margin Margin { get; set; }
    Padding Padding { get; set; }
    
    // Events
    EventDispatcher<MouseEventArgs> OnClick { get; }
    EventDispatcher<MouseEventArgs> OnMouseEnter { get; }
    // ...
    
    // Abstract
    abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
```

### Container

Çocuk eleman yönetimi ve layout uygulama:

```csharp
public class Container : UIElement, IContainer
{
    ILayoutStrategy? LayoutStrategy { get; set; }
    
    void AddChild(IUIElement child);
    void RemoveChild(IUIElement child);
    
    // Layout yeniden hesaplama
    void InvalidateLayout();
}
```

## Layout Modülü

Strategy Pattern ile değiştirilebilir layout algoritmaları:

```csharp
public interface ILayoutStrategy
{
    void ApplyLayout(IContainer container);
}
```

### Mevcut Stratejiler

1. **StackLayout**: Yatay veya dikey yığın, dağıtım modları destekler
2. **FlexLayout**: CSS Flexbox benzeri esnek kutu yerleşimi
3. **GridLayout**: Satır/sütun tabanlı ızgara (Pixel, Star, Auto boyutlandırma)
4. **WrapLayout**: Satır/sütun wrap eden galeri düzeni
5. **DockLayout**: Kenar yerleştirme (WPF tarzı)
6. **AbsoluteLayout**: Manuel pozisyonlama

## Input Modülü

Merkezi input durumu yönetimi:

```csharp
public interface IInputState
{
    Point MousePosition { get; }
    bool IsMouseButtonPressed(MouseButton button);
    bool IsKeyPressed(Keys key);
    // ...
}
```

Input propagation (Event Bubbling):

```
1. En üst çocuktan başla
2. HandleMouseInput() çağır
3. true dönerse dur, false ise parent'a geç
```

## Scene Modülü

### SceneBase

İnce orkestrasyon katmanı:

```csharp
public abstract class SceneBase : IScene
{
    // Tek root container
    protected Container Root { get; }
    
    // Yaşam döngüsü
    abstract void OnLoadContent();
    virtual void OnUnloadContent() { }
    virtual void OnEnter() { }
    virtual void OnExit() { }
    
    // Kolaylık metodları
    void AddElement(IUIElement element);
    T? GetElement<T>(string name);
}
```

### SceneManager

Stack-tabanlı sahne yönetimi:

```csharp
void ChangeScene(string name);  // Stack'i temizle ve yeni sahne
void PushScene(string name);    // Mevcut sahnenin üstüne ekle
void PopScene();                // En üstteki sahneyi kaldır
```

## Styling Modülü

### Theme

Merkezi renk ve stil tanımları:

```csharp
public class Theme
{
    Color PrimaryColor { get; set; }
    Color BackgroundColor { get; set; }
    Color TextColor { get; set; }
    SpriteFont? DefaultFont { get; set; }
    // ...
}
```

### ThemeManager

Tema değişikliği ve yönetimi:

```csharp
ThemeManager.Instance.SetTheme("Dark");
ThemeManager.Instance.ThemeChanged += OnThemeChanged;
```

## Rendering Modülü

### PrimitiveRenderer

Temel şekil çizimi:

```csharp
static void DrawRectangle(SpriteBatch, Rectangle, Color);
static void DrawRectangleBorder(SpriteBatch, Rectangle, Color, int thickness);
static void DrawRoundedRectangle(SpriteBatch, Rectangle, Color, int cornerRadius);
static void DrawLine(SpriteBatch, Vector2 start, Vector2 end, Color);
static void DrawCircle(SpriteBatch, Vector2 center, float radius, Color);
```

## Veri Akışı

### Update Döngüsü

```
RenUIGame.Update()
    └── UIManager.Update()
        ├── InputManager.Update()     # Input durumunu güncelle
        └── SceneManager.Update()
            └── CurrentScene.Update()
                ├── HandleMouseInput() # Input işle (yukarıdan aşağı)
                ├── HandleKeyboardInput()
                └── Root.Update()      # Çocukları güncelle
                    ├── Check dirty flag
                    ├── PerformLayout() # Dirty ise layout uygula
                    └── Children.Update()
```

### Draw Döngüsü

```
RenUIGame.Draw()
    └── UIManager.Draw()
        └── SceneManager.Draw()
            └── CurrentScene.Draw()
                └── Root.Draw()
                    └── DrawChildren() # DrawOrder'a göre sırala
```

## Best Practices

### Sahne Tasarımı

```csharp
// ✅ DOĞRU: İnce orkestrasyon
public class MenuScene : SceneBase
{
    protected override void OnLoadContent()
    {
        var menuPanel = CreateMenuPanel(); // Factory metod
        AddElement(menuPanel);
    }
    
    private Panel CreateMenuPanel() { /* ... */ }
}

// ❌ YANLIŞ: God class
public class MenuScene : SceneBase
{
    // Yüzlerce satır UI oluşturma kodu
    // İş mantığı
    // Animasyon kodu
    // Ses kodu
    // ...
}
```

### Özel Kontrol Oluşturma

```csharp
// ✅ DOĞRU: Kompozisyon
public class IconButton : Container
{
    private readonly Label _label;
    private readonly Image _icon;
    
    public IconButton(string text, Texture2D icon)
    {
        _icon = new Image(icon);
        _label = new Label(text);
        AddChild(_icon);
        AddChild(_label);
    }
}

// ❌ YANLIŞ: Derin kalıtım
public class IconButton : Button
{
    // Button'un tüm davranışlarını override etmek zorunda
}
```

### Event Handling

```csharp
// ✅ DOĞRU: Weak coupling
button.OnClick.Subscribe(OnButtonClick);

private void OnButtonClick(MouseEventArgs e)
{
    _gameState.StartGame(); // Dependency injection
}

// ❌ YANLIŞ: Tight coupling
button.OnClick.Subscribe(e => 
{
    GameManager.Instance.Player.Health = 100;
    UIManager.Instance.Scenes.ChangeScene("Game");
    AudioManager.Instance.Play("click");
});
```

## Performans Notları

1. **Dirty Flag Pattern**: Container'lar layout değişikliklerini dirty flag ile takip eder. Layout sadece dirty flag true olduğunda `Update()` döngüsünde yeniden hesaplanır:
   - Child ekleme/çıkarma işlemleri dirty flag'i otomatik set eder
   - Container boyut değişiklikleri dirty flag'i otomatik set eder
   - LayoutStrategy değişiklikleri dirty flag'i otomatik set eder
   
2. **Object Pooling**: Sık oluşturulan nesneler için havuz kullanın
3. **Texture Atlas**: Çok sayıda UI elemanı için atlas kullanın
4. **Culling**: Görünür alan dışındaki elemanları çizmeyin

## Genişletme Noktaları

- Yeni UI kontrolleri: `UIElement` veya `Container`'dan türeyin
- Yeni layout: `ILayoutStrategy` implement edin
- Yeni temalar: `Theme` oluşturup kaydedin
- Yeni event türleri: `UIEventArgs`'dan türeyin
