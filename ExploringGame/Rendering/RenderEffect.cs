using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
namespace ExploringGame.Rendering;

public interface IRenderEffect
{
    void Draw(GraphicsDevice graphicsDevice, ShapeBuffer[] shapeBuffers, Matrix view, Matrix projection);
}

public abstract class RenderEffect<TEffect> : IRenderEffect
    where TEffect : Effect
{
    private TEffect _effect;

    protected RenderEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        _effect = CreateEffect(graphicsDevice, contentManager, texture);
    }  

    protected abstract TEffect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture);

    public abstract void SetParameters(TEffect effect, Matrix world, Matrix view, Matrix projection);
    
    public void Draw(GraphicsDevice graphicsDevice, ShapeBuffer[] shapeBuffers, Matrix view, Matrix projection)
    {
        foreach (var shapeBuffer in shapeBuffers)
        {
            SetParameters(_effect, shapeBuffer.Shape.GetWorldMatrix(), view, projection);

            graphicsDevice.SetVertexBuffer(shapeBuffer.VertexBuffer);
            graphicsDevice.Indices = shapeBuffer.IndexBuffer;

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();                
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, shapeBuffer.TriangleCount);
            }
        }
    }
}

public class BasicRenderEffect : RenderEffect<BasicEffect>
{
    public BasicRenderEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture) 
        : base(graphicsDevice, contentManager, texture)
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
    public PointLightRenderEffect(GraphicsDevice graphicsDevice, ContentManager content, Texture2D texture) 
        : base(graphicsDevice, content, texture)
    {
    }

    protected override Effect CreateEffect(GraphicsDevice graphicsDevice, ContentManager contentManager, Texture2D texture)
    {
        var pointLightEffect = contentManager.Load<Effect>("PointLightEffect");

        // Set up point light parameters
        Vector3 lightPos = new Vector3(0, 3, 0); // Center of ceiling
        pointLightEffect.Parameters["LightPosition"].SetValue(lightPos);
        pointLightEffect.Parameters["LightColor"].SetValue(new Vector3(1f, 1f, 1f)); // White light
        pointLightEffect.Parameters["LightIntensity"].SetValue(1.0f); // Adjust for brightness
        pointLightEffect.Parameters["AmbientColor"].SetValue(new Vector3(0.08f, 0.08f, 0.08f));
        pointLightEffect.Parameters["Texture"].SetValue(texture);
        return pointLightEffect;
    }

    public override void SetParameters(Effect effect, Matrix world, Matrix view, Matrix projection)
    {
        effect.Parameters["World"].SetValue(world);
        effect.Parameters["View"].SetValue(view);
        effect.Parameters["Projection"].SetValue(projection);
    }
}
