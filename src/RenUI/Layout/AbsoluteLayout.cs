using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

/// <summary>
/// Absolute layout that allows manual positioning of children.
/// Does not automatically arrange children - positions are set explicitly.
/// </summary>
public class AbsoluteLayout : ILayoutStrategy
{
    public void ApplyLayout(IContainer container)
    {
        // Absolute layout doesn't modify child positions
        // Children are positioned exactly where specified
        // This is essentially a no-op, allowing manual positioning
    }

    public Point MeasureContent(IContainer container)
    {
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return Point.Zero;

        int maxRight = 0;
        int maxBottom = 0;

        foreach (var child in visibleChildren)
        {
            if (child is UIElement elem)
            {
                maxRight = Math.Max(maxRight, elem.Bounds.Right + elem.Margin.Right);
                maxBottom = Math.Max(maxBottom, elem.Bounds.Bottom + elem.Margin.Bottom);
            }
        }

        if (container is UIElement containerElem)
        {
            maxRight += containerElem.Padding.Right;
            maxBottom += containerElem.Padding.Bottom;
        }

        return new Point(maxRight, maxBottom);
    }
}
