using RenUI.Elements.Base;
using RenUI.Serialization;

namespace RenUI.Editor.Services;

public class EditorLayoutService
{
    private UILayoutData _currentLayout;
    private string? _currentFilePath;
    private bool _hasUnsavedChanges;

    public UILayoutData CurrentLayout => _currentLayout;
    public string? CurrentFilePath => _currentFilePath;
    public bool HasUnsavedChanges => _hasUnsavedChanges;

    public event Action<UILayoutData>? LayoutChanged;
    public event Action<UIElement>? ElementSelected;
    public event Action<UIElement>? ElementModified;

    public EditorLayoutService()
    {
        _currentLayout = CreateNewLayout();
    }

    public UILayoutData CreateNewLayout()
    {
        _currentLayout = new UILayoutData
        {
            Name = "New Layout",
            Version = "1.0",
            Width = 1920,
            Height = 1080,
            Resources = new ResourcesData()
        };
        _currentFilePath = null;
        _hasUnsavedChanges = false;
        LayoutChanged?.Invoke(_currentLayout);
        return _currentLayout;
    }

    public async Task<bool> LoadLayoutAsync(string filePath)
    {
        var layout = await UISerializer.LoadAsync(filePath);
        if (layout == null) return false;

        _currentLayout = layout;
        _currentFilePath = filePath;
        _hasUnsavedChanges = false;
        LayoutChanged?.Invoke(_currentLayout);
        return true;
    }

    public bool LoadLayout(string filePath)
    {
        var layout = UISerializer.Load(filePath);
        if (layout == null) return false;

        _currentLayout = layout;
        _currentFilePath = filePath;
        _hasUnsavedChanges = false;
        LayoutChanged?.Invoke(_currentLayout);
        return true;
    }

    public async Task SaveLayoutAsync(string? filePath = null)
    {
        var path = filePath ?? _currentFilePath;
        if (string.IsNullOrEmpty(path)) return;

        await UISerializer.SaveAsync(_currentLayout, path);
        _currentFilePath = path;
        _hasUnsavedChanges = false;
    }

    public void SaveLayout(string? filePath = null)
    {
        var path = filePath ?? _currentFilePath;
        if (string.IsNullOrEmpty(path)) return;

        UISerializer.Save(path, _currentLayout);
        _currentFilePath = path;
        _hasUnsavedChanges = false;
    }

    public string GenerateUniqueId(string prefix = "element")
    {
        return $"{prefix}_{Guid.NewGuid():N}"[..24];
    }

    public UIElementData AddElement(string type, string? parentId = null)
    {
        var elementData = new UIElementData
        {
            Id = GenerateUniqueId(type.ToLower()),
            Type = type,
            Name = $"{type}_{_currentLayout.Elements.Count + 1}",
            X = 100,
            Y = 100,
            Width = GetDefaultWidth(type),
            Height = GetDefaultHeight(type)
        };

        SetDefaultProperties(elementData);

        if (string.IsNullOrEmpty(parentId))
        {
            _currentLayout.Elements.Add(elementData);
        }
        else
        {
            var parent = FindElement(parentId, _currentLayout.Elements);
            parent?.Children.Add(elementData);
        }

        _hasUnsavedChanges = true;
        return elementData;
    }

    public void RemoveElement(string id)
    {
        RemoveElementRecursive(id, _currentLayout.Elements);
        _hasUnsavedChanges = true;
    }

    public UIElementData? FindElement(string id)
    {
        return FindElement(id, _currentLayout.Elements);
    }

    public void UpdateElement(UIElementData elementData)
    {
        var existing = FindElement(elementData.Id);
        if (existing == null) return;

        existing.Name = elementData.Name;
        existing.X = elementData.X;
        existing.Y = elementData.Y;
        existing.Width = elementData.Width;
        existing.Height = elementData.Height;
        existing.ScaleX = elementData.ScaleX;
        existing.ScaleY = elementData.ScaleY;
        existing.Rotation = elementData.Rotation;
        existing.Opacity = elementData.Opacity;
        existing.IsEnabled = elementData.IsEnabled;
        existing.IsVisible = elementData.IsVisible;
        existing.TextureId = elementData.TextureId;
        existing.FontId = elementData.FontId;
        existing.Properties = elementData.Properties;

        _hasUnsavedChanges = true;
    }

    public void UpdateElementFromUI(UIElement element)
    {
        var data = UISerializer.ToData(element);
        UpdateElement(data);
    }

    public void AddTexture(string id, string path)
    {
        _currentLayout.Resources.Textures[id] = path;
        _hasUnsavedChanges = true;
    }

    public void AddFont(string id, string path)
    {
        _currentLayout.Resources.Fonts[id] = path;
        _hasUnsavedChanges = true;
    }

    public void RemoveTexture(string id)
    {
        _currentLayout.Resources.Textures.Remove(id);
        _hasUnsavedChanges = true;
    }

    public void RemoveFont(string id)
    {
        _currentLayout.Resources.Fonts.Remove(id);
        _hasUnsavedChanges = true;
    }

    private UIElementData? FindElement(string id, List<UIElementData> elements)
    {
        foreach (var element in elements)
        {
            if (element.Id == id) return element;
            
            var found = FindElement(id, element.Children);
            if (found != null) return found;
        }
        return null;
    }

    private bool RemoveElementRecursive(string id, List<UIElementData> elements)
    {
        for (int i = elements.Count - 1; i >= 0; i--)
        {
            if (elements[i].Id == id)
            {
                elements.RemoveAt(i);
                return true;
            }

            if (RemoveElementRecursive(id, elements[i].Children))
                return true;
        }
        return false;
    }

    private int GetDefaultWidth(string type) => type switch
    {
        "Button" => 120,
        "Label" => 100,
        "TextBox" => 200,
        "CheckBox" => 150,
        "Slider" => 200,
        "Panel" => 300,
        _ => 100
    };

    private int GetDefaultHeight(string type) => type switch
    {
        "Button" => 40,
        "Label" => 24,
        "TextBox" => 32,
        "CheckBox" => 24,
        "Slider" => 20,
        "Panel" => 200,
        _ => 100
    };

    private void SetDefaultProperties(UIElementData data)
    {
        switch (data.Type)
        {
            case "Button":
                data.Properties["text"] = "Button";
                data.Properties["backgroundColor"] = "#464646FF";
                data.Properties["hoverColor"] = "#5A5A5AFF";
                data.Properties["textColor"] = "#FFFFFFFF";
                data.Properties["cornerRadius"] = 4;
                break;

            case "Label":
                data.Properties["text"] = "Label";
                data.Properties["textColor"] = "#FFFFFFFF";
                data.Properties["horizontalAlignment"] = "Start";
                break;

            case "TextBox":
                data.Properties["placeholder"] = "Enter text...";
                data.Properties["backgroundColor"] = "#1E1E1EFF";
                data.Properties["textColor"] = "#FFFFFFFF";
                break;

            case "CheckBox":
                data.Properties["text"] = "CheckBox";
                data.Properties["isChecked"] = false;
                data.Properties["textColor"] = "#FFFFFFFF";
                break;

            case "Slider":
                data.Properties["minValue"] = 0f;
                data.Properties["maxValue"] = 100f;
                data.Properties["value"] = 50f;
                data.Properties["step"] = 1f;
                break;

            case "Panel":
                data.Properties["backgroundColor"] = "#282828FF";
                data.Properties["borderColor"] = "#3C3C3CFF";
                data.Properties["borderThickness"] = 1;
                break;
        }
    }
}
