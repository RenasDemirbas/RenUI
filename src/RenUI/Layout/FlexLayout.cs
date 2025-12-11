using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Core.Primitives;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Layout;

/// <summary>
/// Flexible box layout that arranges children in a row or column with advanced distribution options.
/// Similar to CSS Flexbox, provides powerful alignment and spacing capabilities.
/// </summary>
public class FlexLayout : ILayoutStrategy
{
    private readonly Dictionary<IUIElement, FlexChildSettings> _childSettings = new();

    public FlexDirection Direction { get; set; } = FlexDirection.Row;
    public FlexWrap Wrap { get; set; } = FlexWrap.NoWrap;
    public FlexJustify JustifyContent { get; set; } = FlexJustify.Start;
    public FlexAlign AlignItems { get; set; } = FlexAlign.Stretch;
    public FlexAlign AlignContent { get; set; } = FlexAlign.Stretch;
    public int Gap { get; set; } = 0;
    public int RowGap { get; set; } = 0;
    public int ColumnGap { get; set; } = 0;

    public FlexLayout() { }

    public FlexLayout(FlexDirection direction, int gap = 0)
    {
        Direction = direction;
        Gap = gap;
    }

    /// <summary>
    /// Sets flex properties for a specific child element.
    /// </summary>
    public void SetChildSettings(IUIElement element, FlexChildSettings settings)
    {
        _childSettings[element] = settings;
    }

    /// <summary>
    /// Gets flex properties for a specific child element.
    /// </summary>
    public FlexChildSettings GetChildSettings(IUIElement element)
    {
        return _childSettings.TryGetValue(element, out var settings) ? settings : FlexChildSettings.Default;
    }

    /// <summary>
    /// Sets the flex grow value for a child (how much it should grow relative to siblings).
    /// </summary>
    public void SetFlexGrow(IUIElement element, float grow)
    {
        var settings = GetChildSettings(element);
        settings.Grow = grow;
        _childSettings[element] = settings;
    }

    /// <summary>
    /// Sets the flex shrink value for a child (how much it should shrink relative to siblings).
    /// </summary>
    public void SetFlexShrink(IUIElement element, float shrink)
    {
        var settings = GetChildSettings(element);
        settings.Shrink = shrink;
        _childSettings[element] = settings;
    }

    /// <summary>
    /// Sets the flex basis (initial size) for a child.
    /// </summary>
    public void SetFlexBasis(IUIElement element, int basis)
    {
        var settings = GetChildSettings(element);
        settings.Basis = basis;
        _childSettings[element] = settings;
    }

    /// <summary>
    /// Sets individual alignment for a child, overriding AlignItems.
    /// </summary>
    public void SetAlignSelf(IUIElement element, FlexAlign align)
    {
        var settings = GetChildSettings(element);
        settings.AlignSelf = align;
        _childSettings[element] = settings;
    }

    /// <summary>
    /// Sets the order of a child for reordering within the layout.
    /// </summary>
    public void SetOrder(IUIElement element, int order)
    {
        var settings = GetChildSettings(element);
        settings.Order = order;
        _childSettings[element] = settings;
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
            .OrderBy(c => GetChildSettings(c).Order)
            .ToList();

        if (visibleChildren.Count == 0) return;

        int mainGap = GetMainGap();
        int crossGap = GetCrossGap();

        bool isRow = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
        int mainAxisSize = isRow ? availableWidth : availableHeight;
        int crossAxisSize = isRow ? availableHeight : availableWidth;

        if (Wrap == FlexWrap.NoWrap)
        {
            LayoutSingleLine(visibleChildren, padding, mainAxisSize, crossAxisSize, isRow, mainGap);
        }
        else
        {
            LayoutMultiLine(visibleChildren, padding, mainAxisSize, crossAxisSize, isRow, mainGap, crossGap);
        }
    }

    public Point MeasureContent(IContainer container)
    {
        var visibleChildren = container.Children
            .Where(c => c is UIElement el && el.IsVisible)
            .ToList();

        if (visibleChildren.Count == 0) return Point.Zero;

        bool isRow = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
        int mainGap = GetMainGap();
        int totalMain = 0;
        int maxCross = 0;

        foreach (var child in visibleChildren)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);

