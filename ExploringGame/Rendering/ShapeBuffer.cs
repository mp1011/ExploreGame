using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

public record ShapeBuffer(
    Shape Shape,
    VertexBuffer VertexBuffer,
    IndexBuffer IndexBuffer,
    int TriangleCount,
    TextureSheetKey Texture,
    RasterizerState RasterizerState = null,
    Room LightingGroup = null,
    DepthStencilState DepthStencilState = null)
{}
