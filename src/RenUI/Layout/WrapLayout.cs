using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Core.Primitives;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Layout;

/// <summary>
/// Wrap layout that arranges children in rows or columns, wrapping to the next line when space runs out.
/// Similar to CSS flex-wrap behavior.
/// </summary>
public class WrapLayout : ILayoutStrategy
{
    public Orientation Orientation { get; set; } = Orientation.Horizontal;
    public int Spacing { get; set; } = 4;
    public int LineSpacing { get; set; } = 4;
    public HorizontalAlignment HorizontalAlignment { get; set; } = HorizontalAlignment.Left;
    public VerticalAlignment VerticalAlignment { get; set; } = VerticalAlignment.Top;

    public WrapLayout() { }

    public WrapLayout(Orientation orientation, int spacing = 4)
    {
        Orientation = orientation;
        Spacing = spacing;
        LineSpacing = spacing;
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;

        var bounds = element.Bounds;
        var padding = element.Padding;
        var availableWidth = bounds.Width - padding.Horizontal;
        var availableHeight = bounds.Height - padding.Vertical;

        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return;

        if (Orientation == Orientation.Horizontal)
        {
            LayoutHorizontal(visibleChildren, padding, availableWidth, availableHeight);
        }
        else
        {
            LayoutVertical(visibleChildren, padding, availableWidth, availableHeight);
        }
    }

    private void LayoutHorizontal(List<IUIElement> children, Padding padding, int availableWidth, int availableHeight)
    {
        var lines = new List<WrapLine>();
        var currentLine = new WrapLine();
        int currentLineWidth = 0;

        foreach (var child in children)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);
            int childTotalWidth = childSize.X + marginSize.X;
            int childTotalHeight = childSize.Y + marginSize.Y;

            // Check if we need to wrap
            if (currentLine.Children.Count > 0 && currentLineWidth + childTotalWidth + Spacing > availableWidth)
            {
                lines.Add(currentLine);
                currentLine = new WrapLine();
                currentLineWidth = 0;
            }

            currentLine.Children.Add(child);
            currentLine.Sizes.Add(childSize);
            currentLine.CrossSize = Math.Max(currentLine.CrossSize, childTotalHeight);
            currentLineWidth += childTotalWidth + (currentLine.Children.Count > 1 ? Spacing : 0);
            currentLine.MainSize = currentLineWidth;
        }

        if (currentLine.Children.Count > 0)
            lines.Add(currentLine);

        // Position lines
        int currentY = padding.Top;

        foreach (var line in lines)
        {
            int currentX = padding.Left;

            // Adjust starting X based on horizontal alignment
            int freeSpace = availableWidth - line.MainSize;
            switch (HorizontalAlignment)
            {
                case HorizontalAlignment.Center:
                    currentX += freeSpace / 2;
                    break;
                case HorizontalAlignment.Right:
                    currentX += freeSpace;
                    break;
            }

            for (int i = 0; i < line.Children.Count; i++)
            {
                var child = line.Children[i];
                if (child is not UIElement childElement) continue;

                var margin = childElement.Margin;
                var childSize = line.Sizes[i];

                int y = currentY + margin.Top;
                
                // Vertical alignment within line
                switch (VerticalAlignment)
                {
                    case VerticalAlignment.Center:
                        y = currentY + (line.CrossSize - childSize.Y - margin.Vertical) / 2 + margin.Top;
                        break;
                    case VerticalAlignment.Bottom:
                        y = currentY + line.CrossSize - childSize.Y - margin.Bottom;
                        break;
                }

                childElement.Bounds = new Rectangle(
                    currentX + margin.Left,
                    y,
                    childSize.X,
                    childSize.Y);

                currentX += childSize.X + margin.Horizontal + Spacing;
            }

            currentY += line.CrossSize + LineSpacing;
        }
    }

    private void LayoutVertical(List<IUIElement> children, Padding padding, int availableWidth, int availableHeight)
    {
        var lines = new List<WrapLine>();
        var currentLine = new WrapLine();
        int currentLineHeight = 0;

        foreach (var child in children)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);
            int childTotalWidth = childSize.X + marginSize.X;
            int childTotalHeight = childSize.Y + marginSize.Y;

            // Check if we need to wrap
            if (currentLine.Children.Count > 0 && currentLineHeight + childTotalHeight + Spacing > availableHeight)
            {
                lines.Add(currentLine);
                currentLine = new WrapLine();
                currentLineHeight = 0;
            }

            currentLine.Children.Add(child);
            currentLine.Sizes.Add(childSize);
            currentLine.CrossSize = Math.Max(currentLine.CrossSize, childTotalWidth);
            currentLineHeight += childTotalHeight + (currentLine.Children.Count > 1 ? Spacing : 0);
            currentLine.MainSize = currentLineHeight;
        }

        if (currentLine.Children.Count > 0)
            lines.Add(currentLine);

        // Position lines
        int currentX = padding.Left;

        foreach (var line in lines)
        {
            int currentY = padding.Top;

            // Adjust starting Y based on vertical alignment
            int freeSpace = availableHeight - line.MainSize;
            switch (VerticalAlignment)
            {
                case VerticalAlignment.Center:
                    currentY += freeSpace / 2;
                    break;
                case VerticalAlignment.Bottom:
                    currentY += freeSpace;
                    break;
            }

            for (int i = 0; i < line.Children.Count; i++)
            {
                var child = line.Children[i];
                if (child is not UIElement childElement) continue;

                var margin = childElement.Margin;
                var childSize = line.Sizes[i];

                int x = currentX + margin.Left;

                // Horizontal alignment within line
                switch (HorizontalAlignment)
                {
                    case HorizontalAlignment.Center:
                        x = currentX + (line.CrossSize - childSize.X - margin.Horizontal) / 2 + margin.Left;
                        break;
                    case HorizontalAlignment.Right:
                        x = currentX + line.CrossSize - childSize.X - margin.Right;
                        break;
                }

                childElement.Bounds = new Rectangle(
                    x,
                    currentY + margin.Top,
                    childSize.X,
                    childSize.Y);

                currentY += childSize.Y + margin.Vertical + Spacing;
            }

            currentX += line.CrossSize + LineSpacing;
        }
    }

    public Point MeasureContent(IContainer container)
    {
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return Point.Zero;

        // For measurement, we assume single line (no wrapping)
        int totalMain = 0;
        int maxCross = 0;

        foreach (var child in visibleChildren)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);

            if (Orientation == Orientation.Horizontal)
            {
                totalMain += childSize.X + marginSize.X;
                maxCross = Math.Max(maxCross, childSize.Y + marginSize.Y);
            }
            else
            {
                totalMain += childSize.Y + marginSize.Y;
                maxCross = Math.Max(maxCross, childSize.X + marginSize.X);
            }
        }

        totalMain += (visibleChildren.Count - 1) * Spacing;

        if (container is UIElement elem)
        {
            if (Orientation == Orientation.Horizontal)
            {
                totalMain += elem.Padding.Horizontal;
                maxCross += elem.Padding.Vertical;
            }
            else
            {
                totalMain += elem.Padding.Vertical;
                maxCross += elem.Padding.Horizontal;
            }
        }

        return Orientation == Orientation.Horizontal
            ? new Point(totalMain, maxCross)
            : new Point(maxCross, totalMain);
    }

    private class WrapLine
    {
        public List<IUIElement> Children { get; } = new();
        public List<Point> Sizes { get; } = new();
        public int MainSize { get; set; }
        public int CrossSize { get; set; }
    }
}
