using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

/// <summary>
/// Catch-all render pass for all opaque geometry. Uses the two-pass
/// (ambient + point-light) render effect. Specialized passes are always
/// checked before this one because IsCatchAll is true.
/// </summary>
public class OpaqueRenderPass : IRenderPass
{
    private readonly RoomLightingCalculator _roomLightingCalculator;
    private readonly PointLights _pointLights;
    private TwoPassRenderEffect _effect;

    public int DrawOrder => 0;
    public bool IsCatchAll => true;

    public OpaqueRenderPass(RoomLightingCalculator roomLightingCalculator, PointLights pointLights)
    {
        _roomLightingCalculator = roomLightingCalculator;
        _pointLights = pointLights;
    }

    public bool ClaimsShape(Shape shape) => true;

    public void LoadContent(Game game, LoadedTextureSheets textures)
    {
        var basicEffect = new BasicRenderEffect(_roomLightingCalculator, game);
        var pointLightEffect = new PointLightRenderEffect(_pointLights, _roomLightingCalculator, game);
        _effect = new TwoPassRenderEffect(basicEffect, pointLightEffect);
        _effect.SetTextures(textures);
    }

    public ShapeBuffer BuildBuffer(Shape shape, Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets textureSheets, GraphicsDevice graphicsDevice)
    {
        var builder = new VertexBufferBuilder();
        var singleShapeTris = new Dictionary<Shape, Triangle[]> { [shape] = shapeTriangles[shape] };
        var texture = shape.Theme.TextureSheetKey;
        var buffers = builder.Build(singleShapeTris, textureSheets.Get(texture), graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3,
            texture, this, shape.RasterizerState);
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers,
        Matrix view, Matrix projection)
    {
        _effect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }
}