            int childMain = (isRow ? childSize.X : childSize.Y) + (isRow ? marginSize.X : marginSize.Y);
            int childCross = (isRow ? childSize.Y : childSize.X) + (isRow ? marginSize.Y : marginSize.X);

            totalMain += childMain;
            maxCross = Math.Max(maxCross, childCross);
        }

        totalMain += (visibleChildren.Count - 1) * mainGap;

        if (container is UIElement elem)
        {
            totalMain += isRow ? elem.Padding.Horizontal : elem.Padding.Vertical;
            maxCross += isRow ? elem.Padding.Vertical : elem.Padding.Horizontal;
        }

        return isRow ? new Point(totalMain, maxCross) : new Point(maxCross, totalMain);
    }

    private int GetMainGap()
    {
        bool isRow = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
        return Gap > 0 ? Gap : (isRow ? ColumnGap : RowGap);
    }

    private int GetCrossGap()
    {
        bool isRow = Direction == FlexDirection.Row || Direction == FlexDirection.RowReverse;
        return Gap > 0 ? Gap : (isRow ? RowGap : ColumnGap);
    }

    private void LayoutSingleLine(List<IUIElement> children, Padding padding, int mainAxisSize, int crossAxisSize, bool isRow, int gap)
    {
        // Calculate total main size needed and flex totals
        int totalFixedMain = 0;
        float totalGrow = 0;
        float totalShrink = 0;

        var childMainSizes = new int[children.Count];
        var childCrossSizes = new int[children.Count];

        for (int i = 0; i < children.Count; i++)
        {
            var child = children[i];
            var settings = GetChildSettings(child);
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);

            int basis = settings.Basis >= 0 ? settings.Basis : (isRow ? childSize.X : childSize.Y);
            childMainSizes[i] = basis + (isRow ? marginSize.X : marginSize.Y);
            childCrossSizes[i] = (isRow ? childSize.Y : childSize.X) + (isRow ? marginSize.Y : marginSize.X);

            totalFixedMain += childMainSizes[i];
            totalGrow += settings.Grow;
            totalShrink += settings.Shrink;
        }

        int totalGaps = (children.Count - 1) * gap;
        int freeSpace = mainAxisSize - totalFixedMain - totalGaps;

        // Distribute free space based on grow/shrink
        if (freeSpace > 0 && totalGrow > 0)
        {
            for (int i = 0; i < children.Count; i++)
            {
                var settings = GetChildSettings(children[i]);
                if (settings.Grow > 0)
                {
                    childMainSizes[i] += (int)(freeSpace * (settings.Grow / totalGrow));
                }
            }
        }
        else if (freeSpace < 0 && totalShrink > 0)
        {
            for (int i = 0; i < children.Count; i++)
            {
                var settings = GetChildSettings(children[i]);
                if (settings.Shrink > 0)
                {
                    childMainSizes[i] += (int)(freeSpace * (settings.Shrink / totalShrink));
                    childMainSizes[i] = Math.Max(0, childMainSizes[i]);
                }
            }
        }

        // Calculate starting position and spacing based on JustifyContent
        int currentMain = isRow ? padding.Left : padding.Top;
        int spacing = 0;
        int remainingSpace = mainAxisSize - childMainSizes.Sum() - totalGaps;

        switch (JustifyContent)
        {
            case FlexJustify.Start:
                break;
            case FlexJustify.End:
                currentMain += remainingSpace;
                break;
            case FlexJustify.Center:
                currentMain += remainingSpace / 2;
                break;
            case FlexJustify.SpaceBetween:
                spacing = children.Count > 1 ? remainingSpace / (children.Count - 1) : 0;
                break;
            case FlexJustify.SpaceAround:
                spacing = remainingSpace / children.Count;
                currentMain += spacing / 2;
                break;
            case FlexJustify.SpaceEvenly:
                spacing = remainingSpace / (children.Count + 1);
                currentMain += spacing;
                break;
        }

        // Handle reverse direction
        bool isReverse = Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;
        if (isReverse)
        {
            currentMain = (isRow ? padding.Left : padding.Top) + mainAxisSize - childMainSizes[0];
        }

        // Position children
        for (int i = 0; i < children.Count; i++)
        {
            int index = isReverse ? children.Count - 1 - i : i;
            var child = children[index];
            var childElement = child as UIElement;
            if (childElement == null) continue;

            var margin = childElement.Margin;
            var settings = GetChildSettings(child);
            var align = settings.AlignSelf ?? AlignItems;

            int mainSize = childMainSizes[index] - (isRow ? margin.Horizontal : margin.Vertical);
            int crossSize = childCrossSizes[index] - (isRow ? margin.Vertical : margin.Horizontal);

            // Calculate cross-axis position
            int crossStart = isRow ? padding.Top : padding.Left;
            int crossPosition = crossStart;

            switch (align)
            {
                case FlexAlign.Start:
                    crossPosition = crossStart + (isRow ? margin.Top : margin.Left);
                    break;
                case FlexAlign.End:
                    crossPosition = crossStart + crossAxisSize - crossSize - (isRow ? margin.Bottom : margin.Right);
                    break;
                case FlexAlign.Center:
                    crossPosition = crossStart + (crossAxisSize - crossSize) / 2;
                    break;
                case FlexAlign.Stretch:
                    crossPosition = crossStart + (isRow ? margin.Top : margin.Left);
                    crossSize = crossAxisSize - (isRow ? margin.Vertical : margin.Horizontal);
                    break;
            }

            if (isRow)
            {
                childElement.Bounds = new Rectangle(
                    currentMain + margin.Left,
                    crossPosition,
                    mainSize,
                    crossSize);
            }
            else
            {
                childElement.Bounds = new Rectangle(
                    crossPosition,
                    currentMain + margin.Top,
                    crossSize,
                    mainSize);
            }

            if (isReverse)
            {
                if (i < children.Count - 1)
                    currentMain -= childMainSizes[isReverse ? index - 1 : index + 1] + gap + spacing;
            }
            else
            {
                currentMain += childMainSizes[index] + gap + spacing;
            }
        }
    }

    private void LayoutMultiLine(List<IUIElement> children, Padding padding, int mainAxisSize, int crossAxisSize, bool isRow, int mainGap, int crossGap)
    {
        // Build lines
        var lines = new List<FlexLine>();
        var currentLine = new FlexLine();
        int currentLineMain = 0;

        foreach (var child in children)
        {
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);
            var settings = GetChildSettings(child);

            int childMain = settings.Basis >= 0 ? settings.Basis : (isRow ? childSize.X : childSize.Y);
            childMain += isRow ? marginSize.X : marginSize.Y;

            int childCross = (isRow ? childSize.Y : childSize.X) + (isRow ? marginSize.Y : marginSize.X);

            // Check if we need to wrap
            if (currentLine.Children.Count > 0 && currentLineMain + childMain + mainGap > mainAxisSize)
            {
                lines.Add(currentLine);
                currentLine = new FlexLine();
                currentLineMain = 0;
            }

            currentLine.Children.Add(child);
            currentLine.MainSizes.Add(childMain);
            currentLine.CrossSize = Math.Max(currentLine.CrossSize, childCross);
            currentLineMain += childMain + (currentLine.Children.Count > 1 ? mainGap : 0);
        }

        if (currentLine.Children.Count > 0)
            lines.Add(currentLine);

        // Handle wrap-reverse
        if (Wrap == FlexWrap.WrapReverse)
            lines.Reverse();

        // Calculate cross-axis distribution
        int totalCrossSize = lines.Sum(l => l.CrossSize) + (lines.Count - 1) * crossGap;
        int freeCrossSpace = crossAxisSize - totalCrossSize;
        int crossStart = isRow ? padding.Top : padding.Left;
        int crossSpacing = 0;

        switch (AlignContent)
        {
            case FlexAlign.Start:
                break;
            case FlexAlign.End:
                crossStart += freeCrossSpace;
                break;
            case FlexAlign.Center:
                crossStart += freeCrossSpace / 2;
                break;
            case FlexAlign.Stretch:
                if (lines.Count > 0)
                {
                    int extraPerLine = freeCrossSpace / lines.Count;
                    foreach (var line in lines)
                        line.CrossSize += extraPerLine;
                }
                break;
        }

        // Layout each line
        int currentCross = crossStart;
        foreach (var line in lines)
        {
            LayoutLine(line, padding, mainAxisSize, currentCross, isRow, mainGap);
            currentCross += line.CrossSize + crossGap + crossSpacing;
        }
    }

    private void LayoutLine(FlexLine line, Padding padding, int mainAxisSize, int crossPosition, bool isRow, int gap)
    {
        int totalMain = line.MainSizes.Sum();
        int totalGaps = (line.Children.Count - 1) * gap;
        int freeSpace = mainAxisSize - totalMain - totalGaps;

        // Calculate starting position based on JustifyContent
        int currentMain = isRow ? padding.Left : padding.Top;
        int spacing = 0;

        switch (JustifyContent)
        {
            case FlexJustify.Start:
                break;
            case FlexJustify.End:
                currentMain += freeSpace;
                break;
            case FlexJustify.Center:
                currentMain += freeSpace / 2;
                break;
            case FlexJustify.SpaceBetween:
                spacing = line.Children.Count > 1 ? freeSpace / (line.Children.Count - 1) : 0;
                break;
            case FlexJustify.SpaceAround:
                spacing = freeSpace / line.Children.Count;
                currentMain += spacing / 2;
                break;
            case FlexJustify.SpaceEvenly:
                spacing = freeSpace / (line.Children.Count + 1);
                currentMain += spacing;
                break;
        }

        bool isReverse = Direction == FlexDirection.RowReverse || Direction == FlexDirection.ColumnReverse;

        for (int i = 0; i < line.Children.Count; i++)
        {
            int index = isReverse ? line.Children.Count - 1 - i : i;
            var child = line.Children[index];
            var childElement = child as UIElement;
            if (childElement == null) continue;

            var margin = childElement.Margin;
            var settings = GetChildSettings(child);
            var align = settings.AlignSelf ?? AlignItems;

            int mainSize = line.MainSizes[index] - (isRow ? margin.Horizontal : margin.Vertical);
            int crossSize = (isRow ? childElement.Height : childElement.Width);

            int childCrossPos = crossPosition;
            switch (align)
            {
                case FlexAlign.Start:
                    childCrossPos += isRow ? margin.Top : margin.Left;
                    break;
                case FlexAlign.End:
                    childCrossPos = crossPosition + line.CrossSize - crossSize - (isRow ? margin.Bottom : margin.Right);
                    break;
                case FlexAlign.Center:
                    childCrossPos = crossPosition + (line.CrossSize - crossSize) / 2;
                    break;
                case FlexAlign.Stretch:
                    childCrossPos += isRow ? margin.Top : margin.Left;
                    crossSize = line.CrossSize - (isRow ? margin.Vertical : margin.Horizontal);
                    break;
            }

            if (isRow)
            {
                childElement.Bounds = new Rectangle(
                    currentMain + margin.Left,
                    childCrossPos,
                    mainSize,
                    crossSize);
            }
            else
            {
                childElement.Bounds = new Rectangle(
                    childCrossPos,
                    currentMain + margin.Top,
                    crossSize,
                    mainSize);
            }

            currentMain += line.MainSizes[index] + gap + spacing;
        }
    }

    private class FlexLine
    {
        public List<IUIElement> Children { get; } = new();
        public List<int> MainSizes { get; } = new();
        public int CrossSize { get; set; }
    }
}

