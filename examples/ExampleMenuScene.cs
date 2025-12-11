using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RenUI;
using RenUI.Elements.Controls;
using RenUI.Scenes;

namespace MyGame.Scenes;

/// <summary>
/// Example scene showing how to use RenUI layouts from the editor.
/// </summary>
public class ExampleMenuScene : SceneBase
{
    private Button? _startButton;
    private Button? _optionsButton;
    private Button? _exitButton;
    private Slider? _volumeSlider;

    public ExampleMenuScene() : base("MainMenu") { }

    protected override void OnLoadContent()
    {
        // Method 1: Load and build entire UI from JSON
        // --------------------------------------------------
        // This creates the UI hierarchy from scratch based on the JSON file
        
        UI.Layouts.LoadLayout("MainMenu", "Content/UI/MainMenu.ui.json");
        var root = UI.Layouts.Build("MainMenu");
        
        if (root != null)
        {
            // Add all children from the built layout to this scene
            foreach (var child in root.Children.ToList())
            {
                AddElement(child);
            }
        }
        
        // Access elements by their ID (defined in the editor)
        _startButton = UI.Layouts.GetElement<Button>("MainMenu", "btn_start_game");
        _optionsButton = UI.Layouts.GetElement<Button>("MainMenu", "btn_options");
        _exitButton = UI.Layouts.GetElement<Button>("MainMenu", "btn_exit");
        
        // Setup event handlers
        SetupEventHandlers();
    }

    private void SetupEventHandlers()
    {
        // Connect button clicks to game logic
        _startButton?.OnClick.Subscribe(e => OnStartGameClick());
        _optionsButton?.OnClick.Subscribe(e => OnOptionsClick());
        _exitButton?.OnClick.Subscribe(e => OnExitClick());
    }

    private void OnStartGameClick()
    {
        System.Diagnostics.Debug.WriteLine("Start Game clicked!");
        // UI.Scenes.ChangeScene("GameScene");
    }

    private void OnOptionsClick()
    {
        System.Diagnostics.Debug.WriteLine("Options clicked!");
        // UI.Scenes.PushScene("OptionsScene");
    }

    private void OnExitClick()
    {
        System.Diagnostics.Debug.WriteLine("Exit clicked!");
        // Environment.Exit(0);
    }
}

/// <summary>
/// Example showing how to apply editor settings to code-created UI.
/// </summary>
public class ExampleHybridScene : SceneBase
{
    public ExampleHybridScene() : base("HybridExample") { }

    protected override void OnLoadContent()
    {
        // Method 2: Create UI in code, apply editor values
        // --------------------------------------------------
        // This is useful when you want code control but editor styling
        
        // First, create UI elements with IDs matching the JSON
        var mainPanel = new Panel
        {
            Id = "panel_options_main",  // Must match ID in JSON
            Name = "OptionsMainPanel",
            Bounds = new Rectangle(0, 0, 800, 600)
        };
        
        var volumeSlider = new Slider
        {
            Id = "slider_master_volume",  // Must match ID in JSON
            Name = "MasterVolumeSlider"
        };
        mainPanel.AddChild(volumeSlider);
        
        var applyButton = new Button
        {
            Id = "btn_apply",  // Must match ID in JSON
            Name = "ApplyButton"
        };
        mainPanel.AddChild(applyButton);
        
        AddElement(mainPanel);
        
        // Now load the JSON and apply values to existing elements
        UI.Layouts.LoadLayout("Options", "Content/UI/OptionsPanel.ui.json");
        UI.Layouts.ApplyLayout("Options", mainPanel);
        
        // The slider and button now have the colors, sizes, 
        // positions defined in the editor!
        
        // You can still add event handlers normally
        applyButton.OnClick.Subscribe(e => OnApplyClick());
        volumeSlider.OnValueChanged.Subscribe(e => OnVolumeChanged(e.Value));
    }

    private void OnApplyClick()
    {
        System.Diagnostics.Debug.WriteLine("Apply clicked!");
    }

    private void OnVolumeChanged(float value)
    {
        System.Diagnostics.Debug.WriteLine($"Volume: {value}");
    }
}

/// <summary>
/// Example game class showing full integration.
/// </summary>
public class ExampleGame : RenUIGame
{
    public ExampleGame()
    {
        Graphics.PreferredBackBufferWidth = 1920;
        Graphics.PreferredBackBufferHeight = 1080;
        Window.Title = "My Game with RenUI";
    }

    protected override void OnUIInitialized()
    {
        // Register all scenes
        UI.Scenes.RegisterScene("MainMenu", new ExampleMenuScene());
        UI.Scenes.RegisterScene("HybridExample", new ExampleHybridScene());
        
        // Start with main menu
        UI.Scenes.ChangeScene("MainMenu");
    }
}
