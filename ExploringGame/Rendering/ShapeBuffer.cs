using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

public record ShapeBuffer(Shape Shape, VertexBuffer VertexBuffer, IndexBuffer IndexBuffer, int TriangleCount, TextureSheetKey Texture)
{
}
