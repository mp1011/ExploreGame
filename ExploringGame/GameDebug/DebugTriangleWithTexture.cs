using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.GameDebug
{
    class DebugTriangleWithTexture : IDebugPolygon2d
    {

        public IEnumerable<Vector2> Vertices { get; }

        public (Vector2, string)[] AnnotatedVertices { get;  }

        public DebugTriangleWithTexture(List<VertexPositionColorNormalTexture> vertices, Side side)
        {
            switch(side)
            {
                case Side.Bottom:
                case Side.Top:
                    AnnotatedVertices = vertices.Select(p => (new Vector2(p.Position.X, p.Position.Z), Annotation(p))).ToArray();
                    break;
                default:
                    throw new System.NotImplementedException();
            }

            Vertices = AnnotatedVertices.Select(p => p.Item1).ToArray();
        }

        private string Annotation(VertexPositionColorNormalTexture v)
        {
            return $"{v.TextureCoordinate.X.ToString("0.00")}:{v.TextureCoordinate.Y.ToString("0.00")}";
        }
    }
}
