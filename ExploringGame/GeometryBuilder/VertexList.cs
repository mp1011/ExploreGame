using ExploringGame.Extensions;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder
{
    internal class VertexList
    {
        public VertexPositionColorTexture[] Array { get; }
        private Dictionary<(Vector3, Color), int> _indexCache = new();
        private TextureSheet _textureSheet;

        public VertexList(Dictionary<Shape, Triangle[]> shapeTriangles, TextureSheet textureSheet)
        {
            _textureSheet = textureSheet;
            List<VertexPositionColorTexture> vertices = new();

            foreach (Shape shape in shapeTriangles.Keys)
            {
                var triangles = shapeTriangles[shape]; 
                vertices.AddRange(CreateVertices(Side.West, triangles));
                vertices.AddRange(CreateVertices(Side.North, triangles));
                vertices.AddRange(CreateVertices(Side.East, triangles));
                vertices.AddRange(CreateVertices(Side.South, triangles));
                vertices.AddRange(CreateVertices(Side.Top, triangles));
                vertices.AddRange(CreateVertices(Side.Bottom, triangles));
            }

            Array = vertices.ToArray();

            for(int i = 0; i < Array.Length; i++)
            {
                var key = (Array[i].Position, Array[i].Color);
                _indexCache[key] = i;                
            }
        }

        private IEnumerable<VertexPositionColorTexture> CreateVertices(Side side, IEnumerable<Triangle> triangles)
        {
            var sideTriangles = triangles.Where(p => p.Side == side).ToArray();

            var cornerVertices = GetCornerVertices(side, sideTriangles);

            return triangles.Where(p => p.Side == side).SelectMany(t =>
            {
                return t.Vertices.Select(v => new VertexPositionColorTexture(v, t.TextureInfo.Color, 
                    CalcTextureCoordinates(side, t.TextureInfo.Key, v, cornerVertices.Item1, cornerVertices.Item2)));
            }).ToArray();

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
            if(!sideTriangles.Any())
                return (Vector3.Zero, Vector3.Zero);

            var verts = sideTriangles.SelectMany(p => p.Vertices).ToArray();
            var boundingBoxCorners = verts.GetBoundingBoxCorners(side);

            return (verts.OrderBy(p => p.SquaredDistance(boundingBoxCorners.Item1)).First(),
                    verts.OrderBy(p => p.SquaredDistance(boundingBoxCorners.Item2)).First());
        }

        public Vector2 CalcTextureCoordinates(Side side, TextureKey textureKey, Vector3 position, Vector3 topLeftCorner, Vector3 bottomRightCorner)
        {
            var position2d = position.As2D(side);
            var topLeftCorner2d = topLeftCorner.As2D(side);
            var bottomRightCorner2d = bottomRightCorner.As2D(side);

            var coordinates = position2d.RelativeUnitPosition(topLeftCorner2d, bottomRightCorner2d);
            return _textureSheet.TexturePosition(textureKey, coordinates);
        }

        public int Length => Array.Length;

      
        public int IndexOf(Vector3 vertex, Color color)
        {
            if (_indexCache.TryGetValue((vertex, color), out int index))
            {
                return index;
            }
            return -1;
        }
    }
}
