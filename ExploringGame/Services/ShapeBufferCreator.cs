using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class ShapeBufferCreator
{
    private readonly LoadedTextureSheets _textureSheets;
    private GraphicsDevice _graphicsDevice;
    private Dictionary<Shape, Triangle[]> _shapeTriangles;

    public ShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice)
    {
        _textureSheets = loadedTextureSheets;
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
        var activeObjects = worldSegment.TraverseAllChildren().OfType<IPlaceableObject>().ToArray();
        var activeObjectShapes = activeObjects.SelectMany(p => p.Children).ToArray();
        
        // Get all shapes except active object children
        var allShapes = worldSegment.TraverseAllChildren()
            .Except(activeObjectShapes)
            .Where(p => p.ViewFrom != ViewFrom.None)
            .ToArray();

        // Separate ShapeStamps from regular static shapes
        var shapeStamps = allShapes.OfType<ShapeStamp>().ToArray();
        var staticShapes = allShapes.Except(shapeStamps);
        
        // Group regular static shapes by texture
        var staticShapeGroups = staticShapes.GroupBy(p => p.Theme.TextureSheetKey);

        // Create buffers for grouped static shapes
        foreach (var shapeGroup in staticShapeGroups)
        {
            yield return CreateShapeBuffer(worldSegment, shapeGroup.ToArray(), shapeGroup.Key);
        }

        // Create individual buffers for each ShapeStamp
        foreach (var shapeStamp in shapeStamps)
        {
            yield return CreateShapeBuffer(shapeStamp, new[] { shapeStamp }, shapeStamp.Theme.TextureSheetKey);
        }

        // Create buffers for active objects
        foreach(var activeObject in activeObjects.Where(p => p.Self.ViewFrom != ViewFrom.None))
        {
            yield return CreateShapeBuffer(activeObject.Self, activeObject.Children, worldSegment.Theme.TextureSheetKey);
        }
    }


    private ShapeBuffer CreateShapeBuffer(
        Shape shape,
        Shape[] children,
        TextureSheetKey key)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, _textureSheets.Get(key), _graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, key);
    }
}
