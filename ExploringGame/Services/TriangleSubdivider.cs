using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
namespace ExploringGame.Services;

public static class TriangleSubdivider
{

    public static Triangle[] Subdivide(Triangle t, Vector2 tileSize, Vector3 gridOrigin)
    {
        var subDivided = GridTriangleSubdivider.Subdivide(new GridTriangleSubdivider.Triangle3D(t.A, t.B, t.C), tileSize, gridOrigin);
        var result = subDivided.Select(p => new Triangle(p.A, p.B, p.C, t.TextureInfo, t.Side)).ToArray();

        var r2 = result.Select(p => p.As2D(t.Vertices.Center(), ViewFrom.Inside)).ToArray();
        PolygonVisualizer.SavePolygonImage("subdivided", r2);

        return result;
    }
}

public static class GridTriangleSubdivider
{
    public struct Triangle3D
    {
        public Vector3 A, B, C;
        public Triangle3D(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a; B = b; C = c;
        }
    }

    private struct Triangle2D
    {
        public Vector2 A, B, C;
    }

    public static void ComputeCanonicalBasis(
    Vector3 a,
    Vector3 b,
    Vector3 c,
    out Vector3 normal,
    out Vector3 uAxis,
    out Vector3 vAxis)
    {
        // 1. Plane normal (still depends on winding, so we canonicalize it)
        normal = Vector3.Normalize(Vector3.Cross(b - a, c - a));

        // Optional but STRONGLY recommended:
        // force normal into a consistent hemisphere (e.g. positive Y)
        if (normal.Y < 0)
            normal = -normal;

        // 2. Choose a fixed world reference axis
        // Pick the axis least parallel to the normal
        Vector3 reference =
            MathF.Abs(normal.Y) < 0.999f ? Vector3.UnitY :
            MathF.Abs(normal.X) < 0.999f ? Vector3.UnitX :
                                           Vector3.UnitZ;

        // 3. Project reference axis onto the plane → U
        uAxis = reference - Vector3.Dot(reference, normal) * normal;
        uAxis = Vector3.Normalize(uAxis);

        // 4. Derive V (guaranteed orthonormal and consistent)
        vAxis = Vector3.Cross(normal, uAxis);
    }


