using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.Logics;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
namespace ExploringGame.Rendering;

public interface IRenderEffect
{
    void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection);
    void SetTextures(LoadedTextureSheets textureSheets);
}

public abstract class RenderEffect<TEffect> : IRenderEffect
    where TEffect : Effect
{
    private readonly Game _game;
    private Dictionary<TextureSheetKey, TEffect> _effects = new Dictionary<TextureSheetKey, TEffect>();
    
    protected RenderEffect(Game game)
    {
        _game = game;
    }

    public void SetTextures(LoadedTextureSheets textureSheets)
    {
        _effects.Clear();

        foreach (var textureSheet in textureSheets.LoadedTextures)
        {
            _effects[textureSheet.Key] = CreateEffect(_game.GraphicsDevice, _game.Content, textureSheet.Texture);
        }
    }

    protected abstract TEffect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture);

    public abstract void SetParameters(TEffect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection);
    
    public void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        foreach (var shapeBuffer in shapeBuffers)
        {
            var effect = _effects[shapeBuffer.Texture];
            SetParameters(effect, shapeBuffer, view, projection);

            // Apply custom RasterizerState if present (e.g., for wall decals with depth bias)
            var previousRasterizerState = graphicsDevice.RasterizerState;
            if (shapeBuffer.RasterizerState != null)
            {
                graphicsDevice.RasterizerState = shapeBuffer.RasterizerState;
            }

            // Apply custom DepthStencilState if present (e.g., for skybox)
            var previousDepthStencilState = graphicsDevice.DepthStencilState;
            if (shapeBuffer.DepthStencilState != null)
            {
                graphicsDevice.DepthStencilState = shapeBuffer.DepthStencilState;
            }

            graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
            graphicsDevice.Indices = shapeBuffer.IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();                
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
            }

            // Restore previous DepthStencilState
            if (shapeBuffer.DepthStencilState != null)
            {
                graphicsDevice.DepthStencilState = previousDepthStencilState;
            }

            // Restore previous RasterizerState
            if (shapeBuffer.RasterizerState != null)
            {
                graphicsDevice.RasterizerState = previousRasterizerState;
            }
        }
    }
}

public class BasicRenderEffect : RenderEffect<BasicEffect>
{
    private readonly RoomLightingCalculator _roomLightingCalculator;

    public BasicRenderEffect(RoomLightingCalculator roomLightingCalculator, Game game) : base(game)
    {
        _roomLightingCalculator = roomLightingCalculator;
    }

    protected override BasicEffect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var effect = new BasicEffect(graphicsDevice)
        {
            TextureEnabled = true,
            VertexColorEnabled = true,
            LightingEnabled = true,
            PreferPerPixelLighting = true,
            FogEnabled = false
        };
        effect.DirectionalLight0.Enabled = false;
        effect.Texture = texture;
        return effect;
    }

    public override void SetParameters(BasicEffect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var world = shapeBuffer.Shape.GetWorldMatrix();
        effect.World = world;
        effect.View = view;
        effect.Projection = projection;

        var b = 0.01f;
        effect.AmbientLightColor = new Vector3(b,b,b); // AmbientLight(shapeBuffer);
    }

    private Vector3 AmbientLight(ShapeBuffer shapeBuffer)
    {

        float brightness = LightIntensity.DefaultAmbientLight;
        var lightingGroup = shapeBuffer.LightingGroup;

        if (lightingGroup is Room room && _roomLightingCalculator.RoomLightGraph.TryGet(room, out var lightData))
        {
            brightness += lightData.TotalLight;
        }

        brightness /= 10f;
        return new Vector3(brightness, brightness, brightness);
    }
}   

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

        effect.Parameters["Arg0"]?.SetValue(Arg0);
        effect.Parameters["Arg1"]?.SetValue(Arg1);
        effect.Parameters["Arg2"]?.SetValue(Arg2);
        effect.Parameters["Arg3"]?.SetValue(Arg3);

        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
    }

    public (Vector3[] positions, Vector3[] colors, float[] intensities, int count) GetActiveLightsForBuffer(ShapeBuffer shapeBuffer)
    {
        // Pack only lights that are physically in this room's lighting group
        var positions = new Vector3[PointLights.MAX_LIGHTS];
        var colors = new Vector3[PointLights.MAX_LIGHTS];
        var intensities = new float[PointLights.MAX_LIGHTS];
        int activeLightCount = 0;

        if (shapeBuffer.LightData != null)
        {
            var lights = shapeBuffer.LightData.SortedContributions
                .Select(p => p.LightSource)
                .ToArray();
    
            foreach (var lightSource in lights)
            {
                if (!lightSource.On || activeLightCount >= PointLights.MAX_LIGHTS)
                    break;

                positions[activeLightCount] = lightSource.LightPosition;

                if(shapeBuffer.Shape is IPlaceableObject)
                {
                    positions[activeLightCount] = lightSource.LightPosition - shapeBuffer.Shape.Position;
                }

                colors[activeLightCount] = lightSource.Color.ToVector3();

                intensities[activeLightCount] = lightSource.Intensity;
                activeLightCount++;
            }
        }

        return (positions, colors, intensities, activeLightCount);
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

public class SkyboxRenderEffect : RenderEffect<Effect>
{
    public SkyboxRenderEffect(Game game) : base(game)
    {
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var skyboxEffect = contentManager.Load<Effect>("SkyboxEffect").Clone();
        skyboxEffect.Parameters["Texture"].SetValue(texture);
        return skyboxEffect;
    }

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var world = shapeBuffer.Shape.GetWorldMatrix();
        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
    }
}