# RenUI - MonoGame UI Library

Modüler, genişletilebilir ve endüstri standartlarına uygun MonoGame UI kütüphanesi.

## 🎯 Özellikler

- **Modüler Mimari**: Her sistem bağımsız modüller halinde tasarlanmıştır
- **Temiz Kod**: SOLID prensiplerine uygun, test edilebilir yapı
- **Layout Sistemleri**: StackLayout, GridLayout, DockLayout, AbsoluteLayout
- **Tema Desteği**: Kolay özelleştirilebilir Dark/Light temalar
- **Scene Yönetimi**: Stack-tabanlı sahne geçişleri
- **Input Yönetimi**: Merkezi klavye ve fare girişi işleme

## 📁 Proje Yapısı

```
RenUI/
├── src/
│   ├── RenUI/                      # Ana kütüphane
│   │   ├── Core/                   # Temel arayüzler ve yapılar
│   │   │   ├── Interfaces/         # IUIElement, IScene, IContainer vb.
│   │   │   ├── Events/             # Event sistemi
│   │   │   └── Primitives/         # Padding, Margin, Anchor, Size
│   │   ├── Elements/               # UI bileşenleri
│   │   │   ├── Base/               # UIElement, Container
│   │   │   └── Controls/           # Button, Label, TextBox, Panel vb.
│   │   ├── Input/                  # Input yönetimi
│   │   ├── Layout/                 # Layout stratejileri
│   │   ├── Rendering/              # Çizim yardımcıları
│   │   ├── Scenes/                 # Sahne sistemi
│   │   ├── Styling/                # Tema ve stil sistemi
│   │   ├── UIManager.cs            # Ana yönetici sınıf
│   │   └── RenUIGame.cs            # Temel Game sınıfı
│   └── RenUI.Editor/               # Editör uygulaması
│       ├── Scenes/
│       └── ...
└── README.md
```

## 🚀 Kurulum

### NuGet ile (Gelecekte)
```bash
dotnet add package RenUI
```

### Proje Referansı ile
1. RenUI projesini solution'ınıza ekleyin
2. Projenize referans ekleyin:
```xml
<ProjectReference Include="..\RenUI\RenUI.csproj" />
```

## 📖 Kullanım

### Temel Kullanım

```csharp
using RenUI;
using RenUI.Scenes;

public class MyGame : RenUIGame
{
    protected override void OnUIInitialized()
    {
        // Sahneleri kaydet
        UI.Scenes.RegisterScene("MainMenu", new MainMenuScene());
        UI.Scenes.RegisterScene("Game", new GameScene());
        
        // İlk sahneye geç
        UI.Scenes.ChangeScene("MainMenu");
    }
}
```

### Sahne Oluşturma

```csharp
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Elements.Controls;
using RenUI.Layout;
using RenUI.Scenes;

public class MainMenuScene : SceneBase
{
    private SpriteFont _font;

    public MainMenuScene() : base("MainMenu") { }

    protected override void OnLoadContent()
    {
        _font = Content.Load<SpriteFont>("DefaultFont");
        
        // Layout ayarla
        Root.LayoutStrategy = new StackLayout(Orientation.Vertical, 16)
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Root.Padding = new Padding(0, 100, 0, 0);

        // Başlık
        var title = new Label("My Game")
        {
            Font = _font,
            TextColor = Color.White,
            HorizontalAlignment = TextAlignment.Center
        };
        AddElement(title);

        // Başla butonu
        var startButton = new Button("Start Game")
        {
            Font = _font,
            Width = 200,
            Height = 50
        };
        startButton.OnClick.Subscribe(e => OnStartClick());
        AddElement(startButton);

        // Çıkış butonu
        var exitButton = new Button("Exit")
        {
            Font = _font,
            Width = 200,
            Height = 50
        };
        exitButton.OnClick.Subscribe(e => OnExitClick());
        AddElement(exitButton);
    }

    private void OnStartClick()
    {
        UIManager.Instance.Scenes.ChangeScene("Game");
    }

    private void OnExitClick()
    {
        Environment.Exit(0);
    }
}
```

### Layout Sistemleri

#### StackLayout
```csharp
var panel = new Panel();
panel.LayoutStrategy = new StackLayout(Orientation.Vertical, 8)
{
    HorizontalAlignment = HorizontalAlignment.Stretch
};
```

#### GridLayout
```csharp
var grid = new Panel();
var gridLayout = new GridLayout
{
    ColumnSpacing = 4,
    RowSpacing = 4
};
gridLayout.AddColumns(GridLength.Star, GridLength.Star, GridLength.Pixel(100));
gridLayout.AddRows(GridLength.Pixel(50), GridLength.Star);

grid.LayoutStrategy = gridLayout;

// Eleman pozisyonu ayarla
gridLayout.SetPosition(element, row: 0, column: 1, columnSpan: 2);
grid.AddChild(element);
```

#### DockLayout
```csharp
var container = new Panel();
var dockLayout = new DockLayout { LastChildFill = true };
container.LayoutStrategy = dockLayout;

var header = new Panel { Height = 50 };
dockLayout.SetDock(header, Dock.Top);
container.AddChild(header);

var sidebar = new Panel { Width = 200 };
dockLayout.SetDock(sidebar, Dock.Left);
container.AddChild(sidebar);

var content = new Panel(); // LastChildFill ile kalan alanı doldurur
container.AddChild(content);
```

### UI Kontrolleri

