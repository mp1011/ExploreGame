using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

/// <summary>
/// Render pass for SkyboxShape. Renders last with a depth-write-disabled
/// depth-stencil state and uses the rotation-only (SkyboxView) camera matrix.
/// </summary>
public class SkyboxRenderPass : IRenderPass
{
    private readonly CameraService _cameraService;
    private SkyboxRenderEffect _effect;

    public int DrawOrder => 90;
    public bool IsCatchAll => false;

    public SkyboxRenderPass(CameraService cameraService)
    {
        _cameraService = cameraService;
    }

    public bool ClaimsShape(Shape shape) => shape is SkyboxShape;

    public void LoadContent(Game game, LoadedTextureSheets textures)
    {
        _effect = new SkyboxRenderEffect(game);
        _effect.SetTextures(textures);
    }

    public ShapeBuffer BuildBuffer(Shape shape, Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets textureSheets, GraphicsDevice graphicsDevice)
    {
        var skybox = (SkyboxShape)shape;
        var skyboxTriangles = new Dictionary<Shape, Triangle[]> { [skybox] = shapeTriangles[skybox] };

        var skyboxDepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        var builder = new VertexBufferBuilder();
        var buffers = builder.Build(skyboxTriangles,
            textureSheets.Get(skybox.Theme.TextureSheetKey), graphicsDevice);

        return new ShapeBuffer(skybox, buffers.Item1, buffers.Item2, buffers.Item3,
            skybox.Theme.TextureSheetKey, this, skybox.RasterizerState, skybox,
            skyboxDepthStencilState);
    }

    public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers,
        Matrix view, Matrix projection)
    {
        // Skybox uses the rotation-only view to avoid translation parallax
        _effect.Draw(graphicsDevice, shapeBuffers, _cameraService.SkyboxView, projection);
    }
}
