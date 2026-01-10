using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Linq;

namespace ExploringGame.Services;

public class VertexOffsetter
{
    public Triangle[] Execute(Shape shape, Triangle[] triangles, VertexOffset vertexOffset)
    {
        var vertices = GetVerticesToAdjust(shape, triangles, vertexOffset);

        var offsetVertices = vertices.Select(vertex => (vertex, vertex + vertexOffset.Offset)).ToArray();

        return triangles.Select(t =>
        {
            foreach (var offset in offsetVertices)
            {
                t = t.ReplaceVertex(offset.vertex, offset.Item2);
            }
            return t;
        }).ToArray();
    }

    private Vector3[] GetVerticesToAdjust(Shape shape, Triangle[] triangles, VertexOffset vertexOffset)
    {
        Vector3[] vertices = triangles.SelectMany(p => p.Vertices).ToArray();

        foreach (var side in vertexOffset.Side.Decompose())
        {
            vertices = side switch
            { 
                Side.West => vertices.Where(p => p.X.IsAlmost(shape.GetSide(Side.West))).ToArray(),
                Side.East => vertices.Where(p => p.X.IsAlmost(shape.GetSide(Side.East))).ToArray(),
                Side.South => vertices.Where(p => p.Z.IsAlmost(shape.GetSide(Side.South))).ToArray(),
                Side.North => vertices.Where(p => p.Z.IsAlmost(shape.GetSide(Side.North))).ToArray(),
                Side.Top => vertices.Where(p => p.Y.IsAlmost(shape.GetSide(Side.Top))).ToArray(),
                Side.Bottom => vertices.Where(p => p.Y.IsAlmost(shape.GetSide(Side.Bottom))).ToArray(),
                _ => vertices
            };           
        }

        return vertices.ToArray();
    }
}
