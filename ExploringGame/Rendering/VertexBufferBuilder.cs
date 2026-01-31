using ExploringGame.Extensions;
using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExploringGame.Rendering;

public class VertexBufferBuilder
{
    public (VertexBuffer, IndexBuffer, int) Build(Dictionary<Shape, Triangle[]> triangles, TextureSheet textureSheet, GraphicsDevice graphicsDevice)
    {
        List<VertexPositionColorNormalTexture> vertices = new();
        List<int> indices = new();
        Dictionary<(Vector3, Color, Vector2), int> indexCache = new();

        BuildBuffers(triangles, vertices, indices, indexCache, textureSheet);

        var vb = new VertexBuffer(graphicsDevice, typeof(VertexPositionColorNormalTexture), vertices.Count, BufferUsage.WriteOnly);
        vb.SetData(vertices.ToArray());

        var ib = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Count, BufferUsage.WriteOnly);
        ib.SetData(indices.ToArray());

        return (vb, ib, triangles.SelectMany(p => p.Value).Count());
    }

    private void BuildBuffers(Dictionary<Shape, Triangle[]> shapeTriangles,
                             List<VertexPositionColorNormalTexture> vertices,
                             List<int> indices,
                             Dictionary<(Vector3, Color, Vector2), int> indexCache,
                             TextureSheet textureSheet)
    {
        foreach (Shape shape in shapeTriangles.Keys)
        {
            var triangles = shapeTriangles[shape];
            CreateVertices(Side.West, textureSheet, triangles, vertices, indices, indexCache);
            CreateVertices(Side.North, textureSheet, triangles, vertices, indices, indexCache);
            CreateVertices(Side.East, textureSheet, triangles, vertices, indices, indexCache);
            CreateVertices(Side.South, textureSheet, triangles, vertices, indices, indexCache);
            CreateVertices(Side.Top, textureSheet, triangles, vertices, indices, indexCache);
            CreateVertices(Side.Bottom, textureSheet, triangles, vertices, indices, indexCache);
        }
    }


    private void CreateVertices(Side side,
                                TextureSheet textureSheet,
                                IEnumerable<Triangle> triangles,
                                List<VertexPositionColorNormalTexture> vertices,
                                List<int> indices,
                                Dictionary<(Vector3, Color, Vector2), int> indexCache)
    {
        var sideTriangles = triangles.Where(p => p.Side == side).ToArray();

        var cornerVertices = sideTriangles.GetCornerVertices(side);

        foreach (var triangle in sideTriangles)
        {
            foreach (var vertex in triangle.Vertices)
            {
                var textureCoords = CalcTextureCoordinates(side, textureSheet, triangle, vertex, cornerVertices);
                int index;
                if (!indexCache.TryGetValue((vertex, triangle.TextureInfo.Color, textureCoords), out index))
                {
                    indexCache.Add((vertex, triangle.TextureInfo.Color, textureCoords), vertices.Count);
                    indices.Add(vertices.Count);
                    vertices.Add(new VertexPositionColorNormalTexture(vertex, triangle.TextureInfo.Color, triangle.Normal, textureCoords));
                }
                else
                {
                    indices.Add(index);
                }
            }
        }            
    }

    public Vector2 CalcTextureCoordinates(Side side, TextureSheet textureSheet, Triangle triangle, Vector3 position, (Vector3, Vector3) corners)
    {
        var texture = triangle.TextureInfo;
        var textureCoordinates = texture.Style switch
        {
            TextureStyle.FillSide => CalcTextureCoordinates_FillSide(side, textureSheet, texture, position, corners),
            TextureStyle.Tile => CalcTextureCoordinates_Tile(side, textureSheet, triangle, position, corners),
            TextureStyle.HorizontalRepeat => CalcTextureCoordinates_HorizontalRepeat(side, textureSheet, triangle, texture, position, corners),
            _ => throw new System.ArgumentException($"Unknown texture style {texture.Style}")
        };

        return textureSheet.TexturePosition(texture.Key, textureCoordinates);
    }

    private Vector2 CalcTextureCoordinates_FillSide(Side side, TextureSheet textureSheet, TextureInfo texture, Vector3 position, (Vector3, Vector3) corners)
    {
        var position2d = position.As2D(side);
        var topLeftCorner2d = corners.Item1.As2D(side);
        var bottomRightCorner2d = corners.Item2.As2D(side);

        var coordinates = position2d.RelativeUnitPosition(topLeftCorner2d, bottomRightCorner2d);
        return coordinates;
    }

    private Vector2 CalcTextureCoordinates_Tile(Side side, TextureSheet textureSheet, Triangle triangle, Vector3 position, (Vector3, Vector3) corners)
    {
        var gridOrigin = corners.Item1;

        var axisUV = side.GetAxisUV();
        var axisU = axisUV.Item1;
        var axisV = axisUV.Item2;

        var textureSize = triangle.TextureInfo.TileSize.Value;

        var u = position.AxisValue(axisU) - gridOrigin.AxisValue(axisU);
        var v = position.AxisValue(axisV) - gridOrigin.AxisValue(axisV);

        var uMod = u.NMod(textureSize) / textureSize;
        var vMod = v.NMod(textureSize) / textureSize;

        var isUMax = position.AxisValue(axisU) == triangle.Vertices.Max(p => p.AxisValue(axisU));
        var isVMax = position.AxisValue(axisV) == triangle.Vertices.Max(p => p.AxisValue(axisV));

        if (isUMax && uMod == 0f)
            uMod = 1.0f;

        if (isVMax && vMod == 0f)
            vMod = 1.0f;

        return new Vector2(uMod, vMod);
    }


    private Vector2 CalcTextureCoordinates_Tile_deleteme(Side side, TextureSheet textureSheet, Triangle triangle, Vector3 position, (Vector3, Vector3) corners)
    {
        var axisUV = side.GetAxisUV();
        var axisU = axisUV.Item1;
        var axisV = axisUV.Item2;

        var textureSize = triangle.TextureInfo.TileSize.Value;

        var uBegin = Math.Min(corners.Item1.AxisValue(axisU), corners.Item2.AxisValue(axisU));
        var vBegin = Math.Min(corners.Item1.AxisValue(axisV), corners.Item2.AxisValue(axisV));

        var u = position.AxisValue(axisU) - uBegin;
        var v = position.AxisValue(axisV) - vBegin;

        var uMod = u.NMod(textureSize) / textureSize;
        var vMod = v.NMod(textureSize) / textureSize;

        var isUMax = position.AxisValue(axisU) == triangle.Vertices.Max(p => p.AxisValue(axisU));
        var isVMax = position.AxisValue(axisV) == triangle.Vertices.Max(p => p.AxisValue(axisV));

        if (isUMax && uMod == 0f)
            uMod = 1.0f;

        if (isVMax && vMod == 0f)
            vMod = 1.0f;

        return new Vector2(uMod, vMod);
    }

    private Vector2 CalcTextureCoordinates_HorizontalRepeat(Side side, TextureSheet textureSheet, Triangle triangle, TextureInfo texture, Vector3 position, (Vector3,Vector3) corners)
    {     
        var uCoordindates = CalcTextureCoordinates_Tile(side, textureSheet, triangle, position, corners);
        var vCoordinates = CalcTextureCoordinates_FillSide(side, textureSheet, texture, position, corners);

        return new Vector2(uCoordindates.X, vCoordinates.Y);
    }

}