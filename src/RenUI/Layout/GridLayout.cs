using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

/// <summary>
/// Grid layout that arranges children in rows and columns.
/// Supports pixel, star (proportional), and auto sizing.
/// </summary>
public class GridLayout : ILayoutStrategy
{
    private readonly List<GridLength> _columnDefinitions = new();
    private readonly List<GridLength> _rowDefinitions = new();
    private readonly Dictionary<IUIElement, GridPosition> _childPositions = new();
    private readonly Dictionary<IUIElement, GridCellAlignment> _childAlignments = new();

    public int ColumnSpacing { get; set; } = 4;
    public int RowSpacing { get; set; } = 4;

    public IReadOnlyList<GridLength> ColumnDefinitions => _columnDefinitions.AsReadOnly();
    public IReadOnlyList<GridLength> RowDefinitions => _rowDefinitions.AsReadOnly();

    public void AddColumn(GridLength length) => _columnDefinitions.Add(length);
    public void AddRow(GridLength length) => _rowDefinitions.Add(length);

    public void AddColumns(params GridLength[] lengths)
    {
        foreach (var length in lengths)
            _columnDefinitions.Add(length);
    }

    public void AddRows(params GridLength[] lengths)
    {
        foreach (var length in lengths)
            _rowDefinitions.Add(length);
    }

    public void ClearColumns() => _columnDefinitions.Clear();
    public void ClearRows() => _rowDefinitions.Clear();
    
    /// <summary>
    /// Removes a column at the specified index.
    /// </summary>
    public void RemoveColumn(int index)
    {
        if (index >= 0 && index < _columnDefinitions.Count)
            _columnDefinitions.RemoveAt(index);
    }

    /// <summary>
    /// Removes a row at the specified index.
    /// </summary>
    public void RemoveRow(int index)
    {
        if (index >= 0 && index < _rowDefinitions.Count)
            _rowDefinitions.RemoveAt(index);
    }

    /// <summary>
    /// Inserts a column at the specified index.
    /// </summary>
    public void InsertColumn(int index, GridLength length)
    {
        if (index >= 0 && index <= _columnDefinitions.Count)
            _columnDefinitions.Insert(index, length);
    }

    /// <summary>
    /// Inserts a row at the specified index.
    /// </summary>
    public void InsertRow(int index, GridLength length)
    {
        if (index >= 0 && index <= _rowDefinitions.Count)
            _rowDefinitions.Insert(index, length);
    }

    public void SetPosition(IUIElement element, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        _childPositions[element] = new GridPosition(row, column, rowSpan, columnSpan);
    }

    public GridPosition? GetPosition(IUIElement element)
    {
        return _childPositions.TryGetValue(element, out var pos) ? pos : null;
    }

    /// <summary>
    /// Sets the alignment of a child within its grid cell.
    /// </summary>
    public void SetCellAlignment(IUIElement element, HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        _childAlignments[element] = new GridCellAlignment(horizontal, vertical);
    }

