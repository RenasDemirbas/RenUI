using RenUI.Core.Interfaces;

namespace RenUI.Layout;

public interface ILayoutStrategy
{
    void ApplyLayout(IContainer container);
}
