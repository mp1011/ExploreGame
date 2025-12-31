using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

/// <summary>
/// Executes one iteration of the Sierpinski triangle on each triangle
/// </summary>
public class SierpinskiSplitter
{
    public static bool DebugColoring = false;
    private Random _rng = new Random();

    public Triangle[] Execute(Triangle[] triangles)
    {
        if (triangles.Length == 0)
            return triangles;

        return triangles.SelectMany(Split).ToArray();
    }

    public Triangle[] ExecuteUntilLengthReached(Triangle[] triangles, float targetLength)
    {
        if (triangles.Length == 0)
            return triangles;

        int loopCount = 0;
        while (++loopCount < 100)
        {
            triangles = Execute(triangles);

            if ((triangles[0].A - triangles[0].B).Length() <= targetLength)
                break;
        }

        return triangles;
    }

    private IEnumerable<Triangle> Split(Triangle triangle)
    {
        var a = triangle.A;
        var b = triangle.B;
        var c = triangle.C;

        // Midpoints of each edge
        Vector3 ab = (a + b) * 0.5f;
        Vector3 bc = (b + c) * 0.5f;
        Vector3 ca = (c + a) * 0.5f;

        if (DebugColoring)
        {
            yield return new Triangle(a, ab, ca, DebugTexture(triangle.TextureInfo), triangle.Side);
            yield return new Triangle(ab, b, bc, DebugTexture(triangle.TextureInfo), triangle.Side);
            yield return new Triangle(ca, bc, c, DebugTexture(triangle.TextureInfo), triangle.Side);
            yield return new Triangle(ab, bc, ca, DebugTexture(triangle.TextureInfo), triangle.Side);
        }
        else
        {
            yield return new Triangle(a, ab, ca, triangle.TextureInfo, triangle.Side);
            yield return new Triangle(ab, b, bc, triangle.TextureInfo, triangle.Side);
            yield return new Triangle(ca, bc, c, triangle.TextureInfo, triangle.Side);
            yield return new Triangle(ab, bc, ca, triangle.TextureInfo, triangle.Side);
        }
    }

    private TextureInfo DebugTexture(TextureInfo original)
    {
        var color = new Color(_rng.Next(255), _rng.Next(255), _rng.Next(255));
        return new TextureInfo(color, original.Key, original.Style);
    }
}
