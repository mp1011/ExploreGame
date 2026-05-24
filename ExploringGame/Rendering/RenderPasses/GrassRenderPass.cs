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
/// Render pass for grass surfaces using custom billboard shader.
/// </summary>
public class GrassRenderPass : IRenderPass
{
    private readonly GrassRenderEffect _grassEffect;
    private readonly GrassVertexBufferBuilder _vertexBufferBuilder = new();
    private LoadedTextureSheets _textureSheets;

    public GrassRenderPass(GrassRenderEffect grassEffect)
    {
        _grassEffect = grassEffect;
    }

    public ShapeBufferType ShapeBufferType => ShapeBufferType.Grass;

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        _grassEffect.Draw(graphicsDevice, shapeBuffers, view, projection);        
    }

    public void LoadContent(Game game, LoadedTextureSheets textureSheets)
    {
        _textureSheets = textureSheets;
        _grassEffect.SetTextures(textureSheets);
    }
}
