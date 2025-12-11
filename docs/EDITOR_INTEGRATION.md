# RenUI Editör Entegrasyonu

Bu döküman, RenUI editöründe tasarlanan UI'ların projelere nasıl entegre edileceğini açıklar.

## Temel Konsept

RenUI'da UI tasarımı iki aşamadan oluşur:

1. **Editörde Tasarım**: UI elemanlarını görsel olarak konumlandırma, stil ayarlama
2. **Projede Kullanım**: JSON dosyasını yükleyip oyun mantığını bağlama

```
┌─────────────────┐         ┌─────────────────┐         ┌─────────────────┐
│   RenUI Editor  │ ──────► │   .ui.json      │ ──────► │   Your Game     │
│   (Tasarım)     │  Export │   (Ayarlar)     │  Load   │   (Mantık)      │
└─────────────────┘         └─────────────────┘         └─────────────────┘
```

## JSON Formatı

Her UI layout bir JSON dosyasında saklanır:

```json
{
  "version": "1.0",
  "name": "MainMenu",
  "width": 1920,
  "height": 1080,
  "resources": {
    "textures": { "logo": "Textures/logo" },
    "fonts": { "default": "Fonts/DefaultFont" }
  },
  "elements": [
    {
      "id": "btn_start",
      "type": "Button",
      "name": "StartButton",
      "fontId": "default",
      "x": 100, "y": 200,
      "width": 200, "height": 50,
      "properties": {
        "text": "Start Game",
        "backgroundColor": "#0078D7FF"
      }
    }
  ]
}
```

## Entegrasyon Yöntemleri

### Yöntem 1: Tam JSON Tabanlı (Önerilen)

UI'ı tamamen JSON'dan oluşturur. En hızlı ve temiz yöntem.

```csharp
public class MainMenuScene : SceneBase
{
    public MainMenuScene() : base("MainMenu") { }

    protected override void OnLoadContent()
    {
        // 1. JSON'ı yükle
        UI.Layouts.LoadLayout("MainMenu", "Content/UI/MainMenu.ui.json");
        
        // 2. UI hiyerarşisini oluştur
        var root = UI.Layouts.Build("MainMenu");
        
        // 3. Sahneye ekle
        if (root != null)
        {
            foreach (var child in root.Children.ToList())
            {
                AddElement(child);
            }
        }
        
        // 4. Elemanlara ID ile eriş
        var startBtn = UI.Layouts.GetElement<Button>("MainMenu", "btn_start");
        var optionsBtn = UI.Layouts.GetElement<Button>("MainMenu", "btn_options");
        
        // 5. Event handler'ları bağla
        startBtn?.OnClick.Subscribe(e => StartGame());
        optionsBtn?.OnClick.Subscribe(e => OpenOptions());
    }
    
    private void StartGame() { /* ... */ }
    private void OpenOptions() { /* ... */ }
}
```

### Yöntem 2: Hibrit (Kod + Editör)

UI'ı kodda oluşturur, editörden sadece stil/pozisyon alır.

```csharp
protected override void OnLoadContent()
{
    // Kod ile UI oluştur
    var panel = new Panel { Id = "panel_main" };
    
    var slider = new Slider { Id = "slider_volume" };
    panel.AddChild(slider);
    
    var button = new Button { Id = "btn_apply" };
    panel.AddChild(button);
    
    AddElement(panel);
    
    // Editör ayarlarını uygula
    UI.Layouts.LoadLayout("Settings", "Content/UI/Settings.ui.json");
    UI.Layouts.ApplyLayout("Settings", panel);
    
    // Artık slider ve button editördeki renk, boyut, pozisyona sahip
}
```

### Yöntem 3: Sadece Değer Okuma

Spesifik değerleri okuyup manuel uygulama.

```csharp
protected override void OnLoadContent()
{
    var layout = UI.Layouts.LoadLayout("Config", "Content/UI/Config.ui.json");
    
    // Element verisini al
    var btnData = layout?.Elements.FirstOrDefault(e => e.Id == "btn_start");
    
    if (btnData != null)
    {
        var button = new Button
        {
            Text = btnData.Properties["text"]?.ToString() ?? "Button",
            Bounds = new Rectangle(btnData.X, btnData.Y, btnData.Width, btnData.Height)
        };
        AddElement(button);
    }
}
```

## ID Sistemi

Her UI elemanı benzersiz bir ID'ye sahiptir:

