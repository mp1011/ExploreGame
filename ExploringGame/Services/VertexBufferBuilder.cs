using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class VertexBufferBuilder
{
    public (VertexBuffer, IndexBuffer, int) Build(Shape master, GraphicsDevice graphicsDevice, QualityLevel qualityLevel)
    {
        var triangles = master.Build(qualityLevel);
        var vertices = new VertexList(triangles);

        var vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        vb.SetData(vertices.Array);

        int[] indices = BuildIndices(triangles, vertices);
        var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
        ib.SetData(indices);

        return (vb, ib, triangles.Length);
    }

    private int[] BuildIndices(IEnumerable<Triangle> triangles, VertexList vertices)
    {
        return triangles.SelectMany(t =>
        {
            return t.Vertices.Select(v => vertices.IndexOf(v, t.Color));
        }).ToArray();       
    }
}
