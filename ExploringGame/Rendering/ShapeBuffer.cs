using ExploringGame.GeometryBuilder;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

public record ShapeBuffer(Shape Shape, VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int TriangleCount)
{
}
