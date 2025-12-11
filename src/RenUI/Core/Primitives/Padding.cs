namespace RenUI.Core.Primitives;

public readonly struct Padding : IEquatable<Padding>
{
    public int Left { get; }
    public int Top { get; }
    public int Right { get; }
    public int Bottom { get; }

    public int Horizontal => Left + Right;
    public int Vertical => Top + Bottom;

    public static Padding Zero => new(0);

    public Padding(int all)
    {
        Left = Top = Right = Bottom = all;
    }

    public Padding(int horizontal, int vertical)
    {
        Left = Right = horizontal;
        Top = Bottom = vertical;
    }

    public Padding(int left, int top, int right, int bottom)
    {
        Left = left;
        Top = top;
        Right = right;
        Bottom = bottom;
    }

    public bool Equals(Padding other) =>
        Left == other.Left && Top == other.Top && Right == other.Right && Bottom == other.Bottom;

    public override bool Equals(object? obj) => obj is Padding other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Left, Top, Right, Bottom);

    public static bool operator ==(Padding left, Padding right) => left.Equals(right);
    public static bool operator !=(Padding left, Padding right) => !left.Equals(right);
}
