namespace RenUI.Core.Interfaces;

public interface IContainer : IUIElement
{
    IReadOnlyList<IUIElement> Children { get; }
    void AddChild(IUIElement child);
    void RemoveChild(IUIElement child);
    void ClearChildren();
    IUIElement? GetChildByName(string name);
}
