using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GeometryBuilder
{
    internal class VertexList
    {
        public VertexPositionColor[] Array { get; }
        private Dictionary<(Vector3, Color), int> _indexCache = new();

        public VertexList(IEnumerable<Triangle> triangles)
        {
            Array = triangles.SelectMany(t =>
            {
                return t.Vertices.Select(v => new VertexPositionColor(v, t.Color));
            }).ToArray();

            for(int i = 0; i < Array.Length; i++)
            {
                var key = (Array[i].Position, Array[i].Color);
                _indexCache[key] = i;                
            }
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
