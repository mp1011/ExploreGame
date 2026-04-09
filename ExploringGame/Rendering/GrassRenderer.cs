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
    private const float BladeHalfWidth = 0.01f;   // ~1 inch lateral spread
    private const float BladeHeight    = 0.25f;   // ~6 inches tall
    private const int   BladesPerAxis  = 250;       // 25x25 = 625 blades total

    private VertexBuffer _vertexBuffer;
    private IndexBuffer  _indexBuffer;
    private int          _triangleCount;
    private Effect       _effect;
    private Texture2D    _grassTexture;

    public GrassRenderer(GraphicsDevice graphicsDevice, ContentManager content, FrontYard frontYard, Texture2D grassTexture)
    {
        _effect = content.Load<Effect>("GrassEffect");
        _grassTexture = grassTexture;
        BuildBuffers(graphicsDevice, frontYard);
    }

    private void BuildBuffers(GraphicsDevice graphicsDevice, FrontYard frontYard)
    {
        float floorY = frontYard.GetSide(Side.Bottom);
        float west   = frontYard.GetSide(Side.West);
        float east   = frontYard.GetSide(Side.East);
        float north  = frontYard.GetSide(Side.North);
        float south  = frontYard.GetSide(Side.South);

        var vertices = new List<GrassVertex>(BladesPerAxis * BladesPerAxis * 4);
        var indices  = new List<int>(BladesPerAxis * BladesPerAxis * 6);

        var rng = new Random(42);

        // Grass texture coordinates from OutdoorsTextureSheet (TextureKey.Grass)
        // left: 299, top: 268, right: 1812, bottom: 1756
        float grassTexLeft = 299f / _grassTexture.Width;
        float grassTexTop = 268f / _grassTexture.Height;
        float grassTexRight = 1812f / _grassTexture.Width;
        float grassTexBottom = 1756f / _grassTexture.Height;
        float grassTexWidth = grassTexRight - grassTexLeft;
        float grassTexHeight = grassTexBottom - grassTexTop;

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

                // Generate random rotation for this blade (0 to 2*PI radians)
                float rotation = (float)(rng.NextDouble() * Math.PI * 2.0);

                // Generate random texture coordinates from a small region within the grass texture
                float texOffsetX = (float)rng.NextDouble() * (grassTexWidth - 0.02f);
                float texOffsetY = (float)rng.NextDouble() * (grassTexHeight - 0.02f);
                float texU = grassTexLeft + texOffsetX;
                float texV = grassTexTop + texOffsetY;
                float texURight = texU + 0.008f; // Smaller portion of texture for each blade
                float texVBottom = texV + 0.015f;

                // Create a thin quad (4 vertices forming 2 triangles)
                // Raise one top vertex higher to create a blade shape with a tapered point
                float topLeftHeight = BladeHeight * 1.2f;  // Taller side
                float topRightHeight = BladeHeight * 0.8f; // Shorter side

                // Bottom-left vertex
                vertices.Add(new GrassVertex(root, new Vector2(-BladeHalfWidth, 0f), new Vector2(texU, texVBottom), rotation));
                // Bottom-right vertex
                vertices.Add(new GrassVertex(root, new Vector2( BladeHalfWidth, 0f), new Vector2(texURight, texVBottom), rotation));
                // Top-left vertex (taller)
                vertices.Add(new GrassVertex(root, new Vector2(-BladeHalfWidth, topLeftHeight), new Vector2(texU, texV), rotation));
                // Top-right vertex (shorter)
                vertices.Add(new GrassVertex(root, new Vector2( BladeHalfWidth, topRightHeight), new Vector2(texURight, texV), rotation));

                // First triangle (bottom-left, bottom-right, top-left)
                indices.Add(baseIndex);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 2);

                // Second triangle (bottom-right, top-right, top-left)
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 3);
                indices.Add(baseIndex + 2);
            }
        }

        _triangleCount = BladesPerAxis * BladesPerAxis * 2;

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
        _effect.Parameters["GrassTexture"].SetValue(_grassTexture);

        // Extract camera position from inverse view matrix for billboarding
        Matrix inverseView = Matrix.Invert(view);
        Vector3 cameraPosition = inverseView.Translation;
        _effect.Parameters["CameraPosition"].SetValue(cameraPosition);

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
