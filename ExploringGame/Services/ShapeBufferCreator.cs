using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Rendering;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Services;

internal class ShapeBufferCreator
{
    private readonly LoadedTextureSheets _textureSheets;
    private GraphicsDevice _graphicsDevice;
    private Dictionary<Shape, Triangle[]> _shapeTriangles;
    private AnnotatedGraph<RoomLightData> _roomLightGraph;
    private readonly RenderPassRegistry _registry;
    private readonly IRenderPass _opaquePass;

    public ShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice,
        AnnotatedGraph<RoomLightData> roomLightGraph = null,
        RenderPassRegistry registry = null)
    {
        _textureSheets = loadedTextureSheets;
        _graphicsDevice = graphicsDevice;
        _shapeTriangles = shapeTriangles;
        _roomLightGraph = roomLightGraph;
        _registry = registry;
        _opaquePass = registry?.CatchAllPass;
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

        // Route shapes that are claimed by specialized (non-catch-all) passes.
        // Grass surfaces must always be excluded from the opaque batching path even if
        // no registry is present, because they require a different vertex format.
        var claimedShapes = new HashSet<Shape>();

        if (_registry != null)
        {
            foreach (var shape in allShapes)
            {
                var specializedPass = _registry.FindSpecializedPassForShape(shape);
                if (specializedPass != null)
                {
                    var buffer = specializedPass.BuildBuffer(shape, _shapeTriangles, _textureSheets, _graphicsDevice);
                    if (buffer != null)
                        yield return buffer;
                    claimedShapes.Add(shape);
                }
            }
        }
        else
        {
            // Legacy fallback: exclude grass so VertexBufferBuilder is not called with
            // grass triangles (wrong vertex format).
            foreach (var grassSurface in allShapes.OfType<GrassSurface>())
                claimedShapes.Add(grassSurface);
        }

        // Remaining shapes are handled by the opaque batching path.
        var remainingShapes = allShapes.Where(s => !claimedShapes.Contains(s)).ToArray();

        // Separate ShapeStamps, StampedShapes from regular static shapes
        var shapeStamps = remainingShapes.OfType<ShapeStamp>().ToArray();
        var stampedShapes = remainingShapes.OfType<StampedShape>().ToArray();
        var staticShapes = remainingShapes.Except(shapeStamps).Except(stampedShapes).ToArray();

        // Group static shapes by LightingGroup and Texture
        // Only group Room shapes and their direct static children (not StampedShapes, ShapeStamps, or active objects)
        var shapesGroupedByLightingGroup = new Dictionary<(ILightingGroup LightingGroup, TextureSheetKey Texture), List<Shape>>();
        var remainingStaticShapes = new List<Shape>();

        foreach (var shape in staticShapes)
        {
            // Only group Room shapes and their immediate furniture/fixture children
            // Exclude dynamic shapes, stamped shapes, and shapes with special placement logic
            Room parentRoom = shape as Room;
            bool shouldGroup = false;

            if (parentRoom != null)
            {
                shouldGroup = true;
            }
            else
            {
                var parent = shape.Parent as Room;
                if (parent != null)
                {
                    var isActiveObjectChild = activeObjects.Any(ao => ao.Self == shape || ao.Children.Contains(shape));
                    shouldGroup = !isActiveObjectChild;
                }
            }

            if (shouldGroup && parentRoom == null)
            {
                parentRoom = shape.Parent as Room;
            }

            if (shouldGroup && parentRoom != null)
            {
                var lightingGroup = parentRoom.LightingGroup;
                var textureKey = shape.Theme.TextureSheetKey;
                var key = (lightingGroup, textureKey);

                if (!shapesGroupedByLightingGroup.ContainsKey(key))
                {
                    shapesGroupedByLightingGroup[key] = new List<Shape>();
                }
                shapesGroupedByLightingGroup[key].Add(shape);
            }
            else
            {
                remainingStaticShapes.Add(shape);
            }
        }

        // Create buffers for shapes grouped by LightingGroup and Texture
        foreach (var group in shapesGroupedByLightingGroup)
        {
            var lightingGroup = group.Key.LightingGroup;
            var textureKey = group.Key.Texture;
            var shapes = group.Value.ToArray();

            yield return CreateShapeBuffer(worldSegment, shapes, textureKey, lightingGroup);
        }

        // Create buffers for remaining static shapes grouped by texture only
        var remainingShapeGroups = remainingStaticShapes.GroupBy(p => p.Theme.TextureSheetKey);
        foreach (var shapeGroup in remainingShapeGroups)
        {
            yield return CreateShapeBuffer(worldSegment, shapeGroup.ToArray(), shapeGroup.Key, lightingGroup: null);
        }

        Dictionary<Type, ShapeBuffer> shapeStampBuffers = new();
        // Create individual buffers for each ShapeStamp
        foreach (var shapeStamp in shapeStamps)
        {
            var buffer = CreateShapeBuffer(shapeStamp, new[] { shapeStamp }, shapeStamp.Theme.TextureSheetKey);
            yield return buffer;
            shapeStampBuffers[shapeStamp.GetType()] = buffer;
        }

        // Create buffers for active objects
        foreach (var activeObject in activeObjects.Where(p => p.Self.ViewFrom != ViewFrom.None))
        {
            if (activeObject.Self is StampedShape ss)
                yield return CreateStampShapeBuffer(ss, shapeStampBuffers);
            else
            {
                Room lightingGroup = activeObject.Room?.LightingGroup;
                yield return CreateShapeBuffer(activeObject.Self, activeObject.Children, worldSegment.Theme.TextureSheetKey, lightingGroup);
            }
        }

        // create buffers for stamped shapes
        foreach (var stampedShape in stampedShapes)
        {
            yield return CreateStampShapeBuffer(stampedShape, shapeStampBuffers);
        }
    }

    private ShapeBuffer CreateStampShapeBuffer(StampedShape stampedShape, Dictionary<Type, ShapeBuffer> shapeStampBuffers)
    {
        var buffer = stampedShape.GetStampBuffer(shapeStampBuffers);
        return new ShapeBuffer(stampedShape, buffer.VertexBuffer, buffer.IndexBuffer, buffer.TriangleCount,
            buffer.Texture, buffer.RenderPass ?? _opaquePass, buffer.RasterizerState, buffer.LightingGroup);
    }

    private ShapeBuffer CreateShapeBuffer(
        Shape shape,
        Shape[] children,
        TextureSheetKey key,
        ILightingGroup lightingGroup = null)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, _textureSheets.Get(key), _graphicsDevice);
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, key,
            _opaquePass, shape.RasterizerState, lightingGroup);
    }
}
