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
            .Where(p => p.Self.ViewFrom != ViewFrom.None)
            .ToArray();

        return placeableShapes.SelectMany(p => CreateShapeBuffers(worldSegment, p));
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
}
