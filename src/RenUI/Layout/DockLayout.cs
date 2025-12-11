using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

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
}

public enum Dock
{
    Left,
    Top,
    Right,
    Bottom
}
