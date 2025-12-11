using Microsoft.Xna.Framework;
using RenUI.Editor.Scenes;

namespace RenUI.Editor;

public class EditorGame : RenUIGame
{
    public EditorGame()
    {
        Graphics.PreferredBackBufferWidth = 1280;
        Graphics.PreferredBackBufferHeight = 720;
        Window.Title = "RenUI Editor";
    }

    protected override void OnUIInitialized()
    {
        UI.Scenes.RegisterScene("Editor", new EditorScene());
        UI.Scenes.ChangeScene("Editor");
    }
}
