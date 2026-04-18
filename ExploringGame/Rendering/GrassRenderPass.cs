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
/// Render pass for grass surfaces using custom billboard shader.
/// </summary>
public class GrassRenderPass : IRenderPass
{
    private readonly GrassRenderEffect _grassEffect;
    private readonly GrassVertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public int DrawOrder => 10; // Draw after opaque geometry

    public GrassRenderPass(GrassRenderEffect grassEffect)
    {
        _grassEffect = grassEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Grass;

    public ShapeBuffer BuildBuffer(GraphicsDevice graphicsDevice, Shape shape, QualityLevel quality)
    {
        // This method isn't used in the current implementation since ShapeBufferCreator
        // builds buffers with its own logic. This pass only handles drawing.
        throw new System.NotImplementedException("GrassRenderPass uses ShapeBufferCreator for buffer building");
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        foreach (var buffer in shapeBuffers)
        {
            _grassEffect.Draw(graphicsDevice, buffer, view, projection);
        }
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _grassEffect.SetTextures(textureSheets);
    }
}
