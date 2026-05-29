using ExploringGame.GeometryBuilder;
using ExploringGame.GeometryBuilder.Shapes;
using ExploringGame.GeometryBuilder.Shapes.WorldSegments;
using ExploringGame.Logics;
using ExploringGame.Logics.Pathfinding;
using ExploringGame.Texture;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.Linq;

namespace ExploringGame.Rendering.ShapeBufferCreators;

class ActiveObjectShapeBufferCreator : ShapeBufferCreator
{
    public ActiveObjectShapeBufferCreator(Dictionary<Shape, Triangle[]> shapeTriangles, LoadedTextureSheets loadedTextureSheets, GraphicsDevice graphicsDevice, AnnotatedGraph<RoomLightData> roomLightGraph = null) 
        : base(shapeTriangles, loadedTextureSheets, graphicsDevice, roomLightGraph)
    {
    }

    protected override IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment)
    {
        var placeableShapes = worldSegment.TraverseAllChildren()
            .OfType<IPlaceableObject>()
            .Where(p => p.Self.ViewFrom != ViewFrom.None && !IsChildOfPlaceableShape(p))
            .ToArray();

        return placeableShapes.SelectMany(p => CreateShapeBuffers(worldSegment, p));
    }

    private bool IsChildOfPlaceableShape(IPlaceableObject obj)
    {
        var parent = obj.Self.Parent;
        while(parent != null)
        {
            if (parent is IPlaceableObject)
                return true;

            parent = parent.Parent;
        }

        return false;
    }

    private IEnumerable<ShapeBuffer> CreateShapeBuffers(WorldSegment worldSegment, IPlaceableObject activeObject)
    {
        foreach(var textureGroup in activeObject.Self.TraverseAllChildren().GroupBy(p=>p.Theme.TextureSheetKey))
        {
            // Use the Room's LightingGroup if the active object has a room assigned
            var lightingGroup = activeObject.Room?.LightingGroup;           
            yield return CreateShapeBuffer(activeObject.Self, activeObject.Children, textureGroup.Key, lightingGroup);
        }
    }

    protected ShapeBuffer CreateShapeBuffer(
      Shape shape,
      Shape[] children,
      TextureSheetKey key,
      ILightingGroup lightingGroup)
    {
        var worldSegmentTriangles = new Dictionary<Shape, Triangle[]>();
        foreach (var child in children)
            worldSegmentTriangles[child] = _shapeTriangles[child];

        var buffers = _vertexBufferBuilder.Build(worldSegmentTriangles, _textureSheets.Get(key), _graphicsDevice);

        var lights = GetLights(lightingGroup);

        return new ShapeBuffer(shape, buffers.Item1, buffers.Item2, buffers.Item3, key, shape.RasterizerState, lightingGroup,
            LightData: lights,
            Type: ShapeBufferType.Normal,
            DistanceLightScale: 0.4f);
    }
}
