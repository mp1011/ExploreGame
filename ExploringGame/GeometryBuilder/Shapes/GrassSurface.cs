using Microsoft.Xna.Framework;
using ExploringGame.Texture;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

/// <summary>
/// Represents a grass surface that can be added as a child of any shape.
/// Generates grass blade geometry for rendering with the grass shader.
/// </summary>
public class GrassSurface : Shape
{
    private const float BladeHalfWidth = 0.01f;   // ~1 inch lateral spread
    private const float BladeHeight    = 0.25f;   // ~6 inches tall
    private const int   BladesPerUnit  = 25;      // 25x25 = 625 blades per 1.0x1.0 area

    private readonly bool _followTerrain;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new GrassTheme();

    /// <summary>
    /// Creates a grass surface that covers the entire area defined by the shape's size.
    /// When <paramref name="terrain"/> is provided, each blade root is raised to match
    /// the terrain height at its world-space (x, z) position.
    /// </summary>
    public GrassSurface(Shape parent, TerrainSurface terrain = null)
    {
        parent.AddChild(this);
        Position = parent.Position;
        Size = parent.Size;
        _followTerrain = terrain != null;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        // Get the bounds of this grass surface
        float floorY = GetSide(Side.Bottom);
        float west   = GetSide(Side.West);
        float east   = GetSide(Side.East);
        float north  = GetSide(Side.North);
        float south  = GetSide(Side.South);

        var triangles = new List<Triangle>();
        var rng = new Random(42);

        float surfaceWidth = east - west;
        float surfaceDepth = south - north;

        int bladesInWidth = (int)(surfaceWidth * BladesPerUnit);
        int bladesInDepth = (int)(surfaceDepth * BladesPerUnit);

        float cellW = surfaceWidth / bladesInWidth;
        float cellD = surfaceDepth / bladesInDepth;


        for (int row = 0; row < bladesInDepth; row++)
        {
            for (int col = 0; col < bladesInWidth; col++)
            {
                // Jitter within cell for natural scatter
                float jitterX = (float)(rng.NextDouble() - 0.5) * cellW * 0.8f;
                float jitterZ = (float)(rng.NextDouble() - 0.5) * cellD * 0.8f;

                float x = west  + (col + 0.5f) * cellW + jitterX;
                float z = north + (row + 0.5f) * cellD + jitterZ;

                float rootY = _followTerrain
                    ? floorY + TerrainSurface.AntiClipLift + TerrainSurface.SampleNoise(x, z)
                    : floorY;

                var root = new Vector3(x, rootY, z);

                // Generate random rotation for this blade (0 to 2*PI radians)
                // We'll encode this in the position X component offset
                float rotation = (float)(rng.NextDouble() * Math.PI * 2.0);

                // Create a thin quad using 2 triangles
                // Raise one top vertex higher to create a blade shape with a tapered point
                float topLeftHeight = BladeHeight * 1.2f;  // Taller side
                float topRightHeight = BladeHeight * 0.8f; // Shorter side

                // Define the 4 corners of the quad in local blade space
                // We'll encode the rotation in a special way that GrassVertexBufferBuilder can extract
                var bottomLeft  = new Vector3(root.X - BladeHalfWidth, rootY, root.Z);
                var bottomRight = new Vector3(root.X + BladeHalfWidth, rootY, root.Z);
                var topLeft     = new Vector3(root.X - BladeHalfWidth, rootY + topLeftHeight, root.Z);
                var topRight    = new Vector3(root.X + BladeHalfWidth, rootY + topRightHeight, root.Z);

                // First triangle (bottom-left, bottom-right, top-left)
                triangles.Add(new Triangle(
                    bottomLeft,
                    bottomRight,
                    topLeft,
                    Theme.MainTexture,
                    Side.Bottom
                ));

                // Second triangle (bottom-right, top-right, top-left)
                triangles.Add(new Triangle(
                    bottomRight,
                    topRight,
                    topLeft,
                    Theme.MainTexture,
                    Side.Bottom
                ));
            }
        }

        return triangles.ToArray();
    }
}
