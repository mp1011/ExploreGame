using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace ExploringGame.Services;

public class SplitTrianglesForTiling
{
    private const float TargetWidth = 10.0f;

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
        return shape.MainTexture.Style == TextureStyle.XZTile ||
               shape.SideTextures.Values.Any(p => p.Style == TextureStyle.XZTile);
    }

    private IEnumerable<Triangle> SplitTrianglesIfNeeded(Shape shape, Triangle[] triangles, Side side)
    {
        var sideTriangles = triangles.Where(p => p.Side == side).ToArray();

        if(shape.TextureInfoForSide(side).Style != TextureStyle.XZTile)
            return sideTriangles;

        return SplitTriangles(sideTriangles);
    }

    private IEnumerable<Triangle> SplitTriangles(Triangle[] triangles)
    {
        return new SierpinskiSplitter().ExecuteUntilLengthReached(triangles, TargetWidth);
    }
}
