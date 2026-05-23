using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Creates vertex and index buffers specifically for GrassSurface shapes.
/// Converts standard triangles into GrassVertex format with texture coordinates and rotation.
/// </summary>
public class GrassVertexBufferBuilder
{
    public (VertexBuffer, IndexBuffer, int) Build(Dictionary<Shape, Triangle[]> shapeTriangles, TextureSheet textureSheet, GraphicsDevice graphicsDevice)
    {
        var grassShapes = shapeTriangles.Where(kvp => kvp.Key is GrassSurface).ToArray();

        if (!grassShapes.Any())
            return (null, null, 0);

        var vertices = new List<GrassVertex>();
        var indices = new List<int>();
        int totalTriangles = 0;

        var rng = new Random(42);

        foreach (var (shape, triangles) in grassShapes)
        {
            foreach (var triangle in triangles)
            {
                int baseIndex = vertices.Count;

                // Generate random rotation for this blade (0 to 2*PI radians)
                float rotation = (float)(rng.NextDouble() * Math.PI * 2.0);

                var texTopLeft = textureSheet.TexturePosition(TextureKey.Plain, Vector2.Zero);
                var texBottomRight = textureSheet.TexturePosition(TextureKey.Plain, Vector2.One);

                float texU = texTopLeft.X;
                float texV = texTopLeft.Y;
                float texURight = texBottomRight.X;
                float texVBottom = texBottomRight.Y;

                // Calculate the root position (lowest Y value of the triangle vertices)
                var positions = new[] { triangle.A, triangle.B, triangle.C };
                var rootY = positions.Min(p => p.Y);
                var avgX = (float)positions.Average(p => p.X);
                var avgZ = (float)positions.Average(p => p.Z);
                var rootPosition = new Vector3(avgX, rootY, avgZ);

                // Convert each vertex to GrassVertex format
                // Calculate lateral offset as distance in the horizontal (XZ) plane
                foreach (var vertex in positions)
                {
                    var offset = vertex - rootPosition;

                    // Calculate horizontal distance from root (this captures both X and Z differences)
                    float horizontalDist = (float)Math.Sqrt(offset.X * offset.X + offset.Z * offset.Z);

                    // Determine if this is left or right based on X position
                    float lateralOffset = vertex.X < rootPosition.X ? -horizontalDist : horizontalDist;

                    Vector2 texCoord;

                    // Assign texture coordinates based on vertex height
                    if (Math.Abs(offset.Y) < 0.01f)
                    {
                        // Bottom vertices
                        texCoord = new Vector2(lateralOffset < 0 ? texU : texURight, texVBottom);
                    }
                    else
                    {
                        // Top vertices
                        texCoord = new Vector2(lateralOffset < 0 ? texU : texURight, texV);
                    }

                    vertices.Add(new GrassVertex(
                        rootPosition,
                        new Vector2(lateralOffset, offset.Y),
                        texCoord,
                        rotation,
                        triangle.TextureInfo.Color,
                        triangle.Normal
                    ));
                }

                // Add indices for this triangle
                indices.Add(baseIndex);
                indices.Add(baseIndex + 1);
                indices.Add(baseIndex + 2);
                totalTriangles++;
            }
        }

        // Create vertex buffer
        var vertexBuffer = new VertexBuffer(
            graphicsDevice,
            typeof(GrassVertex),
            vertices.Count,
            BufferUsage.WriteOnly
        );
        vertexBuffer.SetData(vertices.ToArray());

        // Create index buffer
        var indexBuffer = new IndexBuffer(
            graphicsDevice,
            IndexElementSize.ThirtyTwoBits,
            indices.Count,
            BufferUsage.WriteOnly
        );
        indexBuffer.SetData(indices.ToArray());

        return (vertexBuffer, indexBuffer, totalTriangles);
    }
}
