using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Renders light sources
/// </summary>
public class PointLightRenderPass : IRenderPass
{
    private readonly PointLightRenderEffect _renderEffect;
    private readonly VertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public PointLightRenderPass(PointLightRenderEffect renderEffect)
    {
        _renderEffect = renderEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.PointLight;

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
       // _renderEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _renderEffect.SetTextures(textureSheets);
    }
}
