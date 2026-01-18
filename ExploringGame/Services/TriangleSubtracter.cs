using Clipper2Lib;
using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using LibTessDotNet;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class TriangleSubtracter
{
    private const int mult = 100000;

    public static bool DebugVisualize = false;

    public Triangle2D[] Subtract(Triangle2D triangle, ConvexHull cutout)
    {        
        if(DebugVisualize)
            PolygonVisualizer.SavePolygonImage("subtract_before", triangle, cutout);

        if (CutoutEntirelyInsideTriangle(triangle, cutout))
            return SplitTriangleAroundConvexHole(triangle, cutout);

        var trianglePath = new Paths64
        {
            new Path64
            {
                 new Point64(triangle.A.X * mult, triangle.A.Y * mult),
                 new Point64(triangle.B.X * mult, triangle.B.Y * mult),
                 new Point64(triangle.C.X * mult, triangle.C.Y * mult),
                 new Point64(triangle.A.X * mult, triangle.A.Y * mult),
            }
        };

        var cutoutVertices = cutout.Vertices.Select(p => new Point64(p.X * mult, p.Y * mult)).ToList();
        cutoutVertices.Add(new Point64(cutout.Vertices.First().X * mult, cutout.Vertices.First().Y * mult));
        var cutoutPath = new Paths64
        {
            new Path64(cutoutVertices),
        };

        var result = Clipper.Difference(trianglePath, cutoutPath, FillRule.NonZero); 

        
        var resultTriangles = result.SelectMany(p=> PolygonToTriangles(p, triangle)).ToArray();

        if (DebugVisualize)
            PolygonVisualizer.SavePolygonImage("subtract_result", resultTriangles);

        return resultTriangles;
    }

    private bool CutoutEntirelyInsideTriangle(Triangle2D triangle, ConvexHull cutout)
    {
        return cutout.Vertices.All(p => triangle.ContainsPoint(p));
    }

    public static Triangle2D[] SplitTriangleAroundConvexHole(Triangle2D triangle, ConvexHull cutout)
    {
        // 1. Create tessellator
        var tess = new Tess();

        // 2. Add outer contour (triangle)
        tess.AddContour(new ContourVertex[]
        {
        new ContourVertex() { Position = new Vec3 { X = triangle.A.X, Y = triangle.A.Y, Z = 0 } },
        new ContourVertex() { Position = new Vec3 { X = triangle.B.X, Y = triangle.B.Y, Z = 0 } },
        new ContourVertex() { Position = new Vec3 { X = triangle.C.X, Y = triangle.C.Y, Z = 0 } },
        }, ContourOrientation.Clockwise);

        // 3. Add hole contour (convex hull)
        var hull = cutout.Vertices;
        var holeVertices = new ContourVertex[hull.Length];
        for (int i = 0; i < hull.Length; i++)
            holeVertices[i] = new ContourVertex { Position = new Vec3 { X = hull[i].X, Y = hull[i].Y, Z = 0 } };

        tess.AddContour(holeVertices, ContourOrientation.CounterClockwise);

        // 4. Tessellate
        tess.Tessellate(WindingRule.Positive, ElementType.Polygons, 3);

        // 5. Extract triangles
        List<Triangle2D> triangles = new List<Triangle2D>();
        for (int i = 0; i < tess.ElementCount; i++)
        {
            int i0 = tess.Elements[i * 3 + 0];
            int i1 = tess.Elements[i * 3 + 1];
            int i2 = tess.Elements[i * 3 + 2];

            var a = new Vector2(tess.Vertices[i0].Position.X, tess.Vertices[i0].Position.Y);
            var b = new Vector2(tess.Vertices[i1].Position.X, tess.Vertices[i1].Position.Y);
            var c = new Vector2(tess.Vertices[i2].Position.X, tess.Vertices[i2].Position.Y);
            triangles.Add(new Triangle2D(a, b, c, triangle.Original));
        }

        if (DebugVisualize)
            PolygonVisualizer.SavePolygonImage("subtract_result_tess", triangles.ToArray());
        return triangles.ToArray(); ;
    }


    private Triangle2D[] PolygonToTriangles(Path64 path, Triangle2D triangle)
    {
        var pts = path.Select(q => new Vector2(q.X / (float)mult, q.Y / (float)mult)).ToArray();

        var tess = new Tess();

        // Outer contour (CCW)
        tess.AddContour(pts.Select(p => new ContourVertex(new Vec3(p.X, p.Y, 0))).ToArray(), ContourOrientation.CounterClockwise);
        tess.Tessellate(WindingRule.NonZero, ElementType.Polygons, polySize: 3);

        List<Triangle2D> triangles = new();

        for (int i = 0; i < tess.ElementCount; i++)
        {
            int i0 = tess.Elements[i * 3 + 0];
            int i1 = tess.Elements[i * 3 + 1];
            int i2 = tess.Elements[i * 3 + 2];

            var v0 = tess.Vertices[i0].Position;
            var v1 = tess.Vertices[i1].Position;
            var v2 = tess.Vertices[i2].Position;

            triangles.Add(new Triangle2D(
                original: triangle.Original,
                a: new Vector2((float)v0.X, (float)v0.Y),
                b: new Vector2((float)v1.X, (float)v1.Y),
                c: new Vector2((float)v2.X, (float)v2.Y)
            ));
        }

        return triangles.ToArray();
    }
}
