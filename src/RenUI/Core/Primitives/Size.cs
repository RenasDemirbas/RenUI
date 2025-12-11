namespace RenUI.Core.Primitives;

public readonly struct Size : IEquatable<Size>
{
    public int Width { get; }
    public int Height { get; }

    public static Size Zero => new(0, 0);
    public static Size One => new(1, 1);

    public Size(int width, int height)
    {
        Width = Math.Max(0, width);
        Height = Math.Max(0, height);
    }

    public Size(int both) : this(both, both) { }

    public bool Equals(Size other) => Width == other.Width && Height == other.Height;

    public override bool Equals(object? obj) => obj is Size other && Equals(other);

    public override int GetHashCode() => HashCode.Combine(Width, Height);

    public static bool operator ==(Size left, Size right) => left.Equals(right);
    public static bool operator !=(Size left, Size right) => !left.Equals(right);

    public static Size operator +(Size left, Size right) => new(left.Width + right.Width, left.Height + right.Height);
    public static Size operator -(Size left, Size right) => new(left.Width - right.Width, left.Height - right.Height);

    public override string ToString() => $"({Width}, {Height})";
}
