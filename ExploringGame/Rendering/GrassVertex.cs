using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace ExploringGame.Rendering;

/// <summary>
/// Custom vertex for a single grass blade vertex.
/// Each blade triangle shares the same RootPosition for all three vertices;
/// the vertex shader uses Offset to spread base vertices laterally and lift the apex.
/// </summary>
[StructLayout(LayoutKind.Sequential, Pack = 1)]
public struct GrassVertex : IVertexType
{
    /// <summary>World-space base center of this grass blade.</summary>
    public Vector3 RootPosition;

    /// <summary>Per-vertex displacement: X = lateral offset, Y = height above ground.</summary>
    public Vector2 Offset;

    /// <summary>Vertex colour (dark green at base, lighter green at apex).</summary>
    public Color Color;

    public static readonly VertexDeclaration VertexDeclaration = new(
        new VertexElement(0,  VertexElementFormat.Vector3, VertexElementUsage.Position,         0),
        new VertexElement(12, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
        new VertexElement(20, VertexElementFormat.Color,   VertexElementUsage.Color,             0)
    );

    VertexDeclaration IVertexType.VertexDeclaration => VertexDeclaration;

    public GrassVertex(Vector3 rootPosition, Vector2 offset, Color color)
    {
        RootPosition = rootPosition;
        Offset       = offset;
        Color        = color;
    }
}
