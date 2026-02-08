using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Security.Cryptography;
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

    public abstract void SetParameters(TEffect effect, Matrix world, Matrix view, Matrix projection);
    
    public void Draw(GraphicsDevice graphicsDevice, IEnumerable<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
    {
        foreach (var shapeBuffer in shapeBuffers)
        {
            var effect = _effects[shapeBuffer.Texture];
            SetParameters(effect, shapeBuffer.Shape.GetWorldMatrix(), view, projection);

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
    public BasicRenderEffect(Game game) : base(game)
    {
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
        effect.AmbientLightColor = new Vector3(0.38f, 0.38f, 0.38f); // Very low ambient
        effect.DirectionalLight0.Enabled = false;
        effect.Texture = texture;
        return effect;
    }

    public override void SetParameters(BasicEffect effect, Matrix world, Matrix view, Matrix projection)
    {
        effect.World = world;
        effect.View = view;
        effect.Projection = projection;
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

    public override void SetParameters(Effect effect, Matrix world, Matrix view, Matrix projection)
    {
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
