# RenUI Layout Modülü API Referansı

## İçindekiler
- [ILayoutStrategy](#ilayoutstrategy)
- [StackLayout](#stacklayout)
- [FlexLayout](#flexlayout)
- [GridLayout](#gridlayout)
- [WrapLayout](#wraplayout)
- [DockLayout](#docklayout)
- [AbsoluteLayout](#absolutelayout)

> **⚠️ ÖNEMLİ:** Layout sistemini kullanırken parent container'ın boyutlarının (Width/Height) doğru ayarlandığından emin olun. Layout stratejileri, container'ın mevcut boyutlarına göre çalışır.

---

## ILayoutStrategy

Tüm layout stratejilerinin temel arayüzü.

```csharp
public interface ILayoutStrategy
{
    void ApplyLayout(IContainer container);
    Point MeasureContent(IContainer container);
}
```

### MeasureContent Metodu

`MeasureContent` metodu, container'ın içeriğine göre tercih edilen boyutu hesaplar. Bu, Auto boyutlandırma için kullanılır:

```csharp
var layout = new StackLayout(Orientation.Vertical);
panel.LayoutStrategy = layout;

// İçerik boyutunu ölç
var preferredSize = layout.MeasureContent(panel);
```

---

## StackLayout

Elemanları yatay veya dikey olarak sıralar.

```csharp
public class StackLayout : ILayoutStrategy
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public int Spacing { get; set; } = 4;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
    public StackDistribution Distribution { get; set; } = StackDistribution.Start;
    public bool Reverse { get; set; } = false;
    
    public StackLayout();
    public StackLayout(Orientation orientation, int spacing = 4);
    public StackLayout(Orientation orientation, StackDistribution distribution, int spacing = 4);
    
    // Child sıralama
    public void SetOrder(IUIElement element, int order);
    public int GetOrder(IUIElement element);
}

public enum HorizontalAlignment { Left, Center, Right, Stretch }
public enum VerticalAlignment { Top, Center, Bottom, Stretch }

// Dağıtım modları
public enum StackDistribution
{
    Start,        // Başlangıçta topla
    End,          // Sonda topla
    Center,       // Ortala
    SpaceBetween, // İlk ve son kenarda, aralar eşit
    SpaceAround,  // Her elemanın etrafında eşit boşluk
    SpaceEvenly   // Tüm boşluklar eşit
}
```

### Dikey Stack (Menü)

```csharp
var menuPanel = new Panel();
menuPanel.LayoutStrategy = new StackLayout(Orientation.Vertical, 16)
{
    HorizontalAlignment = HorizontalAlignment.Center
};

menuPanel.AddChild(new Button("Start") { Width = 200, Height = 50 });
menuPanel.AddChild(new Button("Options") { Width = 200, Height = 50 });
menuPanel.AddChild(new Button("Exit") { Width = 200, Height = 50 });
```

**Sonuç:**
```
       ┌─────────────────┐
       │     Start       │
       └─────────────────┘
              ↓ 16px
       ┌─────────────────┐
       │    Options      │
       └─────────────────┘
              ↓ 16px
       ┌─────────────────┐
       │      Exit       │
       └─────────────────┘
```

### Yatay Stack (Toolbar)

```csharp
var toolbar = new Panel { Height = 40 };
toolbar.LayoutStrategy = new StackLayout(Orientation.Horizontal, 8)
{
    VerticalAlignment = VerticalAlignment.Center
};

toolbar.AddChild(new Button("New") { Width = 60, Height = 30 });
toolbar.AddChild(new Button("Open") { Width = 60, Height = 30 });
toolbar.AddChild(new Button("Save") { Width = 60, Height = 30 });
```

**Sonuç:**
```
┌──────┐ 8px ┌──────┐ 8px ┌──────┐
│ New  │ ←→  │ Open │ ←→  │ Save │
└──────┘     └──────┘     └──────┘
```

### Stretch Alignment

```csharp
var form = new Panel { Width = 400 };
form.LayoutStrategy = new StackLayout(Orientation.Vertical, 8)
{
    HorizontalAlignment = HorizontalAlignment.Stretch  // Tam genişlik
};

form.AddChild(new TextBox { Height = 32 });  // Genişlik otomatik 400-padding
form.AddChild(new TextBox { Height = 32 });
form.AddChild(new Button("Submit") { Height = 40 });
```

### Dağıtım Modları (Distribution)

```csharp
// SpaceBetween: İlk ve son eleman kenarlarda
var toolbar = new Panel { Width = 800, Height = 50 };
toolbar.LayoutStrategy = new StackLayout(Orientation.Horizontal, StackDistribution.SpaceBetween);

toolbar.AddChild(new Button("Sol") { Width = 100, Height = 40 });
toolbar.AddChild(new Button("Orta") { Width = 100, Height = 40 });
toolbar.AddChild(new Button("Sağ") { Width = 100, Height = 40 });
```

**Sonuç:**
```
┌──────┐                    ┌──────┐                    ┌──────┐
│ Sol  │ ←── otomatik ──→  │ Orta │ ←── otomatik ──→  │ Sağ  │
└──────┘                    └──────┘                    └──────┘
```

### Child Sıralama (Ordering)

```csharp
var panel = new Panel();
var stack = new StackLayout(Orientation.Vertical, 8);
panel.LayoutStrategy = stack;

var btn1 = new Button("Birinci");
var btn2 = new Button("İkinci");
var btn3 = new Button("Üçüncü");

// Sıralamayı değiştir
stack.SetOrder(btn3, 0);  // Üçüncü en üste
stack.SetOrder(btn1, 1);  // Birinci ortaya
stack.SetOrder(btn2, 2);  // İkinci en alta

panel.AddChild(btn1);
panel.AddChild(btn2);
panel.AddChild(btn3);
// Görüntüleme: Üçüncü → Birinci → İkinci
```

---

## FlexLayout

CSS Flexbox benzeri esnek kutu yerleşimi. Row/Column yönünde gelişmiş dağıtım ve hizalama.

```csharp
public class FlexLayout : ILayoutStrategy
{
    public FlexDirection Direction { get; set; } = FlexDirection.Row;
    public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
    public FlexJustify JustifyContent { get; set; } = FlexJustify.Start;
    public FlexAlign AlignItems { get; set; } = FlexAlign.Stretch;
    public FlexAlign AlignContent { get; set; } = FlexAlign.Stretch;
    public int Gap { get; set; } = 0;
    public int RowGap { get; set; } = 0;
    public int ColumnGap { get; set; } = 0;
    
    // Child ayarları
    public void SetFlexGrow(IUIElement element, float grow);
    public void SetFlexShrink(IUIElement element, float shrink);
    public void SetFlexBasis(IUIElement element, int basis);
    public void SetAlignSelf(IUIElement element, FlexAlign align);
    public void SetOrder(IUIElement element, int order);
}

public enum FlexDirection { Row, RowReverse, Column, ColumnReverse }
public enum FlexWrap { NoWrap, Wrap, WrapReverse }
public enum FlexJustify { Start, End, Center, SpaceBetween, SpaceAround, SpaceEvenly }
public enum FlexAlign { Start, End, Center, Stretch }
```

### Temel Flex Örneği

```csharp
var container = new Panel { Width = 600, Height = 100 };
var flex = new FlexLayout
{
    Direction = FlexDirection.Row,
    JustifyContent = FlexJustify.SpaceBetween,
    AlignItems = FlexAlign.Center,
    Gap = 16
};
container.LayoutStrategy = flex;

container.AddChild(new Button("A") { Width = 80, Height = 40 });
container.AddChild(new Button("B") { Width = 80, Height = 40 });
container.AddChild(new Button("C") { Width = 80, Height = 40 });
```

### Flex Grow/Shrink

```csharp
var container = new Panel { Width = 500, Height = 60 };
var flex = new FlexLayout { Direction = FlexDirection.Row, Gap = 8 };
container.LayoutStrategy = flex;

var sidebar = new Panel { Width = 100, Height = 50 };
var content = new Panel { Width = 100, Height = 50 };
var aside = new Panel { Width = 100, Height = 50 };

// Content kalan alanı doldursun
flex.SetFlexGrow(content, 1);

container.AddChild(sidebar);
container.AddChild(content);
container.AddChild(aside);
```

**Sonuç:**
```
┌────────┐┌──────────────────────────────┐┌────────┐
│Sidebar ││         Content (grow)       ││ Aside  │
│ 100px  ││        ~284px (esnek)        ││ 100px  │
└────────┘└──────────────────────────────┘└────────┘
```

### Wrap ile Çok Satırlı Layout

```csharp
var grid = new Panel { Width = 400, Height = 300 };
var flex = new FlexLayout
{
    Direction = FlexDirection.Row,
    Wrap = FlexWrap.Wrap,
    Gap = 10,
    JustifyContent = FlexJustify.Start,
    AlignContent = FlexAlign.Start
};
grid.LayoutStrategy = flex;

// 120px genişliğinde kartlar - satır başına 3 sığar
for (int i = 0; i < 8; i++)
{
    grid.AddChild(new Panel { Width = 120, Height = 80 });
}
```

---

## GridLayout

Satır ve sütun tabanlı ızgara yerleşimi.

```csharp
public class GridLayout : ILayoutStrategy
{
    public int ColumnSpacing { get; set; } = 4;
    public int RowSpacing { get; set; } = 4;
    public IReadOnlyList<GridLength> ColumnDefinitions { get; }
    public IReadOnlyList<GridLength> RowDefinitions { get; }
    
    // Sütun/Satır tanımları
    public void AddColumn(GridLength length);
    public void AddRow(GridLength length);
    public void AddColumns(params GridLength[] lengths);
    public void AddRows(params GridLength[] lengths);
    public void ClearColumns();
    public void ClearRows();
    public void RemoveColumn(int index);
    public void RemoveRow(int index);
    public void InsertColumn(int index, GridLength length);
    public void InsertRow(int index, GridLength length);
    
    // Eleman pozisyonlama
    public void SetPosition(IUIElement element, int row, int column, 
                            int rowSpan = 1, int columnSpan = 1);
    public GridPosition? GetPosition(IUIElement element);
    
    // Hücre hizalama
    public void SetCellAlignment(IUIElement element, HorizontalAlignment h, VerticalAlignment v);
    public GridCellAlignment GetCellAlignment(IUIElement element);
}

// Hücre hizalama yapısı
public readonly struct GridCellAlignment
{
    public HorizontalAlignment Horizontal { get; }
    public VerticalAlignment Vertical { get; }
    
    public static GridCellAlignment Stretch { get; }
    public static GridCellAlignment Center { get; }
    public static GridCellAlignment TopLeft { get; }
    public static GridCellAlignment TopRight { get; }
    public static GridCellAlignment BottomLeft { get; }
    public static GridCellAlignment BottomRight { get; }
}
```

### GridLength Yapısı

```csharp
public readonly struct GridLength
{
    public float Value { get; }
    public GridLengthType Type { get; }
    
    public static GridLength Auto { get; }   // İçeriğe göre
    public static GridLength Star { get; }   // 1* (kalan alan)
    
    public static GridLength Pixel(float value);  // Sabit piksel
    public static GridLength Stars(float value);  // n* (oransal)
}

public enum GridLengthType { Pixel, Star, Auto }
```

### GridPosition Yapısı

```csharp
public readonly struct GridPosition
{
    public int Row { get; }
    public int Column { get; }
    public int RowSpan { get; }
    public int ColumnSpan { get; }
}
```

### Temel Grid Örneği

```csharp
var grid = new Panel();
var layout = new GridLayout { ColumnSpacing = 8, RowSpacing = 8 };

// 3 sütun tanımla
layout.AddColumns(
    GridLength.Pixel(100),    // 100px sabit
    GridLength.Star,          // Kalan alanın 1 parçası
    GridLength.Stars(2)       // Kalan alanın 2 parçası
);

// 2 satır tanımla
layout.AddRows(
    GridLength.Pixel(50),     // 50px sabit
    GridLength.Star           // Kalan alan
);

grid.LayoutStrategy = layout;

// Elemanları pozisyonla
var header = new Label("Header");
layout.SetPosition(header, row: 0, column: 0, columnSpan: 3);
grid.AddChild(header);

var sidebar = new Panel { BackgroundColor = Color.DarkBlue };
layout.SetPosition(sidebar, row: 1, column: 0);
grid.AddChild(sidebar);

var content = new Panel { BackgroundColor = Color.DarkGreen };
layout.SetPosition(content, row: 1, column: 1, columnSpan: 2);
grid.AddChild(content);
```

**Sonuç (800px genişlik):**
```
┌──────────────────────────────────────────────────┐
│                    HEADER                         │ 50px
├─────────┬────────────────────────────────────────┤
│         │                                        │
│ SIDEBAR │              CONTENT                   │ *
│  100px  │         (~233px + ~466px)              │
│         │                                        │
└─────────┴────────────────────────────────────────┘
```

### Form Layout Örneği

```csharp
var form = new Panel { Width = 400, Height = 200 };
var layout = new GridLayout { ColumnSpacing = 12, RowSpacing = 8 };

layout.AddColumns(GridLength.Pixel(100), GridLength.Star);
layout.AddRows(
    GridLength.Pixel(32),
    GridLength.Pixel(32),
    GridLength.Pixel(40)
);

form.LayoutStrategy = layout;

// Row 0: Username
var lblUser = new Label("Username:");
layout.SetPosition(lblUser, 0, 0);
form.AddChild(lblUser);

var txtUser = new TextBox();
layout.SetPosition(txtUser, 0, 1);
form.AddChild(txtUser);

// Row 1: Password
var lblPass = new Label("Password:");
layout.SetPosition(lblPass, 1, 0);
form.AddChild(lblPass);

var txtPass = new TextBox { IsPassword = true };
layout.SetPosition(txtPass, 1, 1);
form.AddChild(txtPass);

// Row 2: Button (span 2 columns)
var btnLogin = new Button("Login");
layout.SetPosition(btnLogin, 2, 0, columnSpan: 2);
form.AddChild(btnLogin);
```

---

## DockLayout

Elemanları kenarlar boyunca yerleştirir.

```csharp
public class DockLayout : ILayoutStrategy
{
    public bool LastChildFill { get; set; } = true;
    
    public void SetDock(IUIElement element, Dock dock);
    public Dock GetDock(IUIElement element);
}

public enum Dock { Left, Top, Right, Bottom }
```

### IDE Benzeri Layout

```csharp
var mainWindow = new Panel();
var dock = new DockLayout { LastChildFill = true };
mainWindow.LayoutStrategy = dock;

// Üst menü bar
var menuBar = new Panel { Height = 30, BackgroundColor = Color.DarkGray };
dock.SetDock(menuBar, Dock.Top);
mainWindow.AddChild(menuBar);

// Sol panel (dosya gezgini)
var leftPanel = new Panel { Width = 200, BackgroundColor = Color.DarkSlateGray };
dock.SetDock(leftPanel, Dock.Left);
mainWindow.AddChild(leftPanel);

// Sağ panel (özellikler)
var rightPanel = new Panel { Width = 250, BackgroundColor = Color.DarkSlateGray };
dock.SetDock(rightPanel, Dock.Right);
mainWindow.AddChild(rightPanel);

// Alt panel (konsol)
var bottomPanel = new Panel { Height = 150, BackgroundColor = Color.DarkSlateGray };
dock.SetDock(bottomPanel, Dock.Bottom);
mainWindow.AddChild(bottomPanel);

// Merkez (editör) - LastChildFill = true
var centerPanel = new Panel { BackgroundColor = Color.Black };
mainWindow.AddChild(centerPanel);
```

**Sonuç:**
```
┌──────────────────────────────────────────────────┐
│                    MENU BAR                       │ Dock.Top
├─────────┬────────────────────────────┬───────────┤
│         │                            │           │
│  LEFT   │          CENTER            │   RIGHT   │
│  PANEL  │         (Fill)             │   PANEL   │
│         │                            │           │
│ D.Left  │                            │  D.Right  │
├─────────┴────────────────────────────┴───────────┤
│                  BOTTOM PANEL                     │ Dock.Bottom
└──────────────────────────────────────────────────┘
```

### Yerleştirme Sırası Önemi

Elemanların eklenme sırası önemlidir:

```csharp
// Sıra 1: Top, Left, Content
// Sol panel tam yükseklik

// Sıra 2: Left, Top, Content  
// Üst panel tam genişlik
```

---

## WrapLayout

Elemanları satır veya sütun şeklinde sıralar, alan bitince bir sonraki satıra/sütuna geçer.

```csharp
public class WrapLayout : ILayoutStrategy
{
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    public int Spacing { get; set; } = 4;
    public int LineSpacing { get; set; } = 4;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
}
```

### Galeri/Kart Görünümü

```csharp
var gallery = new Panel { Width = 500, Height = 400 };
gallery.LayoutStrategy = new WrapLayout
{
    Orientation = Orientation.Horizontal,
    Spacing = 10,
    LineSpacing = 10,
    HorizontalAlignment = HorizontalAlignment.Left
};

// 100x100 kartlar - satır başına 4 sığar
for (int i = 0; i < 12; i++)
{
    gallery.AddChild(new Panel 
    { 
        Width = 100, 
        Height = 100,
        BackgroundColor = Color.DarkBlue 
    });
}
```

**Sonuç:**
```
┌────┐ ┌────┐ ┌────┐ ┌────┐
│ 1  │ │ 2  │ │ 3  │ │ 4  │  ← Satır 1
└────┘ └────┘ └────┘ └────┘
┌────┐ ┌────┐ ┌────┐ ┌────┐
│ 5  │ │ 6  │ │ 7  │ │ 8  │  ← Satır 2
└────┘ └────┘ └────┘ └────┘
┌────┐ ┌────┐ ┌────┐ ┌────┐
│ 9  │ │10  │ │11  │ │12  │  ← Satır 3
└────┘ └────┘ └────┘ └────┘
```

### Dikey Wrap (Tag Cloud)

```csharp
var tagCloud = new Panel { Width = 200, Height = 400 };
tagCloud.LayoutStrategy = new WrapLayout
{
    Orientation = Orientation.Vertical,
    Spacing = 4,
    LineSpacing = 8
};

// Farklı boyutlu etiketler
tagCloud.AddChild(new Button("C#") { Width = 50, Height = 24 });
tagCloud.AddChild(new Button("MonoGame") { Width = 80, Height = 24 });
tagCloud.AddChild(new Button("UI") { Width = 40, Height = 24 });
// ... daha fazla etiket
```

---

## AbsoluteLayout

Manuel pozisyonlama (pozisyonları değiştirmez).

```csharp
public class AbsoluteLayout : ILayoutStrategy
{
    public void ApplyLayout(IContainer container)
    {
        // No-op - pozisyonlar olduğu gibi kalır
    }
}
```

### Kullanım

```csharp
var canvas = new Panel();
canvas.LayoutStrategy = new AbsoluteLayout();

// Elemanları manuel pozisyonla
var btn1 = new Button("A") { 
    Bounds = new Rectangle(50, 50, 100, 40) 
};
var btn2 = new Button("B") { 
    Bounds = new Rectangle(200, 100, 100, 40) 
};

canvas.AddChild(btn1);
canvas.AddChild(btn2);
```

### Ne Zaman Kullanılır

- Özel animasyonlar
- Drag & drop arayüzler
- Oyun içi HUD elemanları
- Serbest form canvas'lar

---

## Layout Karşılaştırma Tablosu

| Layout | Kullanım Alanı | Esneklik | Karmaşıklık |
|--------|---------------|----------|-------------|
| **StackLayout** | Menüler, formlar, listeler | Orta | Düşük |
| **FlexLayout** | Dinamik UI, responsive tasarım | Çok Yüksek | Orta |
| **GridLayout** | Karmaşık formlar, dashboard | Yüksek | Orta |
| **WrapLayout** | Galeriler, tag cloud, kartlar | Orta | Düşük |
| **DockLayout** | IDE, uygulama arayüzleri | Orta | Düşük |
| **AbsoluteLayout** | Özel konumlandırma | Tam | Düşük |

---

## Önemli Uyarılar

> **⚠️ Container Boyutu:** Layout stratejileri container'ın `Width` ve `Height` değerlerine göre çalışır. Container boyutu 0 ise layout uygulanmaz.

> **⚠️ Child Boyutları:** Auto boyutlandırma için child elemanların `Width` ve `Height` değerlerinin set edilmiş olması gerekir. Aksi halde 0 olarak hesaplanır.

> **⚠️ Dinamik Güncelleme:** Layout değişikliklerinden sonra `container.InvalidateLayout()` çağrılmalıdır (child ekleme/çıkarma otomatik yapar).

### Layout Seçim Rehberi

```
Basit liste/menü mi?           → StackLayout
Esnek row/column dağıtım mı?   → FlexLayout  
Satır/sütun tabanlı form mu?   → GridLayout
Kenar bazlı yerleşim mi?       → DockLayout
Wrap eden galeri mi?           → WrapLayout
Manuel pozisyonlama mı?        → AbsoluteLayout
```
