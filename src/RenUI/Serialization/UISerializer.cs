using System.Text.Json;
using System.Text.Json.Serialization;
using RenUI.Core.Interfaces;
using RenUI.Core.Primitives;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Serialization;

public static class UISerializer
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    public static string Serialize(UILayoutData layout)
    {
        return JsonSerializer.Serialize(layout, JsonOptions);
    }

    public static UILayoutData? Deserialize(string json)
    {
        return JsonSerializer.Deserialize<UILayoutData>(json, JsonOptions);
    }

    public static async Task SaveAsync(UILayoutData layout, string filePath)
    {
        var json = Serialize(layout);
        await File.WriteAllTextAsync(filePath, json);
    }

    public static async Task<UILayoutData?> LoadAsync(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = await File.ReadAllTextAsync(filePath);
        return Deserialize(json);
    }

    public static UILayoutData Save(string filePath, UILayoutData layout)
    {
        var json = Serialize(layout);
        File.WriteAllText(filePath, json);
        return layout;
    }

    public static UILayoutData? Load(string filePath)
    {
        if (!File.Exists(filePath)) return null;
        var json = File.ReadAllText(filePath);
        return Deserialize(json);
    }

    public static UIElementData ToData(UIElement element)
    {
        var data = new UIElementData
        {
            Id = element.Id,
            Type = element.GetType().Name,
            Name = element.Name,
            TextureId = element.TextureId,
            FontId = element.FontId,
            X = element.Bounds.X,
            Y = element.Bounds.Y,
            Width = element.Bounds.Width,
            Height = element.Bounds.Height,
            IsEnabled = element.IsEnabled,
            IsVisible = element.IsVisible,
            DrawOrder = element.DrawOrder,
            Anchor = element.Anchor.ToString(),
            Margin = new SpacingData
            {
                Left = element.Margin.Left,
                Top = element.Margin.Top,
                Right = element.Margin.Right,
                Bottom = element.Margin.Bottom
            },
            Padding = new SpacingData
            {
                Left = element.Padding.Left,
                Top = element.Padding.Top,
                Right = element.Padding.Right,
                Bottom = element.Padding.Bottom
            }
        };

        ExtractTypeSpecificProperties(element, data);

        if (element is IContainer container)
        {
            foreach (var child in container.Children)
            {
                if (child is UIElement childElement)
                {
                    data.Children.Add(ToData(childElement));
                }
            }
        }

        return data;
    }

    private static void ExtractTypeSpecificProperties(UIElement element, UIElementData data)
    {
        switch (element)
        {
            case Button button:
                data.Properties["text"] = button.Text;
                data.Properties["backgroundColor"] = ColorToHex(button.BackgroundColor);
                data.Properties["hoverColor"] = ColorToHex(button.HoverColor);
                data.Properties["pressedColor"] = ColorToHex(button.PressedColor);
                data.Properties["textColor"] = ColorToHex(button.TextColor);
                data.Properties["borderThickness"] = button.BorderThickness;
                data.Properties["cornerRadius"] = button.CornerRadius;
                break;

            case Label label:
                data.Properties["text"] = label.Text;
                data.Properties["textColor"] = ColorToHex(label.TextColor);
                data.Properties["horizontalAlignment"] = label.HorizontalAlignment.ToString();
                data.Properties["verticalAlignment"] = label.VerticalAlignment.ToString();
                data.Properties["autoSize"] = label.AutoSize;
                break;

            case TextBox textBox:
                data.Properties["text"] = textBox.Text;
                data.Properties["placeholder"] = textBox.Placeholder;
                data.Properties["textColor"] = ColorToHex(textBox.TextColor);
                data.Properties["backgroundColor"] = ColorToHex(textBox.BackgroundColor);
                data.Properties["maxLength"] = textBox.MaxLength;
                data.Properties["isReadOnly"] = textBox.IsReadOnly;
                data.Properties["isPassword"] = textBox.IsPassword;
                break;

            case CheckBox checkBox:
                data.Properties["text"] = checkBox.Text;
                data.Properties["isChecked"] = checkBox.IsChecked;
                data.Properties["textColor"] = ColorToHex(checkBox.TextColor);
                data.Properties["checkColor"] = ColorToHex(checkBox.CheckColor);
                data.Properties["boxSize"] = checkBox.BoxSize;
                break;

            case Slider slider:
                data.Properties["value"] = slider.Value;
                data.Properties["minValue"] = slider.MinValue;
                data.Properties["maxValue"] = slider.MaxValue;
                data.Properties["step"] = slider.Step;
                data.Properties["orientation"] = slider.Orientation.ToString();
                data.Properties["trackColor"] = ColorToHex(slider.TrackColor);
                data.Properties["trackFillColor"] = ColorToHex(slider.TrackFillColor);
                data.Properties["thumbColor"] = ColorToHex(slider.ThumbColor);
                break;

            case Panel panel:
                data.Properties["backgroundColor"] = ColorToHex(panel.BackgroundColor);
                data.Properties["borderColor"] = ColorToHex(panel.BorderColor);
                data.Properties["borderThickness"] = panel.BorderThickness;
                data.Properties["cornerRadius"] = panel.CornerRadius;
                break;
        }
    }

    private static string ColorToHex(Microsoft.Xna.Framework.Color color)
    {
        return $"#{color.R:X2}{color.G:X2}{color.B:X2}{color.A:X2}";
    }
}
