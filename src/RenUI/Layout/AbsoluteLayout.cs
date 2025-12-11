using Microsoft.Xna.Framework;
using RenUI.Core.Interfaces;
using RenUI.Elements.Base;

namespace RenUI.Layout;

public class AbsoluteLayout : ILayoutStrategy
{
    public void ApplyLayout(IContainer container)
    {
        // Absolute layout doesn't modify child positions
        // Children are positioned exactly where specified
        // This is essentially a no-op, allowing manual positioning
    }
}
