
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace ExploringGame.Services;

/// <summary>
/// authored by ChatGPT
/// </summary>
public static class TriangleSubdivider
{
    /// <summary>
    /// Subdivides a triangle into smaller triangles such that the horizontal distance
    /// between any vertices is at most targetX and the vertical distance is at most targetY.
    /// The output triangles exactly cover the same area as the input triangle.
    /// </summary>
    public static List<Triangle2D> Subdivide(
        Triangle2D triangle,
        float targetX,
        float targetY)
    {
        var result = new List<Triangle2D>();

        // Axis-aligned bounding box of the triangle
        float minX = MathF.Min(triangle.A.X, MathF.Min(triangle.B.X, triangle.C.X));
        float maxX = MathF.Max(triangle.A.X, MathF.Max(triangle.B.X, triangle.C.X));
        float minY = MathF.Min(triangle.A.Y, MathF.Min(triangle.B.Y, triangle.C.Y));
        float maxY = MathF.Max(triangle.A.Y, MathF.Max(triangle.B.Y, triangle.C.Y));

        int xCells = (int)MathF.Ceiling((maxX - minX) / targetX);
        int yCells = (int)MathF.Ceiling((maxY - minY) / targetY);

        for (int y = 0; y < yCells; y++)
        {
            for (int x = 0; x < xCells; x++)
            {
                float x0 = minX + x * targetX;
                float y0 = minY + y * targetY;
                float x1 = MathF.Min(x0 + targetX, maxX);
                float y1 = MathF.Min(y0 + targetY, maxY);

                var clipped = ClipTriangleAgainstRect(
                    triangle, x0, y0, x1, y1);

                if (clipped.Count >= 3)
                {
                    TriangulateConvexPolygon(triangle, clipped, result);
                }
            }
        }

        return result;
    }

    // -------------------- Clipping --------------------

    private static List<Vector2> ClipTriangleAgainstRect(
        Triangle2D tri,
        float minX, float minY,
        float maxX, float maxY)
    {
        var poly = new List<Vector2> { tri.A, tri.B, tri.C };

        poly = Clip(poly, v => v.X >= minX, (a, b) => IntersectX(a, b, minX));
        poly = Clip(poly, v => v.X <= maxX, (a, b) => IntersectX(a, b, maxX));
        poly = Clip(poly, v => v.Y >= minY, (a, b) => IntersectY(a, b, minY));
        poly = Clip(poly, v => v.Y <= maxY, (a, b) => IntersectY(a, b, maxY));

        return poly;
    }

    private static List<Vector2> Clip(
        List<Vector2> input,
        Func<Vector2, bool> inside,
        Func<Vector2, Vector2, Vector2> intersect)
    {
        var output = new List<Vector2>();
        if (input.Count == 0)
            return output;

        Vector2 prev = input[input.Count - 1];
        bool prevInside = inside(prev);

        foreach (var curr in input)
        {
            bool currInside = inside(curr);

            if (currInside)
            {
                if (!prevInside)
                    output.Add(intersect(prev, curr));
                output.Add(curr);
            }
            else if (prevInside)
            {
                output.Add(intersect(prev, curr));
            }

            prev = curr;
            prevInside = currInside;
        }

        return output;
    }

    // -------------------- Intersections --------------------

    private static Vector2 IntersectX(Vector2 a, Vector2 b, float x)
    {
        float t = (x - a.X) / (b.X - a.X);
        return Vector2.Lerp(a, b, t);
    }

    private static Vector2 IntersectY(Vector2 a, Vector2 b, float y)
    {
        float t = (y - a.Y) / (b.Y - a.Y);
        return Vector2.Lerp(a, b, t);
    }

    // -------------------- Triangulation --------------------

    private static void TriangulateConvexPolygon(
        Triangle2D original,
        List<Vector2> polygon,
        List<Triangle2D> output)
    {
        // Fan triangulation (polygon is guaranteed convex)
        for (int i = 1; i < polygon.Count - 1; i++)
        {
            output.Add(new Triangle2D(
                polygon[0],
                polygon[i],
                polygon[i + 1], original.Original));
        }
    }
}


