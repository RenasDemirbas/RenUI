# RenUI Layout Modülü API Referansı

## İçindekiler
- [ILayoutStrategy](#ilayoutstrategy)
- [StackLayout](#stacklayout)
- [GridLayout](#gridlayout)
- [DockLayout](#docklayout)
- [AbsoluteLayout](#absolutelayout)

---

## ILayoutStrategy

Tüm layout stratejilerinin temel arayüzü.

```csharp
public interface ILayoutStrategy
{
    void ApplyLayout(IContainer container);
}
```

**Özel Layout Oluşturma:**
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
    
    public StackLayout();
    public StackLayout(Orientation orientation, int spacing = 4);
}

public enum HorizontalAlignment { Left, Center, Right, Stretch }
public enum VerticalAlignment { Top, Center, Bottom, Stretch }
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
    
    // Eleman pozisyonlama
    public void SetPosition(IUIElement element, int row, int column, 
                            int rowSpan = 1, int columnSpan = 1);
    public GridPosition? GetPosition(IUIElement element);
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
| **GridLayout** | Karmaşık formlar, dashboard | Yüksek | Orta |
| **DockLayout** | IDE, uygulama arayüzleri | Orta | Düşük |
| **AbsoluteLayout** | Özel konumlandırma | Tam | Düşük |