    public static List<Triangle3D> Subdivide(
        Triangle3D source,
        Vector2 gridSize,
        Vector3 gridOrigin)
    {
        // 1. Build plane basis
        Vector3 normal, uAxis, vAxis;
        ComputeCanonicalBasis(source.A, source.B, source.C, out normal, out uAxis, out vAxis);
        // Vector3 normal = Vector3.Normalize(Vector3.Cross(source.B - source.A, source.C - source.A));
        //Vector3 uAxis = Vector3.Normalize(source.B - source.A);
        //Vector3 vAxis = Vector3.Normalize(Vector3.Cross(normal, uAxis));

        Vector2 Project(Vector3 p)
        {
            Vector3 d = p - gridOrigin;
            return new Vector2(Vector3.Dot(d, uAxis), Vector3.Dot(d, vAxis));
        }

        Vector3 Unproject(Vector2 p)
        {
            return gridOrigin + p.X * uAxis + p.Y * vAxis;
        }

        // 2. Project triangle to 2D
        Triangle2D tri2D = new Triangle2D
        {
            A = Project(source.A),
            B = Project(source.B),
            C = Project(source.C)
        };

        // 3. Bounding box in grid space
        Vector2 min = Vector2.Min(tri2D.A, Vector2.Min(tri2D.B, tri2D.C));
        Vector2 max = Vector2.Max(tri2D.A, Vector2.Max(tri2D.B, tri2D.C));

        int minX = (int)Math.Floor(min.X / gridSize.X);
        int maxX = (int)Math.Ceiling(max.X / gridSize.X);
        int minY = (int)Math.Floor(min.Y / gridSize.Y);
        int maxY = (int)Math.Ceiling(max.Y / gridSize.Y);

        // 4. Collect all grid intersection points inside triangle
        HashSet<Vector2> vertices = new HashSet<Vector2>(new Vec2Comparer());

        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 p = new Vector2(x * gridSize.X, y * gridSize.Y);
                if (PointInTriangle(p, tri2D))
                    vertices.Add(p);
            }
        }

        // 5. Add triangle vertices
        vertices.Add(tri2D.A);
        vertices.Add(tri2D.B);
        vertices.Add(tri2D.C);

        // 6. Add grid–edge intersections
        AddEdgeGridIntersections(tri2D.A, tri2D.B, gridSize, vertices);
        AddEdgeGridIntersections(tri2D.B, tri2D.C, gridSize, vertices);
        AddEdgeGridIntersections(tri2D.C, tri2D.A, gridSize, vertices);

        // 7. Triangulate per grid cell
        List<Triangle3D> result = new List<Triangle3D>();

        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2 cellMin = new Vector2(x * gridSize.X, y * gridSize.Y);
                Vector2 cellMax = cellMin + gridSize;

                List<Vector2> cellPoints = new List<Vector2>();
                foreach (var v in vertices)
                {
                    if (v.X >= cellMin.X - 1e-5f && v.X <= cellMax.X + 1e-5f &&
                        v.Y >= cellMin.Y - 1e-5f && v.Y <= cellMax.Y + 1e-5f)
                    {
                        cellPoints.Add(v);
                    }
                }

                if (cellPoints.Count < 3)
                    continue;

                Vector2 center = Vector2.Zero;
                foreach (var p in cellPoints) center += p;
                center /= cellPoints.Count;

                cellPoints.Sort((a, b) =>
                    MathF.Atan2(a.Y - center.Y, a.X - center.X)
                        .CompareTo(MathF.Atan2(b.Y - center.Y, b.X - center.X)));

                for (int i = 1; i + 1 < cellPoints.Count; i++)
                {
                    Vector3 A3 = Unproject(cellPoints[0]);
                    Vector3 B3 = Unproject(cellPoints[i]);
                    Vector3 C3 = Unproject(cellPoints[i + 1]);
                    result.Add(new Triangle3D(A3, B3, C3));
                }
            }
        }

        return result;
    }

    private static void AddEdgeGridIntersections(
    Vector2 a,
    Vector2 b,
    Vector2 gridSize,
    HashSet<Vector2> output)
    {
        Vector2 d = b - a;

        // Vertical grid lines: x = k * gridSize.X
        if (MathF.Abs(d.X) > 1e-6f)
        {
            float minX = MathF.Min(a.X, b.X);
            float maxX = MathF.Max(a.X, b.X);

            int k0 = (int)MathF.Floor(minX / gridSize.X);
            int k1 = (int)MathF.Ceiling(maxX / gridSize.X);

            for (int k = k0; k <= k1; k++)
            {
                float x = k * gridSize.X;
                float t = (x - a.X) / d.X;

                if (t > 0f && t < 1f)
                {
                    float y = a.Y + t * d.Y;
                    output.Add(new Vector2(x, y));
                }
            }
        }

        // Horizontal grid lines: y = k * gridSize.Y
        if (MathF.Abs(d.Y) > 1e-6f)
        {
            float minY = MathF.Min(a.Y, b.Y);
            float maxY = MathF.Max(a.Y, b.Y);

            int k0 = (int)MathF.Floor(minY / gridSize.Y);
            int k1 = (int)MathF.Ceiling(maxY / gridSize.Y);

            for (int k = k0; k <= k1; k++)
            {
                float y = k * gridSize.Y;
                float t = (y - a.Y) / d.Y;

                if (t > 0f && t < 1f)
                {
                    float x = a.X + t * d.X;
                    output.Add(new Vector2(x, y));
                }
            }
        }
    }


    private static bool PointInTriangle(Vector2 p, Triangle2D t)
    {
        float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) -
                   (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        float d1 = Sign(p, t.A, t.B);
        float d2 = Sign(p, t.B, t.C);
        float d3 = Sign(p, t.C, t.A);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private class Vec2Comparer : IEqualityComparer<Vector2>
    {
        public bool Equals(Vector2 a, Vector2 b)
            => Vector2.DistanceSquared(a, b) < 1e-8f;

        public int GetHashCode(Vector2 v)
            => HashCode.Combine(
                MathF.Round(v.X, 5),
                MathF.Round(v.Y, 5));
    }
}



