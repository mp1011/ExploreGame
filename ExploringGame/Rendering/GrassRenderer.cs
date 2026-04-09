using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

/// <summary>
/// Proof-of-concept grass renderer.
/// Scatters individual grass blade triangles over the floor of a <see cref="FrontYard"/>.
/// Each blade is a single triangle whose vertices are positioned by the GrassEffect vertex shader.
/// </summary>
public class GrassRenderer
{
    private const float BladeHalfWidth = 0.04f;   // ~1 inch lateral spread
    private const float BladeHeight    = 0.25f;   // ~6 inches tall
    private const int   BladesPerAxis  = 25;       // 25x25 = 625 blades total

    private static readonly Color BaseColor = new Color(28, 90, 28);
    private static readonly Color ApexColor = new Color(80, 180, 60);

    private VertexBuffer _vertexBuffer;
    private IndexBuffer  _indexBuffer;
    private int          _triangleCount;
    private Effect       _effect;

    public GrassRenderer(GraphicsDevice graphicsDevice, ContentManager content, FrontYard frontYard)
    {
        _effect = content.Load<Effect>("GrassEffect");
        BuildBuffers(graphicsDevice, frontYard);
    }

    private void BuildBuffers(GraphicsDevice graphicsDevice, FrontYard frontYard)
    {
        float floorY = frontYard.GetSide(Side.Bottom);
        float west   = frontYard.GetSide(Side.West);
        float east   = frontYard.GetSide(Side.East);
        float north  = frontYard.GetSide(Side.North);
        float south  = frontYard.GetSide(Side.South);

        var vertices = new List<GrassVertex>(BladesPerAxis * BladesPerAxis * 3);
        var indices  = new List<int>(BladesPerAxis * BladesPerAxis * 3);

        var rng = new Random(42);

        float cellW = (east  - west)  / BladesPerAxis;
        float cellD = (south - north) / BladesPerAxis;

        for (int row = 0; row < BladesPerAxis; row++)
        {
            for (int col = 0; col < BladesPerAxis; col++)
            {
                // Jitter within cell for natural scatter
                float jitterX = (float)(rng.NextDouble() - 0.5) * cellW * 0.8f;
                float jitterZ = (float)(rng.NextDouble() - 0.5) * cellD * 0.8f;

                float x = west  + (col + 0.5f) * cellW + jitterX;
                float z = north + (row + 0.5f) * cellD + jitterZ;

                var root = new Vector3(x, floorY, z);
                int  baseIndex = vertices.Count;

                // Base-left vertex
                vertices.Add(new GrassVertex(root, new Vector2(-BladeHalfWidth, 0f), BaseColor));
                // Base-right vertex
                vertices.Add(new GrassVertex(root, new Vector2( BladeHalfWidth, 0f), BaseColor));
                // Apex vertex
                vertices.Add(new GrassVertex(root, new Vector2(0f, BladeHeight), ApexColor));

                indices.Add(baseIndex);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 2);
            }
        }

        _triangleCount = BladesPerAxis * BladesPerAxis;

        _vertexBuffer = new VertexBuffer(graphicsDevice, GrassVertex.VertexDeclaration, vertices.Count, BufferUsage.WriteOnly);
        _vertexBuffer.SetData(vertices.ToArray());

        _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
        _indexBuffer.SetData(indices.ToArray());
    }

    public void Draw(GraphicsDevice graphicsDevice, Matrix view, Matrix projection)
    {
        _effect.Parameters["World"].SetValue(Matrix.Identity);
        _effect.Parameters["View"].SetValue(view);
        _effect.Parameters["Projection"].SetValue(projection);

        graphicsDevice.SetVertexBuffer(_vertexBuffer);
        graphicsDevice.Indices = _indexBuffer;

        // Grass blades are visible from both sides
        var prevRasterizer = graphicsDevice.RasterizerState;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        foreach (var pass in _effect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
        }

        graphicsDevice.RasterizerState = prevRasterizer;
    }
}
