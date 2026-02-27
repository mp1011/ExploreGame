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

            graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
            graphicsDevice.Indices = shapeBuffer.IndexBuffer;

            foreach (var pass in effect.CurrentTechnique.Passes)
            {
                pass.Apply();                
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
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
            PreferPerPixelLighting = true
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
        effect.AmbientLightColor = AmbientLight(shapeBuffer);
    }

    private Vector3 AmbientLight(ShapeBuffer shapeBuffer)
    {

        var brightness = LightIntensity.DefaultAmbientLight; 
        var lightingGroup = shapeBuffer.LightingGroup;
        if (lightingGroup != null && _roomLightingCalculator.RoomLightGraph.TryGet(lightingGroup, out var lightData))
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
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var pointLightEffect = contentManager.Load<Effect>("PointLightEffect").Clone();
        // No texture needed for additive light pass
        return pointLightEffect;
    }

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var world = shapeBuffer.Shape.GetWorldMatrix();

        var (positions, colors, intensities, count) = GetActiveLightsForBuffer(shapeBuffer);

        effect.Parameters["LightPositions"].SetValue(positions);
        effect.Parameters["LightColors"].SetValue(colors);
        effect.Parameters["LightIntensities"].SetValue(intensities);
        effect.Parameters["LightCount"].SetValue(count);

        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
    }

    /// <summary>
    /// Gets the active lights for a shape buffer (testable method)
    /// </summary>
    public (Vector3[] positions, Vector3[] colors, float[] intensities, int count) GetActiveLightsForBuffer(ShapeBuffer shapeBuffer)
    {
        // Pack only lights that are physically in this room's lighting group
        var positions = new Vector3[PointLights.MAX_LIGHTS];
        var colors = new Vector3[PointLights.MAX_LIGHTS];
        var intensities = new float[PointLights.MAX_LIGHTS];
        int activeLightCount = 0;

        var lightingGroup = shapeBuffer.LightingGroup;
        if (lightingGroup != null && _roomLightingCalculator.RoomLightGraph.TryGet(lightingGroup, out var lightData))
        {
            // Get only the light sources physically located in this room
            // (not lights from neighboring rooms that contribute to ambient lighting)
            var lightSources = lightData.GetLightSourcesInRoom();

            foreach (var lightSource in lightSources)
            {
                if (activeLightCount >= PointLights.MAX_LIGHTS)
                    break;

                // Only include lights that are currently on
                if (lightSource.On)
                {
                    positions[activeLightCount] = lightSource.LightPosition;
                    colors[activeLightCount] = lightSource.Color.ToVector3();

                    // Apply scaling to expand the intensity range:
                    // Power > 1 makes bright lights brighter and dim lights dimmer
                    // - VeryDim (1) -> ~0.3 (very dim, barely visible)
                    // - Dim (2) -> ~0.9
                    // - IndoorLight (3) -> ~1.6 (moderate)
                    // - Bright (7) -> ~5.9
                    // - ExtremelyBright (10) -> ~10 (blindingly bright)
                    var scaledIntensity = MathF.Pow(lightSource.Intensity / 10f, 1.5f) * 10f;
                    intensities[activeLightCount] = scaledIntensity;
                    activeLightCount++;
                }
            }
        }

        return (positions, colors, intensities, activeLightCount);
    }

    public new void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        var previousBlendState = graphicsDevice.BlendState;
        var previousDepthStencilState = graphicsDevice.DepthStencilState;

        // Set up for second pass additive rendering
        graphicsDevice.BlendState = _additiveBlendState;

        // Configure depth-stencil for second pass:
        // - Enable depth testing (don't render behind geometry)
        // - Disable depth writing (we're not changing geometry)
        // - Use LessEqual comparison (allow rendering at same depth as first pass)
        graphicsDevice.DepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        base.Draw(graphicsDevice, shapeBuffers, view, projection);

        graphicsDevice.BlendState = previousBlendState;
        graphicsDevice.DepthStencilState = previousDepthStencilState;
    }
}

public class TwoPassRenderEffect : IRenderEffect
{
    private readonly BasicRenderEffect _firstPassEffect;
    private readonly PointLightRenderEffect _secondPassEffect;

    public TwoPassRenderEffect(BasicRenderEffect firstPassEffect, PointLightRenderEffect secondPassEffect)
    {
        _firstPassEffect = firstPassEffect;
        _secondPassEffect = secondPassEffect;
    }
    public void SetTextures(LoadedTextureSheets textureSheets)
    {
        _firstPassEffect.SetTextures(textureSheets);
        _secondPassEffect.SetTextures(textureSheets);
    }
    public void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {       
        _firstPassEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
        _secondPassEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }
}