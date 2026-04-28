using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.SimpleShapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Rendering;
using ExploringGame.Story;
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

    public ShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles,
        LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice,
        AnnotatedGraph<RoomLightData> roomLightGraph = null)
    {
        _textureSheets = loadedTextureSheets;
        _graphicsDevice = graphicsDevice;
        _shapeTriangles = shapeTriangles;
        _roomLightGraph = roomLightGraph;
    }

    private readonly VertexBufferBuilder _vertexBufferBuilder = new VertexBufferBuilder();
    private readonly GrassVertexBufferBuilder _grassVertexBufferBuilder = new GrassVertexBufferBuilder();

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

        // Separate ShapeStamps, StampedShapes, and GrassSurface from regular static shapes
        var shapeStamps = allShapes.OfType<ShapeStamp>().ToArray();
        var stampedShapes = allShapes.OfType<StampedShape>().ToArray();
        var grassSurfaces = allShapes.OfType<GrassSurface>().ToArray();
        var glassPanes = allShapes.OfType<GlassPane>().ToArray();

        var staticShapes = allShapes
            .Except(shapeStamps)
            .Except(stampedShapes)
            .Except(grassSurfaces)
            .Except(glassPanes).ToArray();

        // Get all rooms in the world segment
        var allRooms = worldSegment.TraverseAllChildren().OfType<Room>().ToArray();

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
                // The shape itself is a Room, so group it
                shouldGroup = true;
            }
            else
            {
                // Check if this is a simple child of a room (like furniture)
                var parent = shape.Parent as Room;
                if (parent != null)
                {
                    // Only group if it's a direct child of a room and not a special type
                    // Exclude PlaceableObjects and their children (they're handled separately)
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
                // Shape is not part of any room or shouldn't be grouped
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

        if (grassSurfaces.Any())
        {
            yield return CreateGrassShapeBuffer(worldSegment, grassSurfaces);
        }

        if (glassPanes.Any())
        {
            yield return CreateGlassPaneBuffer(worldSegment, glassPanes);
        }

        // Create buffers for active objects
        foreach (var activeObject in activeObjects.Where(p => p.Self.ViewFrom != ViewFrom.None))
        {
            if (activeObject.Self is StampedShape ss)
                yield return CreateStampShapeBuffer(ss, shapeStampBuffers);
            else
            {
                // Use the Room's LightingGroup if the active object has a room assigned
                Room lightingGroup = activeObject.Room?.LightingGroup;
                yield return CreateShapeBuffer(activeObject.Self, activeObject.Children, activeObject.Self.Theme.TextureSheetKey, lightingGroup);
            }
        }

        // create buffers for stamped shapes
        foreach(var stampedShape in stampedShapes)
        {
            yield return CreateStampShapeBuffer(stampedShape, shapeStampBuffers);
        }


    }

    private ShapeBuffer CreateStampShapeBuffer(StampedShape stampedShape, Dictionary<Type, ShapeBuffer> shapeStampBuffers)
    {
        var buffer = stampedShape.GetStampBuffer(shapeStampBuffers);
        return new ShapeBuffer(stampedShape, buffer.VertexBuffer, buffer.IndexBuffer, buffer.TriangleCount, buffer.Texture, buffer.RasterizerState, buffer.LightingGroup);
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
        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, key, shape.RasterizerState, lightingGroup);
    }

    private ShapeBuffer CreateGrassShapeBuffer(WorldSegment worldSegment, GrassSurface[] grassSurfaces)
    {
        var grassTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var grassSurface in grassSurfaces)
            grassTriangles[grassSurface] = _shapeTriangles[grassSurface];

        var grassTexture = _textureSheets.Get(TextureSheetKey.Outdoors);
        var buffers = _grassVertexBufferBuilder.Build(grassTriangles, grassTexture, _graphicsDevice);

        // Use worldSegment as the shape, Outdoors texture, and CullNone rasterizer state
        return new ShapeBuffer(worldSegment, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Outdoors, RasterizerState.CullNone, null, Type: ShapeBufferType.Grass);
    }

    private ShapeBuffer CreateGlassPaneBuffer(WorldSegment worldSegment, GlassPane[] glassPanes)
    {
        var triangles = new Dictionary<Shape, Triangle[]>();
        foreach (var glassPane in glassPanes)
            triangles[glassPane] = _shapeTriangles[glassPane];

        var texture = _textureSheets.Get(TextureSheetKey.Upstairs);
        var buffers = _vertexBufferBuilder.Build(triangles, texture, _graphicsDevice);

        // Use worldSegment as the shape, Outdoors texture, and CullNone rasterizer state
        return new ShapeBuffer(worldSegment, buffers.Item1, buffers.Item2, buffers.Item3, TextureSheetKey.Basement, RasterizerState.CullNone, null, Type: ShapeBufferType.Glass);
    }

    public ShapeBuffer CreateSkyboxBuffer(SkyboxShape skybox)
    {
        if (!_shapeTriangles.ContainsKey(skybox))
            return null;

        var skyboxTriangles = new Dictionary<Shape, Triangle[]>
        {
            [skybox] = _shapeTriangles[skybox]
        };

        var skyboxDepthStencilState = new DepthStencilState
        {
            DepthBufferEnable = true,
            DepthBufferWriteEnable = false,
            DepthBufferFunction = CompareFunction.LessEqual
        };

        var buffers = _vertexBufferBuilder.Build(skyboxTriangles, _textureSheets.Get(skybox.Theme.TextureSheetKey), _graphicsDevice);
        return new ShapeBuffer(skybox, buffers.Item1, buffers.Item2, buffers.Item3, skybox.Theme.TextureSheetKey, skybox.RasterizerState, skybox, skyboxDepthStencilState, ShapeBufferType.Skybox);
    }
}
