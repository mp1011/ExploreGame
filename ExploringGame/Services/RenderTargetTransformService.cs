using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

public interface IRenderTargetTransformer
{
    bool IsActive { get; }
    void LoadContent(ContentManager contentManager);
    void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D sourceTexture, Rectangle destinationRectangle);
}

public class RenderTargetTransformService : IDisposable
{
    private readonly List<IRenderTargetTransformer> _transformers = new();
    private readonly BrightnessRenderTargetTransformer _brightnessTransformer = new();
    private readonly ColorRenderTargetTransformer _colorTransformer = new();

    private ContentManager _contentManager;
    private RenderTarget2D _workingRenderTargetA;
    private RenderTarget2D _workingRenderTargetB;

    public float Brightness
    {
        get => _brightnessTransformer.Brightness;
        set => _brightnessTransformer.Brightness = value;
    }

    public Color Color
    {
        get => _colorTransformer.TintColor;
        set => _colorTransformer.TintColor = value;
    }

    public RenderTargetTransformService()
    {
        AddTransformer(_brightnessTransformer);
        AddTransformer(_colorTransformer);
    }

    public void LoadContent(ContentManager contentManager)
    {
        _contentManager = contentManager;

        foreach (var transformer in _transformers)
        {
            transformer.LoadContent(contentManager);
        }
    }

    public void AddTransformer(IRenderTargetTransformer transformer)
    {
        _transformers.Add(transformer);

        if (_contentManager != null)
        {
            transformer.LoadContent(_contentManager);
        }
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, RenderTarget2D sourceTexture)
    {
        var activeTransformers = _transformers.Where(t => t.IsActive).ToList();
        if (activeTransformers.Count == 0)
        {
            DrawUntransformed(spriteBatch, sourceTexture, graphicsDevice.Viewport.Bounds);
            return;
        }

        EnsureWorkingRenderTargets(graphicsDevice, sourceTexture.Width, sourceTexture.Height);

        var finalRenderTargets = graphicsDevice.GetRenderTargets();
        Texture2D currentTexture = sourceTexture;
        bool useFirstWorkingTarget = true;

        for (int i = 0; i < activeTransformers.Count; i++)
        {
            var transformer = activeTransformers[i];
            bool isLastTransformer = i == activeTransformers.Count - 1;

            if (isLastTransformer)
            {
                RestoreRenderTargets(graphicsDevice, finalRenderTargets);
                transformer.Draw(spriteBatch, graphicsDevice, currentTexture, graphicsDevice.Viewport.Bounds);
                continue;
            }

            var workingRenderTarget = useFirstWorkingTarget ? _workingRenderTargetA : _workingRenderTargetB;
            useFirstWorkingTarget = !useFirstWorkingTarget;

            graphicsDevice.SetRenderTarget(workingRenderTarget);
            graphicsDevice.Clear(Color.Transparent);
            transformer.Draw(
                spriteBatch,
                graphicsDevice,
                currentTexture,
                new Rectangle(0, 0, workingRenderTarget.Width, workingRenderTarget.Height));
            currentTexture = workingRenderTarget;
        }
    }

    private void DrawUntransformed(SpriteBatch spriteBatch, Texture2D sourceTexture, Rectangle destinationRectangle)
    {
        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Opaque,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone);
        spriteBatch.Draw(sourceTexture, destinationRectangle, Color.White);
        spriteBatch.End();
    }

    private void EnsureWorkingRenderTargets(GraphicsDevice graphicsDevice, int width, int height)
    {
        bool matchesExistingTargets =
            _workingRenderTargetA != null &&
            _workingRenderTargetA.Width == width &&
            _workingRenderTargetA.Height == height &&
            _workingRenderTargetB != null &&
            _workingRenderTargetB.Width == width &&
            _workingRenderTargetB.Height == height;

        if (matchesExistingTargets)
        {
            return;
        }

        _workingRenderTargetA?.Dispose();
        _workingRenderTargetB?.Dispose();

        _workingRenderTargetA = CreateWorkingRenderTarget(graphicsDevice, width, height);
        _workingRenderTargetB = CreateWorkingRenderTarget(graphicsDevice, width, height);
    }

    private static RenderTarget2D CreateWorkingRenderTarget(GraphicsDevice graphicsDevice, int width, int height)
    {
        return new RenderTarget2D(
            graphicsDevice,
            width,
            height,
            false,
            SurfaceFormat.Color,
            DepthFormat.None);
    }

    private static void RestoreRenderTargets(GraphicsDevice graphicsDevice, RenderTargetBinding[] renderTargets)
    {
        if (renderTargets.Length == 0)
        {
            graphicsDevice.SetRenderTarget(null);
            return;
        }

        graphicsDevice.SetRenderTargets(renderTargets);
    }

    public void Dispose()
    {
        foreach (var disposableTransformer in _transformers.OfType<IDisposable>())
        {
            disposableTransformer.Dispose();
        }

        _workingRenderTargetA?.Dispose();
        _workingRenderTargetB?.Dispose();
    }
}

internal abstract class EffectRenderTargetTransformer : IRenderTargetTransformer, IDisposable
{
    private readonly string _effectName;
    private Effect _effect;

    protected EffectRenderTargetTransformer(string effectName)
    {
        _effectName = effectName;
    }

    public abstract bool IsActive { get; }

    public void LoadContent(ContentManager contentManager)
    {
        _effect?.Dispose();
        _effect = contentManager.Load<Effect>(_effectName).Clone();
    }

    public void Draw(SpriteBatch spriteBatch, GraphicsDevice graphicsDevice, Texture2D sourceTexture, Rectangle destinationRectangle)
    {
        if (_effect == null)
        {
            throw new InvalidOperationException($"{GetType().Name} must LoadContent before drawing.");
        }

        _effect.Parameters["SceneTexture"].SetValue(sourceTexture);
        _effect.Parameters["ViewportSize"].SetValue(new Vector2(graphicsDevice.Viewport.Width, graphicsDevice.Viewport.Height));
        ConfigureEffect(_effect);

        spriteBatch.Begin(
            SpriteSortMode.Deferred,
            BlendState.Opaque,
            SamplerState.LinearClamp,
            DepthStencilState.None,
            RasterizerState.CullNone,
            _effect);
        spriteBatch.Draw(sourceTexture, destinationRectangle, Color.White);
        spriteBatch.End();
    }

    protected abstract void ConfigureEffect(Effect effect);

    public void Dispose()
    {
        _effect?.Dispose();
    }
}

internal class BrightnessRenderTargetTransformer : EffectRenderTargetTransformer
{
    private float _brightness = 1f;

    public BrightnessRenderTargetTransformer() : base("RenderTargetTransformEffect")
    {
    }

    public float Brightness
    {
        get => _brightness;
        set => _brightness = Math.Max(0f, value);
    }

    public override bool IsActive => Math.Abs(Brightness - 1f) > 0.001f;

    protected override void ConfigureEffect(Effect effect)
    {
        effect.Parameters["Brightness"].SetValue(Brightness);
        effect.Parameters["TintColor"].SetValue(Vector3.One);
    }
}

internal class ColorRenderTargetTransformer : EffectRenderTargetTransformer
{
    public ColorRenderTargetTransformer() : base("RenderTargetTransformEffect")
    {
    }

    public Color TintColor { get; set; } = Color.White;

    public override bool IsActive => TintColor.PackedValue != Color.White.PackedValue;

    protected override void ConfigureEffect(Effect effect)
    {
        effect.Parameters["Brightness"].SetValue(1f);
        effect.Parameters["TintColor"].SetValue(TintColor.ToVector3());
    }
}
