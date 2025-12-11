using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI.Elements.Controls;
using RenUI.Layout;
using RenUI.Scenes;
using RenUI.Styling;

namespace RenUI.Editor.Scenes;

public class EditorScene : SceneBase
{
    private SpriteFont? _font;

    public EditorScene() : base("Editor") { }

    protected override void OnLoadContent()
    {
        // Load font - you'll need to add a font to your Content project
        // _font = Content?.Load<SpriteFont>("DefaultFont");

        var theme = ThemeManager.Instance.CurrentTheme;

        // Main layout with DockLayout
        var dockLayout = new DockLayout { LastChildFill = true };
        Root.LayoutStrategy = dockLayout;

        // Top toolbar
        var toolbar = CreateToolbar();
        dockLayout.SetDock(toolbar, Dock.Top);
        AddElement(toolbar);

        // Left panel - hierarchy
        var leftPanel = CreateLeftPanel();
        dockLayout.SetDock(leftPanel, Dock.Left);
        AddElement(leftPanel);

        // Right panel - properties
        var rightPanel = CreateRightPanel();
        dockLayout.SetDock(rightPanel, Dock.Right);
        AddElement(rightPanel);

        // Bottom panel - console/output
        var bottomPanel = CreateBottomPanel();
        dockLayout.SetDock(bottomPanel, Dock.Bottom);
        AddElement(bottomPanel);

        // Center - viewport/canvas
        var viewport = CreateViewport();
        AddElement(viewport);
    }

    private Panel CreateToolbar()
    {
        var toolbar = new Panel
        {
            Name = "Toolbar",
            Height = 40,
            BackgroundColor = new Color(35, 35, 35),
            BorderThickness = 0
        };

        var layout = new StackLayout(Orientation.Horizontal, 4);
        toolbar.LayoutStrategy = layout;
        toolbar.Padding = new Core.Primitives.Padding(8, 4);

        var fileButton = new Button("File")
        {
            Name = "FileButton",
            Width = 60,
            Height = 28,
            Font = _font
        };
        fileButton.OnClick.Subscribe(e => OnFileClick());
        toolbar.AddChild(fileButton);

        var editButton = new Button("Edit")
        {
            Name = "EditButton",
            Width = 60,
            Height = 28,
            Font = _font
        };
        toolbar.AddChild(editButton);

        var viewButton = new Button("View")
        {
            Name = "ViewButton",
            Width = 60,
            Height = 28,
            Font = _font
        };
        toolbar.AddChild(viewButton);

        return toolbar;
    }

    private Panel CreateLeftPanel()
    {
        var panel = new Panel
        {
            Name = "LeftPanel",
            Width = 250,
            BackgroundColor = new Color(40, 40, 40),
            BorderColor = new Color(50, 50, 50)
        };

        var layout = new StackLayout(Orientation.Vertical, 4);
        panel.LayoutStrategy = layout;
        panel.Padding = new Core.Primitives.Padding(8);

        var hierarchyLabel = new Label("Hierarchy")
        {
            Name = "HierarchyLabel",
            Font = _font,
            TextColor = Color.White,
            Height = 24
        };
        panel.AddChild(hierarchyLabel);

        return panel;
    }

    private Panel CreateRightPanel()
    {
        var panel = new Panel
        {
            Name = "RightPanel",
            Width = 300,
            BackgroundColor = new Color(40, 40, 40),
            BorderColor = new Color(50, 50, 50)
        };

        var layout = new StackLayout(Orientation.Vertical, 4);
        panel.LayoutStrategy = layout;
        panel.Padding = new Core.Primitives.Padding(8);

        var propertiesLabel = new Label("Properties")
        {
            Name = "PropertiesLabel",
            Font = _font,
            TextColor = Color.White,
            Height = 24
        };
        panel.AddChild(propertiesLabel);

        // Example property controls
        var nameLabel = new Label("Name:")
        {
            Font = _font,
            TextColor = new Color(180, 180, 180),
            Height = 20
        };
        panel.AddChild(nameLabel);

        var nameInput = new TextBox
        {
            Name = "NameInput",
            Font = _font,
            Height = 28,
            Placeholder = "Element name"
        };
        panel.AddChild(nameInput);

        var enabledCheck = new CheckBox("Enabled", true)
        {
            Font = _font,
            Height = 24
        };
        panel.AddChild(enabledCheck);

        var visibleCheck = new CheckBox("Visible", true)
        {
            Font = _font,
            Height = 24
        };
        panel.AddChild(visibleCheck);

        var opacityLabel = new Label("Opacity:")
        {
            Font = _font,
            TextColor = new Color(180, 180, 180),
            Height = 20
        };
        panel.AddChild(opacityLabel);

        var opacitySlider = new Slider
        {
            Name = "OpacitySlider",
            MinValue = 0,
            MaxValue = 100,
            Value = 100,
            Height = 20
        };
        panel.AddChild(opacitySlider);

        return panel;
    }

    private Panel CreateBottomPanel()
    {
        var panel = new Panel
        {
            Name = "BottomPanel",
            Height = 150,
            BackgroundColor = new Color(35, 35, 35),
            BorderColor = new Color(50, 50, 50)
        };

        var layout = new StackLayout(Orientation.Vertical, 4);
        panel.LayoutStrategy = layout;
        panel.Padding = new Core.Primitives.Padding(8);

        var consoleLabel = new Label("Console")
        {
            Name = "ConsoleLabel",
            Font = _font,
            TextColor = Color.White,
            Height = 24
        };
        panel.AddChild(consoleLabel);

        return panel;
    }

    private Panel CreateViewport()
    {
        var panel = new Panel
        {
            Name = "Viewport",
            BackgroundColor = new Color(25, 25, 25),
            BorderThickness = 0
        };

        var centerLabel = new Label("Viewport - Drop UI elements here")
        {
            Font = _font,
            TextColor = new Color(100, 100, 100),
            HorizontalAlignment = TextAlignment.Center,
            VerticalAlignment = TextAlignment.Center
        };
        panel.AddChild(centerLabel);

        return panel;
    }

    private void OnFileClick()
    {
        // Handle file menu click
        System.Diagnostics.Debug.WriteLine("File clicked");
    }
}
