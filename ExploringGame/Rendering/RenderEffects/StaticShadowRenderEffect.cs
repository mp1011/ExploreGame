using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.Rendering.RenderEffects;

/// <summary>
/// Render effect for static shadows that darken existing pixels.
/// </summary>
public class StaticShadowRenderEffect : RenderEffect<Effect>
{
    public StaticShadowRenderEffect(Game game) : base(game)
    {
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var effect = contentManager.Load<Effect>("StaticShadowEffect").Clone();
        return effect;
    }

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {      
        var world = shapeBuffer.Shape.GetWorldMatrix();
        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);      
    }

    public new void Draw(GraphicsDevice graphicsDevice, System.Collections.Generic.IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        // Use multiply blend state to darken existing pixels
        var previousBlendState = graphicsDevice.BlendState;
      
        graphicsDevice.BlendState = new BlendState()
        {
            ColorSourceBlend = Blend.Zero,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,

            AlphaSourceBlend = Blend.Zero,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        // Render shadows after opaque geometry, reading from depth buffer but not writing to it
        var previousDepthStencilState = graphicsDevice.DepthStencilState;
        var depthState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };
        graphicsDevice.DepthStencilState = depthState;

        base.Draw(graphicsDevice, shapeBuffers, view, projection);

        // Restore previous states
        graphicsDevice.BlendState = previousBlendState;
        graphicsDevice.DepthStencilState = previousDepthStencilState;
    }
}
