using System.Text.Json.Serialization;

namespace RenUI.Serialization;

public class UIElementData
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("textureId")]
    public string? TextureId { get; set; }

    [JsonPropertyName("fontId")]
    public string? FontId { get; set; }

    [JsonPropertyName("x")]
    public int X { get; set; }

    [JsonPropertyName("y")]
    public int Y { get; set; }

    [JsonPropertyName("width")]
    public int Width { get; set; }

    [JsonPropertyName("height")]
    public int Height { get; set; }

    [JsonPropertyName("scaleX")]
    public float ScaleX { get; set; } = 1f;

    [JsonPropertyName("scaleY")]
    public float ScaleY { get; set; } = 1f;

    [JsonPropertyName("rotation")]
    public float Rotation { get; set; }

    [JsonPropertyName("opacity")]
    public float Opacity { get; set; } = 1f;

    [JsonPropertyName("isEnabled")]
    public bool IsEnabled { get; set; } = true;

    [JsonPropertyName("isVisible")]
    public bool IsVisible { get; set; } = true;

    [JsonPropertyName("drawOrder")]
    public int DrawOrder { get; set; }

    [JsonPropertyName("anchor")]
    public string Anchor { get; set; } = "TopLeft";

    [JsonPropertyName("margin")]
    public SpacingData Margin { get; set; } = new();

    [JsonPropertyName("padding")]
    public SpacingData Padding { get; set; } = new();

    [JsonPropertyName("properties")]
    public Dictionary<string, object> Properties { get; set; } = new();

    [JsonPropertyName("children")]
    public List<UIElementData> Children { get; set; } = new();
}

public class SpacingData
{
    [JsonPropertyName("left")]
    public int Left { get; set; }

    [JsonPropertyName("top")]
    public int Top { get; set; }

    [JsonPropertyName("right")]
    public int Right { get; set; }

    [JsonPropertyName("bottom")]
    public int Bottom { get; set; }
}
