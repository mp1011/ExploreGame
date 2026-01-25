using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using System.Collections.Generic;
using System.Drawing;
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

    private IEnumerable<Triangle> SplitTrianglesIfNeeded(Shape shape, Triangle[] triangles, Side side)
    {
        var sideTriangles = triangles.Where(p => p.Side == side).ToArray();

        if(!shape.TextureInfoForSide(side).Style.HasTiling())
            return sideTriangles;

        return SplitTriangles(sideTriangles, 0.3f);
    }

    private IEnumerable<Triangle> SplitTriangles(Triangle[] triangles, float targetWidth)
    {
        return new SierpinskiSplitter().ExecuteUntilLengthReached(triangles, targetWidth);
    }
}
