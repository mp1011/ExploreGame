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
/// Render pass for standard opaque geometry using two-pass lighting (ambient + point lights).
/// Acts as catch-all for any shapes not claimed by specialized passes.
/// </summary>
public class OpaqueRenderPass : IRenderPass
{
    private readonly TwoPassRenderEffect _renderEffect;
    private readonly VertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public OpaqueRenderPass(TwoPassRenderEffect renderEffect)
    {
        _renderEffect = renderEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Normal;

    public ShapeBuffer BuildBuffer(GraphicsDevice graphicsDevice, Shape shape, QualityLevel quality)
    {
        // This method isn't used in the current implementation since ShapeBufferCreator
        // builds buffers with its own logic. This pass only handles drawing.
        throw new System.NotImplementedException("OpaqueRenderPass uses ShapeBufferCreator for buffer building");
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        _renderEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _renderEffect.SetTextures(textureSheets);
    }
}
