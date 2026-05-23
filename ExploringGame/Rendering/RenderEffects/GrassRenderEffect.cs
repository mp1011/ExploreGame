using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Runtime.InteropServices;

namespace ExploringGame.Rendering.RenderEffects;

/// <summary>
/// Render effect for grass surfaces using the custom grass shader.
/// Handles billboarding, lighting, and texture sampling for grass blades.
/// </summary>
public class GrassRenderEffect : RenderEffect<Effect>
{
    private readonly CameraService _cameraService;
    private readonly ContentManager _contentManager;
    private Texture2D _grassTexture;
    private Vector3 _cameraPosition;

    public GrassRenderEffect(CameraService cameraService, Game game) : base(game)
    {
        _cameraService = cameraService;
        _contentManager = game.Content;
    }

    // check if we need this
    //public void Draw(GraphicsDevice graphicsDevice, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    //{
    //    // Use CullNone for grass so both sides are visible
    //    var previousRasterizerState = graphicsDevice.RasterizerState;
    //    graphicsDevice.RasterizerState = RasterizerState.CullNone;

    //    graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
    //    graphicsDevice.Indices = shapeBuffer.IndexBuffer;

    //    foreach (var pass in _grassEffect.CurrentTechnique.Passes)
    //    {
    //        pass.Apply();
    //        graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
    //    }

    //    // Restore previous state
    //    graphicsDevice.RasterizerState = previousRasterizerState;
    //}

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        return _contentManager.Load<Effect>("GrassEffect");
    }

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var (positions, colors, intensities, count) = GetActiveLightsForBuffer(shapeBuffer);

        // Extract camera position from the inverse of the view matrix
        var inverseView = Matrix.Invert(view);
        _cameraPosition = inverseView.Translation;

        // Grass positions are already in world space, so use identity matrix
        var worldMatrix = Matrix.Identity;

        effect.Parameters["World"].SetValue(worldMatrix);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
        effect.Parameters["CameraPosition"].SetValue(_cameraPosition);

        if(positions.Length >= 2)
        {
            effect.Parameters["LightPosition1"]?.SetValue(positions[0]);
            effect.Parameters["LightIntensity1"]?.SetValue(intensities[0]);
            effect.Parameters["LightPosition2"]?.SetValue(positions[1]);
            effect.Parameters["LightIntensity2"]?.SetValue(intensities[1]);
        }
        else if (positions.Length == 1)
        {
            effect.Parameters["LightPosition1"]?.SetValue(positions[0]);
            effect.Parameters["LightIntensity1"]?.SetValue(intensities[0]);
            effect.Parameters["LightIntensity2"]?.SetValue(0);
        }
        else
        {
            effect.Parameters["LightIntensity1"]?.SetValue(0);
            effect.Parameters["LightIntensity2"]?.SetValue(0);
        }
        effect.Parameters["GrassTexture"].SetValue(_grassTexture);
    }
}
