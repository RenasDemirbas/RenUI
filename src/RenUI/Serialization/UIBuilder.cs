using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Core.Primitives;
using RenUI.Elements.Base;
using RenUI.Elements.Controls;

namespace RenUI.Serialization;

public class UIBuilder
{
    private readonly ContentManager _content;
    private readonly Dictionary<string, Texture2D> _textures = new();
    private readonly Dictionary<string, SpriteFont> _fonts = new();
    private readonly Dictionary<string, UIElement> _elements = new();

    public IReadOnlyDictionary<string, UIElement> Elements => _elements;

    public UIBuilder(ContentManager content)
    {
        _content = content;
    }

    public Container Build(UILayoutData layout)
    {
        _elements.Clear();
        LoadResources(layout.Resources);

        var root = new Container
        {
            Id = "root",
            Name = "Root",
            Bounds = new Rectangle(0, 0, layout.Width, layout.Height)
        };

        foreach (var elementData in layout.Elements)
        {
            var element = BuildElement(elementData);
            if (element != null)
            {
                root.AddChild(element);
            }
        }

        return root;
    }

    public void ApplyLayout(Container root, UILayoutData layout)
    {
        _elements.Clear();
        LoadResources(layout.Resources);

        foreach (var elementData in layout.Elements)
        {
            ApplyToExisting(root, elementData);
        }
    }

    private void ApplyToExisting(Container container, UIElementData data)
    {
        var element = container.GetChildByName(data.Name) as UIElement;
        if (element == null)
        {
            element = FindElementById(container, data.Id) as UIElement;
        }

        if (element != null)
        {
            ApplyDataToElement(element, data);
            _elements[data.Id] = element;

            if (element is Container childContainer && data.Children.Count > 0)
            {
                foreach (var childData in data.Children)
                {
                    ApplyToExisting(childContainer, childData);
                }
            }
        }
    }

    private Core.Interfaces.IUIElement? FindElementById(Container container, string id)
    {
        foreach (var child in container.Children)
        {
            if (child is UIElement element)
            {
                if (element.Id == id) return element;
                
                if (element is Container childContainer)
                {
                    var found = FindElementById(childContainer, id);
                    if (found != null) return found;
                }
            }
        }
        return null;
    }

