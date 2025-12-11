namespace RenUI.Editor;

public static class Program
{
    [STAThread]
    static void Main()
    {
        using var game = new EditorGame();
        game.Run();
    }
}
