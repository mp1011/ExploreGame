using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Rendering.RenderEffects;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.RenderPasses;

/// <summary>
/// Render pass for standard opaque geometry.
/// Acts as catch-all for any shapes not claimed by specialized passes.
/// </summary>
public class OpaqueRenderPass : IRenderPass
{
    private readonly IRenderEffect _renderEffect;

    public OpaqueRenderPass(IRenderEffect renderEffect)
    {
        _renderEffect = renderEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Normal;


    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        _renderEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _renderEffect.SetTextures(textureSheets);
    }
}
