using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.GeometryBuilder.Shapes;

/// <summary>
/// Generates a subdivided ground mesh with subtle smooth height variation,
/// giving outdoor areas a realistic yard-like feel rather than a perfectly flat floor.
/// </summary>
public class TerrainSurface : Shape
{
    // Approximate grid cell size (~1 foot between vertices).
    private const float CellSize = 0.48f;

    // Maximum height deviation from the baseline (~1.5 inches up or down).
    private static readonly float MaxHeight = Measure.Inches(1.5f);

    // Tiny baseline lift above the parent floor so the terrain never clips below it.
    private static readonly float BaselineOffset = MaxHeight;

    public override ViewFrom ViewFrom => ViewFrom.Outside;

    public override Theme Theme => new YardTheme();

    /// <summary>
    /// Creates a terrain surface that covers the floor area of the given parent shape.
    /// </summary>
    public TerrainSurface(Shape parent)
    {
        parent.AddChild(this);
        Position = parent.Position;
        Size = parent.Size;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        float baseY  = GetSide(Side.Bottom) + BaselineOffset;
        float west   = GetSide(Side.West);
        float east   = GetSide(Side.East);
        float north  = GetSide(Side.North);
        float south  = GetSide(Side.South);

        float surfaceWidth = east  - west;
        float surfaceDepth = south - north;

        int cellsX = Math.Max(1, (int)MathF.Ceiling(surfaceWidth / CellSize));
        int cellsZ = Math.Max(1, (int)MathF.Ceiling(surfaceDepth / CellSize));

        float cellW = surfaceWidth / cellsX;
        float cellD = surfaceDepth / cellsZ;

        // Build the Y offsets for every grid vertex.
        float[,] heightMap = new float[cellsX + 1, cellsZ + 1];
        for (int iz = 0; iz <= cellsZ; iz++)
        {
            for (int ix = 0; ix <= cellsX; ix++)
            {
                float wx = west  + ix * cellW;
                float wz = north + iz * cellD;
                heightMap[ix, iz] = SampleNoise(wx, wz);
            }
        }

        var textureInfo = Theme.TextureInfoForSide(Side.Bottom);
        var triangles   = new List<Triangle>((cellsX * cellsZ) * 2);

        for (int iz = 0; iz < cellsZ; iz++)
        {
            for (int ix = 0; ix < cellsX; ix++)
            {
                var v00 = new Vector3(west + ix       * cellW, baseY + heightMap[ix,     iz    ], north + iz       * cellD);
                var v10 = new Vector3(west + (ix + 1) * cellW, baseY + heightMap[ix + 1, iz    ], north + iz       * cellD);
                var v01 = new Vector3(west + ix       * cellW, baseY + heightMap[ix,     iz + 1], north + (iz + 1) * cellD);
                var v11 = new Vector3(west + (ix + 1) * cellW, baseY + heightMap[ix + 1, iz + 1], north + (iz + 1) * cellD);

                // Split each grid cell into two triangles.
                triangles.Add(new Triangle(v00, v10, v11, textureInfo, Side.Bottom));
                triangles.Add(new Triangle(v00, v11, v01, textureInfo, Side.Bottom));
            }
        }

        return triangles.ToArray();
    }

    /// <summary>
    /// Returns a smooth height offset for world position (x, z) by summing several
    /// low-frequency sine waves.  The result always stays within [-MaxHeight, +MaxHeight].
    /// </summary>
    private static float SampleNoise(float x, float z)
    {
        // Three layers at different scales and phases for a natural look.
        float h  = MathF.Sin(x * 0.38f + 0.70f) * MathF.Cos(z * 0.31f + 1.10f) * 0.50f;
              h += MathF.Sin(x * 0.87f + 2.30f) * MathF.Sin(z * 0.73f + 0.55f) * 0.30f;
              h += MathF.Cos(x * 1.53f + 1.75f) * MathF.Cos(z * 1.19f + 3.00f) * 0.20f;

        return h * MaxHeight;
    }
}
