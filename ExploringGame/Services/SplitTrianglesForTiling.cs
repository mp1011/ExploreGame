using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public class SplitTrianglesForTiling
{
    public Triangle[] Execute(Shape shape, Triangle[] triangles)
    {
        if (!ShapeHasTiling(shape))
            return triangles;

        return new[] { Side.Top, Side.Bottom, Side.West, Side.North, Side.South, Side.East }
                     .SelectMany(side => SplitTrianglesIfNeeded(shape, triangles, side))
                     .ToArray();
    }

    private bool ShapeHasTiling(Shape shape)
    {
        return shape.Theme.MainTexture.Style.HasTiling() ||
               shape.Theme.SideTextures.Values.Any(p => p.Style.HasTiling());
    }

    private Triangle[] SplitTrianglesIfNeeded(Shape shape, Triangle[] triangles, Side side)
    {      
        var textureInfo = shape.TextureInfoForSide(side);
        var sideTriangles = triangles.Where(p => p.Side == side).ToArray();

        if (!textureInfo.Style.HasTiling())
            return sideTriangles;

        if (!sideTriangles.Any())
            return Array.Empty<Triangle>();

        var cornerVertices = sideTriangles.GetCornerVertices(side);
        var planeInfo = TilingPlaneHelper.ComputePlaneInfo(sideTriangles, cornerVertices);

        return sideTriangles.SelectMany(t => SplitTriangleIntoTiles(t, new Vector2(textureInfo.TileSize.Value, textureInfo.TileSize.Value), planeInfo))
                        .ToArray();

    }

    private IEnumerable<Triangle> SplitTriangleIntoTiles(Triangle triangle, Vector2 tileSize, TilingPlaneHelper.PlaneInfo planeInfo)
    {
        // Project the triangle to 2D
        var a2D = TilingPlaneHelper.ProjectTo2D(triangle.A, planeInfo);
        var b2D = TilingPlaneHelper.ProjectTo2D(triangle.B, planeInfo);
        var c2D = TilingPlaneHelper.ProjectTo2D(triangle.C, planeInfo);

        // Create a bounding box in 2D grid space
        var min = Vector2.Min(a2D, Vector2.Min(b2D, c2D));
        var max = Vector2.Max(a2D, Vector2.Max(b2D, c2D));

        int minX = (int)Math.Floor(min.X / tileSize.X);
        int maxX = (int)Math.Ceiling(max.X / tileSize.X);
        int minY = (int)Math.Floor(min.Y / tileSize.Y);
        int maxY = (int)Math.Ceiling(max.Y / tileSize.Y);

        // Collect grid vertices that intersect the triangle
        HashSet<Vector2> vertices = new HashSet<Vector2>();

        // Add the triangle's own vertices
        vertices.Add(a2D);
        vertices.Add(b2D);
        vertices.Add(c2D);

        // Add grid points that fall inside the triangle
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                Vector2 gridPoint = new Vector2(x * tileSize.X, y * tileSize.Y);
                if (PointInTriangle2D(gridPoint, a2D, b2D, c2D))
                    vertices.Add(gridPoint);
            }
        }

        // Add edge-grid intersections
        AddEdgeGridIntersections2D(a2D, b2D, tileSize, vertices);
        AddEdgeGridIntersections2D(b2D, c2D, tileSize, vertices);
        AddEdgeGridIntersections2D(c2D, a2D, tileSize, vertices);

        // Triangulate the result in 2D, then unproject back to 3D
        var triangulated2D = TriangulateVertices2D(vertices.ToList(), a2D, b2D, c2D, tileSize, minX, maxX, minY, maxY);

        // Unproject back to 3D
        return triangulated2D.Select(t => new Triangle(
            TilingPlaneHelper.UnprojectTo3D(t.A, planeInfo),
            TilingPlaneHelper.UnprojectTo3D(t.B, planeInfo),
            TilingPlaneHelper.UnprojectTo3D(t.C, planeInfo),
            triangle.TextureInfo,
            triangle.Side
        ));
    }

    private bool PointInTriangle2D(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
        {
            return (p1.X - p3.X) * (p2.Y - p3.Y) - (p2.X - p3.X) * (p1.Y - p3.Y);
        }

        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);

        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);

        return !(hasNeg && hasPos);
    }

    private void AddEdgeGridIntersections2D(Vector2 a, Vector2 b, Vector2 gridSize, HashSet<Vector2> output)
    {
        Vector2 d = b - a;

        // Vertical grid lines
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

        // Horizontal grid lines
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

    private struct Triangle2D
    {
        public Vector2 A, B, C;
        public Triangle2D(Vector2 a, Vector2 b, Vector2 c)
        {
            A = a; B = b; C = c;
        }
    }

    private List<Triangle2D> TriangulateVertices2D(List<Vector2> vertices, Vector2 a, Vector2 b, Vector2 c, Vector2 tileSize, int minX, int maxX, int minY, int maxY)
    {
        List<Triangle2D> result = new List<Triangle2D>();

        // Triangulate per grid cell
        for (int x = minX; x < maxX; x++)
        {
            for (int y = minY; y < maxY; y++)
            {
                Vector2 cellMin = new Vector2(x * tileSize.X, y * tileSize.Y);
                Vector2 cellMax = cellMin + tileSize;

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

                cellPoints.Sort((pa, pb) =>
                    MathF.Atan2(pa.Y - center.Y, pa.X - center.X)
                        .CompareTo(MathF.Atan2(pb.Y - center.Y, pb.X - center.X)));

                for (int i = 1; i + 1 < cellPoints.Count; i++)
                {
                    result.Add(new Triangle2D(cellPoints[0], cellPoints[i], cellPoints[i + 1]));
                }
            }
        }

        return result;
    }
}
