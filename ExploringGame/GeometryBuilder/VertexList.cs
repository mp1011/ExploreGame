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

        public VertexList(IEnumerable<Triangle> triangles)
        {
            Array = triangles.SelectMany(t =>
            {
                return t.Vertices.Select(v => new VertexPositionColor(v, t.Color));
            }).ToArray();
        }

        public int Length => Array.Length;

      
        public int IndexOf(Vector3 vertex, Color color)
        {
            // this can definitely be optimized
            for(int i = 0; i < Array.Length; i++)
            {
                if (Array[i].Position == vertex && Array[i].Color == color)
                    return i;
            }

            return -1;
        }
    }
}
