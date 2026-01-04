using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

class RemoveSurfaceRegion
{
    public static bool DebugShowCutout = false;

    public Triangle[] Execute(Triangle[] triangles, Side surface, Placement2D placement, ViewFrom viewFrom)
    {
        if(placement.Left == 0 && placement.Right == 0 && placement.Top == 0 && placement.Bottom == 0)
            return triangles.Where(p => p.Side != surface).ToArray();

        var sideTriangles = triangles.Where(p=>p.Side == surface).ToArray();
        var sideCenter = sideTriangles.SelectMany(p => p.Vertices).Center();

        var face = new Triangle2DGroup(triangles.Where(p => p.Side == surface).Select(p => p.As2D(sideCenter, viewFrom)).ToArray());
          face.Left += placement.Left;
          face.Right -= placement.Right;
          face.Top -= placement.Top;
          face.Bottom += placement.Bottom;

        if(DebugShowCutout)
            return triangles.Where(p=>p.Side != surface).Union(face.Triangles.Select(p => p.To3D(sideCenter, viewFrom))).ToArray();

        return triangles.SelectMany(p=> RemoveFace(p, surface, face, sideCenter, viewFrom)).ToArray();
    }

    private IEnumerable<Triangle> RemoveFace(Triangle triangle, Side surface, Triangle2DGroup face, Vector3 sideCenter, ViewFrom viewFrom)
    {
        if (triangle.Side != surface)
            return new Triangle[] { triangle };

        var triangles2D = RemoveFace(triangle.As2D(sideCenter, viewFrom), face).ToArray();
        var result = triangles2D.Select(p => p.To3D(sideCenter, viewFrom)).ToArray();

        return result;
    }

    private IEnumerable<Triangle2D> RemoveFace(Triangle2D triangle, Triangle2DGroup cutout)
    {
        return new TriangleSubtracter().Subtract(triangle, cutout);  
    }
}