    /// <summary>
    /// Gets the alignment of a child within its grid cell.
    /// </summary>
    public GridCellAlignment GetCellAlignment(IUIElement element)
    {
        return _childAlignments.TryGetValue(element, out var align) 
            ? align 
            : new GridCellAlignment(HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;
        if (_columnDefinitions.Count == 0 || _rowDefinitions.Count == 0) return;

        var bounds = element.Bounds;
        var padding = element.Padding;
        var availableWidth = bounds.Width - padding.Horizontal - ((_columnDefinitions.Count - 1) * ColumnSpacing);
        var availableHeight = bounds.Height - padding.Vertical - ((_rowDefinitions.Count - 1) * RowSpacing);

        // First pass: measure Auto columns/rows
        var autoColumnWidths = MeasureAutoSizes(_columnDefinitions, container, true);
        var autoRowHeights = MeasureAutoSizes(_rowDefinitions, container, false);

        var columnWidths = CalculateSizes(_columnDefinitions, availableWidth, autoColumnWidths);
        var rowHeights = CalculateSizes(_rowDefinitions, availableHeight, autoRowHeights);

        var columnOffsets = CalculateOffsets(columnWidths, padding.Left, ColumnSpacing);
        var rowOffsets = CalculateOffsets(rowHeights, padding.Top, RowSpacing);

        foreach (var child in container.Children)
        {
            if (child is not UIElement childElement) continue;
            if (!childElement.IsVisible) continue;

            var position = GetPosition(child) ?? new GridPosition(0, 0);
            
            if (position.Row >= rowOffsets.Length || position.Column >= columnOffsets.Length)
                continue;

            int cellX = columnOffsets[position.Column];
            int cellY = rowOffsets[position.Row];
            int cellWidth = CalculateSpannedSize(columnWidths, position.Column, position.ColumnSpan, ColumnSpacing);
            int cellHeight = CalculateSpannedSize(rowHeights, position.Row, position.RowSpan, RowSpacing);

            var margin = childElement.Margin;
            var alignment = GetCellAlignment(child);
            var childSize = LayoutUtils.MeasureChild(child);

            // Calculate actual position and size based on alignment
            int x, y, width, height;

            switch (alignment.Horizontal)
            {
                case HorizontalAlignment.Left:
                    x = cellX + margin.Left;
                    width = childSize.X;
                    break;
                case HorizontalAlignment.Center:
                    width = childSize.X;
                    x = cellX + (cellWidth - width) / 2;
                    break;
                case HorizontalAlignment.Right:
                    width = childSize.X;
                    x = cellX + cellWidth - width - margin.Right;
                    break;
                case HorizontalAlignment.Stretch:
                default:
                    x = cellX + margin.Left;
                    width = cellWidth - margin.Horizontal;
                    break;
            }

            switch (alignment.Vertical)
            {
                case VerticalAlignment.Top:
                    y = cellY + margin.Top;
                    height = childSize.Y;
                    break;
                case VerticalAlignment.Center:
                    height = childSize.Y;
                    y = cellY + (cellHeight - height) / 2;
                    break;
                case VerticalAlignment.Bottom:
                    height = childSize.Y;
                    y = cellY + cellHeight - height - margin.Bottom;
                    break;
                case VerticalAlignment.Stretch:
                default:
                    y = cellY + margin.Top;
                    height = cellHeight - margin.Vertical;
                    break;
            }

            childElement.Bounds = new Rectangle(x, y, Math.Max(0, width), Math.Max(0, height));
        }
    }

    public Point MeasureContent(IContainer container)
    {
        if (_columnDefinitions.Count == 0 || _rowDefinitions.Count == 0)
            return Point.Zero;

        var autoColumnWidths = MeasureAutoSizes(_columnDefinitions, container, true);
        var autoRowHeights = MeasureAutoSizes(_rowDefinitions, container, false);

        int totalWidth = 0;
        int totalHeight = 0;

        for (int i = 0; i < _columnDefinitions.Count; i++)
        {
            var def = _columnDefinitions[i];
            if (def.Type == GridLengthType.Pixel)
                totalWidth += (int)def.Value;
            else if (def.Type == GridLengthType.Auto)
                totalWidth += autoColumnWidths[i];
            // Star columns will be 0 in measure
        }

        for (int i = 0; i < _rowDefinitions.Count; i++)
        {
            var def = _rowDefinitions[i];
            if (def.Type == GridLengthType.Pixel)
                totalHeight += (int)def.Value;
            else if (def.Type == GridLengthType.Auto)
                totalHeight += autoRowHeights[i];
            // Star rows will be 0 in measure
        }

        totalWidth += (_columnDefinitions.Count - 1) * ColumnSpacing;
        totalHeight += (_rowDefinitions.Count - 1) * RowSpacing;

        if (container is UIElement elem)
        {
            totalWidth += elem.Padding.Horizontal;
            totalHeight += elem.Padding.Vertical;
        }

        return new Point(totalWidth, totalHeight);
    }

    private int[] MeasureAutoSizes(List<GridLength> definitions, IContainer container, bool isColumn)
    {
        var sizes = new int[definitions.Count];

        foreach (var child in container.Children)
        {
            if (child is not UIElement childElement || !childElement.IsVisible)
                continue;

            var position = GetPosition(child) ?? new GridPosition(0, 0);
            var childSize = LayoutUtils.MeasureChild(child);
            var marginSize = LayoutUtils.GetMarginSize(child);

            int index = isColumn ? position.Column : position.Row;
            int span = isColumn ? position.ColumnSpan : position.RowSpan;
            int size = (isColumn ? childSize.X : childSize.Y) + (isColumn ? marginSize.X : marginSize.Y);

            // Only affect single-span cells for auto sizing
            if (span == 1 && index < definitions.Count && definitions[index].Type == GridLengthType.Auto)
            {
                sizes[index] = Math.Max(sizes[index], size);
            }
        }

        return sizes;
    }

    private int[] CalculateSizes(List<GridLength> definitions, int availableSize, int[] autoSizes)
    {
        var sizes = new int[definitions.Count];
        int usedSize = 0;
        float totalStar = 0;

        for (int i = 0; i < definitions.Count; i++)
        {
            var def = definitions[i];
            if (def.Type == GridLengthType.Pixel)
            {
                sizes[i] = (int)def.Value;
                usedSize += sizes[i];
            }
            else if (def.Type == GridLengthType.Auto)
            {
                sizes[i] = autoSizes[i];
                usedSize += sizes[i];
            }
            else if (def.Type == GridLengthType.Star)
            {
                totalStar += def.Value;
            }
        }

        int remainingSize = Math.Max(0, availableSize - usedSize);

        for (int i = 0; i < definitions.Count; i++)
        {
            var def = definitions[i];
            if (def.Type == GridLengthType.Star)
            {
                sizes[i] = totalStar > 0 ? (int)(remainingSize * (def.Value / totalStar)) : 0;
            }
        }

        return sizes;
    }

    private int[] CalculateOffsets(int[] sizes, int start, int spacing)
    {
        var offsets = new int[sizes.Length];
        int current = start;

        for (int i = 0; i < sizes.Length; i++)
        {
            offsets[i] = current;
            current += sizes[i] + spacing;
        }

        return offsets;
    }

    private int CalculateSpannedSize(int[] sizes, int start, int span, int spacing)
    {
        int size = 0;
        int end = Math.Min(start + span, sizes.Length);

        for (int i = start; i < end; i++)
        {
            size += sizes[i];
            if (i < end - 1) size += spacing;
        }

        return size;
    }
}

public readonly struct GridPosition
{
    public int Row { get; }
    public int Column { get; }
    public int RowSpan { get; }
    public int ColumnSpan { get; }

    public GridPosition(int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        Row = Math.Max(0, row);
        Column = Math.Max(0, column);
        RowSpan = Math.Max(1, rowSpan);
        ColumnSpan = Math.Max(1, columnSpan);
    }
}

public readonly struct GridLength
{
    public float Value { get; }
    public GridLengthType Type { get; }

    public static GridLength Auto => new(0, GridLengthType.Auto);
    public static GridLength Star => new(1, GridLengthType.Star);

    public GridLength(float value, GridLengthType type = GridLengthType.Pixel)
    {
        Value = value;
        Type = type;
    }

    public static GridLength Pixel(float value) => new(value, GridLengthType.Pixel);
    public static GridLength Stars(float value) => new(value, GridLengthType.Star);
}

public enum GridLengthType
{
    Pixel,
    Star,
    Auto
}

/// <summary>
/// Alignment settings for a child within a grid cell.
/// </summary>
public readonly struct GridCellAlignment
{
    public HorizontalAlignment Horizontal { get; }
    public VerticalAlignment Vertical { get; }

    public GridCellAlignment(HorizontalAlignment horizontal, VerticalAlignment vertical)
    {
        Horizontal = horizontal;
        Vertical = vertical;
    }

    public static GridCellAlignment Stretch => new(HorizontalAlignment.Stretch, VerticalAlignment.Stretch);
    public static GridCellAlignment Center => new(HorizontalAlignment.Center, VerticalAlignment.Center);
    public static GridCellAlignment TopLeft => new(HorizontalAlignment.Left, VerticalAlignment.Top);
    public static GridCellAlignment TopRight => new(HorizontalAlignment.Right, VerticalAlignment.Top);
    public static GridCellAlignment BottomLeft => new(HorizontalAlignment.Left, VerticalAlignment.Bottom);
    public static GridCellAlignment BottomRight => new(HorizontalAlignment.Right, VerticalAlignment.Bottom);
}
