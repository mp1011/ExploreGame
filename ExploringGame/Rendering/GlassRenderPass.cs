using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

/// <summary>
/// Render pass for glass panes with transparency.
/// Rendered after opaque geometry but before grass/skybox.
/// </summary>
public class GlassRenderPass : IRenderPass
{
    private readonly GlassRenderEffect _glassEffect;
    private LoadedTextureSheets _textureSheets;

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Glass;

    public GlassRenderPass(GlassRenderEffect glassEffect)
    {
        _glassEffect = glassEffect;
    }

    public ShapeBuffer BuildBuffer(GraphicsDevice graphicsDevice, Shape shape, QualityLevel quality)
    {
        // Not used - ShapeBufferCreator builds buffers
        throw new System.NotImplementedException("GlassRenderPass uses ShapeBufferCreator for buffer building");
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        if (shapeBuffers.Count == 0)
            return;

        _glassEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _glassEffect.SetTextures(textureSheets);
    }
}
