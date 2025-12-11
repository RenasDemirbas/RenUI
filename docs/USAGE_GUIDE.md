# RenUI Kullanım Rehberi

Bu rehber, RenUI kütüphanesinin pratik kullanımını adım adım açıklamaktadır.

---

## İçindekiler

1. [Hızlı Başlangıç](#hızlı-başlangıç)
2. [Proje Oluşturma](#proje-oluşturma)
3. [Sahne Geliştirme](#sahne-geliştirme)
4. [UI Tasarımı](#ui-tasarımı)
5. [Event Yönetimi](#event-yönetimi)
6. [Tema Özelleştirme](#tema-özelleştirme)
7. [JSON ile UI Tasarımı](#json-ile-ui-tasarımı)
8. [Best Practices](#best-practices)
9. [Sık Karşılaşılan Sorunlar](#sık-karşılaşılan-sorunlar)

---

## Hızlı Başlangıç

### 1. Proje Referansı Ekle

```xml
<ProjectReference Include="..\RenUI\RenUI.csproj" />
```

### 2. Game Sınıfı Oluştur

```csharp
using RenUI;
using RenUI.Scenes;

public class MyGame : RenUIGame
{
    protected override void OnUIInitialized()
    {
        UI.Scenes.RegisterScene("Menu", new MainMenuScene());
        UI.Scenes.ChangeScene("Menu");
    }
}
```

### 3. Sahne Oluştur

```csharp
using RenUI.Scenes;
using RenUI.Elements.Controls;
using RenUI.Layout;

public class MainMenuScene : SceneBase
{
    public MainMenuScene() : base("MainMenu") { }
    
    protected override void OnLoadContent()
    {
        var font = Content.Load<SpriteFont>("Fonts/Default");
        
        Root.LayoutStrategy = new StackLayout(Orientation.Vertical, 20)
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Root.Padding = new Padding(0, 200, 0, 0);
        
        AddElement(new Label("MY GAME") { Font = font, TextColor = Color.Gold });
        
        var startBtn = new Button("Start") { Font = font, Width = 200, Height = 50 };
        startBtn.OnClick.Subscribe(e => Console.WriteLine("Started!"));
        AddElement(startBtn);
    }
}
```

### 4. Çalıştır

```csharp
using var game = new MyGame();
game.Run();
```

---

## Proje Oluşturma

### Önerilen Proje Yapısı

```
MyGame/
├── Content/
│   ├── Fonts/
│   │   └── DefaultFont.spritefont
│   ├── Textures/
│   │   └── ...
│   └── UI/
│       ├── MainMenu.ui.json
│       └── Settings.ui.json
├── Scenes/
│   ├── MainMenuScene.cs
│   ├── GameScene.cs
│   └── SettingsScene.cs
├── MyGame.cs
└── Program.cs
```

### Content Pipeline Ayarları

`Content/Content.mgcb`:
```
#----------------------------- Global Properties ----------------------------#
/outputDir:bin/$(Platform)
/intermediateDir:obj/$(Platform)
/platform:DesktopGL
/config:
/profile:Reach
/compress:False

#---------------------------------- Content ---------------------------------#
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/build:Fonts/DefaultFont.spritefont
```

### Font Tanımı

`Content/Fonts/DefaultFont.spritefont`:
```xml
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

---

## Sahne Geliştirme

### Sahne Yaşam Döngüsü

```
RegisterScene() → LoadContent() → OnEnter() → Update/Draw → OnExit() → UnloadContent()
```

### Tam Sahne Örneği

```csharp
public class GameMenuScene : SceneBase
{
    private SpriteFont _titleFont;
    private SpriteFont _buttonFont;
    
    public GameMenuScene() : base("GameMenu") { }
    
    protected override void OnLoadContent()
    {
        // Fontları yükle
        _titleFont = Content.Load<SpriteFont>("Fonts/TitleFont");
        _buttonFont = Content.Load<SpriteFont>("Fonts/ButtonFont");
        
        // Ana container ayarları
        Root.LayoutStrategy = new StackLayout(Orientation.Vertical, 24)
        {
            HorizontalAlignment = HorizontalAlignment.Center
        };
        Root.Padding = new Padding(0, 150, 0, 0);
        
        // Başlık
        AddElement(CreateTitle());
        
        // Buton paneli
        AddElement(CreateButtonPanel());
        
        // Versiyon etiketi
        AddElement(CreateVersionLabel());
    }
    
    private Label CreateTitle()
    {
        return new Label("AWESOME GAME")
        {
            Font = _titleFont,
            TextColor = Color.Gold,
            HorizontalAlignment = TextAlignment.Center
        };
    }
    
    private Panel CreateButtonPanel()
    {
        var panel = new Panel
        {
            Width = 300,
            Height = 250,
            BackgroundColor = Color.Transparent,
            BorderThickness = 0
        };
        
        panel.LayoutStrategy = new StackLayout(Orientation.Vertical, 16)
        {
            HorizontalAlignment = HorizontalAlignment.Stretch
        };
        
        panel.AddChild(CreateMenuButton("New Game", OnNewGame));
        panel.AddChild(CreateMenuButton("Continue", OnContinue));
        panel.AddChild(CreateMenuButton("Settings", OnSettings));
        panel.AddChild(CreateMenuButton("Exit", OnExit));
        
        return panel;
    }
    
    private Button CreateMenuButton(string text, Action onClick)
    {
        var button = new Button(text)
        {
            Font = _buttonFont,
            Height = 50,
            BackgroundColor = new Color(50, 50, 60),
            HoverColor = new Color(70, 70, 85),
            PressedColor = new Color(40, 40, 50),
            CornerRadius = 8,
            BorderThickness = 0
        };
        
        button.OnClick.Subscribe(e => onClick());
        return button;
    }
    
    private Label CreateVersionLabel()
    {
        return new Label("v1.0.0")
        {
            Font = _buttonFont,
            TextColor = new Color(128, 128, 128),
            Margin = new Margin(0, 50, 0, 0)
        };
    }
    
    private void OnNewGame()
    {
        UIManager.Instance.Scenes.ChangeScene("Game");
    }
    
    private void OnContinue()
    {
        // Save yükle ve oyuna geç
    }
    
    private void OnSettings()
    {
        UIManager.Instance.Scenes.PushScene("Settings");
    }
    
    private void OnExit()
    {
        Environment.Exit(0);
    }
    
    protected override void OnUpdate(GameTime gameTime)
    {
        // ESC ile çıkış
        if (InputManager.Instance.State.IsKeyPressed(Keys.Escape))
            OnExit();
    }
}
```

### Sahne Geçişleri

```csharp
// Temiz geçiş (önceki sahne unload edilir)
UI.Scenes.ChangeScene("Game");

// Üst üste sahne (pause menü için)
UI.Scenes.PushScene("Pause");

// Üstteki sahneyi kapat
UI.Scenes.PopScene();

// Sahne değişikliğini dinle
UI.Scenes.SceneChanged += (oldScene, newScene) =>
{
    Console.WriteLine($"Geçiş: {oldScene?.Name} → {newScene?.Name}");
};
```

---

## UI Tasarımı

### Form Tasarımı

```csharp
private Panel CreateLoginForm()
{
    var form = new Panel
    {
        Width = 350,
        Height = 200,
        BackgroundColor = new Color(40, 40, 45),
        CornerRadius = 12,
        Padding = new Padding(24)
    };
    
    var layout = new GridLayout { RowSpacing = 12, ColumnSpacing = 12 };
    layout.AddColumns(GridLength.Pixel(80), GridLength.Star);
    layout.AddRows(
        GridLength.Pixel(32),
        GridLength.Pixel(32),
        GridLength.Pixel(44)
    );
    form.LayoutStrategy = layout;
    
    // Username row
    var lblUser = new Label("Username:") { Font = _font };
    layout.SetPosition(lblUser, 0, 0);
    form.AddChild(lblUser);
    
    var txtUser = new TextBox { Font = _font, Height = 32, Placeholder = "Enter username" };
    layout.SetPosition(txtUser, 0, 1);
    form.AddChild(txtUser);
    
    // Password row
    var lblPass = new Label("Password:") { Font = _font };
    layout.SetPosition(lblPass, 1, 0);
    form.AddChild(lblPass);
    
    var txtPass = new TextBox { Font = _font, Height = 32, IsPassword = true };
    layout.SetPosition(txtPass, 1, 1);
    form.AddChild(txtPass);
    
    // Login button
    var btnLogin = new Button("Login") { Font = _font, Height = 40, CornerRadius = 6 };
    layout.SetPosition(btnLogin, 2, 0, columnSpan: 2);
    btnLogin.OnClick.Subscribe(e => Login(txtUser.Text, txtPass.Text));
    form.AddChild(btnLogin);
    
    return form;
}
```

### Settings Panel

```csharp
private Panel CreateSettingsPanel()
{
    var panel = new Panel
    {
        Width = 400,
        Height = 350,
        BackgroundColor = new Color(35, 35, 40),
        CornerRadius = 8,
        Padding = new Padding(20)
    };
    
    panel.LayoutStrategy = new StackLayout(Orientation.Vertical, 16);
    
    // Başlık
    panel.AddChild(new Label("Settings")
    {
        Font = _titleFont,
        TextColor = Color.White,
        Margin = new Margin(0, 0, 0, 10)
    });
    
    // Ses ayarları
    panel.AddChild(CreateSettingRow("Master Volume", CreateVolumeSlider("master")));
    panel.AddChild(CreateSettingRow("Music Volume", CreateVolumeSlider("music")));
    panel.AddChild(CreateSettingRow("SFX Volume", CreateVolumeSlider("sfx")));
    
    // Toggle ayarları
    panel.AddChild(new CheckBox("Fullscreen") { Font = _font, Name = "chkFullscreen" });
    panel.AddChild(new CheckBox("VSync") { Font = _font, Name = "chkVsync", IsChecked = true });
    
    // Butonlar
    var buttonRow = new Panel { Height = 50, BackgroundColor = Color.Transparent };
    buttonRow.LayoutStrategy = new StackLayout(Orientation.Horizontal, 12)
    {
        HorizontalAlignment = HorizontalAlignment.Right
    };
    
    var btnCancel = new Button("Cancel") { Font = _font, Width = 100, Height = 40 };
    btnCancel.OnClick.Subscribe(e => UI.Scenes.PopScene());
    buttonRow.AddChild(btnCancel);
    
    var btnSave = new Button("Save") { Font = _font, Width = 100, Height = 40 };
    btnSave.OnClick.Subscribe(e => SaveSettings());
    buttonRow.AddChild(btnSave);
    
    panel.AddChild(buttonRow);
    
    return panel;
}

private Panel CreateSettingRow(string label, UIElement control)
{
    var row = new Panel { Height = 30, BackgroundColor = Color.Transparent };
    row.LayoutStrategy = new GridLayout { ColumnSpacing = 16 };
    
    var layout = row.LayoutStrategy as GridLayout;
    layout.AddColumns(GridLength.Pixel(120), GridLength.Star);
    layout.AddRows(GridLength.Star);
    
    var lbl = new Label(label) { Font = _font };
    layout.SetPosition(lbl, 0, 0);
    row.AddChild(lbl);
    
    layout.SetPosition(control, 0, 1);
    row.AddChild(control);
    
    return row;
}

private Slider CreateVolumeSlider(string name)
{
    return new Slider
    {
        Name = $"slider_{name}",
        MinValue = 0,
        MaxValue = 100,
        Value = 80,
        Step = 5,
        Height = 20
    };
}
```

---

## Event Yönetimi

### Event Abone Olma Yöntemleri

```csharp
// 1. Lambda ile
button.OnClick.Subscribe(e => HandleClick());

// 2. Metot referansı ile
button.OnClick.Subscribe(HandleButtonClick);

// 3. Inline işlem
button.OnClick.Subscribe(e =>
{
    Console.WriteLine($"Clicked at {e.Position}");
    e.Handled = true;  // Event yayılmasını durdur
});
```

### Event Aboneliğinden Çıkma

```csharp
// Metot referansı ile çıkış
button.OnClick.Unsubscribe(HandleButtonClick);

// Tüm abonelikleri temizle
button.OnClick.Clear();
```

### Event Propagation

```csharp
button.OnClick.Subscribe(e =>
{
    DoSomething();
    e.Handled = true;  // Parent'a yayılmayı engelle
});
```

### Yaygın Event Patterns

```csharp
// TextBox validation
textBox.OnTextChanged.Subscribe(e =>
{
    var isValid = ValidateInput(textBox.Text);
    textBox.BorderColor = isValid ? Color.Green : Color.Red;
});

// Slider feedback
slider.OnValueChanged.Subscribe(e =>
{
    volumeLabel.Text = $"Volume: {(int)e.Value}%";
    AudioManager.SetVolume(e.Value / 100f);
});

// CheckBox toggle
checkbox.OnCheckedChanged.Subscribe(e =>
{
    if (e.IsChecked)
        EnableFeature();
    else
        DisableFeature();
});

// Focus tracking
textBox.OnFocusChanged.Subscribe(e =>
{
    if (e.IsFocused)
        textBox.BackgroundColor = new Color(50, 50, 60);
    else
        textBox.BackgroundColor = new Color(30, 30, 35);
});
```

---

## Tema Özelleştirme

### Hazır Temaları Kullanma

```csharp
// Dark tema (varsayılan)
UI.Themes.SetTheme("Dark");

// Light tema
UI.Themes.SetTheme("Light");
```

### Özel Tema Oluşturma

```csharp
var cyberpunkTheme = new Theme
{
    Name = "Cyberpunk",
    
    // Ana renkler
    PrimaryColor = new Color(255, 0, 128),      // Neon pink
    SecondaryColor = new Color(0, 255, 255),    // Cyan
    AccentColor = new Color(255, 255, 0),       // Yellow
    
    // Arka plan
    BackgroundColor = new Color(10, 5, 20),
    SurfaceColor = new Color(20, 10, 40),
    PanelColor = new Color(15, 8, 30),
    
    // Metin
    TextColor = new Color(0, 255, 255),
    TextSecondaryColor = new Color(150, 200, 200),
    TextDisabledColor = new Color(80, 100, 100),
    
    // Kenarlık
    BorderColor = new Color(255, 0, 128, 150),
    BorderFocusedColor = new Color(255, 0, 128),
    
    // Kontroller
    ControlBackground = new Color(30, 15, 50),
    ControlHover = new Color(50, 25, 80),
    ControlPressed = new Color(40, 20, 65),
    
    // Stil
    DefaultCornerRadius = 0,  // Keskin köşeler
    DefaultBorderThickness = 2
};

UI.Themes.RegisterTheme(cyberpunkTheme);
UI.Themes.SetTheme("Cyberpunk");
```

### Tema Değişikliğini Dinleme

```csharp
UI.Themes.ThemeChanged += (theme) =>
{
    Console.WriteLine($"Tema değişti: {theme.Name}");
    RefreshUIColors();
};
```

---

## JSON ile UI Tasarımı

### JSON Dosya Yapısı

```json
{
  "version": "1.0",
  "name": "MainMenu",
  "description": "Ana menü UI layout",
  "width": 1920,
  "height": 1080,
  "resources": {
    "fonts": {
      "title": "Fonts/TitleFont",
      "default": "Fonts/DefaultFont"
    },
    "textures": {
      "logo": "Textures/logo"
    }
  },
  "elements": [
    {
      "id": "panel_main",
      "type": "Panel",
      "name": "MainPanel",
      "x": 0, "y": 0,
      "width": 1920, "height": 1080,
      "properties": {
        "backgroundColor": "#1A1A1AFF"
      },
      "children": [
        {
          "id": "lbl_title",
          "type": "Label",
          "name": "Title",
          "fontId": "title",
          "x": 760, "y": 200,
          "width": 400, "height": 80,
          "properties": {
            "text": "MY GAME",
            "textColor": "#FFD700FF",
            "horizontalAlignment": "Center"
          }
        },
        {
          "id": "btn_start",
          "type": "Button",
          "name": "StartButton",
          "fontId": "default",
          "x": 810, "y": 400,
          "width": 300, "height": 60,
          "properties": {
            "text": "Start Game",
            "backgroundColor": "#0078D7FF",
            "hoverColor": "#1E90FFFF",
            "cornerRadius": 8
          }
        }
      ]
    }
  ]
}
```

### JSON'dan UI Yükleme

```csharp
protected override void OnLoadContent()
{
    // JSON'u yükle
    var layout = UISerializer.Load("Content/UI/MainMenu.ui.json");
    if (layout == null) return;
    
    // UI'ı oluştur
    var builder = new UIBuilder(Content);
    var uiRoot = builder.Build(layout);
    
    // Root'a ekle
    foreach (var child in uiRoot.Children.ToList())
    {
        AddElement(child);
    }
    
    // Event'leri bağla
    var startBtn = builder.GetElement<Button>("btn_start");
    startBtn?.OnClick.Subscribe(e => StartGame());
    
    var exitBtn = builder.GetElement<Button>("btn_exit");
    exitBtn?.OnClick.Subscribe(e => Exit());
}
```

---

## Best Practices

### 1. Sahne Organizasyonu

```csharp
// ✅ DOĞRU: Factory metodlar kullan
protected override void OnLoadContent()
{
    AddElement(CreateHeader());
    AddElement(CreateContent());
    AddElement(CreateFooter());
}

// ❌ YANLIŞ: Her şeyi OnLoadContent'e yığma
protected override void OnLoadContent()
{
    var header = new Panel();
    header.LayoutStrategy = new StackLayout();
    // ... 200 satır kod
}
```

### 2. Event Handler Yönetimi

```csharp
// ✅ DOĞRU: Metot referansı (unsubscribe edilebilir)
button.OnClick.Subscribe(HandleClick);

protected override void OnUnloadContent()
{
    button.OnClick.Unsubscribe(HandleClick);
}

// ⚠️ DİKKAT: Lambda (unsubscribe zor)
button.OnClick.Subscribe(e => DoSomething());
```

### 3. Layout Stratejisi Seçimi

| Senaryo | Önerilen Layout |
|---------|-----------------|
| Menü butonları | `StackLayout` (Vertical) |
| Toolbar | `StackLayout` (Horizontal) |
| Form | `GridLayout` |
| IDE arayüzü | `DockLayout` |
| Serbest pozisyon | `AbsoluteLayout` |

### 4. Performans İpuçları

```csharp
// ✅ Gereksiz UI güncellemelerinden kaçın
if (_previousValue != newValue)
{
    _previousValue = newValue;
    label.Text = newValue.ToString();
}

// ✅ Görünmez elemanları disable et
panel.IsEnabled = panel.IsVisible;

// ✅ Büyük listeler için virtualization düşün
```

---

## Sık Karşılaşılan Sorunlar

### Font Yüklenmiyor

```csharp
// Sorun: Content.Load<SpriteFont> hata veriyor

// Çözüm 1: Content.mgcb'de font tanımlı mı kontrol et
// Çözüm 2: Dosya yolu doğru mu kontrol et (Content/ prefix'i olmadan)
var font = Content.Load<SpriteFont>("Fonts/DefaultFont"); // ✅
var font = Content.Load<SpriteFont>("Content/Fonts/DefaultFont"); // ❌
```

### Click Event Çalışmıyor

```csharp
// Sorun: Button'a tıklanınca event tetiklenmiyor

// Çözüm 1: IsEnabled = true mi kontrol et
button.IsEnabled = true;

// Çözüm 2: Üstte başka bir eleman var mı kontrol et
// DrawOrder düşük olan üstte çizilir ama input'u engelleyebilir

// Çözüm 3: Parent container enabled mi
panel.IsEnabled = true;
```

### Layout Çalışmıyor

```csharp
// Sorun: Elemanlar pozisyonlanmıyor

// Çözüm 1: Container'a LayoutStrategy atandı mı
panel.LayoutStrategy = new StackLayout();

// Çözüm 2: Çocukların Width/Height değerleri var mı
button.Width = 200;
button.Height = 50;

// Çözüm 3: InvalidateLayout çağır
panel.InvalidateLayout();
```

### TextBox Input Almıyor

```csharp
// Sorun: TextBox'a yazı yazılamıyor

// Çözüm 1: Focus var mı
// Tıklayınca otomatik focus alır

// Çözüm 2: IsReadOnly = false mu
textBox.IsReadOnly = false;

// Çözüm 3: Font atandı mı (görüntüleme için gerekli)
textBox.Font = myFont;
```
