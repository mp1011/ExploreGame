using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

public class ConvexHull
{
    public Vector2[] Vertices { get; }

    public ConvexHull(Triangle2D[] triangles)
    {
        var side = triangles[0].Original.Side;
        foreach (var tri in triangles)
        {
            if (tri.Original.Side != side)
                throw new System.Exception("All triangles in group must be from the same side");
        }

        Vertices = CalcConvexHull(triangles).ToArray();
    }

    public float Left
    {
        get => Vertices.Min(p => p.X);
        set
        {
            var originalLeft = Left;
            ReplacePoints(originalLeft, value, Axis.X);
        }
    }

    public float Right
    {
        get => Vertices.Max(p => p.X);
        set
        {
            var originalRight = Right;
            ReplacePoints(originalRight, value, Axis.X);
        }
    }
    public float Top
    {
        get => Vertices.Max(p => p.Y);
        set
        {
            var originalTop = Top;
            ReplacePoints(originalTop, value, Axis.Y);
        }
    }
    public float Bottom
    {
        get => Vertices.Min(p => p.Y);
        set
        {
            var originalBottom = Bottom;
            ReplacePoints(originalBottom, value, Axis.Y);
        }
    }

    private void ReplacePoints(float originalValue, float newValue, Axis axis)
    {
        for(int i =0; i< Vertices.Length; i++)
        {
            if (Vertices[i].AxisValue(axis) == originalValue)
                Vertices[i] = Vertices[i].Set(axis, newValue);
        }             
    }

    private IEnumerable<Vector2> CalcConvexHull(IEnumerable<Triangle2D> triangles)
    {
        var vertices = triangles.SelectMany(p=>p.Vertices).ToArray();
        var points = vertices.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
        if (points.Count <= 1) return points;

        List<Vector2> hull = new();

        // Lower hull
        foreach (var p in points)
        {
            while (hull.Count >= 2 && Cross(hull[^2], hull[^1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        // Upper hull
        int t = hull.Count + 1;
        for (int i = points.Count - 2; i >= 0; i--)
        {
            var p = points[i];
            while (hull.Count >= t && Cross(hull[^2], hull[^1], p) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1); // Remove duplicate
        return hull;

        static float Cross(Vector2 o, Vector2 a, Vector2 b)
            => (a.X - o.X) * (b.Y - o.Y) - (a.Y - o.Y) * (b.X - o.X);
    }
}
