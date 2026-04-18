using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering;

/// <summary>
/// Render pass for skybox using custom depth=1.0 shader.
/// </summary>
public class SkyboxRenderPass : IRenderPass
{
    private readonly SkyboxRenderEffect _skyboxEffect;
    private readonly VertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public int DrawOrder => 90; // Draw skybox near the end (before HUD/transparent)

    public SkyboxRenderPass(SkyboxRenderEffect skyboxEffect)
    {
        _skyboxEffect = skyboxEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Skybox;

    public ShapeBuffer BuildBuffer(GraphicsDevice graphicsDevice, Shape shape, QualityLevel quality)
    {
        // This method isn't used in the current implementation since ShapeBufferCreator
        // builds buffers with its own logic. This pass only handles drawing.
        throw new System.NotImplementedException("SkyboxRenderPass uses ShapeBufferCreator for buffer building");
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        // Take only the first skybox buffer to avoid rendering duplicates across segments
        var skyboxBuffer = shapeBuffers.FirstOrDefault();
        if (skyboxBuffer != null)
        {
            _skyboxEffect.Draw(graphicsDevice, new[] { skyboxBuffer }, view, projection);
        }
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _skyboxEffect.SetTextures(textureSheets);
    }
}
