using Microsoft.Xna.Framework.Content;
using RenUI.Elements.Base;

namespace RenUI.Serialization;

public sealed class UILayoutManager
{
    private static UILayoutManager? _instance;
    private static readonly object _lock = new();

    public static UILayoutManager Instance
    {
        get
        {
            if (_instance == null)
            {
                lock (_lock)
                {
                    _instance ??= new UILayoutManager();
                }
            }
            return _instance;
        }
    }

    private readonly Dictionary<string, UILayoutData> _layouts = new();
    private readonly Dictionary<string, UIBuilder> _builders = new();
    private ContentManager? _content;

    private UILayoutManager() { }

    public void Initialize(ContentManager content)
    {
        _content = content;
    }

    public void RegisterLayout(string name, UILayoutData layout)
    {
        _layouts[name] = layout;
    }

    public void UnregisterLayout(string name)
    {
        _layouts.Remove(name);
        _builders.Remove(name);
    }

    public UILayoutData? GetLayout(string name)
    {
        return _layouts.TryGetValue(name, out var layout) ? layout : null;
    }

    public async Task<UILayoutData?> LoadLayoutAsync(string name, string filePath)
    {
        var layout = await UISerializer.LoadAsync(filePath);
        if (layout != null)
        {
            layout.Name = name;
            _layouts[name] = layout;
        }
        return layout;
    }

    public UILayoutData? LoadLayout(string name, string filePath)
    {
        var layout = UISerializer.Load(filePath);
        if (layout != null)
        {
            layout.Name = name;
            _layouts[name] = layout;
        }
        return layout;
    }

    public async Task SaveLayoutAsync(string name, string filePath)
    {
        if (_layouts.TryGetValue(name, out var layout))
        {
            await UISerializer.SaveAsync(layout, filePath);
        }
    }

    public void SaveLayout(string name, string filePath)
    {
        if (_layouts.TryGetValue(name, out var layout))
        {
            UISerializer.Save(filePath, layout);
        }
    }

    public Container? Build(string layoutName)
    {
        if (_content == null)
            throw new InvalidOperationException("UILayoutManager not initialized");

        if (!_layouts.TryGetValue(layoutName, out var layout))
            return null;

        var builder = new UIBuilder(_content);
        _builders[layoutName] = builder;

        return builder.Build(layout);
    }

    public void ApplyLayout(string layoutName, Container root)
    {
        if (_content == null)
            throw new InvalidOperationException("UILayoutManager not initialized");

        if (!_layouts.TryGetValue(layoutName, out var layout))
            return;

        var builder = new UIBuilder(_content);
        _builders[layoutName] = builder;

        builder.ApplyLayout(root, layout);
    }

    public UIBuilder? GetBuilder(string layoutName)
    {
        return _builders.TryGetValue(layoutName, out var builder) ? builder : null;
    }

    public T? GetElement<T>(string layoutName, string elementId) where T : UIElement
    {
        return _builders.TryGetValue(layoutName, out var builder) 
            ? builder.GetElement<T>(elementId) 
            : null;
    }

    public IEnumerable<string> GetLayoutNames()
    {
        return _layouts.Keys;
    }

    public static void Reset()
    {
        lock (_lock)
        {
            _instance = null;
        }
    }
}