#### Button
```csharp
var button = new Button("Click Me")
{
    Font = font,
    Width = 120,
    Height = 40,
    BackgroundColor = new Color(70, 70, 70),
    HoverColor = new Color(90, 90, 90),
    CornerRadius = 4
};
button.OnClick.Subscribe(e => Console.WriteLine("Clicked!"));
```

#### TextBox
```csharp
var textBox = new TextBox
{
    Font = font,
    Width = 200,
    Height = 32,
    Placeholder = "Enter text...",
    MaxLength = 50
};
textBox.OnTextChanged.Subscribe(e => Console.WriteLine($"Text: {textBox.Text}"));
textBox.OnSubmit.Subscribe(e => Console.WriteLine("Enter pressed"));
```

#### CheckBox
```csharp
var checkbox = new CheckBox("Enable Feature", isChecked: true)
{
    Font = font
};
checkbox.OnCheckedChanged.Subscribe(e => 
    Console.WriteLine($"Checked: {e.IsChecked}"));
```

#### Slider
```csharp
var slider = new Slider
{
    MinValue = 0,
    MaxValue = 100,
    Value = 50,
    Step = 5,
    Width = 200,
    Height = 20
};
slider.OnValueChanged.Subscribe(e => 
    Console.WriteLine($"Value: {e.Value}"));
```

### Tema Sistemi

```csharp
// Mevcut temayı değiştir
ThemeManager.Instance.SetTheme("Light");

// Özel tema oluştur
var customTheme = new Theme
{
    Name = "Custom",
    PrimaryColor = new Color(100, 50, 200),
    BackgroundColor = new Color(20, 20, 30),
    TextColor = Color.White
};
ThemeManager.Instance.RegisterTheme(customTheme);
ThemeManager.Instance.SetTheme("Custom");

// Tema değişikliğini dinle
ThemeManager.Instance.ThemeChanged += theme => 
{
    Console.WriteLine($"Theme changed to: {theme.Name}");
};
```

### Event Sistemi

```csharp
// Event'e abone ol
button.OnClick.Subscribe(HandleClick);

// Event'ten çık
button.OnClick.Unsubscribe(HandleClick);

// Event'i temizle
button.OnClick.Clear();

private void HandleClick(MouseEventArgs e)
{
    Console.WriteLine($"Clicked at: {e.Position}");
    e.Handled = true; // Event'in yayılmasını durdur
}
```

## 🏗️ Mimari Prensipler

### Modülerlik
Her sistem bağımsız çalışır ve basit referanslarla entegre edilir:

```csharp
// Sadece input sistemi kullan
var inputManager = InputManager.Instance;
inputManager.Update(gameTime);
var state = inputManager.State;

// Sadece tema sistemi kullan  
var themeManager = ThemeManager.Instance;
var currentTheme = themeManager.CurrentTheme;
```

### Single Responsibility
- `UIElement`: Tek bir UI elemanını temsil eder
- `Container`: Çocuk elemanları yönetir
- `ILayoutStrategy`: Yerleşim mantığını uygular
- `SceneBase`: Sahne orkestrasyonu yapar

### Dependency Injection Ready
```csharp
// Interface tabanlı tasarım
public class MyScene : SceneBase
{
    private readonly IInputState _inputState;
    
    public MyScene(IInputState inputState) : base("MyScene")
    {
        _inputState = inputState;
    }
}
```

## 🔧 Özelleştirme

### Özel UI Elemanı Oluşturma

```csharp
public class ProgressBar : UIElement
{
    private float _progress;
    
    public float Progress
    {
        get => _progress;
        set
        {
            _progress = Math.Clamp(value, 0, 1);
            MarkDirty();
        }
    }
    
    public Color FillColor { get; set; } = Color.Green;
    public Color TrackColor { get; set; } = Color.DarkGray;

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;
        
        var bounds = GetAbsoluteBounds();
        
        // Track
        PrimitiveRenderer.DrawRectangle(spriteBatch, bounds, TrackColor);
        
        // Fill
        var fillWidth = (int)(bounds.Width * Progress);
        var fillRect = new Rectangle(bounds.X, bounds.Y, fillWidth, bounds.Height);
        PrimitiveRenderer.DrawRectangle(spriteBatch, fillRect, FillColor);
    }
}
```

### Özel Layout Stratejisi

```csharp
public class WrapLayout : ILayoutStrategy
{
    public int Spacing { get; set; } = 4;
    
    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;
        
        var bounds = element.Bounds;
        int x = element.Padding.Left;
        int y = element.Padding.Top;
        int rowHeight = 0;
        
        foreach (var child in container.Children)
        {
            if (child is not UIElement childElement) continue;
            
            if (x + childElement.Width > bounds.Width - element.Padding.Right)
            {
                x = element.Padding.Left;
                y += rowHeight + Spacing;
                rowHeight = 0;
            }
            
            childElement.Position = new Point(x, y);
            x += childElement.Width + Spacing;
            rowHeight = Math.Max(rowHeight, childElement.Height);
        }
    }
}
```

## 📋 Gereksinimler

- .NET 8.0 veya üzeri
- MonoGame 3.8.2 veya üzeri

## 📝 Lisans

MIT License

## 🤝 Katkıda Bulunma

1. Fork yapın
2. Feature branch oluşturun (`git checkout -b feature/amazing-feature`)
3. Değişikliklerinizi commit edin (`git commit -m 'Add amazing feature'`)
4. Branch'i push edin (`git push origin feature/amazing-feature`)
5. Pull Request açın
