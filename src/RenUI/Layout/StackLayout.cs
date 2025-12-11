using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Layout;

/// <summary>
/// Stack layout that arranges children in a single row or column.
/// Supports distribution modes for spacing and child ordering.
/// </summary>
public class StackLayout : ILayoutStrategy
{
    private readonly Dictionary<IUIElement, int> _childOrder = new();

    public Orientation Orientation { get; set; } = Orientation.Vertical;
    public int Spacing { get; set; } = 4;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Stretch;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;
    
    /// <summary>
    /// Distribution mode for spacing between children.
    /// </summary>
    public StackDistribution Distribution { get; set; } = StackDistribution.Start;
    
    /// <summary>
    /// Whether to reverse the order of children.
    /// </summary>
    public bool Reverse { get; set; } = false;

    public StackLayout() { }

    public StackLayout(Orientation orientation, int spacing = 4)
    {
        Orientation = orientation;
        Spacing = spacing;
    }

    public StackLayout(Orientation orientation, StackDistribution distribution, int spacing = 4)
    {
        Orientation = orientation;
        Distribution = distribution;
        Spacing = spacing;
    }

    /// <summary>
    /// Sets the order of a child for custom ordering within the layout.
    /// Lower values appear first.
    /// </summary>
    public void SetOrder(IUIElement element, int order)
    {
        _childOrder[element] = order;
    }

    /// <summary>
    /// Gets the order of a child element.
    /// </summary>
    public int GetOrder(IUIElement element)
    {
        return _childOrder.TryGetValue(element, out var order) ? order : 0;
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;

        var bounds = element.Bounds;
        var padding = element.Padding;
        var availableWidth = bounds.Width - padding.Horizontal;
        var availableHeight = bounds.Height - padding.Vertical;

        // Get visible children and sort by order
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .OrderBy(c => GetOrder(c))
            .ToList();

        if (Reverse)
            visibleChildren.Reverse();

        if (visibleChildren.Count == 0) return;

        // Calculate total content size
        int totalContentSize = 0;
        var childSizes = new List<Point>();

        foreach (var child in visibleChildren)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);
            childSizes.Add(childSize);

            if (Orientation == Orientation.Vertical)
                totalContentSize += childSize.Y + marginSize.Y;
            else
                totalContentSize += childSize.X + marginSize.X;
        }

        // Calculate spacing based on distribution
        int mainAxisSize = Orientation == Orientation.Vertical ? availableHeight : availableWidth;
        int totalGaps = (visibleChildren.Count - 1) * Spacing;
        int freeSpace = mainAxisSize - totalContentSize - totalGaps;
        
        int currentX = padding.Left;
        int currentY = padding.Top;
        int dynamicSpacing = Spacing;
        int startOffset = 0;

        switch (Distribution)
        {
            case StackDistribution.Start:
                break;
            case StackDistribution.End:
                startOffset = freeSpace;
                break;
            case StackDistribution.Center:
                startOffset = freeSpace / 2;
                break;
            case StackDistribution.SpaceBetween:
                if (visibleChildren.Count > 1)
                    dynamicSpacing = Spacing + freeSpace / (visibleChildren.Count - 1);
                break;
            case StackDistribution.SpaceAround:
                dynamicSpacing = Spacing + freeSpace / visibleChildren.Count;
                startOffset = (dynamicSpacing - Spacing) / 2;
                break;
            case StackDistribution.SpaceEvenly:
                int evenSpace = (mainAxisSize - totalContentSize) / (visibleChildren.Count + 1);
                startOffset = evenSpace;
                dynamicSpacing = evenSpace;
                break;
        }

        if (Orientation == Orientation.Vertical)
            currentY += startOffset;
        else
            currentX += startOffset;

        for (int i = 0; i < visibleChildren.Count; i++)
        {
            var child = visibleChildren[i];
            if (child is not UIElement childElement) continue;

            var margin = childElement.Margin;
            var childWidth = childSizes[i].X;
            var childHeight = childSizes[i].Y;

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
                currentY += childHeight + margin.Vertical + dynamicSpacing;
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
                currentX += childWidth + margin.Horizontal + dynamicSpacing;
            }
        }
    }

    public Point MeasureContent(IContainer container)
    {
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return Point.Zero;

        int totalMain = 0;
        int maxCross = 0;

        foreach (var child in visibleChildren)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);

            if (Orientation == Orientation.Vertical)
            {
                totalMain += childSize.Y + marginSize.Y;
                maxCross = Math.Max(maxCross, childSize.X + marginSize.X);
            }
            else
            {
                totalMain += childSize.X + marginSize.X;
                maxCross = Math.Max(maxCross, childSize.Y + marginSize.Y);
            }
        }

        totalMain += (visibleChildren.Count - 1) * Spacing;

        if (container is UIElement elem)
        {
            if (Orientation == Orientation.Vertical)
            {
                totalMain += elem.Padding.Vertical;
                maxCross += elem.Padding.Horizontal;
            }
            else
            {
                totalMain += elem.Padding.Horizontal;
                maxCross += elem.Padding.Vertical;
            }
        }

        return Orientation == Orientation.Vertical 
            ? new Point(maxCross, totalMain) 
            : new Point(totalMain, maxCross);
    }
}

/// <summary>
/// Distribution mode for stack layout spacing.
/// </summary>
public enum StackDistribution
{
    /// <summary>Items packed at the start.</summary>
    Start,
    /// <summary>Items packed at the end.</summary>
    End,
    /// <summary>Items centered in the container.</summary>
    Center,
    /// <summary>Items evenly distributed, first and last items touch the edges.</summary>
    SpaceBetween,
    /// <summary>Items evenly distributed with equal space around each item.</summary>
    SpaceAround,
    /// <summary>Items evenly distributed with equal space between and around items.</summary>
    SpaceEvenly
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

