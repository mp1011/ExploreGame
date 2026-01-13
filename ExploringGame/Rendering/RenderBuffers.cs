using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Services;
using ExploringGame.Texture;
using Microsoft.Xna.Framework;

namespace ExploringGame.Rendering;

public class RenderBuffers
{
    private readonly Game _game;
    private ShapeBuffer[] _nextSegmentShapeBuffers;

    public ShapeBuffer[] ActiveShapeBuffers { get; private set; }

    public TextureSheet CurrentTexture { get; set; }

    public RenderBuffers(Game game)
    {
        _game = game;
    }

    public void BuildSegment(WorldSegment worldSegment)
    {
        var triangles = worldSegment.Build((QualityLevel)8); //todo, quality level
        ActiveShapeBuffers = new ShapeBufferCreator(triangles, CurrentTexture, _game.GraphicsDevice).Execute();
    }

    public void PrepareNextSegment(WorldSegment worldSegment)
    {
        var triangles = worldSegment.Build((QualityLevel)8); //todo, quality level
        _nextSegmentShapeBuffers = new ShapeBufferCreator(triangles, CurrentTexture, _game.GraphicsDevice).Execute();
    }

    public void SwapActive()
    {
        ActiveShapeBuffers = _nextSegmentShapeBuffers;
        _nextSegmentShapeBuffers = null;
    }
}
