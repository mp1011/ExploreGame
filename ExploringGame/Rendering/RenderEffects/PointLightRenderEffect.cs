using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.Logics;
using ExploringGame.Services;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExploringGame.Rendering.RenderEffects;

public class PointLightRenderEffect : RenderEffect<Effect>
{
    private PointLights _pointLights;
    private RoomLightingCalculator _roomLightingCalculator;
    private BlendState _additiveBlendState;
    private DepthStencilState _depthStencilState;
    private RasterizerState _rasterizerState;

    public PointLightRenderEffect(PointLights pointLights, RoomLightingCalculator roomLightingCalculator, Game game) : base(game)
    {
        _pointLights = pointLights;
        _roomLightingCalculator = roomLightingCalculator;

        // Set up additive blend state for the second pass
        _additiveBlendState = new BlendState
        {
            ColorSourceBlend = Blend.One,
            ColorDestinationBlend = Blend.One,
            ColorBlendFunction = BlendFunction.Add,
            AlphaSourceBlend = Blend.One,
            AlphaDestinationBlend = Blend.One,
            AlphaBlendFunction = BlendFunction.Add
        };

        // Configure depth-stencil for second pass:
        // - Enable depth testing (don't render behind geometry)
        // - Disable depth writing (we're not changing geometry)
        // - Use LessEqual comparison (allow rendering at same depth as first pass)
        _depthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        // Apply a small depth bias to push the second pass slightly forward
        // This prevents z-fighting due to floating-point precision issues
        _rasterizerState = new RasterizerState
        {
            CullMode = CullMode.CullCounterClockwiseFace,
            DepthBias = -0.00001f,
            SlopeScaleDepthBias = -1f
        };
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var pointLightEffect = contentManager.Load<Effect>("PointLightEffect").Clone();
        pointLightEffect.Parameters["Texture"]?.SetValue(texture);
        return pointLightEffect;
    }

    public static float Arg0 = -0.16f;
    public static float Arg1 = 0.3f;
    public static float Arg2 = -0.2f;
    public static float Arg3 = 0.0f;

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var world = shapeBuffer.Shape.GetWorldMatrix();

        var (positions, colors, intensities, count) = GetActiveLightsForBuffer(shapeBuffer);

        effect.Parameters["LightPositions"]?.SetValue(positions);
        effect.Parameters["LightColors"]?.SetValue(colors);
        effect.Parameters["LightIntensities"]?.SetValue(intensities);
        effect.Parameters["LightCount"]?.SetValue(count);

        effect.Parameters["NormalLightScale"]?.SetValue(shapeBuffer.NormalLightScale);
        effect.Parameters["DistanceLightScale"]?.SetValue(shapeBuffer.DistanceLightScale);


        // debugging, currently not wired up
        effect.Parameters["Arg0"]?.SetValue(Arg0);
        effect.Parameters["Arg1"]?.SetValue(Arg1);
        effect.Parameters["Arg2"]?.SetValue(Arg2);
        effect.Parameters["Arg3"]?.SetValue(Arg3);

        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
    }


    public new void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        var previousBlendState = graphicsDevice.BlendState;
        var previousDepthStencilState = graphicsDevice.DepthStencilState;
        var previousRasterizerState = graphicsDevice.RasterizerState;

        // Set up for second pass additive rendering
        graphicsDevice.BlendState = _additiveBlendState;
        graphicsDevice.DepthStencilState = _depthStencilState;
        graphicsDevice.RasterizerState = _rasterizerState;

        base.Draw(graphicsDevice, shapeBuffers, view, projection);

        graphicsDevice.BlendState = previousBlendState;
        graphicsDevice.DepthStencilState = previousDepthStencilState;
        graphicsDevice.RasterizerState = previousRasterizerState;
    }
}
