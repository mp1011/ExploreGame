using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.Furniture;
using ExploringGame.GeometryBuilder.Shapes.Rooms.ExteriorRooms;
using ExploringGame.GeometryBuilder.Shapes.Rooms.UpstairsRooms;
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
namespace ExploringGame.Rendering.RenderEffects;

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

    public (Vector3[] positions, Vector3[] colors, float[] intensities, int count) GetActiveLightsForBuffer(ShapeBuffer shapeBuffer)
    {
        // Pack only lights that are physically in this room's lighting group
        var positions = new Vector3[PointLights.MAX_LIGHTS];
        var colors = new Vector3[PointLights.MAX_LIGHTS];
        var intensities = new float[PointLights.MAX_LIGHTS];
        int activeLightCount = 0;

        if (shapeBuffer.LightData != null)
        {
            foreach (var lightContribution in shapeBuffer.LightData.SortedContributions)
            {
                if (!lightContribution.LightSource.On || activeLightCount >= PointLights.MAX_LIGHTS)
                    break;

                positions[activeLightCount] = lightContribution.AdjustedPosition();

                if (shapeBuffer.Shape is IPlaceableObject)
                {
                    positions[activeLightCount] = lightContribution.LightSource.LightPosition - shapeBuffer.Shape.LocalPosition;
                }

                colors[activeLightCount] = lightContribution.LightSource.Color.ToVector3();

                intensities[activeLightCount] = lightContribution.LightSource.Intensity;
                activeLightCount++;
            }
        }

        return (positions, colors, intensities, activeLightCount);
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