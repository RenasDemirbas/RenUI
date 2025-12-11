using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Interfaces;
using RenUI.Layout;

namespace RenUI.Elements.Base;

public class Container : UIElement, IContainer
{
    private readonly List<IUIElement> _children = new();
    private ILayoutStrategy? _layoutStrategy;

    public IReadOnlyList<IUIElement> Children => _children.AsReadOnly();

    public ILayoutStrategy? LayoutStrategy
    {
        get => _layoutStrategy;
        set
        {
            _layoutStrategy = value;
            InvalidateLayout();
        }
    }

    public void AddChild(IUIElement child)
    {
        if (child == null) throw new ArgumentNullException(nameof(child));
        if (_children.Contains(child)) return;

        child.Parent = this;
        _children.Add(child);
        InvalidateLayout();
    }

    public void RemoveChild(IUIElement child)
    {
        if (child == null) return;

        if (_children.Remove(child))
        {
            child.Parent = null;
            InvalidateLayout();
        }
    }

    public void ClearChildren()
    {
        foreach (var child in _children)
        {
            child.Parent = null;
        }
        _children.Clear();
        InvalidateLayout();
    }

    public IUIElement? GetChildByName(string name)
    {
        return _children.FirstOrDefault(c => c.Name == name);
    }

    public T? GetChild<T>(string name) where T : class, IUIElement
    {
        return GetChildByName(name) as T;
    }

    public override void Initialize()
    {
        base.Initialize();
        foreach (var child in _children)
        {
            child.Initialize();
        }
    }

    public override void Update(GameTime gameTime)
    {
        if (!IsEnabled) return;

        base.Update(gameTime);

        if (IsDirty)
        {
            PerformLayout();
        }

        foreach (var child in _children)
        {
            if (child.IsEnabled)
            {
                child.Update(gameTime);
            }
        }
    }

    public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
    {
        if (!IsVisible) return;

        DrawSelf(spriteBatch, gameTime);
        DrawChildren(spriteBatch, gameTime);
    }

    protected virtual void DrawSelf(SpriteBatch spriteBatch, GameTime gameTime) { }

    protected virtual void DrawChildren(SpriteBatch spriteBatch, GameTime gameTime)
    {
        var sortedChildren = _children
            .Where(c => c.IsVisible)
            .OrderBy(c => c.DrawOrder);

        foreach (var child in sortedChildren)
        {
            child.Draw(spriteBatch, gameTime);
        }
    }

    public override bool HandleMouseInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        for (int i = _children.Count - 1; i >= 0; i--)
        {
            if (_children[i] is IInputHandler handler && handler.HandleMouseInput(inputState))
            {
                return true;
            }
        }

        return base.HandleMouseInput(inputState);
    }

    public override bool HandleKeyboardInput(IInputState inputState)
    {
        if (!IsEnabled || !IsVisible) return false;

        foreach (var child in _children)
        {
            if (child is IInputHandler handler && child.IsFocused && handler.HandleKeyboardInput(inputState))
            {
                return true;
            }
        }

        return base.HandleKeyboardInput(inputState);
    }

    public void InvalidateLayout()
    {
        MarkDirty();
    }

    protected virtual void PerformLayout()
    {
        _layoutStrategy?.ApplyLayout(this);
    }

    protected override void OnBoundsChanged()
    {
        base.OnBoundsChanged();
        InvalidateLayout();
    }
}
