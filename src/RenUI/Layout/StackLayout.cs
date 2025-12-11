using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Layout;

public class StackLayout : ILayoutStrategy
{
    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public int Spacing { get; set; } = 4;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    public StackLayout() { }

    public StackLayout(Orientation orientation, int spacing = 4)
    {
        Orientation = orientation;
        Spacing = spacing;
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;

        var bounds = element.Bounds;
        var padding = element.Padding;
        var availableWidth = bounds.Width - padding.Horizontal;
        var availableHeight = bounds.Height - padding.Vertical;

        int currentX = padding.Left;
        int currentY = padding.Top;

        foreach (var child in container.Children)
        {
            if (child is not UIElement childElement) continue;
            if (!childElement.IsVisible) continue;

            var margin = childElement.Margin;
            var childWidth = childElement.Width;
            var childHeight = childElement.Height;

            if (Orientation == Orientation.Vertical)
            {
                int x = HorizontalAlignment switch
                {
                    HorizontalAlignment.Left => padding.Left + margin.Left,
                    HorizontalAlignment.Center => padding.Left + (availableWidth - childWidth) / 2,
                    HorizontalAlignment.Right => bounds.Width - padding.Right - margin.Right - childWidth,
                    HorizontalAlignment.Stretch => padding.Left + margin.Left,
                    _ => padding.Left + margin.Left
                };

                if (HorizontalAlignment == HorizontalAlignment.Stretch)
                {
                    childWidth = availableWidth - margin.Horizontal;
                }

                childElement.Bounds = new Rectangle(x, currentY + margin.Top, childWidth, childHeight);
                currentY += childHeight + margin.Vertical + Spacing;
            }
            else
            {
                int y = VerticalAlignment switch
                {
                    VerticalAlignment.Top => padding.Top + margin.Top,
                    VerticalAlignment.Center => padding.Top + (availableHeight - childHeight) / 2,
                    VerticalAlignment.Bottom => bounds.Height - padding.Bottom - margin.Bottom - childHeight,
                    VerticalAlignment.Stretch => padding.Top + margin.Top,
                    _ => padding.Top + margin.Top
                };

                if (VerticalAlignment == VerticalAlignment.Stretch)
                {
                    childHeight = availableHeight - margin.Vertical;
                }

                childElement.Bounds = new Rectangle(currentX + margin.Left, y, childWidth, childHeight);
                currentX += childWidth + margin.Horizontal + Spacing;
            }
        }
    }
}

public enum HorizontalAlignment
{
    Left,
    Center,
    Right,
    Stretch
}

public enum VerticalAlignment
{
    Top,
    Center,
    Bottom,
    Stretch
}
