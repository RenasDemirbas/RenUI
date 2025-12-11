using Microsoft.Xna.Framework;

namespace RenUI.Styling;

public class ElementStyle
{
    public Color? BackgroundColor { get; set; }
    public Color? ForegroundColor { get; set; }
    public Color? BorderColor { get; set; }
    public int? BorderThickness { get; set; }
    public int? CornerRadius { get; set; }
    public float? Opacity { get; set; }

    public ElementStyle() { }

    public ElementStyle(ElementStyle other)
    {
        BackgroundColor = other.BackgroundColor;
        ForegroundColor = other.ForegroundColor;
        BorderColor = other.BorderColor;
        BorderThickness = other.BorderThickness;
        CornerRadius = other.CornerRadius;
        Opacity = other.Opacity;
    }

    public ElementStyle Merge(ElementStyle? other)
    {
        if (other == null) return this;

        return new ElementStyle
        {
            BackgroundColor = other.BackgroundColor ?? BackgroundColor,
            ForegroundColor = other.ForegroundColor ?? ForegroundColor,
            BorderColor = other.BorderColor ?? BorderColor,
            BorderThickness = other.BorderThickness ?? BorderThickness,
            CornerRadius = other.CornerRadius ?? CornerRadius,
            Opacity = other.Opacity ?? Opacity
        };
    }
}
