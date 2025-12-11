using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

/// <summary>
/// Dock layout that arranges children along the edges of the container.
/// Children are docked to Left, Top, Right, or Bottom, with the last child optionally filling remaining space.
/// </summary>
public class DockLayout : ILayoutStrategy
{
    private readonly Dictionary<IUIElement, Dock> _dockPositions = new();

    public bool LastChildFill { get; set; } = true;

    public void SetDock(IUIElement element, Dock dock)
    {
        _dockPositions[element] = dock;
    }

    public Dock GetDock(IUIElement element)
    {
        return _dockPositions.TryGetValue(element, out var dock) ? dock : Dock.Left;
    }

    /// <summary>
    /// Clears dock position for an element.
    /// </summary>
    public void ClearDock(IUIElement element)
    {
        _dockPositions.Remove(element);
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;

        var bounds = element.Bounds;
        var padding = element.Padding;

        var availableRect = new Rectangle(
            padding.Left,
            padding.Top,
            bounds.Width - padding.Horizontal,
            bounds.Height - padding.Vertical);

        var children = container.Children.ToList();
        int lastIndex = LastChildFill ? children.Count - 1 : -1;

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            if (child is not UIElement childElement) continue;
            if (!childElement.IsVisible) continue;

            var margin = childElement.Margin;

            if (i == lastIndex && LastChildFill)
            {
                childElement.Bounds = new Rectangle(
                    availableRect.X + margin.Left,
                    availableRect.Y + margin.Top,
                    availableRect.Width - margin.Horizontal,
                    availableRect.Height - margin.Vertical);
                continue;
            }

            var dock = GetDock(child);
            var childWidth = childElement.Width;
            var childHeight = childElement.Height;

            switch (dock)
            {
                case Dock.Left:
                    childElement.Bounds = new Rectangle(
                        availableRect.X + margin.Left,
                        availableRect.Y + margin.Top,
                        childWidth,
                        availableRect.Height - margin.Vertical);
                    availableRect.X += childWidth + margin.Horizontal;
                    availableRect.Width -= childWidth + margin.Horizontal;
                    break;

                case Dock.Right:
                    childElement.Bounds = new Rectangle(
                        availableRect.Right - childWidth - margin.Right,
                        availableRect.Y + margin.Top,
                        childWidth,
                        availableRect.Height - margin.Vertical);
                    availableRect.Width -= childWidth + margin.Horizontal;
                    break;

                case Dock.Top:
                    childElement.Bounds = new Rectangle(
                        availableRect.X + margin.Left,
                        availableRect.Y + margin.Top,
                        availableRect.Width - margin.Horizontal,
                        childHeight);
                    availableRect.Y += childHeight + margin.Vertical;
                    availableRect.Height -= childHeight + margin.Vertical;
                    break;

                case Dock.Bottom:
                    childElement.Bounds = new Rectangle(
                        availableRect.X + margin.Left,
                        availableRect.Bottom - childHeight - margin.Bottom,
                        availableRect.Width - margin.Horizontal,
                        childHeight);
                    availableRect.Height -= childHeight + margin.Vertical;
                    break;
            }
        }
    }

    public Point MeasureContent(IContainer container)
    {
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return Point.Zero;

        int totalWidth = 0;
        int totalHeight = 0;
        int leftRight = 0;
        int topBottom = 0;

        foreach (var child in visibleChildren)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);
            var dock = GetDock(child);

            switch (dock)
            {
                case Dock.Left:
                case Dock.Right:
                    leftRight += childSize.X + marginSize.X;
                    totalHeight = Math.Max(totalHeight, topBottom + childSize.Y + marginSize.Y);
                    break;
                case Dock.Top:
                case Dock.Bottom:
                    topBottom += childSize.Y + marginSize.Y;
                    totalWidth = Math.Max(totalWidth, leftRight + childSize.X + marginSize.X);
                    break;
            }
        }

        totalWidth = Math.Max(totalWidth, leftRight);
        totalHeight = Math.Max(totalHeight, topBottom);

        if (container is UIElement elem)
        {
            totalWidth += elem.Padding.Horizontal;
            totalHeight += elem.Padding.Vertical;
        }

        return new Point(totalWidth, totalHeight);
    }
}

public enum Dock
{
    Left,
    Top,
    Right,
    Bottom
}
