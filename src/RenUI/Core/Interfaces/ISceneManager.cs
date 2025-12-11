namespace RenUI.Core.Interfaces;

public interface ISceneManager
{
    IScene? CurrentScene { get; }
    void RegisterScene(string name, IScene scene);
    void UnregisterScene(string name);
    void ChangeScene(string name);
    void PushScene(string name);
    void PopScene();
}