//authored by ChatGPT
public class GridTriangleSubdivider_v1
{
    const float EPS = 1e-5f;

    public struct Triangle3D
    {
        public Vector3 A, B, C;
        public Triangle3D(Vector3 a, Vector3 b, Vector3 c)
        {
            A = a; B = b; C = c;
        }
    }

    struct Vertex2D
    {
        public Vector2 P;
        public Vertex2D(Vector2 p) => P = p;
    }

    // ------------------------------------------------------------
    // PUBLIC ENTRY POINT
    // ------------------------------------------------------------

    public class BasicPolygon : IPolygon2D
    {
        private Vector2[] _v;
        public IEnumerable<Vector2> Vertices => _v;

        public BasicPolygon(Vector2[] v)
        {
            _v = v;
        }
    }

    public static List<Triangle3D> Subdivide(
        Triangle3D tri,
        Vector2 gridSize,
        Vector3 gridOrigin)
    {
        // 1. Build plane basis
        BuildPlaneBasis(tri, out Vector3 origin, out Vector3 U, out Vector3 V);

        // 2. Project triangle into 2D
        Vector2 a2 = Project(tri.A, origin, U, V);
        Vector2 b2 = Project(tri.B, origin, U, V);
        Vector2 c2 = Project(tri.C, origin, U, V);

        var tri2D = new[] { a2, b2, c2 };

        // 3. Collect vertices (original + grid intersections)
        var vertices = CollectVertices(tri2D, gridSize, Project(gridOrigin, origin, U, V));

        // 4. Triangulate in 2D
        var triangles2D = Triangulate(tri2D, vertices);

        PolygonVisualizer.SavePolygonImage("gridSplit", triangles2D.Select(p => new BasicPolygon(p)).ToArray());

        // 5. Lift back to 3D
        var result = new List<Triangle3D>();
        foreach (var t in triangles2D)
        {
            result.Add(new Triangle3D(
                Lift(t[0], origin, U, V),
                Lift(t[1], origin, U, V),
                Lift(t[2], origin, U, V)
            ));
        }

        return result;
    }

    // ------------------------------------------------------------
    // PLANE BASIS
    // ------------------------------------------------------------

    static void BuildPlaneBasis(
        Triangle3D tri,
        out Vector3 origin,
        out Vector3 U,
        out Vector3 V)
    {
        origin = tri.A;

        Vector3 normal = Vector3.Normalize(
            Vector3.Cross(tri.B - tri.A, tri.C - tri.A));

        Vector3 arbitrary = MathF.Abs(normal.Z) < 0.9f
            ? Vector3.UnitZ
            : Vector3.UnitY;

        U = Vector3.Normalize(Vector3.Cross(arbitrary, normal));
        V = Vector3.Cross(normal, U);
    }

    static Vector2 Project(Vector3 p, Vector3 origin, Vector3 U, Vector3 V)
    {
        Vector3 d = p - origin;
        return new Vector2(Vector3.Dot(d, U), Vector3.Dot(d, V));
    }

    static Vector3 Lift(Vector2 p, Vector3 origin, Vector3 U, Vector3 V)
    {
        return origin + p.X * U + p.Y * V;
    }

    // ------------------------------------------------------------
    // GRID INTERSECTION
    // ------------------------------------------------------------

    static List<Vector2> CollectVertices(
        Vector2[] tri,
        Vector2 gridSize,
        Vector2 gridOrigin)
    {
        var verts = new List<Vector2>();

        // Original vertices
        verts.AddRange(tri);

        // Bounding box
        float minX = MathF.Min(tri[0].X, MathF.Min(tri[1].X, tri[2].X));
        float maxX = MathF.Max(tri[0].X, MathF.Max(tri[1].X, tri[2].X));
        float minY = MathF.Min(tri[0].Y, MathF.Min(tri[1].Y, tri[2].Y));
        float maxY = MathF.Max(tri[0].Y, MathF.Max(tri[1].Y, tri[2].Y));

        int x0 = (int)MathF.Floor((minX - gridOrigin.X) / gridSize.X);
        int x1 = (int)MathF.Ceiling((maxX - gridOrigin.X) / gridSize.X);
        int y0 = (int)MathF.Floor((minY - gridOrigin.Y) / gridSize.Y);
        int y1 = (int)MathF.Ceiling((maxY - gridOrigin.Y) / gridSize.Y);

        // Vertical grid lines
        for (int i = x0; i <= x1; i++)
        {
            float x = gridOrigin.X + i * gridSize.X;
            IntersectLine(tri, true, x, verts);
        }

        // Horizontal grid lines
        for (int i = y0; i <= y1; i++)
        {
            float y = gridOrigin.Y + i * gridSize.Y;
            IntersectLine(tri, false, y, verts);
        }

        return Deduplicate(verts);
    }

