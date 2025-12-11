# RenUI Elements Modülü API Referansı

## İçindekiler
- [UIElement (Base)](#uielement)
- [Container](#container)
- [Controls](#controls)
  - [Button](#button)
  - [Label](#label)
  - [TextBox](#textbox)
  - [CheckBox](#checkbox)
  - [Slider](#slider)
  - [Panel](#panel)

---

## UIElement

Tüm UI elemanlarının abstract base sınıfı.

```csharp
public abstract class UIElement : IUIElement, IInputHandler
{
    // Kimlik
    public string Id { get; set; }
    public string Name { get; set; }
    public IUIElement? Parent { get; set; }
    
    // Kaynak referansları
    public string? TextureId { get; set; }
    public string? FontId { get; set; }
    
    // Pozisyon ve Boyut
    public Rectangle Bounds { get; set; }
    public Point Position { get; set; }
    public Point Size { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    
    // Durum
    public bool IsEnabled { get; set; } = true;
    public bool IsVisible { get; set; } = true;
    public bool IsFocused { get; set; }
    public bool IsHovered { get; set; }
    public int DrawOrder { get; set; }
    
    // Layout
    public Anchor Anchor { get; set; } = Anchor.TopLeft;
    public Margin Margin { get; set; } = Margin.Zero;
    public Padding Padding { get; set; } = Padding.Zero;
    public Size MinSize { get; set; }
    public Size MaxSize { get; set; }
    
    // Stil
    public ElementStyle Style { get; set; }
    
    // Events
    public EventDispatcher<MouseEventArgs> OnClick { get; }
    public EventDispatcher<MouseEventArgs> OnMouseEnter { get; }
    public EventDispatcher<MouseEventArgs> OnMouseLeave { get; }
    public EventDispatcher<MouseEventArgs> OnMouseDown { get; }
    public EventDispatcher<MouseEventArgs> OnMouseUp { get; }
    public EventDispatcher<FocusEventArgs> OnFocusChanged { get; }
    
    // Metodlar
    public virtual void Initialize();
    public virtual void Update(GameTime gameTime);
    public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    public virtual bool Contains(Point point);
    public Rectangle GetAbsoluteBounds();
    
    // Protected
    protected void MarkDirty();
}
```

### Özel UIElement Oluşturma

```csharp
public class ProgressBar : UIElement
{
    private float _progress;
    
    public float Progress
    {
        get => _progress;
        set { _progress = Math.Clamp(value, 0, 1); MarkDirty(); }
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

---

## Container

Çocuk elemanları yönetebilen container sınıfı.

```csharp
public class Container : UIElement, IContainer
{
    public IReadOnlyList<IUIElement> Children { get; }
    public ILayoutStrategy? LayoutStrategy { get; set; }
    
    public void AddChild(IUIElement child);
    public void RemoveChild(IUIElement child);
    public void ClearChildren();
    public IUIElement? GetChildByName(string name);
    public T? GetChild<T>(string name) where T : class, IUIElement;
    public void InvalidateLayout();
    
    protected virtual void DrawSelf(SpriteBatch spriteBatch, GameTime gameTime);
    protected virtual void DrawChildren(SpriteBatch spriteBatch, GameTime gameTime);
    protected virtual void PerformLayout();
}
```

**Kullanım:**
```csharp
var container = new Container();
container.Bounds = new Rectangle(0, 0, 800, 600);
container.LayoutStrategy = new StackLayout(Orientation.Vertical, 8);

container.AddChild(new Button("Button 1"));
container.AddChild(new Button("Button 2"));

var btn = container.GetChild<Button>("Button 1");
```

---

## Controls

### Button

Tıklanabilir buton kontrolü.

```csharp
public class Button : UIElement
{
    public string Text { get; set; }
    public SpriteFont? Font { get; set; }
    
    public Color TextColor { get; set; } = Color.White;
    public Color BackgroundColor { get; set; } = new Color(70, 70, 70);
    public Color HoverColor { get; set; } = new Color(90, 90, 90);
    public Color PressedColor { get; set; } = new Color(50, 50, 50);
    public Color DisabledColor { get; set; } = new Color(45, 45, 45);
    public Color BorderColor { get; set; } = new Color(100, 100, 100);
    public int BorderThickness { get; set; } = 1;
    public int CornerRadius { get; set; } = 4;
    
    public Button();
    public Button(string text);
}
```

**Özellikler:**

| Özellik | Varsayılan | Açıklama |
|---------|------------|----------|
| `Text` | `""` | Buton metni |
| `BackgroundColor` | `#464646` | Normal arka plan |
| `HoverColor` | `#5A5A5A` | Hover arka plan |
| `PressedColor` | `#323232` | Basılı arka plan |
| `CornerRadius` | `4` | Köşe yuvarlaklığı |

**Kullanım:**
```csharp
var button = new Button("Start Game")
{
    Font = myFont,
    Width = 200,
    Height = 50,
    BackgroundColor = new Color(0, 120, 215),
    CornerRadius = 8
};

button.OnClick.Subscribe(e => StartGame());
button.OnMouseEnter.Subscribe(e => PlayHoverSound());
```

---

### Label

Metin görüntüleme kontrolü.

```csharp
public class Label : UIElement
{
    public string Text { get; set; }
    public SpriteFont? Font { get; set; }
    public Color TextColor { get; set; } = Color.White;
    public TextAlignment HorizontalAlignment { get; set; } = TextAlignment.Start;
    public TextAlignment VerticalAlignment { get; set; } = TextAlignment.Center;
    public bool AutoSize { get; set; } = true;
    
    public Label();
    public Label(string text);
}

public enum TextAlignment { Start, Center, End }
```

**Kullanım:**
```csharp
var title = new Label("Game Title")
{
    Font = titleFont,
    TextColor = Color.Gold,
    HorizontalAlignment = TextAlignment.Center
};

var description = new Label("Long text here...")
{
    Font = defaultFont,
    Width = 300,
    Height = 100,
    AutoSize = false
};
```

---

### TextBox

Metin giriş kontrolü.

```csharp
public class TextBox : UIElement
{
    public string Text { get; set; }
    public SpriteFont? Font { get; set; }
    public int CursorPosition { get; set; }
    public string Placeholder { get; set; } = "";
    
    public Color TextColor { get; set; } = Color.White;
    public Color PlaceholderColor { get; set; } = Color.Gray;
    public Color BackgroundColor { get; set; } = new Color(30, 30, 30);
    public Color FocusedBorderColor { get; set; } = new Color(0, 120, 215);
    public Color BorderColor { get; set; } = new Color(60, 60, 60);
    public Color SelectionColor { get; set; } = new Color(0, 120, 215, 128);
    public Color CursorColor { get; set; } = Color.White;
    
    public int BorderThickness { get; set; } = 1;
    public int MaxLength { get; set; } = int.MaxValue;
    public bool IsReadOnly { get; set; } = false;
    public bool IsPassword { get; set; } = false;
    public char PasswordChar { get; set; } = '*';
    
    // Events
    public EventDispatcher<UIEventArgs> OnTextChanged { get; }
    public EventDispatcher<UIEventArgs> OnSubmit { get; }
}
```

**Klavye Kısayolları:**

| Kısayol | İşlev |
|---------|-------|
| `Backspace` | Önceki karakteri sil |
| `Delete` | Sonraki karakteri sil |
| `Left/Right` | İmleç hareket |
| `Home/End` | Satır başı/sonu |
| `Ctrl+A` | Tümünü seç |
| `Enter` | OnSubmit tetikle |

**Kullanım:**
```csharp
var username = new TextBox
{
    Font = font,
    Width = 250,
    Height = 36,
    Placeholder = "Kullanıcı adı...",
    MaxLength = 20
};

username.OnTextChanged.Subscribe(e => ValidateUsername());
username.OnSubmit.Subscribe(e => Login());

// Şifre alanı
var password = new TextBox
{
    IsPassword = true,
    PasswordChar = '●',
    Placeholder = "Şifre..."
};
```

---

### CheckBox

İşaretleme kutusu kontrolü.

```csharp
public class CheckBox : UIElement
{
    public bool IsChecked { get; set; }
    public string Text { get; set; }
    public SpriteFont? Font { get; set; }
    
    public int BoxSize { get; set; } = 18;
    public int Spacing { get; set; } = 8;
    public Color TextColor { get; set; } = Color.White;
    public Color BoxColor { get; set; } = new Color(50, 50, 50);
    public Color BoxHoverColor { get; set; } = new Color(70, 70, 70);
    public Color CheckColor { get; set; } = new Color(0, 120, 215);
    public Color BorderColor { get; set; } = new Color(80, 80, 80);
    public int BorderThickness { get; set; } = 1;
    
    public EventDispatcher<CheckedChangedEventArgs> OnCheckedChanged { get; }
    
    public CheckBox();
    public CheckBox(string text, bool isChecked = false);
}
```

**Kullanım:**
```csharp
var soundEnabled = new CheckBox("Ses Etkin", isChecked: true)
{
    Font = font,
    CheckColor = Color.Green
};

soundEnabled.OnCheckedChanged.Subscribe(e =>
{
    if (e.IsChecked) AudioManager.Enable();
    else AudioManager.Disable();
});
```

---

### Slider

Değer kaydırma kontrolü.

```csharp
public class Slider : UIElement
{
    public float Value { get; set; }
    public float MinValue { get; set; } = 0f;
    public float MaxValue { get; set; } = 100f;
    public float Step { get; set; } = 1f;
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    
    public int TrackThickness { get; set; } = 4;
    public int ThumbSize { get; set; } = 16;
    public Color TrackColor { get; set; } = new Color(50, 50, 50);
    public Color TrackFillColor { get; set; } = new Color(0, 120, 215);
    public Color ThumbColor { get; set; } = new Color(200, 200, 200);
    public Color ThumbHoverColor { get; set; } = Color.White;
    public Color ThumbDragColor { get; set; } = new Color(0, 120, 215);
    
    public EventDispatcher<ValueChangedEventArgs> OnValueChanged { get; }
}

public enum Orientation { Horizontal, Vertical }
```

**Kullanım:**
```csharp
var volume = new Slider
{
    MinValue = 0,
    MaxValue = 100,
    Value = 50,
    Step = 5,
    Width = 250,
    Height = 24
};

volume.OnValueChanged.Subscribe(e =>
{
    AudioManager.SetVolume(e.Value / 100f);
});

// Dikey slider
var brightness = new Slider
{
    Orientation = Orientation.Vertical,
    Width = 24,
    Height = 200
};
```

---

### Panel

Container panel kontrolü.

```csharp
public class Panel : Container
{
    public Color BackgroundColor { get; set; } = new Color(40, 40, 40);
    public Color BorderColor { get; set; } = new Color(60, 60, 60);
    public int BorderThickness { get; set; } = 1;
    public int CornerRadius { get; set; } = 0;
}
```

**Kullanım:**
```csharp
var settingsPanel = new Panel
{
    Bounds = new Rectangle(100, 100, 400, 300),
    BackgroundColor = new Color(30, 30, 30),
    CornerRadius = 8,
    Padding = new Padding(16)
};

settingsPanel.LayoutStrategy = new StackLayout(Orientation.Vertical, 12);
settingsPanel.AddChild(new Label("Ayarlar"));
settingsPanel.AddChild(new CheckBox("Ses"));
settingsPanel.AddChild(new Slider());
```
