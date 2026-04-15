using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace ExploringGame.Rendering;

public interface IRenderPass
{
    /// <summary>
    /// Controls draw sequence. Lower values render first (e.g. 0=opaque, 10=grass, 90=skybox).
    /// </summary>
    int DrawOrder { get; }

    /// <summary>
    /// When true this pass is the generic catch-all; specialized passes are always
    /// checked before this one regardless of DrawOrder.
    /// </summary>
    bool IsCatchAll { get; }

    /// <summary>Returns true if this pass should own and render the given shape.</summary>
    bool ClaimsShape(Shape shape);

    /// <summary>
    /// Builds a ShapeBuffer for a single shape that this pass owns.
    /// </summary>
    ShapeBuffer BuildBuffer(Shape shape, Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets textureSheets, GraphicsDevice graphicsDevice);

    /// <summary>Renders the supplied buffers using this pass's shader.</summary>
    void Draw(GraphicsDevice graphicsDevice, IReadOnlyList<ShapeBuffer> shapeBuffers,
        Matrix view, Matrix projection);

    /// <summary>Loads shader/texture resources needed by this pass.</summary>
    void LoadContent(Game game, LoadedTextureSheets textures);
}
