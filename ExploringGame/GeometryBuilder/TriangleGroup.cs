using ExploringGame.Extensions;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder;

public class Triangle2DGroup
{
    public Side Side { get; }

    public Triangle2DGroup(Triangle2D[] triangles)
    {
        Triangles = triangles;
        Side = triangles[0].Original.Side;
        foreach (var tri in triangles)
        {
            if (tri.Original.Side != Side)
                throw new System.Exception("All triangles in group must be from the same side");
        }
    }

    public Triangle2D[] Triangles { get; private set; }

    public IEnumerable<Vector2> Vertices => Triangles.SelectMany(p => p.Vertices);

    public float Left
    {
        get => Vertices.Min(p => p.X);
        set
        {
            var originalLeft = Left;
            var verts = Vertices.Where(p=>p.X == originalLeft);
            ReplacePoints(verts, value, Axis.X);
        }
    }

    public float Right
    {
        get => Vertices.Max(p => p.X);
        set
        {
            var originalRight = Right;
            var verts = Vertices.Where(p => p.X == originalRight);
            ReplacePoints(verts, value, Axis.X);
        }
    }
    public float Top
    {
        get => Vertices.Max(p => p.Y);
        set
        {
            var originalTop = Top;
            var verts = Vertices.Where(p => p.Y == originalTop);
            ReplacePoints(verts, value, Axis.Y);
        }
    }
    public float Bottom
    {
        get => Vertices.Min(p => p.Y);
        set
        {
            var originalBottom = Bottom;
            var verts = Vertices.Where(p => p.Y == originalBottom);
            ReplacePoints(verts, value, Axis.Y);
        }
    }

    public IEnumerable<Vector2> ConvexHull()
    {
        var points = Vertices.Distinct().OrderBy(p => p.X).ThenBy(p => p.Y).ToList();
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

    private void ReplacePoints(IEnumerable<Vector2> vertices, float newValue, Axis axis)
    {
        foreach(var originalVertex in vertices)
        {
            var newVertex = originalVertex.Set(axis, newValue);

            foreach (var triangle in Triangles)
                triangle.ReplaceVertex(originalVertex, newVertex);
        }
    }

}
