using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder.Shapes;

/// <summary>
/// Generates a subdivided ground mesh with subtle smooth height variation,
/// giving outdoor areas a realistic yard-like feel rather than a perfectly flat floor.
/// </summary>
public class TerrainSurface : Shape
{
    public static readonly float DefaultLawn = Measure.Inches(10.5f);
    public float MaxHeight { get; }

    // Approximate grid cell size (~1 foot between vertices).
    private const float CellSize = 0.48f;

    // Raises the terrain mesh's mean Y by a fixed amount so that even the lowest
    // noise valleys sit at or above the parent floor, preventing geometry clipping.
    // This should remain constant regardless of MaxHeight changes.
    internal static readonly float AntiClipLift = Measure.Inches(1.5f);

    // --- Noise layer parameters (frequency, phase, and relative amplitude) ---
    // Layer 1: primary large-scale undulation (~16-foot wavelength)
    private const float Layer1FreqX = 0.38f, Layer1PhaseX = 0.70f;
    private const float Layer1FreqZ = 0.31f, Layer1PhaseZ = 1.10f;
    private const float Layer1Amplitude = 0.50f;

    // Layer 2: secondary medium-scale variation (~7-foot wavelength)
    private const float Layer2FreqX = 0.87f, Layer2PhaseX = 2.30f;
    private const float Layer2FreqZ = 0.73f, Layer2PhaseZ = 0.55f;
    private const float Layer2Amplitude = 0.30f;

    // Layer 3: fine detail (~4-foot wavelength)
    private const float Layer3FreqX = 1.53f, Layer3PhaseX = 1.75f;
    private const float Layer3FreqZ = 1.19f, Layer3PhaseZ = 3.00f;
    private const float Layer3Amplitude = 0.20f;

    public override ViewFrom ViewFrom => ViewFrom.Inside;

    public override Theme Theme => new TerrainTheme();

    /// <summary>
    /// Creates a terrain surface that covers the floor area of the given parent shape.
    /// </summary>
    public TerrainSurface(Shape parent, float maxHeight)
    {
        MaxHeight = maxHeight;
        parent.AddChild(this);
        Position = parent.Position;
        Size = parent.Size;
    }

    protected override Triangle[] BuildInternal(QualityLevel quality)
    {
        float baseY  = GetSide(Side.Bottom) + AntiClipLift;
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

        var minY = triangles.Min(p => p.A.Y);
        var maxY = triangles.Max(p => p.A.Y);

        return triangles.ToArray();
    }

    /// <summary>
    /// Returns a smooth height offset for world position (x, z) by summing three
    /// low-frequency sine-wave layers.  The result is within [-MaxHeight, +MaxHeight].
    /// </summary>
    public float SampleNoise(float x, float z)
    {
        float h  = MathF.Sin(x * Layer1FreqX + Layer1PhaseX) * MathF.Cos(z * Layer1FreqZ + Layer1PhaseZ) * Layer1Amplitude;
              h += MathF.Sin(x * Layer2FreqX + Layer2PhaseX) * MathF.Sin(z * Layer2FreqZ + Layer2PhaseZ) * Layer2Amplitude;
              h += MathF.Cos(x * Layer3FreqX + Layer3PhaseX) * MathF.Cos(z * Layer3FreqZ + Layer3PhaseZ) * Layer3Amplitude;

        return h * MaxHeight;
    }
}