/// <summary>
/// Direction of the main axis for flex layout.
/// </summary>
public enum FlexDirection
{
    Row,
    RowReverse,
    Column,
    ColumnReverse
}

/// <summary>
/// Wrapping behavior for flex items.
/// </summary>
public enum FlexWrap
{
    NoWrap,
    Wrap,
    WrapReverse
}

/// <summary>
/// Justification of items along the main axis.
/// </summary>
public enum FlexJustify
{
    Start,
    End,
    Center,
    SpaceBetween,
    SpaceAround,
    SpaceEvenly
}

/// <summary>
/// Alignment of items along the cross axis.
/// </summary>
public enum FlexAlign
{
    Start,
    End,
    Center,
    Stretch
}

/// <summary>
/// Individual flex settings for a child element.
/// </summary>
public struct FlexChildSettings
{
    public float Grow { get; set; }
    public float Shrink { get; set; }
    public int Basis { get; set; }
    public FlexAlign? AlignSelf { get; set; }
    public int Order { get; set; }

    public static FlexChildSettings Default => new()
    {
        Grow = 0,
        Shrink = 1,
        Basis = -1,
        AlignSelf = null,
        Order = 0
    };

    public FlexChildSettings(float grow = 0, float shrink = 1, int basis = -1)
    {
        Grow = grow;
        Shrink = shrink;
        Basis = basis;
        AlignSelf = null;
        Order = 0;
    }
}
