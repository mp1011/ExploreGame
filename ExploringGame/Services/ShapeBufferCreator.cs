using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.Logics;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class ShapeBufferCreator
{
    private readonly TextureSheet _textureSheet;
    private GraphicsDevice _graphicsDevice;
    private Dictionary<Shape, Triangle[]> _shapeTriangles;

    public ShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, 
        TextureSheet textureSheet, GraphicsDevice graphicsDevice)
    {
        _textureSheet = textureSheet;
        _graphicsDevice = graphicsDevice;
        _shapeTriangles = shapeTriangles;
    }

    private readonly VertexBufferBuilder _vertexBufferBuilder = new VertexBufferBuilder();

    public ShapeBuffer[] Execute()
    {
        return _shapeTriangles.Keys.OfType<WorldSegment>()
            .SelectMany(p => CreateShapeBuffers(p))
            .ToArray();
    }

    private IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        var activeObjects = worldSegment.TraverseAllChildren().OfType<IActiveObject>().ToArray();
        var activeObjectShapes = activeObjects.SelectMany(p => p.Children).ToArray();
        var staticShapes = worldSegment.TraverseAllChildren().Except(activeObjectShapes).ToArray();

        yield return CreateShapeBuffer(worldSegment, staticShapes);

        foreach(var activeObject in activeObjects)
        {
            yield return CreateShapeBuffer(activeObject.Self, activeObject.Children);
        }
    }


    private ShapeBuffer CreateShapeBuffer(
        Shape shape,
        Shape[] children)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, _textureSheet, _graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3);
    }
}
