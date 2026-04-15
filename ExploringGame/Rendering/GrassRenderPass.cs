using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

/// <summary>
/// Render pass for GrassSurface shapes. Claims every GrassSurface and
/// renders them with the billboarding grass shader.
/// </summary>
public class GrassRenderPass : IRenderPass
{
    private readonly CameraService _cameraService;
    private Effect _grassEffect;
    private Texture2D _grassTexture;

    public int DrawOrder => 10;
    public bool IsCatchAll => false;

    public GrassRenderPass(CameraService cameraService)
    {
        _cameraService = cameraService;
    }

    public bool ClaimsShape(Shape shape) => shape is GrassSurface;

    public void LoadContent(Game game, LoadedTextureSheets textures)
    {
        _contentManager = game.Content;
        _grassTexture = textures.Get(TextureSheetKey.Outdoors).Texture;
        _grassEffect = _contentManager.Load<Effect>("GrassEffect");
    }

    public ShapeBuffer BuildBuffer(Shape shape, Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets textureSheets, GraphicsDevice graphicsDevice)
    {
        var grassTriangles = new Dictionary<Shape, Triangle[]> { [shape] = shapeTriangles[shape] };
        var grassTexture = textureSheets.Get(TextureSheetKey.Outdoors);
        var builder = new GrassVertexBufferBuilder();
        var buffers = builder.Build(grassTriangles, grassTexture, graphicsDevice);
        if (buffers.Item1 == null)
            return null;
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3,
            TextureSheetKey.Outdoors, this, RasterizerState.CullNone);
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers,
        Matrix view, Matrix projection)
    {
        var inverseView = Matrix.Invert(view);
        var cameraPosition = inverseView.Translation;

        _grassEffect.Parameters["World"].SetValue(Matrix.Identity);
        _grassEffect.Parameters["View"].SetValue(view);
        _grassEffect.Parameters["Projection"].SetValue(projection);
        _grassEffect.Parameters["CameraPosition"].SetValue(cameraPosition);
        _grassEffect.Parameters["LightDirection"].SetValue(
            Vector3.Normalize(new Vector3(0.5f, -1f, 0.3f)));
        _grassEffect.Parameters["GrassTexture"].SetValue(_grassTexture);

        var previousRasterizerState = graphicsDevice.RasterizerState;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        foreach (var shapeBuffer in shapeBuffers)
        {
            if (shapeBuffer?.VertexBuffer == null)
                continue;
            graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
            graphicsDevice.Indices = shapeBuffer.IndexBuffer;
            foreach (var pass in _grassEffect.CurrentTechnique.Passes)
            {
                pass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
                    shapeBuffer.TriangleCount);
            }
        }

        graphicsDevice.RasterizerState = previousRasterizerState;
    }
}
