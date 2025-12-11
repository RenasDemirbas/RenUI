namespace RenUI.Core.Primitives;

public readonly struct Margin : IEquatable<Margin>
{
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;

    public static Margin Zero => new(0);

    public Margin(int all)
    {
        Left = Top = Right = Bottom = all;
    }

    public Margin(int horizontal, int vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public Margin(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public bool Equals(Margin other) =>
        Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

    public override bool Equals(object? obj) => obj is Margin other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    public static bool operator ==(Margin left, Margin right) => left.Equals(right);
    public static bool operator !=(Margin left, Margin right) => !left.Equals(right);
}
