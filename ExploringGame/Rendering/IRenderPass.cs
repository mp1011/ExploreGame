using ExploringGame.GeometryBuilder;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

public enum DrawOrder
{
    Opaque,
    Grass,
    Skybox,
    Glass
}

/// <summary>
/// Represents a rendering pass that handles specific types of shapes.
/// Each pass declares which shapes it owns, how to build buffers for them, and how to render them.
/// </summary>
public interface IRenderPass
{
    /// <summary>
    /// Determines what type of shape buffer is processed by this render pass.
    /// This also determines ordering.
    /// </summary>
    ShapeBufferType ShapeBufferType { get; }

    /// <summary>
    /// Creates a ShapeBuffer for a shape claimed by this pass.
    /// </summary>
    ShapeBuffer BuildBuffer(GraphicsDevice graphicsDevice, Shape shape, QualityLevel quality);

    /// <summary>
    /// Draws all buffers owned by this pass.
    /// </summary>
    void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers, Matrix view, Matrix projection);

    /// <summary>
    /// Loads content (shaders, textures) for this render pass.
    /// </summary>
    void LoadContent(Game game, LoadedTextureSheets textureSheets);
}
