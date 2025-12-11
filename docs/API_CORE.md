# RenUI Core Modülü API Referansı

## İçindekiler
- [Interfaces](#interfaces)
- [Events](#events)
- [Primitives](#primitives)

---

## Interfaces

### IUIElement

Tüm UI elemanlarının temel arayüzü.

```csharp
public interface IUIElement : IUpdatable, IDrawable
{
    string Name { get; set; }
    IUIElement? Parent { get; set; }
    Rectangle Bounds { get; set; }
    bool IsFocused { get; set; }
    bool IsHovered { get; set; }
    
    void Initialize();
    bool Contains(Point point);
    void OnFocusGained();
    void OnFocusLost();
}
```

### IContainer

Çocuk elemanları yönetebilen container arayüzü.

```csharp
public interface IContainer : IUIElement
{
    IReadOnlyList<IUIElement> Children { get; }
    void AddChild(IUIElement child);
    void RemoveChild(IUIElement child);
    void ClearChildren();
    IUIElement? GetChildByName(string name);
}
```

### IInputState

Input durumu sorgulama arayüzü.

```csharp
public enum MouseButton { Left, Right, Middle }

public interface IInputState
{
    // Mouse
    MouseState CurrentMouseState { get; }
    MouseState PreviousMouseState { get; }
    Point MousePosition { get; }
    Point MouseDelta { get; }
    int ScrollWheelDelta { get; }
    
    // Keyboard
    KeyboardState CurrentKeyboardState { get; }
    KeyboardState PreviousKeyboardState { get; }
    
    // Mouse metodları
    bool IsMouseButtonPressed(MouseButton button);   // Bu frame basıldı
    bool IsMouseButtonReleased(MouseButton button);  // Bu frame bırakıldı
    bool IsMouseButtonDown(MouseButton button);      // Şu an basılı
    
    // Keyboard metodları
    bool IsKeyPressed(Keys key);
    bool IsKeyReleased(Keys key);
    bool IsKeyDown(Keys key);
}
```

### IInputHandler

```csharp
public interface IInputHandler
{
    bool HandleMouseInput(IInputState inputState);
    bool HandleKeyboardInput(IInputState inputState);
}
```

### IScene

```csharp
public interface IScene : IUpdatable, IDrawable
{
    string Name { get; }
    bool IsLoaded { get; }
    
    void LoadContent(ContentManager content, GraphicsDevice graphicsDevice);
    void UnloadContent();
    void OnEnter();
    void OnExit();
}
```

### IUpdatable & IDrawable

```csharp
public interface IUpdatable
{
    bool IsEnabled { get; set; }
    void Update(GameTime gameTime);
}

public interface IDrawable
{
    bool IsVisible { get; set; }
    int DrawOrder { get; set; }
    void Draw(SpriteBatch spriteBatch, GameTime gameTime);
}
```

---

## Events

### EventDispatcher<T>

Thread-safe generic event dispatcher.

```csharp
public sealed class EventDispatcher<TEventArgs> where TEventArgs : UIEventArgs
{
    void Subscribe(Action<TEventArgs> handler);
    void Unsubscribe(Action<TEventArgs> handler);
    void Dispatch(TEventArgs args);
    void Clear();
}
```

**Kullanım:**
```csharp
// Abone ol
button.OnClick.Subscribe(e => Console.WriteLine("Clicked!"));

// Metot referansı ile
button.OnClick.Subscribe(HandleClick);
button.OnClick.Unsubscribe(HandleClick);

// Temizle
button.OnClick.Clear();
```

### Event Args Sınıfları

#### UIEventArgs (Base)
```csharp
public class UIEventArgs : EventArgs
{
    public object? Sender { get; }
    public GameTime? GameTime { get; }
    public bool Handled { get; set; }  // Event yayılmasını durdur
}
```

#### MouseEventArgs
```csharp
public class MouseEventArgs : UIEventArgs
{
    public Point Position { get; }
    public Point Delta { get; }
    public MouseButton Button { get; }
}
```

#### KeyEventArgs
```csharp
public class KeyEventArgs : UIEventArgs
{
    public Keys Key { get; }
    public bool IsRepeat { get; }
}
```

#### FocusEventArgs
```csharp
public class FocusEventArgs : UIEventArgs
{
    public bool IsFocused { get; }
}
```

#### CheckedChangedEventArgs
```csharp
public class CheckedChangedEventArgs : UIEventArgs
{
    public bool IsChecked { get; }
}
```

#### ValueChangedEventArgs
```csharp
public class ValueChangedEventArgs : UIEventArgs
{
    public float Value { get; }
}
```

---

## Primitives

### Padding

İç boşluk yapısı.

```csharp
public readonly struct Padding : IEquatable<Padding>
{
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }
    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;
    
    public static Padding Zero { get; }
    
    public Padding(int all);
    public Padding(int horizontal, int vertical);
    public Padding(int left, int top, int right, int bottom);
}
```

**Kullanım:**
```csharp
var p1 = new Padding(10);           // Tüm kenarlar 10px
var p2 = new Padding(20, 10);       // Yatay 20, dikey 10
var p3 = new Padding(5, 10, 15, 20); // Sol, üst, sağ, alt
```

### Margin

Dış boşluk yapısı (Padding ile aynı yapı).

```csharp
public readonly struct Margin : IEquatable<Margin>
{
    // Padding ile aynı özellikler
}
```

### Anchor

Konumlandırma çapa noktası.

```csharp
public enum Anchor
{
    TopLeft, TopCenter, TopRight,
    CenterLeft, Center, CenterRight,
    BottomLeft, BottomCenter, BottomRight,
    Fill
}
```

### Size

Boyut yapısı.

```csharp
public readonly struct Size : IEquatable<Size>
{
    public int Width { get; }
    public int Height { get; }
    public static Size Zero { get; }
    
    public Size(int width, int height);
}
```
