using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class VertexBufferBuilder
{
    public (VertexBuffer, IndexBuffer, int) Build(Shape master, GraphicsDevice graphicsDevice)
    {
        var triangles = master.Build(2);
        var vertices = new VertexList(triangles);

        var vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), vertices.Length, BufferUsage.WriteOnly);
        vb.SetData(vertices.Array);

        short[] indices = BuildIndices(triangles, vertices);
        var ib = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Length, BufferUsage.WriteOnly);
        ib.SetData(indices);

        return (vb, ib, triangles.Length);
    }

    private short[] BuildIndices(IEnumerable<Triangle> triangles, VertexList vertices)
    {
        return triangles.SelectMany(t =>
        {
            return t.Vertices.Select(v => (short)vertices.IndexOf(v, t.Color));
        }).ToArray();       
    }
}
