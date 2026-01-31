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
            return triangles;

        var corner = sideTriangles.GetCornerVertices(side).Item1;


        return sideTriangles.SelectMany(t => SplitTriangleIntoTiles(t, new Vector2(textureInfo.TileSize.Value, textureInfo.TileSize.Value), corner))
                        .ToArray();
        
    }

    private IEnumerable<Triangle> SplitTriangleIntoTiles(Triangle triangle, Vector2 tileSize, Vector3 gridOrigin)
    {
        return TriangleSubdivider.Subdivide(triangle, tileSize, gridOrigin);
    }
}
