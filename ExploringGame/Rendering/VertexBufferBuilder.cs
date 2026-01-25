using ExploringGame.Extensions;
using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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

        var cornerVertices = GetCornerVertices(side, sideTriangles);

        foreach (var triangle in sideTriangles)
        {
            foreach (var vertex in triangle.Vertices)
            {
                var textureCoords = CalcTextureCoordinates(side, textureSheet, triangle.TextureInfo, vertex, cornerVertices.Item1, cornerVertices.Item2);
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

    /// <summary>
    /// Returns the two vertices which should have 0,0 and 1,1 texture coordinates
    /// </summary>
    /// <param name="side"></param>
    /// <param name="sideTriangles"></param>
    /// <returns></returns>
    /// <exception cref="System.NotImplementedException"></exception>
    private (Vector3, Vector3) GetCornerVertices(Side side, IEnumerable<Triangle> sideTriangles)
    {
        if (!sideTriangles.Any())
            return (Vector3.Zero, Vector3.Zero);

        var verts = sideTriangles.SelectMany(p => p.Vertices).ToArray();
        var boundingBoxCorners = verts.GetBoundingBoxCorners(side);

        return (verts.OrderBy(p => p.SquaredDistance(boundingBoxCorners.Item1)).First(),
                verts.OrderBy(p => p.SquaredDistance(boundingBoxCorners.Item2)).First());
    }

    public Vector2 CalcTextureCoordinates(Side side, TextureSheet textureSheet, TextureInfo texture, Vector3 position, Vector3 topLeftCorner, Vector3 bottomRightCorner)
    {
        var textureCoordinates = texture.Style switch
        {
            TextureStyle.FillSide => CalcTextureCoordinates_FillSide(side, textureSheet, texture, position, topLeftCorner, bottomRightCorner),
            TextureStyle.XZTile => CalcTextureCoordinates_XZTile(side, textureSheet, texture, position, topLeftCorner, bottomRightCorner),
            TextureStyle.HorizontalRepeat => CalcTextureCoordinates_HorizontalRepeat(side, textureSheet, texture, position, topLeftCorner, bottomRightCorner),
            _ => throw new System.ArgumentException($"Unknown texture style {texture.Style}")
        };

        return textureSheet.TexturePosition(texture.Key, textureCoordinates);
    }

    private Vector2 CalcTextureCoordinates_FillSide(Side side, TextureSheet textureSheet, TextureInfo texture, Vector3 position, Vector3 topLeftCorner, Vector3 bottomRightCorner)
    {
        var position2d = position.As2D(side);
        var topLeftCorner2d = topLeftCorner.As2D(side);
        var bottomRightCorner2d = bottomRightCorner.As2D(side);

        var coordinates = position2d.RelativeUnitPosition(topLeftCorner2d, bottomRightCorner2d);
        return coordinates;
    }

    private Vector2 CalcTextureCoordinates_XZTile(Side side, TextureSheet textureSheet, TextureInfo texture, Vector3 position, Vector3 topLeftCorner, Vector3 bottomRightCorner)
    {
        var tileSize = texture.TileSize.Value;
        var tx = position.X.NMod(tileSize) / tileSize;
        var ty = position.Z.NMod(tileSize) / tileSize;
        return new Vector2(tx, ty);
    }

    private Vector2 CalcTextureCoordinates_HorizontalRepeat(Side side, TextureSheet textureSheet, TextureInfo texture, Vector3 position, Vector3 topLeftCorner, Vector3 bottomRightCorner)
    {     
        var coordinates = CalcTextureCoordinates_FillSide(side, textureSheet, texture, position, topLeftCorner, bottomRightCorner);

        switch (side)
        {
            case Side.West:
            case Side.East:
                var tileSize = texture.TileSize.Value;
                var tx = position.Z.NMod(tileSize) / tileSize;
                return new Vector2(tx, coordinates.Y);
            case Side.North:
            case Side.South:
                tileSize = texture.TileSize.Value;
                tx = position.X.NMod(tileSize) / tileSize;
                return new Vector2(tx, coordinates.Y);

            default:
                throw new System.ArgumentException("HorizontalRepeat texture style cannot apply to Top or Bottom sides");
        }
    }

}