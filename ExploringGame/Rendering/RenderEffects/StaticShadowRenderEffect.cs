using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace ExploringGame.Rendering.RenderEffects;

/// <summary>
/// Render effect for glass panes with transparency and slight tint.
/// </summary>
public class StaticShadowRenderEffect : RenderEffect<Effect>
{
    private BlendState _transparentBlendState;

    public StaticShadowRenderEffect(Game game) : base(game)
    {
        _transparentBlendState = new BlendState
        {
            ColorSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.Zero,
            AlphaBlendFunction = BlendFunction.Add
        };
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
        // Enable alpha blending for transparent glass
        var previousBlendState = graphicsDevice.BlendState;
        graphicsDevice.BlendState = _transparentBlendState;

        // Render shadows after opaque geometry, reading from depth buffer but not writing to it
        var previousDepthStencilState = graphicsDevice.DepthStencilState;
        var depthState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false, // Don't write to depth buffer
            DepthBufferFunction = CompareFunction.LessEqual
        };
        graphicsDevice.DepthStencilState = depthState;

        base.Draw(graphicsDevice, shapeBuffers, view, projection);

        // Restore previous states
        graphicsDevice.BlendState = previousBlendState;
        graphicsDevice.DepthStencilState = previousDepthStencilState;
    }
}
