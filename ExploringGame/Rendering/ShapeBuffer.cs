using ExploringGame.GeometryBuilder;
using ExploringGame.Logics;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

// lower entries are drawn first
public enum ShapeBufferType
{
    Static = 1,
    ActiveObject = 2,
    Normal = Static | ActiveObject,
    Grass = 4,
    Skybox = 8,
    Glass = 16,
    UI = 32
}

public record ShapeBuffer(
    Shape Shape,
    VertexBuffer VertexBuffer,
    IndexBuffer IndexBuffer,
    int TriangleCount,
    TextureSheetKey Texture,
    RasterizerState RasterizerState = null,
    ILightingGroup LightingGroup = null,
    ILightSource[] PointLights = null,
    DepthStencilState DepthStencilState = null,
    ShapeBufferType Type = ShapeBufferType.Normal)
{}
