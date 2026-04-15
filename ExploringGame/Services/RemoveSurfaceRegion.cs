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
        foreach (var cutoutShape in shape.Children.OfType<ICutoutShape>())
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

    public Triangle[] RemoveCutouts_alt(Shape shape, Triangle[] triangles)
    {
        foreach(var cutoutShape in shape.Children.OfType<ICutoutShape>())
        {
            var cutoutSurface = cutoutShape.Build().Where(p => p.Side == cutoutShape.ParentCutoutSide.Opposite()).ToArray();
            if (cutoutSurface.Length == 0)
                continue;

            var parentSide = triangles.Where(p => p.Side == cutoutShape.ParentCutoutSide).ToArray();
            var sideCenter = parentSide.SelectMany(p => p.Vertices).Center();

            cutoutSurface = cutoutSurface.Select(p => p.SetSide(cutoutShape.ParentCutoutSide, shape.GetSide(cutoutShape.ParentCutoutSide))).ToArray();
            var cutout2D = new ConvexHull(cutoutSurface.Select(p => p.As2D(sideCenter, shape.ViewFrom)).ToArray());
                      
            triangles = triangles.SelectMany(p => RemoveFace(p, cutoutShape.ParentCutoutSide, cutout2D, sideCenter, shape.ViewFrom)).ToArray();
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

      /// <summary>
    /// Subtracts the volume of a cutout shape from triangles.
    /// Automatically handles projection based on each triangle's orientation.
    /// </summary>
    public Triangle[] SubtractShape(Triangle[] triangles, Shape cutoutShape)
    {
        // Group triangles by side so we can process each surface independently
        var trianglesBySide = triangles.GroupBy(t => t.Side).ToArray();

        var result = new List<Triangle>();

        foreach (var group in trianglesBySide)
        {
            var side = group.Key;
            var sideTriangles = group.ToArray();

            // Calculate center for this surface
            var sideCenter = sideTriangles.SelectMany(p => p.Vertices).Center();

            // Determine viewFrom based on the side (assume Outside for now, can be made configurable if needed)
            var viewFrom = ViewFrom.Outside;

            // Get cutout bounds projected onto this surface
            var cutoutBounds = GetShapeBoundsOn2DSurface(cutoutShape, side, sideCenter, viewFrom);

            // Check if the cutout actually intersects this surface
            if (!DoesCutoutIntersectSurface(cutoutShape, side))
            {
                // No intersection, keep all triangles on this side as-is
                result.AddRange(sideTriangles);
            }
            else
            {
                // Subtract the cutout from each triangle
                foreach (var triangle in sideTriangles)
                {
                    result.AddRange(RemoveFace(triangle, side, cutoutBounds, sideCenter, viewFrom));
                }
            }
        }

        return result.ToArray();
    }

    /// <summary>
    /// Checks if a cutout shape intersects with a specific surface
    /// </summary>
    private bool DoesCutoutIntersectSurface(Shape cutoutShape, Side surface)
    {
        // For axis-aligned surfaces, check if the cutout's bounds overlap the surface plane
        switch (surface)
        {
            case Side.North:
            case Side.South:
                // Check if cutout spans this Z plane
                return cutoutShape.GetSide(Side.North) <= cutoutShape.GetSide(Side.South);
            case Side.East:
            case Side.West:
                // Check if cutout spans this X plane
                return cutoutShape.GetSide(Side.West) <= cutoutShape.GetSide(Side.East);
            case Side.Top:
            case Side.Bottom:
                // Check if cutout spans this Y plane
                return cutoutShape.GetSide(Side.Bottom) <= cutoutShape.GetSide(Side.Top);
            default:
                return true;
        }
    }

    /// <summary>
    /// Projects a 3D shape's bounds onto a 2D surface
    /// </summary>
    private ConvexHull GetShapeBoundsOn2DSurface(Shape shape, Side surface, Vector3 sideCenter, ViewFrom viewFrom)
    {
        // Get the 3D bounding box of the shape
        var bounds = new[]
        {
            new Vector3(shape.GetSide(Side.West), shape.GetSide(Side.Bottom), shape.GetSide(Side.North)),
            new Vector3(shape.GetSide(Side.West), shape.GetSide(Side.Bottom), shape.GetSide(Side.South)),
            new Vector3(shape.GetSide(Side.West), shape.GetSide(Side.Top), shape.GetSide(Side.North)),
            new Vector3(shape.GetSide(Side.West), shape.GetSide(Side.Top), shape.GetSide(Side.South)),
            new Vector3(shape.GetSide(Side.East), shape.GetSide(Side.Bottom), shape.GetSide(Side.North)),
            new Vector3(shape.GetSide(Side.East), shape.GetSide(Side.Bottom), shape.GetSide(Side.South)),
            new Vector3(shape.GetSide(Side.East), shape.GetSide(Side.Top), shape.GetSide(Side.North)),
            new Vector3(shape.GetSide(Side.East), shape.GetSide(Side.Top), shape.GetSide(Side.South))
        };

        // Project the bounds onto 2D surface
        var faceBasis = FaceBasis.FromSide(surface, viewFrom);
        var points2D = bounds.Select(p => p.Project(sideCenter, faceBasis.U, faceBasis.V)).ToArray();

        // Create a simple rectangle from min/max bounds
        var minX = points2D.Min(p => p.X);
        var maxX = points2D.Max(p => p.X);
        var minY = points2D.Min(p => p.Y);
        var maxY = points2D.Max(p => p.Y);

        var rectangle = new[]
        {
            new Vector2(minX, minY),
            new Vector2(maxX, minY),
            new Vector2(maxX, maxY),
            new Vector2(minX, maxY)
        };

        // Create a dummy triangle to satisfy ConvexHull constructor
        var dummyTriangle = new Triangle(Vector3.Zero, Vector3.Zero, Vector3.Zero, null, surface);
        var triangles2D = new[]
        {
            new Triangle2D(rectangle[0], rectangle[1], rectangle[2], dummyTriangle),
            new Triangle2D(rectangle[0], rectangle[2], rectangle[3], dummyTriangle)
        };

        return new ConvexHull(triangles2D);
    }
}
