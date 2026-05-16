using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

// lower entries are drawn first
public enum ShapeBufferType
{
    Static = 1,
    PointLight = 2,
    ActiveObject = 4,
    Normal = Static | PointLight | ActiveObject,
    Grass = 8,
    Skybox = 16,
    Glass = 32,
    UI = 64
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
