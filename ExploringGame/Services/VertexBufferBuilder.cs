using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class VertexBufferBuilder
{
    public (VertexBuffer, IndexBuffer, int) Build(Shape master, TextureSheet textureSheet, GraphicsDevice graphicsDevice, QualityLevel qualityLevel)
    {
        var triangles = master.Build(qualityLevel);
        var vertices = new VertexList(triangles, textureSheet);

        var vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorTexture), vertices.Length, BufferUsage.WriteOnly);
        vb.SetData(vertices.Array);

        var allTriangles = triangles.SelectMany(p => p.Value).ToArray();

        int[] indices = BuildIndices(allTriangles, vertices);
        var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
        ib.SetData(indices);

        return (vb, ib, allTriangles.Length);
    }

    private int[] BuildIndices(IEnumerable<Triangle> triangles, VertexList vertices)
    {
        return triangles.SelectMany(t =>
        {
            return t.Vertices.Select(v => vertices.IndexOf(v, t.TextureInfo.Color));
        }).ToArray();       
    }
}
