using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RenUI.Styling;

public class Theme
{
    public string Name { get; set; } = "Default";
    
    // Primary colors
    public Color PrimaryColor { get; set; } = new Color(0, 120, 215);
    public Color SecondaryColor { get; set; } = new Color(100, 100, 100);
    public Color AccentColor { get; set; } = new Color(0, 153, 255);

    // Background colors
    public Color BackgroundColor { get; set; } = new Color(30, 30, 30);
    public Color SurfaceColor { get; set; } = new Color(45, 45, 45);
    public Color PanelColor { get; set; } = new Color(40, 40, 40);

    // Text colors
    public Color TextColor { get; set; } = Color.White;
    public Color TextSecondaryColor { get; set; } = new Color(180, 180, 180);
    public Color TextDisabledColor { get; set; } = new Color(100, 100, 100);

    // Border colors
    public Color BorderColor { get; set; } = new Color(60, 60, 60);
    public Color BorderFocusedColor { get; set; } = new Color(0, 120, 215);

    // Control colors
    public Color ControlBackground { get; set; } = new Color(50, 50, 50);
    public Color ControlHover { get; set; } = new Color(70, 70, 70);
    public Color ControlPressed { get; set; } = new Color(40, 40, 40);
    public Color ControlDisabled { get; set; } = new Color(35, 35, 35);

    // Selection
    public Color SelectionColor { get; set; } = new Color(0, 120, 215, 128);

    // Fonts
    public SpriteFont? DefaultFont { get; set; }
    public SpriteFont? HeadingFont { get; set; }
    public SpriteFont? SmallFont { get; set; }

    // Spacing
    public int DefaultSpacing { get; set; } = 8;
    public int DefaultPadding { get; set; } = 8;
    public int DefaultBorderThickness { get; set; } = 1;
    public int DefaultCornerRadius { get; set; } = 4;

    public static Theme Dark => new()
    {
        Name = "Dark",
        PrimaryColor = new Color(0, 120, 215),
        SecondaryColor = new Color(100, 100, 100),
        AccentColor = new Color(0, 153, 255),
        BackgroundColor = new Color(30, 30, 30),
        SurfaceColor = new Color(45, 45, 45),
        PanelColor = new Color(40, 40, 40),
        TextColor = Color.White,
        TextSecondaryColor = new Color(180, 180, 180),
        TextDisabledColor = new Color(100, 100, 100),
        BorderColor = new Color(60, 60, 60),
        BorderFocusedColor = new Color(0, 120, 215),
        ControlBackground = new Color(50, 50, 50),
        ControlHover = new Color(70, 70, 70),
        ControlPressed = new Color(40, 40, 40),
        ControlDisabled = new Color(35, 35, 35),
        SelectionColor = new Color(0, 120, 215, 128)
    };

    public static Theme Light => new()
    {
        Name = "Light",
        PrimaryColor = new Color(0, 120, 215),
        SecondaryColor = new Color(150, 150, 150),
        AccentColor = new Color(0, 102, 204),
        BackgroundColor = new Color(240, 240, 240),
        SurfaceColor = new Color(255, 255, 255),
        PanelColor = new Color(250, 250, 250),
        TextColor = new Color(30, 30, 30),
        TextSecondaryColor = new Color(100, 100, 100),
        TextDisabledColor = new Color(160, 160, 160),
        BorderColor = new Color(200, 200, 200),
        BorderFocusedColor = new Color(0, 120, 215),
        ControlBackground = new Color(255, 255, 255),
        ControlHover = new Color(230, 230, 230),
        ControlPressed = new Color(210, 210, 210),
        ControlDisabled = new Color(245, 245, 245),
        SelectionColor = new Color(0, 120, 215, 80)
    };
}
