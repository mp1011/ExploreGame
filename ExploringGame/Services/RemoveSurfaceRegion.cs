using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

class RemoveSurfaceRegion
{
    public Triangle[] Execute(Triangle[] triangles, Side surface, Placement2D placement)
    {
        var sideTriangles = triangles.Where(p=>p.Side == surface).ToArray();
        var sideCenter = sideTriangles.SelectMany(p => p.Vertices).Center();

        var face = new Triangle2DGroup(triangles.Where(p => p.Side == surface).Select(p => p.As2D(sideCenter)).ToArray());
          face.Left += placement.Left;
          face.Right -= placement.Right;
          face.Top -= placement.Top;
          face.Bottom += placement.Bottom;

      //  return triangles.Where(p=>p.Side != surface).Union(face.Triangles.Select(p => p.To3D(sideCenter))).ToArray();

        return triangles.SelectMany(p=> RemoveFace(p, surface, face, sideCenter)).ToArray();
    }

    private IEnumerable<Triangle> RemoveFace(Triangle triangle, Side surface, Triangle2DGroup face, Vector3 sideCenter)
    {
        if (triangle.Side != surface)
            return new Triangle[] { triangle };

        var triangles2D = RemoveFace(triangle.As2D(sideCenter), face).ToArray();
        var result = triangles2D.Select(p => p.To3D(sideCenter));

        // todo - why is this?
      //  if (surface == Side.North || surface == Side.East)
        //    result = result.Select(p => p.Invert());

        return result;
    }

    private IEnumerable<Triangle2D> RemoveFace(Triangle2D triangle, Triangle2DGroup cutout)
    {
        return new TriangleSubtracter().Subtract(triangle, cutout);  
    }
}
