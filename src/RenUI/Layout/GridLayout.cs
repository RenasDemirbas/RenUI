using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

public class GridLayout : ILayoutStrategy
{
    private readonly List<GridLength> _columnDefinitions = new();
    private readonly List<GridLength> _rowDefinitions = new();
    private readonly Dictionary<IUIElement, GridPosition> _childPositions = new();

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

    public void SetPosition(IUIElement element, int row, int column, int rowSpan = 1, int columnSpan = 1)
    {
        _childPositions[element] = new GridPosition(row, column, rowSpan, columnSpan);
    }

    public GridPosition? GetPosition(IUIElement element)
    {
        return _childPositions.TryGetValue(element, out var pos) ? pos : null;
    }

    public void ApplyLayout(IContainer container)
    {
        if (container is not UIElement element) return;
        if (_columnDefinitions.Count == 0 || _rowDefinitions.Count == 0) return;

        var bounds = element.Bounds;
        var padding = element.Padding;
        var availableWidth = bounds.Width - padding.Horizontal - ((_columnDefinitions.Count - 1) * ColumnSpacing);
        var availableHeight = bounds.Height - padding.Vertical - ((_rowDefinitions.Count - 1) * RowSpacing);

        var columnWidths = CalculateSizes(_columnDefinitions, availableWidth);
        var rowHeights = CalculateSizes(_rowDefinitions, availableHeight);

        var columnOffsets = CalculateOffsets(columnWidths, padding.Left, ColumnSpacing);
        var rowOffsets = CalculateOffsets(rowHeights, padding.Top, RowSpacing);

        foreach (var child in container.Children)
        {
            if (child is not UIElement childElement) continue;
            if (!childElement.IsVisible) continue;

            var position = GetPosition(child) ?? new GridPosition(0, 0);
            
            if (position.Row >= rowOffsets.Length || position.Column >= columnOffsets.Length)
                continue;

            int x = columnOffsets[position.Column];
            int y = rowOffsets[position.Row];
            int width = CalculateSpannedSize(columnWidths, position.Column, position.ColumnSpan, ColumnSpacing);
            int height = CalculateSpannedSize(rowHeights, position.Row, position.RowSpan, RowSpacing);

            var margin = childElement.Margin;
            childElement.Bounds = new Rectangle(
                x + margin.Left,
                y + margin.Top,
                width - margin.Horizontal,
                height - margin.Vertical);
        }
    }

    private int[] CalculateSizes(List<GridLength> definitions, int availableSize)
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
            else if (def.Type == GridLengthType.Auto)
            {
                sizes[i] = 0; // Auto sizing requires measuring children
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