    private void LoadResources(ResourcesData resources)
    {
        _textures.Clear();
        _fonts.Clear();

        foreach (var (id, path) in resources.Textures)
        {
            try
            {
                _textures[id] = _content.Load<Texture2D>(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load texture '{id}': {ex.Message}");
            }
        }

        foreach (var (id, path) in resources.Fonts)
        {
            try
            {
                _fonts[id] = _content.Load<SpriteFont>(path);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Failed to load font '{id}': {ex.Message}");
            }
        }
    }

    private UIElement? BuildElement(UIElementData data)
    {
        UIElement? element = data.Type switch
        {
            "Panel" or "Container" => new Panel(),
            "Button" => new Button(),
            "Label" => new Label(),
            "TextBox" => new TextBox(),
            "CheckBox" => new CheckBox(),
            "Slider" => new Slider(),
            "Image" => new Image(),
            _ => null
        };

        if (element == null) return null;

        ApplyDataToElement(element, data);
        _elements[data.Id] = element;

        if (element is Container container)
        {
            foreach (var childData in data.Children)
            {
                var child = BuildElement(childData);
                if (child != null)
                {
                    container.AddChild(child);
                }
            }
        }

        return element;
    }

    private void ApplyDataToElement(UIElement element, UIElementData data)
    {
        element.Id = data.Id;
        element.Name = data.Name;
        element.TextureId = data.TextureId;
        element.FontId = data.FontId;
        element.Bounds = new Rectangle(data.X, data.Y, data.Width, data.Height);
        element.IsEnabled = data.IsEnabled;
        element.IsVisible = data.IsVisible;
        element.DrawOrder = data.DrawOrder;

        if (Enum.TryParse<Anchor>(data.Anchor, out var anchor))
        {
            element.Anchor = anchor;
        }

        element.Margin = new Margin(data.Margin.Left, data.Margin.Top, data.Margin.Right, data.Margin.Bottom);
        element.Padding = new Padding(data.Padding.Left, data.Padding.Top, data.Padding.Right, data.Padding.Bottom);

        if (!string.IsNullOrEmpty(data.FontId) && _fonts.TryGetValue(data.FontId, out var font))
        {
            SetFont(element, font);
        }

        if (!string.IsNullOrEmpty(data.TextureId) && _textures.TryGetValue(data.TextureId, out var texture))
        {
            SetTexture(element, texture);
        }

        ApplyTypeSpecificProperties(element, data);
    }

    private void SetFont(UIElement element, SpriteFont font)
    {
        switch (element)
        {
            case Button button: button.Font = font; break;
            case Label label: label.Font = font; break;
            case TextBox textBox: textBox.Font = font; break;
            case CheckBox checkBox: checkBox.Font = font; break;
        }
    }

    private void SetTexture(UIElement element, Texture2D texture)
    {
        switch (element)
        {
            case Image image: image.Texture = texture; break;
        }
    }

    private void ApplyTypeSpecificProperties(UIElement element, UIElementData data)
    {
        var props = data.Properties;

        switch (element)
        {
            case Button button:
                if (props.TryGetValue("text", out var btnText))
                    button.Text = btnText?.ToString() ?? "";
                if (props.TryGetValue("backgroundColor", out var btnBg))
                    button.BackgroundColor = HexToColor(btnBg?.ToString());
                if (props.TryGetValue("hoverColor", out var btnHover))
                    button.HoverColor = HexToColor(btnHover?.ToString());
                if (props.TryGetValue("pressedColor", out var btnPressed))
                    button.PressedColor = HexToColor(btnPressed?.ToString());
                if (props.TryGetValue("textColor", out var btnTextColor))
                    button.TextColor = HexToColor(btnTextColor?.ToString());
                if (props.TryGetValue("borderThickness", out var btnBorder))
                    button.BorderThickness = Convert.ToInt32(btnBorder);
                if (props.TryGetValue("cornerRadius", out var btnRadius))
                    button.CornerRadius = Convert.ToInt32(btnRadius);
                break;

            case Label label:
                if (props.TryGetValue("text", out var lblText))
                    label.Text = lblText?.ToString() ?? "";
                if (props.TryGetValue("textColor", out var lblColor))
                    label.TextColor = HexToColor(lblColor?.ToString());
                if (props.TryGetValue("horizontalAlignment", out var lblHAlign) && 
                    Enum.TryParse<TextAlignment>(lblHAlign?.ToString(), out var hAlign))
                    label.HorizontalAlignment = hAlign;
                if (props.TryGetValue("verticalAlignment", out var lblVAlign) && 
                    Enum.TryParse<TextAlignment>(lblVAlign?.ToString(), out var vAlign))
                    label.VerticalAlignment = vAlign;
                if (props.TryGetValue("autoSize", out var lblAuto))
                    label.AutoSize = Convert.ToBoolean(lblAuto);
                break;

            case TextBox textBox:
                if (props.TryGetValue("text", out var tbText))
                    textBox.Text = tbText?.ToString() ?? "";
                if (props.TryGetValue("placeholder", out var tbPlaceholder))
                    textBox.Placeholder = tbPlaceholder?.ToString() ?? "";
                if (props.TryGetValue("textColor", out var tbTextColor))
                    textBox.TextColor = HexToColor(tbTextColor?.ToString());
                if (props.TryGetValue("backgroundColor", out var tbBg))
                    textBox.BackgroundColor = HexToColor(tbBg?.ToString());
                if (props.TryGetValue("maxLength", out var tbMax))
                    textBox.MaxLength = Convert.ToInt32(tbMax);
                if (props.TryGetValue("isReadOnly", out var tbReadOnly))
                    textBox.IsReadOnly = Convert.ToBoolean(tbReadOnly);
                if (props.TryGetValue("isPassword", out var tbPassword))
                    textBox.IsPassword = Convert.ToBoolean(tbPassword);
                break;

            case CheckBox checkBox:
                if (props.TryGetValue("text", out var cbText))
                    checkBox.Text = cbText?.ToString() ?? "";
                if (props.TryGetValue("isChecked", out var cbChecked))
                    checkBox.IsChecked = Convert.ToBoolean(cbChecked);
                if (props.TryGetValue("textColor", out var cbTextColor))
                    checkBox.TextColor = HexToColor(cbTextColor?.ToString());
                if (props.TryGetValue("checkColor", out var cbCheckColor))
                    checkBox.CheckColor = HexToColor(cbCheckColor?.ToString());
                if (props.TryGetValue("boxSize", out var cbBoxSize))
                    checkBox.BoxSize = Convert.ToInt32(cbBoxSize);
                break;

            case Slider slider:
                if (props.TryGetValue("minValue", out var slMin))
                    slider.MinValue = Convert.ToSingle(slMin);
                if (props.TryGetValue("maxValue", out var slMax))
                    slider.MaxValue = Convert.ToSingle(slMax);
                if (props.TryGetValue("value", out var slValue))
                    slider.Value = Convert.ToSingle(slValue);
                if (props.TryGetValue("step", out var slStep))
                    slider.Step = Convert.ToSingle(slStep);
                if (props.TryGetValue("orientation", out var slOrient) && 
                    Enum.TryParse<Orientation>(slOrient?.ToString(), out var orient))
                    slider.Orientation = orient;
                if (props.TryGetValue("trackColor", out var slTrack))
                    slider.TrackColor = HexToColor(slTrack?.ToString());
                if (props.TryGetValue("trackFillColor", out var slFill))
                    slider.TrackFillColor = HexToColor(slFill?.ToString());
                if (props.TryGetValue("thumbColor", out var slThumb))
                    slider.ThumbColor = HexToColor(slThumb?.ToString());
                break;

            case Panel panel:
                if (props.TryGetValue("backgroundColor", out var pnlBg))
                    panel.BackgroundColor = HexToColor(pnlBg?.ToString());
                if (props.TryGetValue("borderColor", out var pnlBorder))
                    panel.BorderColor = HexToColor(pnlBorder?.ToString());
                if (props.TryGetValue("borderThickness", out var pnlThickness))
                    panel.BorderThickness = Convert.ToInt32(pnlThickness);
                if (props.TryGetValue("cornerRadius", out var pnlRadius))
                    panel.CornerRadius = Convert.ToInt32(pnlRadius);
                break;

            case Image image:
                if (props.TryGetValue("tint", out var imgTint))
                    image.Tint = HexToColor(imgTint?.ToString());
                if (props.TryGetValue("rotation", out var imgRotation))
                    image.Rotation = Convert.ToSingle(imgRotation);
                if (props.TryGetValue("stretchMode", out var imgStretch) &&
                    Enum.TryParse<ImageStretchMode>(imgStretch?.ToString(), out var stretchMode))
                    image.StretchMode = stretchMode;
                if (props.TryGetValue("horizontalAlignment", out var imgHAlign) &&
                    Enum.TryParse<HorizontalAlignment>(imgHAlign?.ToString(), out var imgHorizontal))
                    image.HorizontalAlignment = imgHorizontal;
                if (props.TryGetValue("verticalAlignment", out var imgVAlign) &&
                    Enum.TryParse<VerticalAlignment>(imgVAlign?.ToString(), out var imgVertical))
                    image.VerticalAlignment = imgVertical;
                if (props.TryGetValue("preserveAspectRatio", out var imgPreserve))
                    image.PreserveAspectRatio = Convert.ToBoolean(imgPreserve);
                if (props.TryGetValue("autoSize", out var imgAutoSize))
                    image.AutoSize = Convert.ToBoolean(imgAutoSize);
                if (props.TryGetValue("sourceX", out var srcX) && props.TryGetValue("sourceY", out var srcY) &&
                    props.TryGetValue("sourceWidth", out var srcW) && props.TryGetValue("sourceHeight", out var srcH))
                    image.SourceRectangle = new Rectangle(
                        Convert.ToInt32(srcX), Convert.ToInt32(srcY),
                        Convert.ToInt32(srcW), Convert.ToInt32(srcH));
                break;
        }
    }

    private Color HexToColor(string? hex)
    {
        if (string.IsNullOrEmpty(hex)) return Color.White;
        
        hex = hex.TrimStart('#');
        
        if (hex.Length == 6)
        {
            return new Color(
                Convert.ToInt32(hex[..2], 16),
                Convert.ToInt32(hex[2..4], 16),
                Convert.ToInt32(hex[4..6], 16));
        }
        else if (hex.Length == 8)
        {
            return new Color(
                Convert.ToInt32(hex[..2], 16),
                Convert.ToInt32(hex[2..4], 16),
                Convert.ToInt32(hex[4..6], 16),
                Convert.ToInt32(hex[6..8], 16));
        }

        return Color.White;
    }

    public T? GetElement<T>(string id) where T : UIElement
    {
        return _elements.TryGetValue(id, out var element) ? element as T : null;
    }

    public Texture2D? GetTexture(string id)
    {
        return _textures.TryGetValue(id, out var texture) ? texture : null;
    }

    public SpriteFont? GetFont(string id)
    {
        return _fonts.TryGetValue(id, out var font) ? font : null;
    }
}
