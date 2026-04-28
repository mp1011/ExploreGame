using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

// lower entries are drawn first
public enum ShapeBufferType
{
    Normal,
    Grass,
    Skybox,
    Glass,
    UI
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
