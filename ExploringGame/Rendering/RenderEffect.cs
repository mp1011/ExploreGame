using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
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
        var lightingGroup = shapeBuffer.LightingGroup;
        if (lightingGroup != null && _roomLightingCalculator.RoomLightGraph.TryGet(lightingGroup, out var lightData))
        {
            float brightness = 0.05f + (lightData.TotalLight / 10.0f);
            return new Vector3(brightness, brightness, brightness);
        }
        return new Vector3(0.3f, 0.3f, 0.3f);
    }
}   

public class PointLightRenderEffect : RenderEffect<Effect>
{
    private PointLights _pointLights;

    public PointLightRenderEffect(PointLights pointLights, Game game) : base(game)
    {
        _pointLights = pointLights;
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var pointLightEffect = contentManager.Load<Effect>("PointLightEffect").Clone();
        //TEST  pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.08f, 0.08f, 0.08f));
        pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.0f, 0.0f, 0.0f));
        pointLightEffect.Parameters["Texture"].SetValue(texture);
        return pointLightEffect;
    }

    public override void SetParameters(Effect effect, ShapeBuffer shapeBuffer, Matrix view, Matrix projection)
    {
        var world = shapeBuffer.Shape.GetWorldMatrix();
        Vector3 lightPos = new Vector3(0, 4, 0); // Center of ceiling
        effect.Parameters["LightPositions"].SetValue(_pointLights.Positions);
        effect.Parameters["LightColors"].SetValue(_pointLights.Colors);
        effect.Parameters["LightIntensities"].SetValue(_pointLights.Intensities);

        effect.Parameters["LightRangeMin"]?.SetValue(_pointLights.RangeMins);
        effect.Parameters["LightRangeMax"]?.SetValue(_pointLights.RangeMaxs);

        effect.Parameters["LightCount"].SetValue(_pointLights.Intensities.Length);


        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
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
        // temporarily disabled _secondPassEffect.Draw(graphicsDevice, shapeBuffers, view, projection);
    }
}