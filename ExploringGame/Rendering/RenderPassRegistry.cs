using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Registry of all render passes, sorted by DrawOrder.
/// </summary>
public class RenderPassRegistry
{
    private readonly List<IRenderPass> _passes = new();
    private bool _isSorted = false;

    /// <summary>
    /// All registered passes, sorted by DrawOrder.
    /// </summary>
    public IReadOnlyList<IRenderPass> Passes
    {
        get
        {
            if (!_isSorted)
            {
                _passes.Sort((a, b) => a.ShapeBufferType.CompareTo(b.ShapeBufferType));
                _isSorted = true;
            }
            return _passes;
        }
    }

    /// <summary>
    /// Registers a render pass. Must be called before accessing Passes.
    /// </summary>
    public void Register(IRenderPass pass)
    {
        _passes.Add(pass);
        _isSorted = false;
    }
}
