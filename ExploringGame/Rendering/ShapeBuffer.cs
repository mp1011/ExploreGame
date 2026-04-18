using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

public enum ShapeBufferType
{
    Normal,
    Grass,
    Glass,
    Skybox
}

public record ShapeBuffer(
    Shape Shape,
    VertexBuffer VertexBuffer,
    IndexBuffer IndexBuffer,
    int TriangleCount,
    TextureSheetKey Texture,
    RasterizerState RasterizerState = null,
    ILightingGroup LightingGroup = null,
    DepthStencilState DepthStencilState = null,
    ShapeBufferType Type = ShapeBufferType.Normal)
{}
