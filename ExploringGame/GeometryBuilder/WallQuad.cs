using ExploringGame.GeometryBuilder.Shapes;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

/// <summary>
/// Represents a quad (4-vertex polygon) on a wall surface
/// </summary>
public class WallQuad
{
    public Room Room { get; }
    public Side Side { get; }
    public Vector3[] Vertices { get; }
    public Vector3 Center { get; }
    public float Width { get; }
    public float Height { get; }

    public WallQuad(Room room, Side side, Vector3[] vertices)
    {
        Room = room;
        Side = side;
        Vertices = vertices;
        
        // Calculate center
        Center = (vertices[0] + vertices[1] + vertices[2] + vertices[3]) / 4f;
        
        // Calculate dimensions (approximate as axis-aligned)
        var minX = vertices.Min(v => v.X);
        var maxX = vertices.Max(v => v.X);
        var minY = vertices.Min(v => v.Y);
        var maxY = vertices.Max(v => v.Y);
        var minZ = vertices.Min(v => v.Z);
        var maxZ = vertices.Max(v => v.Z);
        
        // Width and height depend on the side orientation
        if (side == Side.North || side == Side.South)
        {
            Width = maxX - minX;
            Height = maxY - minY;
        }
        else // East or West
        {
            Width = maxZ - minZ;
            Height = maxY - minY;
        }
    }
}
