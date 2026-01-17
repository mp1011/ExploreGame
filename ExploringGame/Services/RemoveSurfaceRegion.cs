using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

class RemoveSurfaceRegion
{
    public Triangle[] Execute(Triangle[] triangles, Side surface, Placement2D placement, ViewFrom viewFrom)
    {
        if(placement.Left == 0 && placement.Right == 0 && placement.Top == 0 && placement.Bottom == 0)
            return triangles.Where(p => p.Side != surface).ToArray();

        var sideTriangles = triangles.Where(p=>p.Side == surface).ToArray();
        if (!sideTriangles.Any())
            return triangles;

        var sideCenter = sideTriangles.SelectMany(p => p.Vertices).Center();

        var face = new ConvexHull(sideTriangles.Select(p => p.As2D(sideCenter, viewFrom)).ToArray());
          face.Left += placement.Left;
          face.Right -= placement.Right;
          face.Top -= placement.Top;
          face.Bottom += placement.Bottom;

        return triangles.SelectMany(p=> RemoveFace(p, surface, face, sideCenter, viewFrom)).ToArray();
    }

    public Triangle[] RemoveCutouts(Shape shape, Triangle[] triangles)
    {
        foreach(var cutoutShape in shape.Children.OfType<ICutoutShape>())
        {
            var cutoutSurface = cutoutShape.Build().Where(p => p.Side == cutoutShape.ParentCutoutSide.Opposite()).ToArray();
            if (cutoutSurface.Length == 0)
                continue;

            cutoutSurface = cutoutSurface.Select(p => p.SetSide(cutoutShape.ParentCutoutSide, shape.GetSide(cutoutShape.ParentCutoutSide))).ToArray();

            var cutoutCenter = cutoutSurface.SelectMany(p => p.Vertices).Center();
            var cutout2D = new ConvexHull(cutoutSurface.Select(p => p.As2D(cutoutCenter, shape.ViewFrom)).ToArray());                       
            triangles = triangles.SelectMany(p => RemoveFace(p, cutoutShape.ParentCutoutSide, cutout2D, cutoutCenter, shape.ViewFrom)).ToArray();
        }

        return triangles;
    }
    
    private IEnumerable<Triangle> RemoveFace(Triangle triangle, Side surface, ConvexHull face, Vector3 sideCenter, ViewFrom viewFrom)
    {
        if (triangle.Side != surface)
            return new Triangle[] { triangle };

        var triangles2D = RemoveFace(triangle.As2D(sideCenter, viewFrom), face).ToArray();
        var result = triangles2D.Select(p => p.To3D(sideCenter, viewFrom)).ToArray();

        return result;
    }

    private IEnumerable<Triangle2D> RemoveFace(Triangle2D triangle, ConvexHull cutout)
    {
        return new TriangleSubtracter().Subtract(triangle, cutout);  
    }
}