    static void IntersectLine(
        Vector2[] tri,
        bool vertical,
        float value,
        List<Vector2> outVerts)
    {
        for (int i = 0; i < 3; i++)
        {
            Vector2 a = tri[i];
            Vector2 b = tri[(i + 1) % 3];

            if (vertical)
            {
                if ((a.X - value) * (b.X - value) > 0) continue;
                if (MathF.Abs(a.X - b.X) < EPS) continue;

                float t = (value - a.X) / (b.X - a.X);
                if (t >= 0 && t <= 1)
                    outVerts.Add(Vector2.Lerp(a, b, t));
            }
            else
            {
                if ((a.Y - value) * (b.Y - value) > 0) continue;
                if (MathF.Abs(a.Y - b.Y) < EPS) continue;

                float t = (value - a.Y) / (b.Y - a.Y);
                if (t >= 0 && t <= 1)
                    outVerts.Add(Vector2.Lerp(a, b, t));
            }
        }
    }

    static List<Vector2> Deduplicate(List<Vector2> verts)
    {
        var result = new List<Vector2>();
        foreach (var v in verts)
        {
            bool found = false;
            foreach (var e in result)
            {
                if (Vector2.DistanceSquared(v, e) < EPS * EPS)
                {
                    found = true;
                    break;
                }
            }
            if (!found)
                result.Add(v);
        }
        return result;
    }

    // ------------------------------------------------------------
    // TRIANGULATION (simple ear clipping)
    // ------------------------------------------------------------

    static List<Vector2[]> Triangulate(
        Vector2[] tri,
        List<Vector2> verts)
    {
        // Convex hull of all points inside triangle
        var polygon = ConvexHull(verts);

        var result = new List<Vector2[]>();
        var poly = new List<Vector2>(polygon);

        while (poly.Count >= 3)
        {
            for (int i = 0; i < poly.Count; i++)
            {
                Vector2 a = poly[(i + poly.Count - 1) % poly.Count];
                Vector2 b = poly[i];
                Vector2 c = poly[(i + 1) % poly.Count];

                if (!IsConvex(a, b, c)) continue;

                result.Add(new[] { a, b, c });
                poly.RemoveAt(i);
                break;
            }
        }

        return result;
    }

    static bool IsConvex(Vector2 a, Vector2 b, Vector2 c)
    {
        return Cross(b - a, c - b) >= 0;
    }

    static float Cross(Vector2 a, Vector2 b)
    {
        return a.X * b.Y - a.Y * b.X;
    }

    // Simple monotone hull (sufficient here)
    static List<Vector2> ConvexHull(List<Vector2> pts)
    {
        pts.Sort((a, b) =>
            a.X == b.X ? a.Y.CompareTo(b.Y) : a.X.CompareTo(b.X));

        var hull = new List<Vector2>();

        foreach (var p in pts)
        {
            while (hull.Count >= 2 &&
                Cross(hull[^1] - hull[^2], p - hull[^1]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        int lowerCount = hull.Count;
        for (int i = pts.Count - 2; i >= 0; i--)
        {
            var p = pts[i];
            while (hull.Count > lowerCount &&
                Cross(hull[^1] - hull[^2], p - hull[^1]) <= 0)
                hull.RemoveAt(hull.Count - 1);
            hull.Add(p);
        }

        hull.RemoveAt(hull.Count - 1);
        return hull;
    }
}
