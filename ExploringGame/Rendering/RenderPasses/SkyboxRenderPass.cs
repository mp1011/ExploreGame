using ExploringGame.GameDebug;
using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Rendering.RenderEffects;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.RenderPasses;

/// <summary>
/// Render pass for skybox using custom depth=1.0 shader.
/// </summary>
public class SkyboxRenderPass : IRenderPass
{
    private readonly SkyboxRenderEffect _skyboxEffect;
    private readonly VertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public SkyboxRenderPass(SkyboxRenderEffect skyboxEffect)
    {
        _skyboxEffect = skyboxEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Skybox;

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        if (Debug.NoDepthStencil)
            return;

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
