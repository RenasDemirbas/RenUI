using Microsoft.Xna.Framework;

namespace RenUI.Core.Interfaces;

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
