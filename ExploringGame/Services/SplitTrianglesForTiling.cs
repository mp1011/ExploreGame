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

        var sideCenter = sideTriangles.SelectMany(p => p.Vertices).Center();
        var triangles2D = sideTriangles.Select(p => p.As2D(sideCenter, shape.ViewFrom)).ToArray();

        triangles2D = triangles2D.SelectMany(p => SplitTrianglesIntoTiles(p, new Vector2(textureInfo.TileSize.Value, textureInfo.TileSize.Value)))
                                 .ToArray();

        return triangles2D.Select(p => p.To3D(sideCenter, shape.ViewFrom)).ToArray();
    }

    private IEnumerable<Triangle2D> SplitTrianglesIntoTiles(Triangle2D triangle, Vector2 tileSize)
    {
        var result = TriangleSubdivider.Subdivide(triangle, tileSize.X, tileSize.Y);

        PolygonVisualizer.SavePolygonImage("sub_divided", result.ToArray());

        return result;
    }
}
