using Clipper2Lib;
using ExploringGame.GeometryBuilder;
using LibTessDotNet;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class TriangleSubtracter
{
    private const int mult = 100000;

    public Triangle2D[] Subtract(Triangle2D triangle, Triangle2DGroup cutout)
    {        
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


        var cutoutPath = new Paths64
        {
            new Path64(cutout.ConvexHull().Select(p=> new Point64(p.X * mult, p.Y * mult))),
        };

        var result = Clipper.Difference(trianglePath, cutoutPath, FillRule.NonZero);
        return result.SelectMany(p=> PolygonToTriangles(p, triangle)).ToArray();
       
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
