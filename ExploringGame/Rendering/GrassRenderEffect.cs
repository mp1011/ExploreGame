using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.Rendering;

/// <summary>
/// Render effect for grass surfaces using the custom grass shader.
/// Handles billboarding, lighting, and texture sampling for grass blades.
/// </summary>
public class GrassRenderEffect
{
    private readonly CameraService _cameraService;
    private readonly ContentManager _contentManager;
    private Effect _grassEffect;
    private Texture2D _grassTexture;
    private Vector3 _cameraPosition;

    public GrassRenderEffect(CameraService cameraService, Game game)
    {
        _cameraService = cameraService;
        _contentManager = game.Content;
    }

    public void SetTextures(LoadedTextureSheets textureSheets)
    {
        _grassTexture = textureSheets.Get(TextureSheetKey.Outdoors).Texture;
    }

    public void Draw(GraphicsDevice graphicsDevice, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        if (shapeBuffer == null)
            return;

        // Load effect if not already loaded
        if (_grassEffect == null)
        {
            _grassEffect = _contentManager.Load<Effect>("GrassEffect");
        }

        // Extract camera position from the inverse of the view matrix
        var inverseView = Matrix.Invert(view);
        _cameraPosition = inverseView.Translation;

        // Grass positions are already in world space, so use identity matrix
        var worldMatrix = Matrix.Identity;

        _grassEffect.Parameters["World"].SetValue(worldMatrix);
        _grassEffect.Parameters["View"].SetValue(view);
        _grassEffect.Parameters["Projection"].SetValue(projection);
        _grassEffect.Parameters["CameraPosition"].SetValue(_cameraPosition);
        _grassEffect.Parameters["LightDirection"].SetValue(Vector3.Normalize(new Vector3(0.5f, -1f, 0.3f)));
        _grassEffect.Parameters["GrassTexture"].SetValue(_grassTexture);

        // Use CullNone for grass so both sides are visible
        var previousRasterizerState = graphicsDevice.RasterizerState;
        graphicsDevice.RasterizerState = RasterizerState.CullNone;

        graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
        graphicsDevice.Indices = shapeBuffer.IndexBuffer;

        foreach (var pass in _grassEffect.CurrentTechnique.Passes)
        {
            pass.Apply();
            graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
        }

        // Restore previous state
        graphicsDevice.RasterizerState = previousRasterizerState;
    }
}
