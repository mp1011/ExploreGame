using ExploringGame.GeometryBuilder.Shapes;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Holds all registered render passes sorted by DrawOrder.
/// Specialized (non-catch-all) passes are tried first when routing shapes.
/// </summary>
public class RenderPassRegistry
{
    private readonly List<IRenderPass> _passes = new();

    public IReadOnlyList<IRenderPass> Passes => _passes.AsReadOnly();

    public void Register(IRenderPass pass)
    {
        _passes.Add(pass);
        _passes.Sort((a, b) => a.DrawOrder.CompareTo(b.DrawOrder));
    }

    /// <summary>
    /// Returns the first specialized pass that claims the shape, or null if none do.
    /// The catch-all pass is intentionally excluded; callers fall back to it themselves.
    /// </summary>
    public IRenderPass FindSpecializedPassForShape(Shape shape)
        => _passes.FirstOrDefault(p => !p.IsCatchAll && p.ClaimsShape(shape));

    /// <summary>Returns the registered catch-all pass, or null.</summary>
    public IRenderPass CatchAllPass
        => _passes.FirstOrDefault(p => p.IsCatchAll);
}
