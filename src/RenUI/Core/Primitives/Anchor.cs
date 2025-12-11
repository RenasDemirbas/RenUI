namespace RenUI.Core.Primitives;

[Flags]
public enum Anchor
{
    None = 0,
    Left = 1 << 0,
    Top = 1 << 1,
    Right = 1 << 2,
    Bottom = 1 << 3,
    CenterHorizontal = 1 << 4,
    CenterVertical = 1 << 5,
    
    TopLeft = Top | Left,
    TopRight = Top | Right,
    TopCenter = Top | CenterHorizontal,
    BottomLeft = Bottom | Left,
    BottomRight = Bottom | Right,
    BottomCenter = Bottom | CenterHorizontal,
    CenterLeft = CenterVertical | Left,
    CenterRight = CenterVertical | Right,
    Center = CenterHorizontal | CenterVertical,
    Fill = Left | Top | Right | Bottom
}
