using System.Text.Json.Serialization;

namespace RenUI.Serialization;

public class UILayoutData
{
    [JsonPropertyName("version")]
    public string Version { get; set; } = "1.0";

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [JsonPropertyName("width")]
    public int Width { get; set; } = 1920;

    [JsonPropertyName("height")]
    public int Height { get; set; } = 1080;

    [JsonPropertyName("resources")]
    public ResourcesData Resources { get; set; } = new();

    [JsonPropertyName("elements")]
    public List<UIElementData> Elements { get; set; } = new();

    [JsonPropertyName("metadata")]
    public Dictionary<string, string> Metadata { get; set; } = new();
}

public class ResourcesData
{
    [JsonPropertyName("textures")]
    public Dictionary<string, string> Textures { get; set; } = new();

    [JsonPropertyName("fonts")]
    public Dictionary<string, string> Fonts { get; set; } = new();

    [JsonPropertyName("sounds")]
    public Dictionary<string, string> Sounds { get; set; } = new();
}