| Prefix | Tür | Örnek |
|--------|-----|-------|
| `btn_` | Button | `btn_start_game` |
| `lbl_` | Label | `lbl_title` |
| `txt_` | TextBox | `txt_username` |
| `chk_` | CheckBox | `chk_fullscreen` |
| `sld_` | Slider | `sld_volume` |
| `pnl_` | Panel | `pnl_main_container` |

## Resource Yönetimi

### Texture Tanımlama

```json
"resources": {
  "textures": {
    "btn_normal": "Textures/UI/button_normal",
    "btn_hover": "Textures/UI/button_hover",
    "icon_settings": "Textures/Icons/settings"
  }
}
```

### Font Tanımlama

```json
"resources": {
  "fonts": {
    "default": "Fonts/DefaultFont",
    "title": "Fonts/TitleFont",
    "small": "Fonts/SmallFont"
  }
}
```

### Kullanım

```csharp
var builder = UI.Layouts.GetBuilder("MainMenu");
var font = builder?.GetFont("title");
var texture = builder?.GetTexture("btn_normal");
```

## Çalışma Akışı

### 1. Editörde Tasarım

1. RenUI.Editor'ü aç
2. Yeni layout oluştur veya mevcut olanı yükle
3. UI elemanlarını ekle (her birine anlamlı ID ver)
4. Pozisyon, boyut, renk ayarla
5. Resource'ları tanımla (font/texture ID'leri)
6. JSON olarak export et

### 2. Projeye Entegrasyon

1. `.ui.json` dosyasını `Content/UI/` klasörüne kopyala
2. Sahne sınıfında `UI.Layouts.LoadLayout()` ile yükle
3. `UI.Layouts.Build()` veya `ApplyLayout()` ile uygula
4. `GetElement<T>()` ile elemanlara eriş
5. Event handler'ları bağla

### 3. İterasyon

1. Editörde değişiklik yap
2. JSON'ı tekrar export et
3. **Oyunu yeniden başlat - değişiklikler otomatik yüklenir**
4. Kod değişikliği gerekmez!

## Best Practices

### ID İsimlendirme

```
✅ btn_start_game
✅ pnl_main_menu
✅ sld_master_volume

❌ Button1
❌ myButton
❌ asdf
```

### Hiyerarşi

```
pnl_root
├── pnl_header
│   ├── lbl_title
│   └── btn_close
├── pnl_content
│   ├── lbl_description
│   └── pnl_options
│       ├── chk_option1
│       └── chk_option2
└── pnl_footer
    ├── btn_cancel
    └── btn_confirm
```

### Resource ID'leri

```
fonts/
  default     → Ana yazı tipi
  title       → Başlık yazı tipi
  small       → Küçük metin

textures/
  btn_normal  → Normal buton arkaplanı
  btn_hover   → Hover durumu
  btn_pressed → Basılı durumu
  icon_*      → İkonlar
```

## Hata Ayıklama

### Layout Yüklenmiyor

```csharp
var layout = UI.Layouts.LoadLayout("Test", "path/to/file.json");
if (layout == null)
{
    Debug.WriteLine("Layout yüklenemedi! Dosya yolu kontrol edin.");
}
```

### Element Bulunamıyor

```csharp
var btn = UI.Layouts.GetElement<Button>("LayoutName", "btn_id");
if (btn == null)
{
    Debug.WriteLine("Element bulunamadı! ID'yi kontrol edin.");
    
    // Mevcut ID'leri listele
    var builder = UI.Layouts.GetBuilder("LayoutName");
    foreach (var (id, element) in builder?.Elements ?? new())
    {
        Debug.WriteLine($"  - {id}: {element.GetType().Name}");
    }
}
```

### Font/Texture Yüklenmiyor

Content pipeline'da resource'ların doğru build edildiğinden emin olun:

```xml
<!-- Content.mgcb -->
#begin Fonts/DefaultFont.spritefont
/importer:FontDescriptionImporter
/processor:FontDescriptionProcessor
/build:Fonts/DefaultFont.spritefont
```

## Örnek Proje Yapısı

```
MyGame/
├── Content/
│   ├── UI/
│   │   ├── MainMenu.ui.json
│   │   ├── Options.ui.json
│   │   └── HUD.ui.json
│   ├── Fonts/
│   │   └── DefaultFont.spritefont
│   └── Textures/
│       └── UI/
│           └── ...
├── Scenes/
│   ├── MainMenuScene.cs
│   ├── OptionsScene.cs
│   └── GameScene.cs
└── MyGame.cs
```
