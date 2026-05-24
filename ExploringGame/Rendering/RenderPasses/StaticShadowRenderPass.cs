using ExploringGame.Rendering.RenderEffects;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering.RenderPasses
{
    internal class StaticShadowRenderPass : IRenderPass
    {
        private readonly StaticShadowRenderEffect _effect;
        public ShapeBufferType ShapeBufferType => ShapeBufferType.StaticShadow;


        public StaticShadowRenderPass(StaticShadowRenderEffect effect)
        {
            _effect = effect;
        }

        public void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection)
        {
            _effect.Draw(graphicsDevice, shapeBuffers, view, projection);
        }

        public void LoadContent(Game game, LoadedTextureSheets textureSheets)
        {
        }
    }
}
