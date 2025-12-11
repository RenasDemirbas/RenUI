using RenUI.Serialization;

namespace RenUI.Editor.Services;

public class ExportService
{
    public void ExportToJson(UILayoutData layout, string filePath)
    {
        var json = UISerializer.Serialize(layout);
        File.WriteAllText(filePath, json);
    }

    public async Task ExportToJsonAsync(UILayoutData layout, string filePath)
    {
        var json = UISerializer.Serialize(layout);
        await File.WriteAllTextAsync(filePath, json);
    }

    public string GenerateCodeSnippet(UILayoutData layout)
    {
        var sb = new System.Text.StringBuilder();
        
        sb.AppendLine("// Auto-generated UI Layout Code");
        sb.AppendLine("// Layout: " + layout.Name);
        sb.AppendLine();
        sb.AppendLine("using RenUI;");
        sb.AppendLine("using RenUI.Serialization;");
        sb.AppendLine();
        sb.AppendLine("// Load the layout in your scene:");
        sb.AppendLine($"var layout = UI.Layouts.LoadLayout(\"{layout.Name}\", \"path/to/{layout.Name}.json\");");
        sb.AppendLine($"var root = UI.Layouts.Build(\"{layout.Name}\");");
        sb.AppendLine();
        sb.AppendLine("// Or apply to existing container:");
        sb.AppendLine($"UI.Layouts.ApplyLayout(\"{layout.Name}\", existingContainer);");
        sb.AppendLine();
        sb.AppendLine("// Access elements by ID:");

        foreach (var element in layout.Elements)
        {
            GenerateElementAccess(sb, layout.Name, element);
        }

        return sb.ToString();
    }

    private void GenerateElementAccess(System.Text.StringBuilder sb, string layoutName, UIElementData element)
    {
        var typeName = element.Type;
        var varName = ToCamelCase(element.Name);
        
        sb.AppendLine($"var {varName} = UI.Layouts.GetElement<{typeName}>(\"{layoutName}\", \"{element.Id}\");");

        foreach (var child in element.Children)
        {
            GenerateElementAccess(sb, layoutName, child);
        }
    }

    private string ToCamelCase(string name)
    {
        if (string.IsNullOrEmpty(name)) return name;
        
        var result = name.Replace(" ", "").Replace("_", "");
        if (result.Length > 0)
        {
            result = char.ToLower(result[0]) + result[1..];
        }
        return result;
    }

    public string GenerateIntegrationGuide(UILayoutData layout)
    {
        return $@"
# {layout.Name} Entegrasyon Kılavuzu

## 1. JSON Dosyasını Projeye Ekle

`{layout.Name}.json` dosyasını projenizin Content klasörüne kopyalayın.

## 2. Sahnenizde Layout'u Yükleyin

```csharp
public class MyScene : SceneBase
{{
    public MyScene() : base(""MyScene"") {{ }}

    protected override void OnLoadContent()
    {{
        // Layout dosyasını yükle
        UI.Layouts.LoadLayout(""{layout.Name}"", ""Content/{layout.Name}.json"");
        
        // UI'ı oluştur ve root container'a ekle
        var uiRoot = UI.Layouts.Build(""{layout.Name}"");
        if (uiRoot != null)
        {{
            foreach (var child in uiRoot.Children.ToList())
            {{
                AddElement(child);
            }}
        }}
        
        // Elemanlara erişim
        SetupEventHandlers();
    }}

    private void SetupEventHandlers()
    {{
{GenerateEventHandlerExamples(layout)}
    }}
}}
```

## 3. Alternatif: Mevcut Container'a Uygula

```csharp
// Kod ile oluşturduğunuz UI'a editör ayarlarını uygulayın
UI.Layouts.ApplyLayout(""{layout.Name}"", myExistingContainer);
```

## Resource ID'leri

### Textures
{GenerateResourceList(layout.Resources.Textures)}

### Fonts
{GenerateResourceList(layout.Resources.Fonts)}

## Element ID'leri
{GenerateElementIdList(layout.Elements)}
";
    }

    private string GenerateEventHandlerExamples(UILayoutData layout)
    {
        var sb = new System.Text.StringBuilder();
        
        foreach (var element in layout.Elements)
        {
            GenerateEventHandlerForElement(sb, layout.Name, element);
        }

        return sb.ToString();
    }

    private void GenerateEventHandlerForElement(System.Text.StringBuilder sb, string layoutName, UIElementData element)
    {
        var varName = ToCamelCase(element.Name);
        
        switch (element.Type)
        {
            case "Button":
                sb.AppendLine($"        var {varName} = UI.Layouts.GetElement<Button>(\"{layoutName}\", \"{element.Id}\");");
                sb.AppendLine($"        {varName}?.OnClick.Subscribe(e => On{element.Name}Click());");
                sb.AppendLine();
                break;
            case "TextBox":
                sb.AppendLine($"        var {varName} = UI.Layouts.GetElement<TextBox>(\"{layoutName}\", \"{element.Id}\");");
                sb.AppendLine($"        {varName}?.OnTextChanged.Subscribe(e => On{element.Name}Changed({varName}?.Text));");
                sb.AppendLine();
                break;
            case "CheckBox":
                sb.AppendLine($"        var {varName} = UI.Layouts.GetElement<CheckBox>(\"{layoutName}\", \"{element.Id}\");");
                sb.AppendLine($"        {varName}?.OnCheckedChanged.Subscribe(e => On{element.Name}Changed(e.IsChecked));");
                sb.AppendLine();
                break;
            case "Slider":
                sb.AppendLine($"        var {varName} = UI.Layouts.GetElement<Slider>(\"{layoutName}\", \"{element.Id}\");");
                sb.AppendLine($"        {varName}?.OnValueChanged.Subscribe(e => On{element.Name}Changed(e.Value));");
                sb.AppendLine();
                break;
        }

        foreach (var child in element.Children)
        {
            GenerateEventHandlerForElement(sb, layoutName, child);
        }
    }

    private string GenerateResourceList(Dictionary<string, string> resources)
    {
        if (resources.Count == 0) return "- (Yok)";
        
        var sb = new System.Text.StringBuilder();
        foreach (var (id, path) in resources)
        {
            sb.AppendLine($"- `{id}`: `{path}`");
        }
        return sb.ToString();
    }

    private string GenerateElementIdList(List<UIElementData> elements, int indent = 0)
    {
        var sb = new System.Text.StringBuilder();
        var prefix = new string(' ', indent * 2);
        
        foreach (var element in elements)
        {
            sb.AppendLine($"{prefix}- **{element.Name}** (`{element.Id}`) - {element.Type}");
            if (element.Children.Count > 0)
            {
                sb.Append(GenerateElementIdList(element.Children, indent + 1));
            }
        }
        
        return sb.ToString();
    }
}
