using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

public interface ILayoutStrategy
{
    void ApplyLayout(IContainer container);
    
    /// <summary>
    /// Calculates the preferred size of the container based on its children.
    /// Used for Auto sizing in parent layouts.
    /// </summary>
    Point MeasureContent(IContainer container);
}

/// <summary>
/// Extension methods and utilities for layout calculations.
/// </summary>
public static class LayoutUtils
{
    /// <summary>
    /// Measures a child element's preferred size, respecting MinSize and MaxSize constraints.
    /// </summary>
    public static Point MeasureChild(IUIElement child)
    {
        if (child is not UIElement element) return Point.Zero;
        
        int width = element.Width;
        int height = element.Height;
        
        // Apply MinSize constraints
        width = Math.Max(width, element.MinSize.Width);
        height = Math.Max(height, element.MinSize.Height);
        
        // Apply MaxSize constraints
        width = Math.Min(width, element.MaxSize.Width);
        height = Math.Min(height, element.MaxSize.Height);
        
        return new Point(width, height);
    }
    
    /// <summary>
    /// Gets the total margin size for an element.
    /// </summary>
    public static Point GetMarginSize(IUIElement child)
    {
        if (child is not UIElement element) return Point.Zero;
        return new Point(element.Margin.Horizontal, element.Margin.Vertical);
    }
    
    /// <summary>
    /// Clamps a value between min and max.
    /// </summary>
    public static int Clamp(int value, int min, int max)
    {
        return Math.Max(min, Math.Min(max, value));
    }
}
