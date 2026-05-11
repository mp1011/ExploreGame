using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Registry of all render passes, sorted by DrawOrder.
/// </summary>
public class RenderPassRegistry
{
    private readonly List<IRenderPass> _environmentPasses = new();
    private readonly List<IRenderPass> _interfacePasses = new();
    private bool _environmentPassesSorted = false;
    private bool _interfacePassesSorted = false;

    /// <summary>
    /// All environment passes, sorted by ShapeBufferType.
    /// </summary>
    public IReadOnlyList<IRenderPass> EnvironmentPasses
    {
        get
        {
            if (!_environmentPassesSorted)
            {
                _environmentPasses.Sort((a, b) => a.ShapeBufferType.CompareTo(b.ShapeBufferType));
                _environmentPassesSorted = true;
            }
            return _environmentPasses;
        }
    }

    /// <summary>
    /// All interface passes, sorted by ShapeBufferType.
    /// </summary>
    public IReadOnlyList<IRenderPass> InterfacePasses
    {
        get
        {
            if (!_interfacePassesSorted)
            {
                _interfacePasses.Sort((a, b) => a.ShapeBufferType.CompareTo(b.ShapeBufferType));
                _interfacePassesSorted = true;
            }
            return _interfacePasses;
        }
    }

    /// <summary>
    /// Registers an environment render pass. Must be called before accessing EnvironmentPasses.
    /// </summary>
    public void Register(IRenderPass pass)
    {
        _environmentPasses.Add(pass);
        _environmentPassesSorted = false;
    }

    /// <summary>
    /// Registers an interface render pass. Must be called before accessing InterfacePasses.
    /// </summary>
    public void RegisterInterface(IRenderPass pass)
    {
        _interfacePasses.Add(pass);
        _interfacePassesSorted = false;
    }
}
