# RenUI API Referans Dokümantasyonu

Bu dokümantasyon, RenUI kütüphanesinin tüm API'lerini detaylı olarak açıklamaktadır.

---

## İçindekiler

1. [Başlarken](#başlarken)
2. [Core Modülü](./API_CORE.md)
3. [Elements Modülü](./API_ELEMENTS.md)
4. [Layout Modülü](./API_LAYOUT.md)
5. [Diğer Modüller](./API_MODULES.md)
6. [Kullanım Rehberi](./USAGE_GUIDE.md)

---

## Başlarken

### Gereksinimler

- .NET 8.0 veya üzeri
- MonoGame 3.8.2 veya üzeri

### Kurulum

```xml
<!-- .csproj dosyanıza ekleyin -->
<ProjectReference Include="..\RenUI\RenUI.csproj" />
```

### Temel Uygulama

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

// Program.cs
using var game = new MyGame();
game.Run();
```

### Namespace'ler

```csharp
using RenUI;                      // Ana sınıflar
using RenUI.Core.Interfaces;      // Arayüzler
using RenUI.Core.Events;          // Event sistemleri
using RenUI.Core.Primitives;      // Padding, Margin, vb.
using RenUI.Elements.Base;        // UIElement, Container
using RenUI.Elements.Controls;    // Button, Label, TextBox, vb.
using RenUI.Layout;               // Layout stratejileri
using RenUI.Scenes;               // Sahne yönetimi
using RenUI.Styling;              // Tema sistemi
using RenUI.Input;                // Input yönetimi
using RenUI.Rendering;            // Çizim yardımcıları
using RenUI.Serialization;        // UI serileştirme
```

---

## Hızlı Referans

### UIManager

```csharp
UIManager.Instance.Input      // InputManager
UIManager.Instance.Scenes     // SceneManager  
UIManager.Instance.Themes     // ThemeManager
UIManager.Instance.Layouts    // UILayoutManager
```

### Temel Kontroller

| Kontrol | Açıklama |
|---------|----------|
| `Button` | Tıklanabilir buton |
| `Label` | Metin görüntüleme |
| `TextBox` | Metin girişi |
| `CheckBox` | İşaret kutusu |
| `Slider` | Değer kaydırıcı |
| `Panel` | Container panel |

### Layout Stratejileri

| Layout | Açıklama |
|--------|----------|
| `StackLayout` | Yatay/dikey yığın |
| `GridLayout` | Satır/sütun ızgarası |
| `DockLayout` | Kenar yerleştirme |
| `AbsoluteLayout` | Manuel pozisyonlama |

---

## Mimari Genel Bakış

```
RenUIGame
    └── UIManager (Facade)
        ├── InputManager
        ├── SceneManager
        │   └── SceneBase
        │       └── Container (Root)
        │           └── UI Elements
        ├── ThemeManager
        └── UILayoutManager
```

Detaylı dokümantasyon için ilgili modül sayfalarına bakınız.
